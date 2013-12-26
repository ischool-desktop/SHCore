using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Customization.Data;
using Aspose.Cells;
using SmartSchool.StudentRelated;
using System.Xml;
using System.Threading;
using FISCA.DSAUtil;
using System.IO;
using System.Diagnostics;
using SmartSchool;
using SmartSchool.Common;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.Others.RibbonBars
{
    public partial class UnfinishScore : RibbonBarBase
    {
        FeatureAccessControl openCtrl;

        public UnfinishScore()
        {
            //InitializeComponent();
        }
        public void Setup()
        {
            //�v���P�_ - ���Z�@�~/���q��J���p
            openCtrl = new FeatureAccessControl("Button0660");
            var btnOpen = MotherForm.RibbonBarItems["�аȧ@�~", "�妸�@�~/�˵�"]["���Z�@�~"];
            //btnOpen.Size = RibbonBarButton.MenuButtonSize.Large; 
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnfinishScore));
            //btnOpen.Image = ( (System.Drawing.Image)( resources.GetObject("btnOpen.Image") ) );
            btnOpen["���q��J���p"].Enable = openCtrl.Executable();
            btnOpen["���q��J���p"].Click += new System.EventHandler(this.btnOpen_Click);
        }

        public override string ProcessTabName
        {
            get
            {
                return "�аȧ@�~";
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            UnfinishedList form = new UnfinishedList();
            form.ShowDialog();
        }

        private void MainRibbonBar_Click(object sender, EventArgs e)
        {
            //if (Control.ModifierKeys == Keys.Shift)
            //    btnRank.Enabled = true;
        }

        private void btnRankSemester_Click(object sender, EventArgs e)
        {
            //new SemesterRatingForm().ShowDialog();
        }

        private void btnRankSchoolYear_Click(object sender, EventArgs e)
        {
            //new SchoolYearRatingForm().ShowDialog();
        }
    }
}
