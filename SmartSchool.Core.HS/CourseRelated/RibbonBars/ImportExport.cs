using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.CourseRelated.RibbonBars.Export;
using SmartSchool.CourseRelated.RibbonBars.Import;
using SmartSchool.Common;
using SmartSchool.Security;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class ImportExport : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl exportCtrl;
        FeatureAccessControl importCtrl;

        public ImportExport()
        {
        }

        internal void Setup()
        {
            //�v���P�_ - �ץX�ҵ{
            exportCtrl = new FeatureAccessControl("Button0600");
            //�v���P�_ - �פJ�ҵ{
            importCtrl = new FeatureAccessControl("Button0610");

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            
            var btnExport = K12.Presentation.NLDPanels.Course.RibbonBarItems["��Ʋέp"]["�ץX"];
            btnExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnExport.Image = (System.Drawing.Image)Properties.Resources.Export_Image;
            //btnExport.Image = ( (System.Drawing.Image)( resources.GetObject("btnExport.Image") ) );
            btnExport["�ץX�ҵ{�򥻸��"].Click += new System.EventHandler(this.buttonItem109_Click);
            btnExport["�ץX�ҵ{�򥻸��"].Enable = exportCtrl.Executable();

            var btnImport = K12.Presentation.NLDPanels.Course.RibbonBarItems["��Ʋέp"]["�פJ"];
            btnImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnImport.Image = (System.Drawing.Image)Properties.Resources.Import_Image;
            //btnImport.Image = ( (System.Drawing.Image)( resources.GetObject("btnImport.Image") ) );
            btnImport["�פJ�ҵ{�򥻸��"].Click += new System.EventHandler(this.buttonItem102_Click);
            btnImport["�פJ�ҵ{�򥻸��"].Enable = importCtrl.Executable();
        }

        private void buttonItem109_Click(object sender, EventArgs e)
        {
            ExportForm form = new ExportForm();
            form.ShowDialog();
        }

        private void buttonItem102_Click(object sender, EventArgs e)
        {
            CourseImportWizard form = new CourseImportWizard();
            form.ShowDialog();
        }
    }
}

