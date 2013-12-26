using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using OnlineUpdateClient;
using SmartSchool.AccessControl;
using SmartSchool.ApplicationLog;
using SmartSchool.ExceptionHandler;

namespace SmartSchool
{
    public class CurrentUser
    {
        #region �W��Ҧ�
        private static CurrentUser _Instance;
        public static CurrentUser Instance
        {
            get
            {
                if ( _Instance == null )
                    _Instance = new CurrentUser();
                return _Instance;
            }
        }
        private CurrentUser()
        {
            _CrossThreadControl = new Control();
            IntPtr p = _CrossThreadControl.Handle;
        }
        #endregion

        private Control _CrossThreadControl;
        //private DSConnection _Conn;
        //private bool _IsLogined;
        private Thread _PreferenceLoading = null;
        private PreferenceCollection _Preference;
        private Thread _ConfigurationLoading = null;
        private ConfigurationCollection _Configuration;
        //private FeatureCollection _allowFeatures;
        private Thread _schoolInfoLoading = null;
        private SchoolInfo _schoolInfo = new SchoolInfo();
        private Thread _schoolConfigLoading = null;
        private SchoolConfig _schoolConfig = new SchoolConfig();
        private Thread _systemConfigLoading = null;
        private SystemConfig _systemConfig = new SystemConfig();
        private VersionManifest _manifest;
        private LogProvider _log;
        private LogSender _log_sender;
        private string _user_name;
        private bool _debug_mode;
        private string _currentAccessPoint;
        private static Thread _rolesLoading = null;
        private static FeatureAcl _acl = new FeatureAcl();
        private List<string> _roles = new List<string>();

        public event EventHandler LoginSuccess;

        public int SchoolYear
        {
            get
            {
                _systemConfigLoading.Join(); return _systemConfig.DefaultSchoolYear;
            }
        }

        public int Semester
        {
            get
            {
                _systemConfigLoading.Join(); return _systemConfig.DefaultSemester;
            }
        }

        public string SchoolChineseName
        {
            get { _schoolInfoLoading.Join(); return _schoolInfo.ChineseName; }
        }

        public string SchoolEnglishName
        {
            get { _schoolInfoLoading.Join(); return _schoolInfo.EnglishName; }
        }

        public string SchoolCode
        {
            get { _schoolInfoLoading.Join(); return _schoolInfo.Code; }
        }

        public string SystemVersion
        {
            get
            {
                if ( _manifest == null )
                    return "0.0.0.0";
                else
                {
                    if ( _manifest.Version == null )
                        return "0.0.0.0";
                    else
                        return _manifest.Version.ToString();
                }
            }
        }

        public string SystemName
        {
            get
            {
                return "Smart School";
            }
        }

        public bool IsDebugMode
        {
            get { return _debug_mode; }
            set { _debug_mode = value; }
        }

        public LogProvider AppLog
        {
            get { return _log; }
        }

        public string UserName
        {
            get { return FISCA.Authentication.DSAServices.UserAccount; }
        }

        //private bool _is_sys_admin;
        public bool IsSysAdmin
        {
            get { return FISCA.Authentication.DSAServices.IsSysAdmin; }
        }

        public string AccessPoint
        {
            get { return FISCA.Authentication.DSAServices.AccessPoint; }
        }

        public SchoolInfo SchoolInfo
        {
            get { _schoolInfoLoading.Join(); return _schoolInfo; }
        }

        public SchoolConfig SchoolConfig
        {
            get { _schoolConfigLoading.Join(); return _schoolConfig; }
        }

        internal SystemConfig SystemConfig
        {
            get { _systemConfigLoading.Join(); return _systemConfig; }
        }

        public bool IsLogined
        {
            get { return FISCA.Authentication.DSAServices.IsLogined; }
        }
        public PreferenceCollection Preference
        {
            get { _PreferenceLoading.Join(); return _Preference; }
        }
        public ConfigurationCollection Configuration
        {
            get { _ConfigurationLoading.Join(); return _Configuration; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static FeatureAcl Acl
        {
            get { _rolesLoading.Join(); return _acl; }
        }

        public void Initial()
        {
            _rolesLoading = new Thread(new ThreadStart(delegate
            {
                GetUserRoles();
            }));
            _rolesLoading.IsBackground = true;
            _rolesLoading.Priority = ThreadPriority.AboveNormal;
            _rolesLoading.Start();

            _PreferenceLoading = new Thread(new ThreadStart(delegate
            {
                _Preference = new PreferenceCollection();
            }));
            _PreferenceLoading.IsBackground = true;
            _PreferenceLoading.Priority = ThreadPriority.AboveNormal;
            _PreferenceLoading.Start();


            _schoolConfigLoading = new Thread(new ThreadStart(delegate
            {
                _schoolConfig.Load(); //���J�Ǯճ]�w
            }));
            _schoolConfigLoading.IsBackground = true;
            _schoolConfigLoading.Priority = ThreadPriority.AboveNormal;
            _schoolConfigLoading.Start();

            _ConfigurationLoading = new Thread(new ThreadStart(delegate
            {
                _Configuration = new ConfigurationCollection();
            }));
            _ConfigurationLoading.IsBackground = true;
            _ConfigurationLoading.Priority = ThreadPriority.AboveNormal;
            _ConfigurationLoading.Start();

            _schoolInfoLoading = new Thread(new ThreadStart(delegate
            {
                _schoolInfo.Load(); //���J�Ǯո�T
            }));
            _schoolInfoLoading.IsBackground = true;
            _schoolInfoLoading.Priority = ThreadPriority.AboveNormal;
            _schoolInfoLoading.Start();

            _systemConfigLoading = new Thread(new ThreadStart(delegate
            {
                _systemConfig.Load(); //���J�t�ά����]�w
            }));
            _systemConfigLoading.IsBackground = true;
            _systemConfigLoading.Priority = ThreadPriority.AboveNormal;
            _systemConfigLoading.Start();


            LoadManifestInfo(); //���J����������T
            try
            {
                SetAsposeComponentsLicense();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitialLogProvider(this.UserName);

        }

        private void GetUserRoles()
        {
            DSXmlHelper helper = new DSXmlHelper("Request");
            helper.AddElement("Condition");
            helper.AddElement("Condition", "UserName", this.UserName.ToUpper());
            DSResponse dsrsp = CallService("SmartSchool.Personal.GetRoles", new DSRequest(helper));
            foreach ( XmlElement roleElement in dsrsp.GetContent().GetElements("Role") )
            {
                string role = roleElement.GetAttribute("Description");
                if ( !_roles.Contains(role) )
                    _roles.Add(role);

                foreach ( XmlElement featureElement in roleElement.SelectNodes("Permissions/Feature") )
                {
                    string code = featureElement.GetAttribute("Code");
                    string perm = featureElement.GetAttribute("Permission");
                    FeatureAce ace = new FeatureAce(code, perm);
                    _acl.MergeAce(ace);
                }
            }
        }

        /// <summary>
        /// �^�����~�T���� Server�C
        /// </summary>
        public static void ReportError(Exception exception)
        {
            CurrentUser user = CurrentUser.Instance;
            BugReporter.ReportException(user.SystemName, user.SystemVersion, exception, false);
        }

        private void InitialLogProvider(string userName)
        {
            LogStorageQueue logstorage = new LogStorageQueue();
            _log_sender = new LogSender(logstorage);
            _log = new LogProvider(logstorage, userName);
        }

        private void SetAsposeComponentsLicense()
        {
            try
            {
                Type type = this.GetType();
                Stream slic = new MemoryStream(Properties.Resources.aspose_total);//type.Assembly.GetManifestResourceStream("SmartSchool.Aspose.Total.lic");

                Aspose.Cells.License lic = new Aspose.Cells.License();
                lic.SetLicense(slic);

                slic.Seek(0, SeekOrigin.Begin);
                Aspose.Words.License licword = new Aspose.Words.License();
                licword.SetLicense(slic);

                slic.Seek(0, SeekOrigin.Begin);
                Aspose.BarCode.License licbarcode = new Aspose.BarCode.License();
                licbarcode.SetLicense(slic);
            }
            catch ( Exception ex )
            {
                BugReporter.ReportException(SystemName, SystemVersion, ex, false);
            }
        }

        //internal bool CheckUserPassword(string loginName, string password)
        //{
        //    try
        //    {
        //        DSXmlHelper request = new DSXmlHelper("Request");
        //        request.AddElement("Condition");
        //        request.AddElement("Condition", "UserName", loginName.ToUpper());
        //        request.AddElement("Condition", "Password", PasswordHash.Compute(password));

        //        DSXmlHelper response = _Conn.SendRequest("SmartSchool.Personal.CheckUserPassword", request);

        //        if ( response.GetElements("User").Length <= 0 )
        //            throw new Exception("�ϥΪ̱b���αK�X���~�C");

        //        XmlElement user = response.GetElement("User");
        //        if ( user.GetAttribute("IsSysAdmin") == "1" )
        //            _is_sys_admin = true;
        //        else
        //            _is_sys_admin = false;


        //        _user_name = loginName;
        //        BugReporter.SetRunTimeMessage("LoginUser", loginName);
        //        _IsLogined = true;

        //        return true;
        //    }
        //    catch ( Exception ex )
        //    {
        //        Framework.MsgBox.Show(ArrangeExceptionMessage(ex));
        //        return false;
        //    }

        //}

        //internal bool SetConnection(LicenseInfo license)
        //{
        //    try
        //    {
        //        DSAServices.SetLicense(license.DecryptLicense());
        //        BugReporter.SetRunTimeMessage("DsnsName", license.AccessPoint);

        //        return true;
        //    }
        //    catch ( DSAServerException ex )
        //    {
        //        switch ( ex.ServerStatus )
        //        {
        //            case DSAServerStatus.AccessDeny:
        //                Framework.MsgBox.Show(DSAServerStatus.AccessDeny.ToString() + " �ڵ��s��");
        //                break;
        //            case DSAServerStatus.ApplicationUnavailable:
        //                Framework.MsgBox.Show(DSAServerStatus.ApplicationUnavailable + " DSA Application �պA�Ϊ��A�����T");
        //                break;
        //            case DSAServerStatus.CredentialInvalid:
        //                Framework.MsgBox.Show("�n�J���ѡA�нT�{�b���K�X");
        //                break;
        //            case DSAServerStatus.InvalidRequestDocument:
        //                Framework.MsgBox.Show(DSAServerStatus.InvalidRequestDocument + " ���X�k���ӽФ��");
        //                break;
        //            case DSAServerStatus.InvalidResponseDocument:
        //                Framework.MsgBox.Show(DSAServerStatus.InvalidResponseDocument + " ���X�k���^�Ф��");
        //                break;
        //            case DSAServerStatus.PassportExpire:
        //                Framework.MsgBox.Show(DSAServerStatus.PassportExpire + " DSA Passport �L��");
        //                break;
        //            case DSAServerStatus.ServerUnavailable:
        //                Framework.MsgBox.Show(DSAServerStatus.ServerUnavailable + " DSA Server �պA�Ϊ��A�����T");
        //                break;
        //            case DSAServerStatus.ServiceActivationError:
        //                Framework.MsgBox.Show(DSAServerStatus.ServiceActivationError + " �A�ȱҰʿ��~");
        //                break;
        //            case DSAServerStatus.ServiceBusy:
        //                Framework.MsgBox.Show(DSAServerStatus.ServiceBusy + " �A�Ȧ��L");
        //                break;
        //            case DSAServerStatus.ServiceExecutionError:
        //                Framework.MsgBox.Show(DSAServerStatus.ServiceExecutionError + " �A�Ȥ������~");
        //                break;
        //            case DSAServerStatus.ServiceNotFound:
        //                Framework.MsgBox.Show(DSAServerStatus.ServiceNotFound + " �A�Ȥ��s�b");
        //                break;
        //            case DSAServerStatus.SessionExpire:
        //                Framework.MsgBox.Show(DSAServerStatus.SessionExpire + " Session �L��");
        //                break;
        //            case DSAServerStatus.Successful:
        //                Framework.MsgBox.Show("���\");
        //                break;
        //            case DSAServerStatus.UnhandledException:
        //                Framework.MsgBox.Show(DSAServerStatus.UnhandledException + " DSA Server ���w���B�z�� Exception");
        //                break;
        //            case DSAServerStatus.Unknow:
        //                Framework.MsgBox.Show(DSAServerStatus.Unknow + " ���������A");
        //                break;
        //            default:
        //                switch ( ex.ServerStatus.ToString() )
        //                {
        //                    case "513":
        //                        Framework.MsgBox.Show("�s�u�� DSNS �D�����~");
        //                        break;
        //                }
        //                break;
        //        }

        //    }
        //    catch ( ConnectException ex )
        //    {
        //        Framework.MsgBox.Show(ArrangeExceptionMessage(ex));
        //    }
        //    catch ( Exception ex )
        //    {
        //        Framework.MsgBox.Show(ArrangeExceptionMessage(ex));
        //    }
        //    return false;
        //}

        private string ArrangeExceptionMessage(Exception ex)
        {
            string msg = string.Empty;
            int level = 0;
            Exception temp = ex;

            while ( temp != null )
            {
                if ( msg != string.Empty )
                    msg += "\n".PadRight(level * 5, ' ') + temp.Message;
                else
                    msg = temp.Message;

                temp = temp.InnerException;
                level++;
            }

            return msg;
        }

        internal DSResponse CallService(string service, DSRequest req)
        {
            return FISCA.Authentication.DSAServices.CallService(service, req);
            //bool isQueryRequest = false;
            //DSResponse resp = null;
            //bool success = false;
            //bool leaveOnError = false;
            //bool isWebException = false;
            //string exceptionMessage = "";
            //for ( int runTimes = 1 ; runTimes > 0 ; runTimes-- )
            //{
            //    try
            //    {
            //        DateTime d1 = DateTime.Now;
            //        MotherForm.NetWorking();
            //        resp =  _Conn.SendRequest(service, req, 60000);  //60��~  Timeout
            //        success = true;
            //        if ( File.Exists(System.IO.Path.Combine(Application.StartupPath, "���ƵL�������")) )//�}�Ҽ����������
            //        {
            //            if ( Control.ModifierKeys == Keys.Alt )// & Control.ModifierKeys == Keys.Control & Control.ModifierKeys == Keys.Shift )
            //            {
            //                success = false; isWebException = true;
            //            }
            //            else
            //                Thread.Sleep(3500);
            //        }
            //        MotherForm.SetupSpeed(resp.GetRawBinary().Length, ( (TimeSpan)( DateTime.Now - d1 ) ).Milliseconds);
            //    }
            //    catch ( Exception e )
            //    {
            //        #region �O�������D
            //        success = false;
            //        isWebException = IsWebException(e);
            //        if ( isWebException )
            //        {
            //            exceptionMessage = GetExceptionMessage<WebException>(e);
            //            //�n�@�U�η�
            //            ExceptionHandler.BugReporter.ReportException(e, false);
            //            foreach ( StackFrame frame in ( new StackTrace() ).GetFrames() )
            //            {
            //                #region LeaveOnErrorCheck
            //                #region �����I�s���
            //                if ( !leaveOnError )
            //                {
            //                    foreach ( object var in frame.GetMethod().GetCustomAttributes(true) )
            //                    {
            //                        if ( var is LeaveOnErrorAttribute )
            //                        {
            //                            leaveOnError = true;
            //                            break;
            //                        }
            //                    }
            //                }
            //                #endregion
            //                #region �A���I�s��ƪ�class
            //                if ( !leaveOnError )
            //                {
            //                    Type type = frame.GetMethod().ReflectedType;
            //                    foreach ( object var in type.GetCustomAttributes(true) )
            //                    {
            //                        if ( var is LeaveOnErrorAttribute )
            //                        {
            //                            leaveOnError = true;
            //                            break;
            //                        }
            //                    }
            //                }
            //                #endregion
            //                if ( leaveOnError ) break;
            //                #endregion
            //                #region isQueryRequestCheck
            //                #region �����I�s���
            //                if ( !isQueryRequest )
            //                {
            //                    foreach ( object var in frame.GetMethod().GetCustomAttributes(true) )
            //                    {
            //                        if ( var is QueryRequestAttribute )
            //                        {
            //                            isQueryRequest = true;
            //                            runTimes += ( var as QueryRequestAttribute ).ReTryTimes;
            //                            break;
            //                        }
            //                    }
            //                }
            //                #endregion
            //                #region �A���I�s��ƪ�class
            //                if ( !isQueryRequest )
            //                {
            //                    Type type = frame.GetMethod().ReflectedType;
            //                    foreach ( object var in type.GetCustomAttributes(true) )
            //                    {
            //                        if ( var is QueryRequestAttribute )
            //                        {
            //                            isQueryRequest = true;
            //                            runTimes += ( var as QueryRequestAttribute ).ReTryTimes;
            //                            break;
            //                        }
            //                    }
            //                }
            //                #endregion
            //                if ( isQueryRequest ) break;
            //                #endregion
            //            }
            //        #endregion
            //        }
            //        else
            //        {
            //            throw new SendRequestException(service, req, e);
            //        }
            //    }
            //    finally
            //    {
            //        if ( success )
            //            runTimes = 0;
            //        //���ѥB�̫�@���j��
            //        if ( isWebException && !success && runTimes <= 1 )
            //        {
            //            if ( MotherForm.IsClosed )
            //            {
            //                if ( !Thread.CurrentThread.IsBackground )
            //                    Thread.CurrentThread.Abort();
            //                Application.ExitThread();
            //                MotherForm.NetWorkFinished(true);
            //            }
            //            else
            //            {
            //                if ( isQueryRequest )
            //                {
            //                    MotherForm.NetWorkFinished(false, exceptionMessage);
            //                    Thread.Sleep(500);
            //                    runTimes++;
            //                }
            //                else
            //                {
            //                    MotherForm.SayByeBye(leaveOnError);
            //                    MotherForm.NetWorkFinished(false, exceptionMessage);
            //                    while ( true )
            //                    {
            //                        Thread.Sleep(1000);
            //                        if ( MotherForm.IsClosed )
            //                        {
            //                            if ( !Thread.CurrentThread.IsBackground )
            //                                Thread.CurrentThread.Abort();
            //                            Application.ExitThread();
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        else
            //            MotherForm.NetWorkFinished(true);
            //    }
            //}
            //return resp;
        }

        private bool IsWebException(Exception e)
        {
            return ( e is WebException ) || ( e.InnerException != null && IsWebException(e.InnerException) );
        }

        private string GetExceptionMessage<T>(Exception e) where T : Exception
        {
            if ( e is T )
                return e.Message;
            else if ( e.InnerException != null )
                return GetExceptionMessage<T>(e.InnerException);
            else
                return "";
        }

        private void LoadManifestInfo()
        {
            string manifestDir = Application.StartupPath;
            string mainfestFile = Path.Combine(manifestDir, "version.manifest");

            if ( File.Exists(mainfestFile) )
            {
                try
                {
                    _manifest = new VersionManifest();
                    _manifest.LoadFromFile(mainfestFile);
                }
                catch ( Exception ex )
                {
                    BugReporter.ReportException("���wĳ", "���wĳ", ex, false);
                    _manifest = null;
                }
            }
            else
                _manifest = null;
        }
    }
}
