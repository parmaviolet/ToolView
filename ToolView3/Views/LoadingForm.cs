using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ToolView3.Controllers;

namespace ToolView3.Views {
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
        }

        public void startProgressBar()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
        }

        List<string> filenames = null;

        private void LoadingForm_Shown(object sender, EventArgs e)
        {
            
            var importCont = new ImportController();
            filenames = importCont.getImportFilenames();

            backgroundWorker1.RunWorkerAsync();
            startProgressBar();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (filenames.Any())
            {
                var importCont = new ImportController();
                importCont.ImportFromNessusFile(filenames);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            filenames.Clear();
            this.Dispose();
        }

        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
