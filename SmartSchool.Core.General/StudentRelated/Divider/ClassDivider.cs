using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.StudentRelated.SourceProvider;
using SmartSchool.ClassRelated;
using SmartSchool.Common;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using System.ComponentModel;

namespace SmartSchool.StudentRelated.Divider
{
    class ClassDivider : IStudentDivider
    {
        private DragDropTreeView _TargetTreeView;
        private TempStudentSourceProvider _TempProvider;
        private List<TreeNode> _RelatedNodes = new List<TreeNode>();
        private TreeNode _SelectedNode;

        private Dictionary<string, GradeStudentSourceProvider> _DictGradeStudentSourceProvider;
        private Dictionary<string, ClassStudentSourceProvider> _DictClassStudentSourceProvider;

        private NonClassStudentSourceProvider _NonClassStudentSourceProvider;
        private NonGradeStudentSourceProvider _NonGradeStudentSourceProvider;
        private AllStudentSourceProvider _AllStudentSourceProvider;

        private void _TargetTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_RelatedNodes.Contains(_TargetTreeView.SelectedNode))
                _SelectedNode = _TargetTreeView.SelectedNode;
        }

        private void ReflshRelatedNodes(TreeNodeCollection treeNodeCollection, bool clearList)
        {
            if (clearList) _RelatedNodes.Clear();
            foreach (TreeNode var in treeNodeCollection)
            {
                _RelatedNodes.Add(var);
                ReflshRelatedNodes(var.Nodes, false);
            }
        }

        public ClassDivider()
        {

        }

        //public List<BriefStudentData> GetStudentByClassId(Dictionary<string, BriefStudentData> source, string classID)
        //{
        //    List<BriefStudentData> classList = new List<BriefStudentData>();
        //    foreach (BriefStudentData student in source.Values)
        //    {
        //        if (student.RefClassID == classID && (student.IsNormal))
        //        {
        //            classList.Add(student);
        //        }
        //    }
        //    return classList;
        //}

        #region IStudentDivider ����

        public TempStudentSourceProvider TempProvider
        {
            get { return _TempProvider; }
            set { _TempProvider = value; }
        }

        public DragDropTreeView TargetTreeView
        {
            get { return _TargetTreeView; }
            set
            {
                if (_TargetTreeView != null)
                {
                    _TargetTreeView.AfterSelect -= new TreeViewEventHandler(_TargetTreeView_AfterSelect);
                    _TargetTreeView.AfterExpand -= new TreeViewEventHandler(_TargetTreeView_AfterExpand);
                }
                _TargetTreeView = value;
                _TargetTreeView.AfterSelect += new TreeViewEventHandler(_TargetTreeView_AfterSelect);
                _TargetTreeView.AfterExpand += new TreeViewEventHandler(_TargetTreeView_AfterExpand);
            }
        }

        void _TargetTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            UpdatePreference();
        }

        public void Divide(Dictionary<string, BriefStudentData> source)
        {
            _TargetTreeView.SuspendLayout();
            #region �إ�SourceProvider
            _NonClassStudentSourceProvider = (_NonClassStudentSourceProvider != null ? _NonClassStudentSourceProvider : new NonClassStudentSourceProvider());
            _NonGradeStudentSourceProvider = (_NonGradeStudentSourceProvider != null ? _NonGradeStudentSourceProvider : new NonGradeStudentSourceProvider());
            _AllStudentSourceProvider = (_AllStudentSourceProvider != null ? _AllStudentSourceProvider : new AllStudentSourceProvider());

            Dictionary<string, GradeStudentSourceProvider> _OldDictGradeStudentSourceProvider = (_DictGradeStudentSourceProvider != null ? _DictGradeStudentSourceProvider : new Dictionary<string, GradeStudentSourceProvider>());
            Dictionary<string, ClassStudentSourceProvider> _OldDictClassStudentSourceProvider = (_DictClassStudentSourceProvider != null ? _DictClassStudentSourceProvider : new Dictionary<string, ClassStudentSourceProvider>());
            _DictGradeStudentSourceProvider = new Dictionary<string, GradeStudentSourceProvider>();
            _DictClassStudentSourceProvider = new Dictionary<string, ClassStudentSourceProvider>();

            #endregion

            #region �NSourcePorvider��iTreeView

            foreach (ClassInfo var in Class.Instance.Items)
            {
                int a;
                if (int.TryParse(var.GradeYear, out a))
                {
                    GradeStudentSourceProvider newGradeNode;

                    if (!_OldDictGradeStudentSourceProvider.ContainsKey(var.GradeYear))
                    {
                        newGradeNode = new GradeStudentSourceProvider();
                        ((GradeStudentSourceProvider)newGradeNode).Grade = var.GradeYear;
                    }
                    else
                    {
                        newGradeNode = _OldDictGradeStudentSourceProvider[var.GradeYear];
                        //_OldDictGradeStudentSourceProvider.Remove(var.GradeYear);
                    }

                    if (!_DictGradeStudentSourceProvider.ContainsKey(var.GradeYear))
                        _DictGradeStudentSourceProvider.Add(var.GradeYear, newGradeNode);
                }

                ClassStudentSourceProvider newClassNode;
                if (!_OldDictClassStudentSourceProvider.ContainsKey(var.ClassID))
                {
                    newClassNode = new ClassStudentSourceProvider();
                    newClassNode.Grade = var.GradeYear;
                    newClassNode.ClassName = var.ClassName;
                    newClassNode.ClassID = var.ClassID;
                }
                else
                {
                    newClassNode = _OldDictClassStudentSourceProvider[var.ClassID];
                    newClassNode.Grade = var.GradeYear;
                    newClassNode.ClassName = var.ClassName;
                    _OldDictClassStudentSourceProvider.Remove(var.ClassID);
                }
                _DictClassStudentSourceProvider.Add(var.ClassID, newClassNode);

            }

            _TargetTreeView.Nodes.Clear();

            //�B�z�Ҧ��ǥͥؿ�
            _AllStudentSourceProvider.Nodes.Clear();

            int Nodeindex = _TargetTreeView.Nodes.Add(_AllStudentSourceProvider);
            TreeNode AllNode = _TargetTreeView.Nodes[Nodeindex];

            //�B�z�~�ťؿ�
            foreach (TreeNode var in _DictGradeStudentSourceProvider.Values)
            {
                AllNode.Nodes.Add(var);
                var.Nodes.Clear();
            }
            _NonGradeStudentSourceProvider.Nodes.Clear();

            //�B�z�Z�ťؿ�
            foreach (ClassStudentSourceProvider var in _DictClassStudentSourceProvider.Values)
            {
                int a;
                if (int.TryParse(var.Grade, out a))
                {
                    //�[�J�b�S�w�~��
                    _DictGradeStudentSourceProvider[var.Grade].Nodes.Add(var);
                }
                else
                {
                    //�[�J�����~�Ť�
                    _NonGradeStudentSourceProvider.Nodes.Add(var);

                }
            }
            #endregion

            #region �إ�Source

            //�����~��
            Dictionary<string, List<BriefStudentData>> _DictGradeStudentSource = new Dictionary<string, List<BriefStudentData>>();
            List<BriefStudentData> _NonGradeStudentSource = new List<BriefStudentData>();

            //�����Z��
            Dictionary<string, List<BriefStudentData>> _DictClassStudentSource = new Dictionary<string, List<BriefStudentData>>();
            List<BriefStudentData> _NonClassStudentSource = new List<BriefStudentData>();

            #endregion

            List<BriefStudentData> All_list = new List<BriefStudentData>();

            foreach (BriefStudentData var in source.Values)
            {
                All_list.Add(var);

                if (var.RefClassID == "") //�S���Z��,�[�J�Z�Ŧr��
                {
                    _NonClassStudentSource.Add(var);
                    _NonGradeStudentSource.Add(var); //��ܤ]�O�����~��
                }
                else //���Z�Ÿ��
                {

                    int a;
                    if (int.TryParse(var.GradeYear, out a))
                    {
                        //�إ߸��
                        if (!_DictGradeStudentSource.ContainsKey(var.GradeYear))
                        {
                            _DictGradeStudentSource.Add(var.GradeYear, new List<BriefStudentData>());
                        }
                        _DictGradeStudentSource[var.GradeYear].Add(var);
                    }
                    else
                    {
                        //�~�Ť��O�Ʀr,�N�C�������~��
                        _NonGradeStudentSource.Add(var);
                    }

                    //�إ߸��
                    if (!_DictClassStudentSource.ContainsKey(var.RefClassID))
                    {
                        _DictClassStudentSource.Add(var.RefClassID, new List<BriefStudentData>());
                    }
                    _DictClassStudentSource[var.RefClassID].Add(var);
                }
            }

            _AllStudentSourceProvider.Source = All_list;

            //�]�w�����Z��
            _NonClassStudentSourceProvider.Source = _NonClassStudentSource;
            //�]�w�����~��
            _NonGradeStudentSourceProvider.Source = _NonGradeStudentSource;

            //�~��
            foreach (string grade in _DictGradeStudentSourceProvider.Keys)
            {
                int a;
                if (int.TryParse(grade, out a))
                {
                    if (_DictGradeStudentSource.ContainsKey(grade))
                        ((GradeStudentSourceProvider)_DictGradeStudentSourceProvider[grade]).Source = _DictGradeStudentSource[grade];
                    else
                        ((GradeStudentSourceProvider)_DictGradeStudentSourceProvider[grade]).Source = new List<BriefStudentData>();
                }
            }

            //�Z��
            foreach (string classid in _DictClassStudentSourceProvider.Keys)
            {
                if (_DictClassStudentSource.ContainsKey(classid))
                    _DictClassStudentSourceProvider[classid].Source = _DictClassStudentSource[classid];
                else
                    _DictClassStudentSourceProvider[classid].Source = new List<BriefStudentData>();
            }

            //�[�J
            AllNode.Nodes.Add(_NonGradeStudentSourceProvider);
            //�p�G�����Z�Ť��e�j��0
            if (_NonClassStudentSourceProvider.Source.Count > 0)
            {
                //��"�����Z��"�[�J"�����~��"
                _NonGradeStudentSourceProvider.Nodes.Add(_NonClassStudentSourceProvider);
            }
            #region �s�W���`�I�qPreferenceŪ���i�}�]�w
            XmlElement pe = Preference;

            _AllStudentSourceProvider.Expand();

            foreach (GradeStudentSourceProvider var in _DictGradeStudentSourceProvider.Values)
            {
                if (!_OldDictGradeStudentSourceProvider.ContainsValue(var) && pe.GetAttribute("G" + var.Grade) == "True")
                {
                    var.Expand();
                }
            }
            #endregion

            _TargetTreeView.SelectedNode = _SelectedNode;
            ReflshRelatedNodes(_TargetTreeView.Nodes, true);
            _TargetTreeView.ResumeLayout();
        }

        #endregion

        #region IDenominated ����

        public string Name
        {
            get { return "�̯Z���˵�"; }
        }

        #endregion

        #region IPreference ����

        public void UpdatePreference()
        {
            XmlElement pe = Preference;
            foreach (GradeStudentSourceProvider var in _DictGradeStudentSourceProvider.Values)
            {
                pe.SetAttribute("G" + var.Grade, "" + var.IsExpanded);
            }

            Preference = pe;
        }

        protected XmlElement Preference
        {
            get
            {
                XmlElement PreferenceElement = CurrentUser.Instance.Preference["ClassDivider"];
                if (PreferenceElement == null)
                {
                    PreferenceElement = new XmlDocument().CreateElement("ClassDivider");
                }
                return PreferenceElement;
            }
            set
            {
                CurrentUser.Instance.Preference["ClassDivider"] = value;
            }
        }
        #endregion
    }
}