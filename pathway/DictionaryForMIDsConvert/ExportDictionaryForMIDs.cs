﻿// --------------------------------------------------------------------------------------------
// <copyright file="ExportDictionaryForMIDs.cs" from='2013' to='2013' company='SIL International'>
//      Copyright © 2013, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Stylepick FeatureSheet
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using SIL.Tool;


namespace SIL.PublishingSolution
{
    public class ExportDictionaryForMIDs : IExportProcess
    {
        private const string LogName = "Convert.log";
        private static DictionaryForMIDsInput _DictionaryForMIDsInput;
        protected static string WorkDir;
        protected static Dictionary<string, Dictionary<string, string>> CssClass;
        protected static DictionaryForMIDsStyle ContentStyles = new DictionaryForMIDsStyle();
        private static bool _isUnixOS = false;

        #region Properties
        #region ExportType
        public string ExportType
        {
            get
            {
                return "DictionaryForMIDs";
            }
        }
        #endregion ExportType

        #region Handle
        public bool Handle(string inputDataType)
        {
            return (inputDataType.ToLower() == "dictionary");
        }
        #endregion Handle
        #endregion Properties

        /// <summary>
        /// Entry point for Dictionary for Mids export
        /// </summary>
        /// <param name="exportType">scripture / dictionary</param>
        /// <param name="publicationInformation">structure with other necessary information about project.</param>
        /// <returns></returns>
        public bool Launch(string exportType, PublicationInformation publicationInformation)
        {
            return Export(publicationInformation);
        }

        public bool Export(PublicationInformation projInfo)
        {
            bool success = false;
            projInfo.OutputExtension = "jar";
            if (projInfo == null || string.IsNullOrEmpty(projInfo.DefaultXhtmlFileWithPath) ||
                string.IsNullOrEmpty(projInfo.DefaultCssFileWithPath))
            {
                return false;
            }
            WorkDir = Path.GetDirectoryName(projInfo.DefaultXhtmlFileWithPath);
            _isUnixOS = Common.UnixVersionCheck();
            var inProcess = new InProcess(0, 8);
            var curdir = Environment.CurrentDirectory;
            var myCursor = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                inProcess.Show();
                inProcess.PerformStep();

                LoadParameters();
                inProcess.PerformStep();

                LoadCss(projInfo);
                inProcess.PerformStep();

                ReformatData(projInfo);
                inProcess.PerformStep();

                CreateProperties(projInfo);
                inProcess.PerformStep();

                CreateDictionaryForMIDs(projInfo);
                inProcess.PerformStep();

                CreateSubmission(projInfo);
                inProcess.PerformStep();

                ReportReults(projInfo);
                inProcess.PerformStep();
                success = true;
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show(string.Format("{0} Display partial results?", ex.Message), "Report: Failure", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    DisplayOutput(projInfo);
                }
            }

            inProcess.Close();
            Environment.CurrentDirectory = curdir;
            Cursor.Current = myCursor;
            Common.CleanupExportFolder(projInfo.DefaultXhtmlFileWithPath, ".tmp,.jar,.zip,.bat,.de", string.Empty, "Empty_Jar-Jad,dictionary");
            if (Common.Testing == false)
            {
                MoveJarFile(projInfo);
                CreateRAMP(projInfo);
            }
            return success;
        }

        private void MoveJarFile(PublicationInformation projInfo)
        {
            var folder = Directory.GetDirectories(Path.GetDirectoryName(projInfo.DefaultXhtmlFileWithPath), "DfM_*");
            if (folder.Length == 0) {
                return;
            }
            for (int i = 0; i < folder.Length; i++)
            {
                var file = Directory.GetFiles(folder[i]);
                foreach (var s1 in file)
                {
                    File.Move(s1, Common.PathCombine(Path.GetDirectoryName(folder[i]), Path.GetFileName(s1)));
                }
                Directory.Delete(folder[i]);
            }
        }

        private void CreateRAMP(PublicationInformation projInfo)
        {
            Ramp ramp = new Ramp();
            ramp.Create(projInfo.DefaultXhtmlFileWithPath, ".jad,.jar", projInfo.ProjectInputType);
        }

        protected void LoadCss(PublicationInformation projInfo)
        {
            var cssTree = new CssTree();
            CssClass = cssTree.CreateCssProperty(projInfo.DefaultCssFileWithPath, true);
        }

        protected void LoadParameters()
        {
            Param.LoadSettings();
            Param.SetValue(Param.InputType, "Dictionary");
            Param.LoadSettings();
        }

        protected void ReformatData(PublicationInformation projInfo)
        {
            var outFile = new DictionaryForMIDsStreamWriter(projInfo);
            outFile.Open();
            var input = Input(projInfo);
            foreach (XmlNode sense in input.SelectNodes("//*[@class = 'entry']//*[@id]"))
            {
                var rec = new DictionaryForMIDsRec { CssClass = CssClass, Styles = ContentStyles };
                rec.AddHeadword(sense);
                rec.AddBeforeSense(sense);
                rec.AddSense(sense);
                rec.AddAfterSense(sense);
                rec.AddReversal(sense);
                outFile.WriteLine(rec.Rec);
            }
            outFile.Close();
        }

        protected void CreateProperties(PublicationInformation projInfo)
        {
            var input = Input(projInfo);
            var myProps = new DictionaryForMIDsProperties(projInfo, ContentStyles);
            myProps.SetLanguage(1, input.VernacularIso(), input.VernacularName());
            myProps.SetLanguage(2, input.AnalysisIso(), input.AnalysisName());
            myProps.InfoText = GetInfo();
            myProps.Write();
            myProps.Close();
        }

        private string GetInfo()
        {
            var sb = new StringBuilder();
            const bool isConfigurationTool = false;
            sb.Append(Param.GetTitleMetadataValue("Title", Param.GetOrganization(), isConfigurationTool));
            AddMetaItem(sb, "Description");
            AddMetaItem(sb, "Copyright Holder");
            AddMetaItem(sb, "Creator");
            return sb.ToString();
        }

        private static void AddMetaItem(StringBuilder sb, string item)
        {
            var val = Param.GetMetadataValue(item);
            if (val != "")
            {
                sb.Append(" ");
                sb.Append(val);
            }
        }

        protected void CreateDictionaryForMIDs(PublicationInformation projInfo)
        {
            var output = new DictionaryForMIDsStreamWriter(projInfo);
            Debug.Assert(output.Directory != null);
            var processFullPath = Common.PathCombine(output.Directory, "go.bat");
            var DictionaryForMIDsPath = Common.FromRegistry("Dic4Mid");
            var creatorPath = Common.PathCombine(DictionaryForMIDsPath, "DfM-Creator");
            FolderTree.Copy(creatorPath, output.Directory);
            const string redirectOutputFileName = LogName;
            SubProcess.RedirectOutput = redirectOutputFileName;
            if (_isUnixOS)
            {
                SubProcess.RunCommand(output.Directory, "wineconsole", processFullPath, true);
            }
            else
            {
                SubProcess.RunCommand(output.Directory, processFullPath, ".", true);
            }
        }

        protected void CreateSubmission(PublicationInformation projInfo)
        {
            var output = new DictionaryForMIDsStreamWriter(projInfo);
            var folder = Directory.GetDirectories(output.Directory, "DfM_*");
            if (folder.Length == 0)
                return;                 // Output was not created!
            var folderName = Path.GetFileName(folder[0]);
            var date = DateTime.Now.ToString("y.M.d");
            var folderParts = folderName.Split('_');
            var submissionName = string.Format("DictionaryForMIDs_{0}_{1}_{2}.zip", date, folderParts[1], folderParts[2]);
            var submissionFullName = Common.PathCombine(output.Directory, submissionName);
            var zip = new FastZip();
            const bool recurse = true;
            zip.CreateZip(submissionFullName, folder[0], recurse, ".*");
        }

        protected void ReportReults(PublicationInformation projInfo)
        {
            var output = new DictionaryForMIDsStreamWriter(projInfo);
            var result = MessageBox.Show(string.Format("Dictionary for Mid output successfully created in {0}. Display output?", output.Directory), "Results", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                DisplayOutput(projInfo);
            }
        }

        private static void DisplayOutput(PublicationInformation projInfo)
        {
            var output = new DictionaryForMIDsStreamWriter(projInfo);
            const bool noWait = false;
            if (_isUnixOS)
            {
                SubProcess.Run(output.Directory, "nautilus", output.Directory, noWait);
            }
            else
            {
                SubProcess.Run(output.Directory, "explorer.exe", output.Directory, noWait);
            }
        }

        protected DictionaryForMIDsInput Input(PublicationInformation projInfo)
        {
            if (_DictionaryForMIDsInput != null)
                return _DictionaryForMIDsInput;
            _DictionaryForMIDsInput = new DictionaryForMIDsInput(projInfo);
            return _DictionaryForMIDsInput;
        }
    }
}
