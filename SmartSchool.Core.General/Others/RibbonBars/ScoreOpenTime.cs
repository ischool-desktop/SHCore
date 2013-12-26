using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Security;
using FISCA.Presentation;
//using SmartSchool.Common;

namespace SmartSchool.Others.RibbonBars
{
    public partial class ScoreOpenTime : SmartSchool.Others.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl setupCtrl;

        public ScoreOpenTime()
        {
            //InitializeComponent();

        }

        internal void Setup()
        {
            MotherForm.RibbonBarItems["�ǰȧ@�~", "�䥦"].Index = 9;

            var btnSetup = MotherForm.RibbonBarItems["�ǰȧ@�~", "�䥦"]["�}��ɶ��]�w"];
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScoreOpenTime));
            btnSetup.Image = ( (System.Drawing.Image)( resources.GetObject("btnSetup.Image") ) );
            btnSetup.Click += new System.EventHandler(this.btnSetup_Click);
            btnSetup.Size = RibbonBarButton.MenuButtonSize.Large;

            ////�v���P�_ - �䥦/�}��ɶ��]�w
            setupCtrl = new FeatureAccessControl("Button0710");
            btnSetup.Enable=setupCtrl.Executable();
        }

        public override string ProcessTabName
        {
            get
            {
                return "�ǰȧ@�~";
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            TeacherDiffOpenConfig.Display();
        }
    }
}

