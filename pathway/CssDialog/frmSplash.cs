﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SIL.PublishingSolution
{
    public partial class frmSplash : Form
    {
        private int _counter = 0;
        public DialogResult dr;
        public frmSplash()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets AssemblyFileDate set by builder
        /// </summary>
        public string AssemblyFileDate
        {
            get
            {
                // first, see if we can get at the last creation date for the calling assembly
                if (File.Exists(Assembly.GetCallingAssembly().Location))
                {
                    return File.GetCreationTime(Assembly.GetCallingAssembly().Location).ToShortDateString();
                }
                // try 2: fallback on the last creation date for the executable path
                return File.GetCreationTime(Application.ExecutablePath).ToShortDateString();
            }
        }

        private void tSplash_Tick(object sender, EventArgs e)
        {
            _counter++;
            if (_counter == 5)
            {
                tSplash.Stop();
                dr = DialogResult.OK;
                this.Close();
            }
        }

        private void frmSplash_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            string versionDate = String.Format("Version {0} ({1})", version, AssemblyFileDate);

            lblVersion.Text = "Version: " + version.Substring(0, 5);

            lblVersionwithDate.Text = "Version: " + versionDate;
            tSplash.Start();
        }
    }
}
