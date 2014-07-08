﻿// --------------------------------------------------------------------------------------------
// <copyright file="XhtmlExportTest.cs" from='2009' to='2014' company='SIL International'>
//      Copyright ( c ) 2014, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Test methods of FlexDePlugin
// </remarks>
// --------------------------------------------------------------------------------------------

#region Using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System.IO;
using NUnit.Framework;
using SIL.Tool;

#endregion Using

namespace Test.XhtmlExport
{
    /// <summary>
    /// Test methods of FlexDePlugin
    /// </summary>
    [TestFixture]
    [Category("BatchTest")]
    //[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify = "HKEY_LOCAL_MACHINE")]
    public class XhtmlExportTest
    {
        #region Setup
        /// <summary>holds path to script file</summary>
        private static string _scriptPath = string.Empty;
        /// <summary>holds full name of the script engine</summary>
        private static string _autoIt = string.Empty;

        private static TestFiles _tf;

        /// <summary>
        /// setup Input, Expected, and Output paths relative to location of program
        /// </summary>
        [TestFixtureSetUp]
        protected void SetUp()
        {
            Common.Testing = true;
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("Software\\Classes\\AutoIt3Script\\Shell\\Run\\Command", false);
            if (myKey != null)
            {
                _autoIt = myKey.GetValue("", "").ToString();
                if (_autoIt.EndsWith(@" ""%1"" %*"))
                    _autoIt = _autoIt.Substring(0, _autoIt.Length - 8);
                else if (_autoIt.EndsWith(@" ""%1"""))
                    _autoIt = _autoIt.Substring(0, _autoIt.Length - 5);     // Remove "%1" at end
            }
            _scriptPath = PathPart.Bin(Environment.CurrentDirectory, "/XhtmlExport");
            _tf = new TestFiles("XhtmlExport");
            var pwf = Common.PathCombine(Common.GetAllUserAppPath(), "SIL");
            var zf = new FastZip();
            zf.ExtractZip(_tf.Input("Pathway.zip"), pwf, ".*");
        }
        #endregion Setup

        #region Internal
        /// <summary>
        /// Launch the AutoIt subprocess to export the Xhtml from FieldWorks
        /// </summary>
        private static void FieldWorksXhtmlExport(string app, string proj, string backup, string message)
        {
            FieldWorksXhtmlExport(app, proj, backup, message, null);
        }

        /// <summary>
        /// Launch the AutoIt subprocess to export the Xhtml from FieldWorks
        /// </summary>
        private static void FieldWorksXhtmlExport(string app, string proj, string backup, string message, string incOpt)
        {
            var p1 = new Process();
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.EnvironmentVariables.Add("proj", proj);
            p1.StartInfo.EnvironmentVariables.Add("Backup", backup);
            p1.StartInfo.EnvironmentVariables.Add("InputPath", _tf.Input(null));
            p1.StartInfo.EnvironmentVariables.Add("OutputPath", _tf.Output(null));
            p1.StartInfo.EnvironmentVariables.Add("IncOpt", incOpt);
            p1.StartInfo.Arguments = Common.PathCombine(_scriptPath, app + "XhtmlExport.au3");
            p1.StartInfo.WorkingDirectory = _scriptPath;
            p1.StartInfo.FileName = _autoIt;
            p1.Start();
            if (p1.Id <= 0)
                throw new MissingSatelliteAssemblyException(proj);
            p1.WaitForExit();
            var xhtmlName = proj + ".xhtml";
            var xhtmlExpect = _tf.Expected(xhtmlName);
            var xhtmlOutput = _tf.Output(xhtmlName);
            var ns = new Dictionary<string, string> {{"x", "http://www.w3.org/1999/xhtml"}};
            XmlAssert.Ignore(xhtmlOutput, "/x:html/x:head/x:meta[@name='description']/@content", ns);
            XmlAssert.Ignore(xhtmlOutput, "//@id", ns);
            XmlAssert.AreEqual(xhtmlExpect, xhtmlOutput, message + ": " + xhtmlName);
            var cssName = proj + ".css";
            var cssExpect = _tf.Expected(cssName);
            var cssOutput = _tf.Output(cssName);
            TextFileAssert.AreEqualEx(cssExpect, cssOutput, new ArrayList{1}, message + ": " + cssName);
        }

        /// <summary>
        /// Runs Pathway on the data and applies the back end
        /// </summary>
        private static void PathawyB(string project, string layout, string inputType, string backend)
        {
            PathawyB(project, layout, inputType, backend, "xhtml");
        }

        /// <summary>
        /// Runs Pathway on the data and applies the back end
        /// </summary>
        private static void PathawyB(string project, string layout, string inputType, string backend, string format)
        {
            const bool overwrite = true;
            const string message = "";
            var xhtmlName = project + "." + format;
            var xhtmlInput = _tf.Input(xhtmlName);
            var xhtmlOutput = _tf.SubOutput(project, xhtmlName);
            File.Copy(xhtmlInput, xhtmlOutput, overwrite);
            var cssName = layout + ".css";
            var cssInput = _tf.Input(cssName);
            var cssOutput = _tf.SubOutput(project, cssName);
            File.Copy(cssInput, cssOutput, overwrite);
            var workingFolder = Path.GetDirectoryName(xhtmlInput);
            if (format == "usx")
            {
                FolderTree.Copy(_tf.Input("gather"), Common.PathCombine(_tf.Output("NKOu3"), "gather"));
                FolderTree.Copy(_tf.Input("figures"), Common.PathCombine(_tf.Output("NKOu3"), "figures"));
                workingFolder = Path.GetDirectoryName(xhtmlOutput);
            }
            var p1 = new Process();
            p1.StartInfo.UseShellExecute = false;
            StringBuilder arg = new StringBuilder(string.Format("-f \"{0}\" ", xhtmlOutput)); 
            arg.Append(string.Format("-c \"{0}\" ", cssOutput));
            arg.Append(string.Format("-t \"{0}\" ", backend));
            arg.Append(string.Format("-i {0} ", inputType));
            arg.Append(string.Format("-n \"{0}\" ", project));
            arg.Append(string.Format("-if {0} ", format));
            arg.Append(string.Format("-d \"{0}\" ", workingFolder));
            p1.StartInfo.Arguments = arg.ToString();
            p1.StartInfo.WorkingDirectory = _tf.Output(null);
            p1.StartInfo.FileName = Common.PathCombine(PathwayPath.GetPathwayDir(), "PathwayB.exe");
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            p1.Start();
            if (p1.Id <= 0)
                throw new MissingSatelliteAssemblyException(project);
            p1.WaitForExit();
            switch (backend)
            {
                case "OpenOffice/LibreOffice":
                    OdtCheck(project, message);
                    break;
                case "InDesign":
                    IdmlCheck(project, message);
                    break;
                case "E-Book (Epub2 and Epub3)":
                    //TODO: Epub needs more than a file compare test.
                    FileCheck(project, ".epub", message);
                    break;
                case "Go Bible":
                    FileCheck(project, ".jar", message);
                    break;
                case "XeLaTex":
                    FileCheck(project, ".tex", message.Length > 0? message: "Tex file mismatch");
                    TextCheck(project, ".log", new ArrayList {1}, message.Length > 0? message: "Log file mismatch");
                    break;
                default:
                    throw new AssertionException("Unrecognized backend type");
            }
        }

        private static void TextCheck(string project, string ext, ArrayList ex, string message)
        {
            var fileExpect = _tf.SubExpected(project, project + ext);
            var fileOutput = _tf.SubOutput(project, project + ext);
            TextFileAssert.AreEqualEx(fileExpect, fileOutput, ex, message);
        }

        private static void FileCheck(string project, string ext, string message)
        {
            var fileExpect = _tf.SubExpected(project, project + ext);
            var fileOutput = _tf.SubOutput(project, project + ext);
            FileAssert.AreEqual(fileExpect, fileOutput, message);
        }

        private static void IdmlCheck(string project, string message)
        {
            var idmlExpect = _tf.SubExpected(project, project + ".idml");
            var idmlOutput = _tf.SubOutput(project, project + ".idml");
            IdmlTest.AreEqual(idmlExpect, idmlOutput, message);
        }

        private static void OdtCheck(string project, string message)
        {
            var odtExpectDir = _tf.Expected(project);
            var odtOutputDir = _tf.Output(project);
            OdtTest.AreEqual(odtExpectDir, odtOutputDir, message);
        }

        #endregion Internal

        #region YCE-Test
        /// <summary>
        /// Export Yce-Test Flex data from Fieldworks, compare results to previous exports
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void YceTestExportTest()
        {
            FieldWorksXhtmlExport("Flex", "YCE-Test", "YCE-Test 2010-11-04 1357.fwbackup", "YCE-Test Export changed");
        }
        #endregion YCE-Test

        #region Buang-Test
        /// <summary>
        /// Export Buang-Test Flex data from Fieldworks, compare results to previous exports
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void BuangTestExportTest()
        {
            FieldWorksXhtmlExport("Flex", "Buang-Test", "Buang-Test 2010-11-04 0844.fwbackup", "Export changed", "cl");
        }
        #endregion Buang-Test

        #region Nkonya Sample
        /// <summary>
        /// Export Nkonya Sample TE data from Fieldworks, compare results to previous exports
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NkonyaSampleExportTest()
        {
            FieldWorksXhtmlExport("Te", "Nkonya Sample", "Nkonya Sample 2011-02-10 1347.fwbackup", "Export changed", "c");
        }
        #endregion Nkonya Sample

        #region Gondwana Sample
        /// <summary>
        /// Export Gondwana Sample Flex data from Fieldworks, compare results to previous exports
        /// </summary>
        [Test]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void GondwanaSampleExportTest()
        {
            FieldWorksXhtmlExport("Flex", "Gondwana Sample", "Gondwana Sample 2011-02-09 0709.fwbackup", "Export changed");
        }
        #endregion Gondwana Sample

        #region Gondwana Sample Open Office
        /// <summary>
        /// Gondwana Sample Open Office Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void GondwanaSampleOpenOfficeTest()
        {
            PathawyB("Gondwana Sample", "Gondwana Sample", "Dictionary", "OpenOffice/LibreOffice");
        }
        #endregion Gondwana Sample Open Office

        #region Nkonya Sample Open Office
        /// <summary>
        /// Nkonya Sample Open Office Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NkonyaSampleOpenOfficeTest()
        {
            PathawyB("Nkonya Sample", "Nkonya Sample", "Scripture", "OpenOffice/LibreOffice");
        }
        #endregion Nkonya Sample Open Office

        #region Paratext NKOu3 Open Office
        /// <summary>
        /// Paratext NKOu3 Open Office Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NKOu3OpenOfficeTest()
        {
            PathawyB("NKOu3", "NKOu3", "Scripture", "OpenOffice/LibreOffice", "usx");
        }
        #endregion Nkonya Sample Open Office

        #region Gondwana Sample InDesign
        /// <summary>
        /// Gondwana Sample InDesign Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void GondwanaSampleInDesignTest()
        {
            PathawyB("Gondwana Sample", "Gondwana Sample", "Dictionary", "InDesign");
        }
        #endregion Gondwana Sample InDesign

        #region Nkonya Sample InDesign
        /// <summary>
        /// Nkonya Sample InDesign Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NkonyaSampleInDesignTest()
        {
            PathawyB("Nkonya Sample", "Nkonya Sample", "Scripture", "InDesign");
        }
        #endregion Nkonya Sample InDesign

        #region Paratext NKOu3 InDesign
        /// <summary>
        /// Paratext NKOu3 InDesign Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NKOu3InDesignTest()
        {
            PathawyB("NKOu3", "NKOu3", "Scripture", "InDesign", "usx");
        }
        #endregion Nkonya Sample Open Office

        //#region Gondwana Sample Epub
        ///// <summary>
        ///// Gondwana Sample E-Book Back End Test
        ///// </summary>
        //[Test]
        //[Category("LongTest")]
        //[Category("SkipOnTeamCity")]
        //public void GondwanaSampleEpubTest()
        //{
        //    PathawyB("Gondwana Sample", "Gondwana Sample", "Dictionary", "E-Book (.epub)");
        //}
        //#endregion Gondwana Sample Epub

        //#region Paratext NKOu3 E-Book
        ///// <summary>
        ///// Paratext NKOu3 E-Book Back End Test
        ///// </summary>
        //[Test]
        //[Category("LongTest")]
        //[Category("SkipOnTeamCity")]
        //public void NKOu3EpubTest()
        //{
        //    PathawyB("NKOu3", "NKOu3", "Scripture", "E-Book (.epub)");
        //}
        //#endregion Nkonya Sample E-Book

        //Dictionaries can not be output with Go Bible!
        //#region Gondwana Sample Go Bible
        ///// <summary>
        ///// Gondwana Sample Go Bible Back End Test
        ///// </summary>
        //[Test]
        //[Category("LongTest")]
        //[Category("SkipOnTeamCity")]
        //public void GondwanaSampleGoBibleTest()
        //{
        //    PathawyB("Gondwana Sample", "Gondwana Sample", "Dictionary", "Go Bible");
        //}
        //#endregion Gondwana Sample Go Bible

        //Todo: PathwayB must be tested with .sfm for new GoBible
        //#region Paratext NKOu3 Go Bible
        ///// <summary>
        ///// Paratext NKOu3 Go Bible Back End Test
        ///// </summary>
        //[Test]
        //[Category("LongTest")]
        //[Category("SkipOnTeamCity")]
        //public void NKOu3GoBibleTest()
        //{
        //    PathawyB("NKOu3", "NKOu3", "Scripture", "Go Bible");
        //}
        //#endregion Nkonya Sample Go Bible

        #region Gondwana Sample XeLaTex
        /// <summary>
        /// Gondwana Sample XeLaTex Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void GondwanaSampleXeLaTexTest()
        {
            PathawyB("Gondwana Sample", "Gondwana Sample", "Dictionary", "XeLaTex");
        }
        #endregion Gondwana Sample XeLaTex

        #region Paratext NKOu3 XeLaTex
        /// <summary>
        /// Paratext NKOu3 XeLaTex Back End Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("LongTest")]
        [Category("SkipOnTeamCity")]
        public void NKOu3XeLaTexTest()
        {
            PathawyB("NKOu3", "NKOu3", "Scripture", "XeLaTex", "usx");
        }
        #endregion Nkonya Sample XeLaTex
    }
}