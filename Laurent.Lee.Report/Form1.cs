using System;
using System.Windows.Forms;

namespace Laurent.Lee.Report
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.reportViewer1.RefreshReport();
        }
    }
}