﻿// --------------------------------------------------------------------------------------------
// <copyright file="TePsExportTest.cs" from='2009' to='2009' company='SIL International'>
//      Copyright © 2009, SIL International. All Rights Reserved.   
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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.PublishingSolution;
using SIL.Tool;
using RevHomographNum;

#endregion Using

namespace Test.PsExport
{
    /// <summary>
    /// Test methods of FlexDePlugin
    /// </summary>
    [TestFixture]
    [Category("BatchTest")]
    public class PsExportTest : SIL.PublishingSolution.PsExport
    {
        #region Setup
        /// <summary>holds path to input folder for all tests</summary>
        private static string _inputBasePath = string.Empty;
        /// <summary>holds path to expected results folder for all tests</summary>
        private static string _expectBasePath = string.Empty;
        /// <summary>holds path to output folder for all tests</summary>
        private static string _outputBasePath = string.Empty;

        /// <summary>
        /// setup Input, Expected, and Output paths relative to location of program
        /// </summary>
        [TestFixtureSetUp]
        protected void SetUp()
        {
            Common.Testing = true;
            string testPath = PathPart.Bin(Environment.CurrentDirectory, "/PsExport/TestFiles");
            _inputBasePath = Common.PathCombine(testPath, "Input");
            _expectBasePath = Common.PathCombine(testPath, "Expect");
            _outputBasePath = Common.PathCombine(testPath, "Output");
            Common.DeleteDirectory(_outputBasePath);
            Directory.CreateDirectory(_outputBasePath);
            // Set application base for test
            //DoBatch("ConfigurationTool", "postBuild.bat", "Debug");
            //Common.ProgInstall = Environment.CurrentDirectory.Replace("Test", "ConfigurationTool");
            Common.ProgInstall = Environment.CurrentDirectory;
            //FolderTree.Copy(Common.PathCombine(testPath, "../../../PsSupport/OfficeFiles"),Path.Combine(Common.ProgInstall,"OfficeFiles"));
            Backend.Load(Common.ProgInstall);
        }

        private static string _BasePath = string.Empty;
        private static string _ConfigPath = string.Empty;

        public static void DoBatch(string project, string process, string config)
        {
            SetBaseAndConfig();
            var folder = _BasePath + project + _ConfigPath;
            var processPath = Common.PathCombine(_BasePath + project, process);
            //MessageBox.Show(folder);
            SubProcess.Run(folder, processPath, config, true);
        }

        private static void SetBaseAndConfig()
        {
            if (_BasePath != string.Empty) return;
            var m = Regex.Match(Environment.CurrentDirectory, "Test");
            Debug.Assert(m.Success);
            _BasePath = Environment.CurrentDirectory.Substring(0, m.Index);
            _ConfigPath = Environment.CurrentDirectory.Substring(m.Index + m.Length);
        }

        /// <summary>
        /// pretend we don't know the type of input after each test
        /// </summary>
        [TearDown]
        protected void TearDown()
        {
            Param.Value[Param.InputType] = "";
        }
        #endregion Setup

        #region Internal
        #region TestPath
        /// <summary>holds path to input folder for all tests</summary>
        private static string _inputTestPath = string.Empty;
        /// <summary>holds path to expected results folder for all tests</summary>
        private static string _expectTestPath = string.Empty;
        /// <summary>holds path to output folder for all tests</summary>
        private static string _outputTestPath = string.Empty;

        private static void TestPathSetup(string testName)
        {
            _inputTestPath = Common.PathCombine(_inputBasePath, testName);
            _expectTestPath = Common.PathCombine(_expectBasePath, testName);
            _outputTestPath = Common.PathCombine(_outputBasePath, testName);
        }

        private static string FileInput(string fileName)
        {
            return Common.PathCombine(_inputTestPath, fileName);
        }

        private static string FileOutput(string fileName)
        {
            return Common.PathCombine(_outputTestPath, fileName);
        }

        private static string FileExpect(string fileName)
        {
            return Common.PathCombine(_expectTestPath, fileName);
        }

        private static string FileCopy(string filename)
        {
            var outFullName = Common.PathCombine(_outputBasePath, filename);
            var inFullName = Common.PathCombine(_inputBasePath, filename);
            const bool overwrite = true;
            File.Copy(inFullName, outFullName, overwrite);
            return outFullName;
        }

        private static string FileTestCopy(string filename)
        {
            var outFullName = Common.PathCombine(_outputTestPath, filename);
            var inFullName = Common.PathCombine(_inputTestPath, filename);
            const bool overwrite = true;
            File.Copy(inFullName, outFullName, overwrite);
            return outFullName;
        }

        private static string TestDataSetup(string test, string data)
        {
            Common.ProgInstall = PathPart.Bin(Environment.CurrentDirectory, @"/../PsSupport");
            Common.SupportFolder = "";
            Common.ProgBase = Common.ProgInstall;
            TestPathSetup(test);
            if (Directory.Exists(_outputTestPath))
                Directory.Delete(_outputTestPath, true);
            Directory.CreateDirectory(_outputTestPath);
            var infile = FileTestCopy(data);
            return infile;
        }
        #endregion TestPath

        #region AcquireUserSettings Common
        /// <summary>
        /// Tests AcquireUserSettings
        /// </summary>
        /// <param name="testName">Test name (and fold name of test).</param>
        /// <param name="mainName">input xhtml name in folder</param>
        /// <param name="cssName">css name in folder</param>
        /// <param name="msg">message identifying test if mismatch (failure).</param>
        protected void AcquireUserSettingsTest(string testName, string mainName, string cssName, string msg)
        {
            CommonOutputSetup(testName);
            Param.SetValue(Param.InputType, "Dictionary");
            Param.LoadSettings();

            File.Copy(FileInput(mainName), FileOutput(mainName));
            JobCopy(cssName);

            var tpe = new SIL.PublishingSolution.PsExport() { DataType = "Scripture" };
            var mainFullName = FileOutput(mainName);
            string job = tpe.GetFluffedCssFullName(mainFullName, _outputTestPath, FileOutput(cssName));
            TextFileAssert.AreEqual(FileExpect(Path.GetFileName(job)), job, msg);
        }

        #endregion AcquireUserSettings Common

        #region SeExport Common
        /// <summary>
        /// Test DeExport function.
        /// </summary>
        /// <param name="testName">test name (also folder name of test)</param>
        /// <param name="mainXhtml">input xhtml name in folder</param>
        /// <param name="jobFileName">job file in folder</param>
        /// <param name="target">desired destination</param>
        /// <param name="msg">message to identify test if error occurs</param>
        protected void SeExportTest(string testName, string mainXhtml, string jobFileName, string target, string msg)
        {
            CommonOutputSetup(testName);
            File.Copy(FileInput(mainXhtml), FileOutput(mainXhtml), true);
            string cssPath = Path.GetFileNameWithoutExtension(mainXhtml);
            File.Copy(FileInput(cssPath) + ".css", FileOutput(cssPath) +".css", true);
            JobCopy(jobFileName);
            FolderTree.Copy(FileInput("Pictures"), FileOutput("Pictures"));

            var tpe = new SIL.PublishingSolution.PsExport { DataType = "Scripture", Destination = target };
            tpe.SeExport(mainXhtml, jobFileName, _outputTestPath);
            switch (target)
            {
                case "OpenOffice/LibreOffice":
                    OdtTest.AreEqual(_expectTestPath, _outputTestPath, msg);
                    break;
                case "Pdf (using Prince)":
                    var outName = Path.GetFileNameWithoutExtension(mainXhtml) + ".pdf";
                    Assert.True(File.Exists(FileOutput(outName)), msg);
                    //FileAssert.AreEqual(FileExpect(outName), FileOutput(outName), msg);
                    break;
                default:
                    Assert.Fail(msg + " unkown destination");
                    break;
            }
        }
        #endregion SeExport Common

        #region PsExport Common
        /// <summary>
        /// Test PsExport function.
        /// </summary>
        /// <param name="testName">test name (also folder name of test)</param>
        /// <param name="mainXhtml">input xhtml name in folder</param>
        /// <param name="target">desired destination</param>
        /// <param name="tests">array of tests to apply to result</param>
        /// <param name="msg">message to identify test if error occurs</param>
        protected void ExportTest(string testName, string mainXhtml, string dataType, string target, string msg = null, ArrayList tests = null)
        {
            CommonOutputSetup(testName);
            CopyExistingFile(mainXhtml);
            var cssName = Path.GetFileNameWithoutExtension(mainXhtml) + ".css";
            CopyExistingFile(cssName);
            if (Directory.Exists(FileInput("Pictures")))
                FolderTree.Copy(FileInput("Pictures"), FileOutput("Pictures"));
            CopyExistingFile("FlexRev.xhtml");
            CopyExistingFile("FlexRev.css");

            var tpe = new SIL.PublishingSolution.PsExport { Destination = target, DataType = dataType};
            if (testName.ToLower() == "t5" || testName.ToLower() == "t8")
            {
                tpe._fromNUnit = true;
            }
            tpe.Export(FileOutput(mainXhtml));
            switch (target)
            {
                case "OpenOffice":
                    if (tests != null)
                        OdtTest.DoTests(_outputTestPath, tests);
                    else
                        OdtTest.AreEqual(_expectTestPath, _outputTestPath, msg);
                    break;
                case "Pdf":
                    var outName = Path.GetFileNameWithoutExtension(mainXhtml) + ".pdf";
                    Assert.True(File.Exists(FileOutput(outName)), msg);
                    //FileAssert.AreEqual(FileExpect(outName), FileOutput(outName), msg);
                    break;
                default:
                    Assert.Fail(msg + " unkown destination");
                    break;
            }
        }
        #endregion PsExport Common

        #region Internal private methods
        /// <summary>
        /// erase previous output, load localization files
        /// </summary>
        private static void CommonOutputSetup(string testName)
        {
            Common.PublishingSolutionsEnvironmentReset();
            TestPathSetup(testName);

            var di = new DirectoryInfo(_outputTestPath);
            //if (di.Exists)
            //    di.Delete(true);
            Common.DeleteDirectory(_outputTestPath);
            di.Create();

            Common.SupportFolder = "";
			Common.ProgBase = Common.GetPSApplicationPath();
            Param.LoadSettings();
        }
        
        /// <summary>
        /// Copies a file if it exists from the input test path to the output
        /// </summary>
        /// <param name="fileName">file to be copied if it exists</param>
        private static void CopyExistingFile(string fileName)
        {
            if (File.Exists(FileInput(fileName)))
                File.Copy(FileInput(fileName), FileOutput(fileName), true);
        }

        /// <summary>
        /// Copy all referenced css files in input folder
        /// </summary>
        /// <param name="jobFileName">Cascading style sheet file</param>
        private static void JobCopy(string jobFileName)
        {
            string jobFullName = FileInput(jobFileName);
            File.Copy(jobFullName, FileOutput(jobFileName), true);
            var sr = new StreamReader(jobFullName);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length == 0 || line.Substring(0, 1) == "/")
                    continue;
                Match m = Regex.Match(line, "@import \"(.*)\";", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    JobCopy(m.Groups[1].Value);
                    continue;
                }
                break;
            }
            sr.Close();
            return;
        }
        #endregion Internal private methods
        #endregion Internal

        #region T1
        /// <summary>
        /// Simple test where no changes are made to the settings.
        /// </summary>
        [Test]
        public void AcquireUserSettingsT1()
        {
            AcquireUserSettingsTest("T1", "1pe.xhtml", "Layout_02.css", "T1: Style sheet default preparation");
            //default action is to set style sheet based on last task selected. Layout_02.css is not used.
        }
        #endregion T1

        #region T2
        /// <summary>
        /// Test ODT export
        /// </summary>
        [Test]
        [Ignore]
        [Category("SkipOnTeamCity")]
        public void SeExportT2()
        {
            SeExportTest("T2", "1pe.xhtml", "Layout_02.css", "OpenOffice/LibreOffice",  "T2: ODT Export Test");
        }
        #endregion T2

        #region T3
        /// <summary>
        /// Test PDF export
        /// </summary>
        [Test]
        [Ignore]
        public void SeExportT3()
        {
            SeExportTest("T3", "1pe.xhtml", "Layout_02.css", "Pdf (using Prince)", "T3: PDF Export Test");
        }
        #endregion T3

        #region T4
        /// <summary>
        /// Test TE Export test
        /// </summary>
        [Test]
        [Category("SkipOnTeamCity")]
        public void AcceptT4NkonyaLO()
        {
            var tests = new ArrayList
            {
                new ODet(ODet.Def, "1st master", "mat21-23.odt", ODet.Content, "//style:style[1]/@style:master-page-name", "masterPage"),
                new ODet(ODet.Def, "page layout", "mat21-23.odt", ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:page-layout-name", "pageLayout"),
                new ODet(ODet.Chk, "page height", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:page-height", "22.9cm"),
                new ODet(ODet.Chk, "page width", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:page-width", "16.2cm"),
                new ODet(ODet.Chk, "page top margin", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-top", "1.15cm"),
                new ODet(ODet.Chk, "page left margin", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-left", "1.5cm"),
                new ODet(ODet.Chk, "page right margin", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-right", "1.5cm"),
                new ODet(ODet.Chk, "page bottom margin", "mat21-23.odt", ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-bottom", "1.15cm"),
                new ODet(ODet.Chk, "title section", "mat21-23.odt", ODet.Content, "//office:body/office:text/*[5]/@text:name", "Sect_scrBook"),
                new ODet(ODet.Chk, "book title", "mat21-23.odt", ODet.Content, "//text:span[@text:style-name='scrBookName_scrBook_scrBody']", "Mateo"),
                new ODet(ODet.Chk, "book code", "mat21-23.odt", ODet.Content, "//text:span[@text:style-name='scrBookCode_scrBook_scrBody']", "MAT"),
                new ODet(ODet.Chk, "main title", "mat21-23.odt", ODet.Content, "//text:span[@text:style-name='span_TitleMain_scrBook_scrBody']", "Mateo"),
                new ODet(ODet.Chk, "main title center", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:text-align", "center"),
                new ODet(ODet.Chk, "main title keep with next", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:keep-with-next", "always"),
                new ODet(ODet.Chk, "main title top pad", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:padding-top", "36pt"),
                new ODet(ODet.Chk, "main title bottom pad", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:padding-bottom", "12pt"),
                new ODet(ODet.Chk, "main title left margin", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:margin-left", "0pt"),
                new ODet(ODet.Chk, "main title indent", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:text-indent", "0pt"),
                new ODet(ODet.Chk, "main title orphans", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:orphans", "5"),
                new ODet(ODet.Chk, "main title font weight", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "main title complex font weight", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "main title style", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:font-style", "normal"),
                new ODet(ODet.Chk, "main title font size", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@fo:font-size", "18pt"),
                new ODet(ODet.Chk, "main title complex font size", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleMain_scrBook_scrBody']//@style:font-size-complex", "18pt"),
                new ODet(ODet.Chk, "2nd secondary title", "mat21-23.odt", ODet.Content, "//text:p[@text:style-name='TitleSecondary_TitleMain_scrBook_scrBody'][2]", "Lɔ́wanlɩ́n"),
                new ODet(ODet.Chk, "secondary title center", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@fo:text-align", "center"),
                new ODet(ODet.Chk, "secondary title bottom pad", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@fo:padding-bottom", "2pt"),
                new ODet(ODet.Chk, "secondary title display", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@text:display", "block"),
                new ODet(ODet.Chk, "secondary title style", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@fo:font-style", "italic"),
                new ODet(ODet.Chk, "secondary title font size", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@fo:font-size", "16pt"),
                new ODet(ODet.Chk, "secondary title complex font size", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='TitleSecondary_TitleMain_scrBook_scrBody']//@style:font-size-complex", "16pt"),
                new ODet(ODet.Chk, "position graphics from top", "mat21-23.odt", ODet.Styles, "//style:style[@style:name='Graphics1']//@style:vertical-pos", "from-top"),
                new ODet(ODet.Chk, "embedded picture", "mat21-23.odt", ODet.Content, "//draw:frame[@draw:style-name='Graphics1']//@xlink:href", "Pictures/2.jpg"),
            };

            ExportTest("T4", "mat21-23.xhtml", "Scripture", "OpenOffice", "", tests);
        }
        #endregion T4

        #region T5
        /// <summary>
        /// Test Flex Export test
        /// </summary>
        /// <remarks>For language, see: http://www.w3schools.com/xslfo/prop_language.asp
        /// and http://www.ietf.org/rfc/rfc3066.txt
        /// For country see: http://www.iso.org/iso/iso-3166-1_decoding_table.html
        /// </remarks>
        [Test]
        [Category("SkipOnTeamCity")]
        public void AcceptT5BuangALO()
        {
            var tests = new ArrayList
            {
                new ODet(ODet.Def, "1st master", ODet.Main, ODet.Content, "//style:style[1]/@style:master-page-name", "masterPage"),
                new ODet(ODet.Def, "page layout", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:page-layout-name", "pageLayout"),
                new ODet(ODet.Chk, "page top margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-top", "2cm"),
                new ODet(ODet.Chk, "page left margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-left", "2cm"),
                new ODet(ODet.Chk, "page right margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-right", "2cm"),
                new ODet(ODet.Chk, "page bottom margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-bottom", "2cm"),
                new ODet(ODet.Chk, "1st Page empty header", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:header-style", ""),
                new ODet(ODet.Chk, "1st Page empty footer", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:footer-style", ""),
                new ODet(ODet.Def, "left master", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:next-style-name", "leftMaster"),
                new ODet(ODet.Chk, "left header variable", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:variable-get/@text:name", "Left_Guideword_L"),
                new ODet(ODet.Def, "left header style", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:span/@text:style-name", "headerTextStyle"),
                //new ODet(ODet.Chk, "left header font", ODet.Main, ODet.Styles, "//style:style[@style:name='{headerTextStyle}']//fo:font-family", "Charis SIL"),
                //new ODet(ODet.Chk, "left header weight", ODet.Main, ODet.Styles, "//style:style[@style:name='{headerTextStyle}']//fo:font-weight", "700"),
                //new ODet(ODet.Chk, "left header size", ODet.Main, ODet.Styles, "//style:style[@style:name='{headerTextStyle}']//fo:font-size", "12pt"),
                new ODet(ODet.Def, "right master", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']/@style:next-style-name", "rightMaster"),
                new ODet(ODet.Chk, "right footer variable", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{rightMaster}']//style:footer//draw:frame//text:variable-get/@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "single column letter header", ODet.Main, ODet.Content, "//style:style[@style:name='Sect_letHead']//@fo:column-count", "1"),
                new ODet(ODet.Chk, "double column data", ODet.Main, ODet.Content, "//style:style[@style:name='Sect_letData']//@fo:column-count", "2"),
                new ODet(ODet.Chk, "letter header center", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:text-align", "center"),
                //new ODet(ODet.Chk, "letter header keep with next", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:keep-with-next", "always"),
                new ODet(ODet.Chk, "letter header top margin", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-top", "18pt"),
                new ODet(ODet.Chk, "letter header bottom margin", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-bottom", "18pt"),
                new ODet(ODet.Chk, "letter header font", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "letter header complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "letter header font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "letter header complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "letter header font size", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-size", "24pt"),
                new ODet(ODet.Chk, "letter header complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-size-complex", "24pt"),
                new ODet(ODet.Chk, "entry background", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:background-color", "transparent"),
                new ODet(ODet.Chk, "entry alignment", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-align", "left"),
                new ODet(ODet.Chk, "entry left margin", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:margin-left", "12pt"),
                new ODet(ODet.Chk, "entry indent", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-indent", "-12pt"),
                new ODet(ODet.Chk, "headword font", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "headword complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "headword font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "headword complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "headword font style", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-style", "normal"),
                new ODet(ODet.Chk, "headword font size", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "headword complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                //new ODet(ODet.Chk, "headword language", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:language", "bzh"),
                //new ODet(ODet.Chk, "headword country", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "headword left variable", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@text:name", "Left_Guideword_L"),
                new ODet(ODet.Chk, "headword right variable", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "headword left variable value", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@office:string-value", "anon"),
                new ODet(ODet.Chk, "headword right variable value", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@office:string-value", "anon"),
                new ODet(ODet.Chk, "pronunciation", ODet.Main, ODet.Content, "//text:span[@text:style-name='pronunciation_pronunciations_entry_letData_dicBody']", "[a.ˈnon]"),
                new ODet(ODet.Chk, "pronunciation font", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "pronunciation complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "pronunciation font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:font-weight", "400"),
                new ODet(ODet.Chk, "pronunciation complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@style:font-weight-complex", "400"),
                new ODet(ODet.Chk, "pronunciation font style", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:font-style", "normal"),
                new ODet(ODet.Chk, "pronunciation font size", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "pronunciation complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                //new ODet(ODet.Chk, "pronunciation language", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:language", "bzh-fonipa"),
                //new ODet(ODet.Chk, "pronunciation country", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "part of speech", ODet.Main, ODet.Content, "//text:span[@text:style-name='partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']", "noun(inal)"),
                new ODet(ODet.Chk, "part of speech parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "grammaticalinfo_sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "part of speech font", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "part of speech complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "part of speech font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:font-weight", "400"),
                new ODet(ODet.Chk, "part of speech complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@style:font-weight-complex", "400"),
                new ODet(ODet.Chk, "part of speech font style", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:font-style", "italic"),
                new ODet(ODet.Chk, "part of speech font size", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "part of speech complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='grammaticalinfo_sense_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "part of speech language", ODet.Main, ODet.Styles, "//style:style[@style:name='partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "part of speech country", ODet.Main, ODet.Styles, "//style:style[@style:name='partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "sense number", ODet.Main, ODet.Content, "//text:span[@text:style-name='xsensenumber_sense_senses_entry_letData_dicBody']", "1) "),
                new ODet(ODet.Chk, "sense number font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='xsensenumber_sense_senses_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "sense number complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='xsensenumber_sense_senses_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "sense number parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='xsensenumber_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "sense number parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='sense_senses_entry_letData_dicBody']//@style:parent-style-name", "senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "sense number parent of parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='senses_entry_letData_dicBody']//@style:parent-style-name", "entry_letData_dicBody"),
                //new ODet(ODet.Chk, "sense number language", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:language", "bzh-fonipa"), -- sense numbers don't have a language
                //new ODet(ODet.Chk, "sense number country", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "definition", ODet.Main, ODet.Content, "//text:span[@text:style-name='xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']", "fruit, seed"),
                new ODet(ODet.Chk, "definition language", ODet.Main, ODet.Styles, "//style:style[@style:name='xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "definition country", ODet.Main, ODet.Styles, "//style:style[@style:name='xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "definition parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "definition_.en_sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "definition parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='definition_.en_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "example font", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "example complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "example font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:font-weight", "400"),
                new ODet(ODet.Chk, "example complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@style:font-weight-complex", "400"),
                new ODet(ODet.Chk, "example font style", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:font-style", "italic"),
                new ODet(ODet.Chk, "example font size", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "example complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                //new ODet(ODet.Chk, "example language", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:language", "bzh"),
                //new ODet(ODet.Chk, "example country", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "translation", ODet.Main, ODet.Content, "//text:span[@text:style-name='translation_.en_translations_xitem_examples_sense_senses_entry_letData_dicBody']", "There is fruit on the corn so let's eat it"),
                new ODet(ODet.Chk, "translation language", ODet.Main, ODet.Styles, "//style:style[@style:name='translation_.en_translations_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "translation country", ODet.Main, ODet.Styles, "//style:style[@style:name='translation_.en_translations_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "translation parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='translation_.en_translations_xitem_examples_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "translations_xitem_examples_sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "translation parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='translations_xitem_examples_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "xitem_examples_sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "translation parent of parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='xitem_examples_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "examples_sense_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "translation parent of parent of parent of parent style", ODet.Main, ODet.Styles, "//style:style[@style:name='examples_sense_senses_entry_letData_dicBody']//@style:parent-style-name", "sense_senses_entry_letData_dicBody"),
                //new ODet(ODet.Chk, "", ODet.Main, ODet.Styles, "", ""),
            };
            ExportTest("T5", "main.xhtml", "Dictionary", "OpenOffice", "", tests);
        }
        #endregion T5

        #region T6
        /// <summary>
        /// Test Flex Export test
        /// </summary>
        [Test]
        [Category("SkipOnTeamCity")]
        public void MainAndRevT6()
        {
            var tests = new ArrayList
            {
                new ODet(ODet.Def, "master main", ODet.Mast, ODet.Content, "//office:text/*[3]/text:section-source/@xlink:href", "../main.odt"),
                new ODet(ODet.Def, "master flexrev", ODet.Mast, ODet.Content, "//office:text/*[4]/text:section-source/@xlink:href", "../FlexRev.odt"),
                new ODet(ODet.Def, "1st master", ODet.Main, ODet.Content, "//style:style[1]/@style:master-page-name", "masterPage"),
                new ODet(ODet.Def, "page layout", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:page-layout-name", "pageLayout"),
                new ODet(ODet.Chk, "page top margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-top", "2cm"),
                new ODet(ODet.Chk, "page left margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-left", "2cm"),
                new ODet(ODet.Chk, "page right margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-right", "2cm"),
                new ODet(ODet.Chk, "page bottom margin", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-bottom", "2cm"),
                new ODet(ODet.Chk, "1st Page empty header", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:header-style", ""),
                new ODet(ODet.Chk, "1st Page empty footer", ODet.Main, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:footer-style", ""),
                new ODet(ODet.Def, "left master", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:next-style-name", "leftMaster"),
                new ODet(ODet.Chk, "left header variable", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:variable-get/@text:name", "Left_Guideword_L"),
                new ODet(ODet.Def, "left header style", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:span/@text:style-name", "headerTextStyle"),
                new ODet(ODet.Def, "right master", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']/@style:next-style-name", "rightMaster"),
                new ODet(ODet.Chk, "right footer variable", ODet.Main, ODet.Styles, "//style:master-page[@style:name='{rightMaster}']//style:footer//draw:frame//text:variable-get/@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "single column letter header", ODet.Main, ODet.Content, "//style:style[@style:name='Sect_letHead']//@fo:column-count", "1"),
                new ODet(ODet.Chk, "double column data", ODet.Main, ODet.Content, "//style:style[@style:name='Sect_letData']//@fo:column-count", "2"),
                new ODet(ODet.Chk, "letter header center", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:text-align", "center"),
                new ODet(ODet.Chk, "letter header top margin", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-top", "18pt"),
                new ODet(ODet.Chk, "letter header bottom margin", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-bottom", "18pt"),
                new ODet(ODet.Chk, "letter header font", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "letter header complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "letter header font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "letter header complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "letter header font size", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-size", "24pt"),
                new ODet(ODet.Chk, "letter header complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-size-complex", "24pt"),
                new ODet(ODet.Chk, "entry background", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:background-color", "transparent"),
                new ODet(ODet.Chk, "entry alignment", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-align", "left"),
                new ODet(ODet.Chk, "entry left margin", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:margin-left", "12pt"),
                new ODet(ODet.Chk, "entry indent", ODet.Main, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-indent", "-12pt"),
                new ODet(ODet.Chk, "headword font", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "headword complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "headword font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "headword complex font weight", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                //new ODet(ODet.Chk, "headword font style", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-style", "normal"),
                new ODet(ODet.Chk, "headword font size", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "headword complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='headword_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "headword left variable", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@text:name", "Left_Guideword_L"),
                new ODet(ODet.Chk, "headword right variable", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "headword left variable value", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@office:string-value", "-d"),
                new ODet(ODet.Chk, "headword right variable value", ODet.Main, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@office:string-value", "-d"),
                new ODet(ODet.Chk, "pronunciation", ODet.Main, ODet.Content, "//text:span[@text:style-name='span_.bzh-fonipa_pronunciation_pronunciations_entry_letData_dicBody']", "ⁿd"),
                new ODet(ODet.Chk, "pronunciation font", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@fo:font-family", "Doulos SIL"),
                new ODet(ODet.Chk, "pronunciation complex font", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@style:font-name-complex", "Doulos SIL"),
                new ODet(ODet.Chk, "pronunciation complex font size", ODet.Main, ODet.Styles, "//style:style[@style:name='pronunciation_pronunciations_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "part of speech", ODet.Main, ODet.Content, "//text:span[@text:style-name='span_.en_partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']", "N(inal)"),
                new ODet(ODet.Chk, "part of speech language", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "part of speech country", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_partofspeech_.en_grammaticalinfo_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "sense number", ODet.Main, ODet.Content, "//text:span[@text:style-name='xsensenumber_sense_senses_entry_letData_dicBody']", "1) "),
                new ODet(ODet.Chk, "definition", ODet.Main, ODet.Content, "//text:span[@text:style-name='span_.en_xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']", "Sunday morning church service."),
                new ODet(ODet.Chk, "definition language", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "definition country", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_xitem_.en_definition_.en_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "example font", ODet.Main, ODet.Styles, "//style:style[@style:name='example_xitem_examples_sense_senses_entry_letData_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "translation", ODet.Main, ODet.Content, "//text:span[@text:style-name='span_.en_translation_.en_translations_examples_sense_senses_entry_letData_dicBody']", "They went to the service in the church."),
                new ODet(ODet.Chk, "translation language", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_translation_.en_translations_examples_sense_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "translation country", ODet.Main, ODet.Styles, "//style:style[@style:name='span_.en_translation_.en_translations_examples_sense_senses_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "reversalform font", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "reversalform complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "reversalform font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "reversalform complex font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "reversalform font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "reversalform complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "reversalform language", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "reversalform country", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "reversalform left variable", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@text:name", "Left_Guideword_L"),
                new ODet(ODet.Chk, "reversalform right variable", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "reversalform left variable value", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@office:string-value", "aha!"),
                new ODet(ODet.Chk, "reversalform right variable value", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@office:string-value", "aha!"),
                new ODet(ODet.Chk, "headref", ODet.Rev, ODet.Content, "//text:span[@text:style-name='revsensenumber_headref_senses_entry_letData_dicBody']", "ee"),
                new ODet(ODet.Chk, "headref font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "headref complex font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "headref font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "headref complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "pronunciation", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']", "ɛː"),
                new ODet(ODet.Chk, "pronunciation language", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@fo:language", "none"),
                new ODet(ODet.Chk, "pronunciation country", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@fo:country", "none"),
                new ODet(ODet.Chk, "part of speech", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.en_partofspeech_.en_grammaticalinfo_senses_entry_letData_dicBody']", "Interj"),
                new ODet(ODet.Chk, "example font", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "example complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "example font style", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-style", "italic"),
                new ODet(ODet.Chk, "example font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "example complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "translation", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.en_translation_.en_translations_examples_senses_entry_letData_dicBody']", "Look! He's come up over there."),
                new ODet(ODet.Chk, "translation language", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_translation_.en_translations_examples_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "translation country", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_translation_.en_translations_examples_senses_entry_letData_dicBody']//@fo:country", "US"),
                //new ODet(ODet.Chk, "", ODet.Main, ODet.Styles, "", ""),
            };
            ExportTest("T6", "main.xhtml", "Dictionary", "OpenOffice", "", tests);
        }
        #endregion T6

        #region T7
        /// <summary>
        /// Test Flex Export test
        /// </summary>
        [Test]
        [Category("SkipOnTeamCity")]
        public void RevT7()
        {
            var tests = new ArrayList
            {
                new ODet(ODet.Def, "1st master", ODet.Rev, ODet.Content, "//style:style[1]/@style:master-page-name", "masterPage"),
                new ODet(ODet.Def, "page layout", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:page-layout-name", "pageLayout"),
                new ODet(ODet.Chk, "page top margin", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-top", "2cm"),
                new ODet(ODet.Chk, "page left margin", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-left", "2cm"),
                new ODet(ODet.Chk, "page right margin", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-right", "2cm"),
                new ODet(ODet.Chk, "page bottom margin", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:page-layout-properties/@fo:margin-bottom", "2cm"),
                new ODet(ODet.Chk, "1st Page empty header", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:header-style", ""),
                new ODet(ODet.Chk, "1st Page empty footer", ODet.Rev, ODet.Styles, "//style:page-layout[@style:name='{pageLayout}']/style:footer-style", ""),
                new ODet(ODet.Def, "left master", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{masterPage}']/@style:next-style-name", "leftMaster"),
                new ODet(ODet.Chk, "left header variable", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:variable-get/@text:name", "Left_Guideword_L"),
                new ODet(ODet.Def, "left header style", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']//text:span/@text:style-name", "headerTextStyle"),
                new ODet(ODet.Def, "right master", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{leftMaster}']/@style:next-style-name", "rightMaster"),
                new ODet(ODet.Chk, "right footer variable", ODet.Rev, ODet.Styles, "//style:master-page[@style:name='{rightMaster}']//style:footer//draw:frame//text:variable-get/@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "single column letter header", ODet.Rev, ODet.Content, "//style:style[@style:name='Sect_letHead']//@fo:column-count", "1"),
                new ODet(ODet.Chk, "double column data", ODet.Rev, ODet.Content, "//style:style[@style:name='Sect_letData']//@fo:column-count", "2"),
                new ODet(ODet.Chk, "letter header center", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:text-align", "center"),
                //new ODet(ODet.Chk, "letter header keep with next", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:keep-with-next", "always"),
                new ODet(ODet.Chk, "letter header top margin", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-top", "18pt"),
                new ODet(ODet.Chk, "letter header bottom margin", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:margin-bottom", "18pt"),
                new ODet(ODet.Chk, "letter header font", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "letter header complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "letter header font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "letter header complex font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "letter header font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@fo:font-size", "24pt"),
                new ODet(ODet.Chk, "letter header complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='letter_letHead_dicBody']//@style:font-size-complex", "24pt"),
                new ODet(ODet.Chk, "entry background", ODet.Rev, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:background-color", "transparent"),
                new ODet(ODet.Chk, "entry alignment", ODet.Rev, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-align", "left"),
                new ODet(ODet.Chk, "entry left margin", ODet.Rev, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:margin-left", "12pt"),
                new ODet(ODet.Chk, "entry indent", ODet.Rev, ODet.Styles, "//style:style[@style:name='entry_letData_dicBody']//@fo:text-indent", "-12pt"),
                new ODet(ODet.Chk, "reversalform font", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "reversalform complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "reversalform font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "reversalform complex font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "reversalform font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "reversalform complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                new ODet(ODet.Chk, "reversalform language", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "reversalform country", ODet.Rev, ODet.Styles, "//style:style[@style:name='reversalform_entry_letData_dicBody']//@fo:country", "US"),
                new ODet(ODet.Chk, "reversalform left variable", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@text:name", "Left_Guideword_L"),
                new ODet(ODet.Chk, "reversalform right variable", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@text:name", "Right_Guideword_R"),
                new ODet(ODet.Chk, "reversalform left variable value", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[2]//@office:string-value", "aha!"),
                new ODet(ODet.Chk, "reversalform right variable value", ODet.Rev, ODet.Content, "//text:p[@text:style-name='entry_letData_dicBody']/text:span[3]//@office:string-value", "aha!"),
                new ODet(ODet.Chk, "headref font", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "headref complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "headref font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:font-weight", "700"),
                new ODet(ODet.Chk, "headref complex font weight", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@style:font-weight-complex", "700"),
                new ODet(ODet.Chk, "headref font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "headref complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                //new ODet(ODet.Chk, "headref language", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:language", "bzh"),
                //new ODet(ODet.Chk, "headref country", ODet.Rev, ODet.Styles, "//style:style[@style:name='headref_senses_entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "pronunciation", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']", "ɛː"),
                new ODet(ODet.Chk, "pronunciation font", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@fo:font-family", "Doulos SIL"),
                new ODet(ODet.Chk, "pronunciation complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@style:font-name-complex", "Doulos SIL"),
                new ODet(ODet.Chk, "pronunciation language", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@fo:language", "none"),
                new ODet(ODet.Chk, "pronunciation country", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.bzh-fonipa_pronunciationform_pronunciation_pronunciations_senses_entry_letData_dicBody']//@fo:country", "none"),
                new ODet(ODet.Chk, "part of speech", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.en_span_.en_grammaticalinfo_senses_entry_letData_dicBody']", "Interj"),
                new ODet(ODet.Chk, "part of speech parent style", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_span_.en_grammaticalinfo_senses_entry_letData_dicBody']//@style:parent-style-name", "span_.en_grammaticalinfo_senses_entry_letData_dicBody"),
                new ODet(ODet.Chk, "example font", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-family", "Charis SIL"),
                new ODet(ODet.Chk, "example complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@style:font-name-complex", "Charis SIL"),
                new ODet(ODet.Chk, "example font style", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-style", "italic"),
                new ODet(ODet.Chk, "example font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:font-size", "10pt"),
                new ODet(ODet.Chk, "example complex font size", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@style:font-size-complex", "10pt"),
                //new ODet(ODet.Chk, "example language", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:language", "bzh"),
                //new ODet(ODet.Chk, "example country", ODet.Rev, ODet.Styles, "//style:style[@style:name='example_examples_senses_entry_letData_dicBody']//@fo:country", "PG"),
                new ODet(ODet.Chk, "translation", ODet.Rev, ODet.Content, "//text:span[@text:style-name='span_.en_span_.en_translations_examples_senses_entry_letData_dicBody']", "Look! He's come up over there."),
                new ODet(ODet.Chk, "translation font", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_span_.en_translations_examples_senses_entry_letData_dicBody']//@fo:font-family", "Times New Roman"),
                new ODet(ODet.Chk, "translation complex font", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_span_.en_translations_examples_senses_entry_letData_dicBody']//@style:font-name-complex", "Times New Roman"),
                new ODet(ODet.Chk, "translation language", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_span_.en_translations_examples_senses_entry_letData_dicBody']//@fo:language", "en"),
                new ODet(ODet.Chk, "translation country", ODet.Rev, ODet.Styles, "//style:style[@style:name='span_.en_span_.en_translations_examples_senses_entry_letData_dicBody']//@fo:country", "US"),
                //new ODet(ODet.Chk, "", ODet.Rev, ODet.Styles, "", ""),
            };

            ExportTest("T7", "FlexRev.xhtml", "Dictionary", "OpenOffice", "", tests);
        }
        #endregion T7

        /// <summary>
        /// Test Flex Export test - Page A5 Test
        /// </summary>
        [Test]
        [Ignore]
        [Category("SkipOnTeamCity")]
        public void PsExportT8()
        {
            ExportTest("T8", "main.xhtml", "Dictionary", "OpenOffice", "T8: Flex ODT Export Test");
        }

        [Test]
        public void AddHomographAndSenseNumClassNamesTest()
        {
            const string testData = "FlexRev.xhtml";
            var flexRevFullName = FileCopy(testData);
            Common.StreamReplaceInFile(flexRevFullName, "class=\"headword\"", "class=\"headref\"");
            AddHomographAndSenseNumClassNames.Execute(flexRevFullName, flexRevFullName);
            var actual = Common.DeclareXMLDocument(false);
            actual.Load(flexRevFullName);
            var nodes = actual.SelectNodes("//*[@class='revhomographnumber']");
            Assert.AreEqual(8, nodes.Count);
        }

        [Test]
        public void XsltPreProcess0Test()
        {
            DataType = "Dictionary";
            var infile = TestDataSetup("Pre0", "predict.xhtml");
            Param.Value["Preprocessing"] = string.Empty;
            XsltPreProcess(infile);
            var files = Directory.GetFiles(_outputTestPath, "*.*");
            Assert.AreEqual(1, files.Length);
        }

        [Test]
        public void XsltPreProcess1Test()
        {
            DataType = "Dictionary";
            const string  data = "predict.xhtml";
            var infile = TestDataSetup("Pre1", data);
            Param.Value["Preprocessing"] = "Filter Empty Entries";
            XsltPreProcess(infile);
            var files = Directory.GetFiles(_outputTestPath, "*.*");
            Assert.AreEqual(2, files.Length);
            XmlAssert.AreEqual(Path.Combine(_expectTestPath, data), infile, "Empty Entries Preprocess produced different results");
        }

        [Test]
        public void XsltPreProcess2Test()
        {
            DataType = "Dictionary";
            const string data = "predict.xhtml";
            var infile = TestDataSetup("Pre2", data);
            Param.Value["Preprocessing"] = "Filter Empty Entries,Fix Duplicate ids";
            XsltPreProcess(infile);
            var files = Directory.GetFiles(_outputTestPath, "*.*");
            Assert.AreEqual(3, files.Length);
            XmlAssert.AreEqual(Path.Combine(_expectTestPath, data), infile, "Preprocess produced different results");
        }
    }
}