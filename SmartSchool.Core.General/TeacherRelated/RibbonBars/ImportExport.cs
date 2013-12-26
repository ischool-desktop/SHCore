using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.TeacherRelated.RibbonBars.Import;
using SmartSchool.Common;
using SmartSchool.Security;

namespace SmartSchool.TeacherRelated.RibbonBars
{
    public partial class ImportExport : SmartSchool.TeacherRelated.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl exportCtrl;
        FeatureAccessControl importCtrl;

        public ImportExport()
        {
            //InitializeComponent();

            ////�v���P�_ - �ץX�Юv
            //exportCtrl = new FeatureAccessControl("Button0490");

            ////�v���P�_ - �פJ�Юv
            //importCtrl = new FeatureAccessControl("Button0500");

            //exportCtrl.Inspect(btnExport);
            //importCtrl.Inspect(btnImport);
        }
        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            //�v���P�_ - �ץX�Юv
            exportCtrl = new FeatureAccessControl("Button0490");
            //�v���P�_ - �פJ�Юv
            importCtrl = new FeatureAccessControl("Button0500");
            var Bar = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["��Ʋέp"];
            Bar.Index = 3;

            var btnExport = Bar["�ץX"];
            btnExport.Image = Properties.Resources.Export_Image;
            btnExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnExport["�ץX�Юv�򥻸��"].Click += new System.EventHandler(this.buttonItem109_Click);
            //btnExport.Image = ( (System.Drawing.Image)( resources.GetObject("btnExport.Image") ) );


            var btnImport = Bar["�פJ"];
            btnImport.Image = Properties.Resources.Import_Image;
            btnImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnImport["�פJ�Юv�򥻸��"].Click += new System.EventHandler(this.buttonItem102_Click);
            //btnImport.Image = ( (System.Drawing.Image)( resources.GetObject("btnImport.Image") ) );


            btnExport.Enable = exportCtrl.Executable();
            btnImport.Enable = importCtrl.Executable();
        }

        private void buttonItem109_Click(object sender, EventArgs e)
        {
            ExportTeacher form = new ExportTeacher();
            form.ShowDialog();
        }

        private void buttonItem102_Click(object sender, EventArgs e)
        {
            TeacherImportWizard wizard = new TeacherImportWizard();
            wizard.ShowDialog();
        }

    }
}

