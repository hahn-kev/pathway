﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;
using SIL.PublishingSolution;
using SIL.Tool;

namespace Test.UIConfigurationToolTest
{
    /// <summary>
    ///This class is for ConfigurationTool Test
    ///</summary>
    [TestFixture]
    public class UIConfigurationToolTest
    {
        #region Setup
        /// <summary>holds path to input folder for all tests</summary>
        private static string _inputBasePath = string.Empty;
        /// <summary>holds path to expected results folder for all tests</summary>
        private static string _expectBasePath = string.Empty;
        /// <summary>holds path to output folder for all tests</summary>
        private static string _outputBasePath = string.Empty;
        private string _supportSource = string.Empty;
        private ConfigurationTool _configTool;
        /// <summary>
        /// setup Input, Expected, and Output paths relative to location of program
        /// </summary>
        [TestFixtureSetUp]
        protected void SetUp()
        {
            _configTool = new ConfigurationTool();

            string testPath = PathPart.Bin(Environment.CurrentDirectory, "/ConfigurationTool/TestFiles");
            _inputBasePath = Common.PathCombine(testPath, "Input");
            _expectBasePath = Common.PathCombine(testPath, "Expected");
            _outputBasePath = Common.PathCombine(testPath, "Output");
            const bool recursiveDelete = true;
            if (Directory.Exists(_outputBasePath))
                Directory.Delete(_outputBasePath, recursiveDelete);
            Directory.CreateDirectory(_outputBasePath);

            _supportSource = Common.DirectoryPathReplace(testPath + "/../../../PsSupport");

            string fileName = "DictionaryStyleSettings.xml";
            CopyFilesSupportToIO(fileName);

            fileName = "ScriptureStyleSettings.xml";
            CopyFilesSupportToIO(fileName);

            fileName = "StyleSettings.xml";
            CopyFilesSupportToIO(fileName);

            fileName = "StyleSettings.xsd";
            CopyFilesSupportToIO(fileName);

            string folderName = "styles";
            CopyFolderSupportToIO(folderName);
            LoadParam();


            //Common.ProgBase = _outputBasePath;
            //Param.Value["UserSheetPath"] = _outputBasePath; 
        }

        private void CopyFilesSupportToIO(string fileName)
        {
            string fromFileName = Common.PathCombine(_supportSource, fileName);
            string toFileName = Common.PathCombine(_inputBasePath, fileName);
            File.Copy(fromFileName, toFileName, true);
            toFileName = Common.PathCombine(_outputBasePath, fileName);
            File.Copy(fromFileName, toFileName, true);
        }

        private void CopyFolderSupportToIO(string fileName)
        {
            string fromFileName = Common.PathCombine(_supportSource, fileName);
            string toFileName = Common.PathCombine(_inputBasePath, fileName);
            FolderTree.Copy(fromFileName, toFileName);

            toFileName = Common.PathCombine(_outputBasePath, fileName);
            FolderTree.Copy(fromFileName, toFileName);
        }
        #endregion Setup

        #region Tests

        /// <summary>
        ///A test for UpdateGridTest
        ///</summary>
        [Test]
        public void UpdateGridTest()
        {
            DataGridView grid = PopulateGrid2();
            // load grid
            const int mySelectedRowIndex = 0;
            _configTool._CToolBL.SelectedRowIndex = 0;
            TextBox tb1 = new TextBox();
            tb1.Name = "txtDesc";
            tb1.Text = "txtDesc123";
            Control myControl = tb1;
            _configTool._CToolBL.UpdateGrid(myControl, grid);
            Assert.AreEqual(grid[_configTool._CToolBL.ColumnDescription, mySelectedRowIndex].Value.ToString(), tb1.Text, "description error");

            TextBox tb2 = new TextBox();
            tb2.Name = "txtComment";
            tb2.Text = "txtComment123";
            myControl = tb2;
            _configTool._CToolBL.UpdateGrid(myControl, grid);
            Assert.AreEqual(grid[_configTool._CToolBL.ColumnComment, mySelectedRowIndex].Value.ToString(), tb2.Text, "comment error");

            TextBox tb3 = new TextBox();
            tb3.Name = "txtName";
            tb3.Text = "txtName123";
            myControl = tb3;
            _configTool._CToolBL.UpdateGrid(myControl, grid);
            Assert.AreEqual(grid[_configTool._CToolBL.ColumnName, mySelectedRowIndex].Value.ToString(), tb3.Text, "name error");

            CheckBox ch1 = new CheckBox();
            ch1.Name = "ChkShown";
            ch1.Checked = true;
            myControl = ch1;
            _configTool._CToolBL.UpdateGrid(myControl, grid);
            Assert.AreEqual(grid[_configTool._CToolBL.ColumnShown, mySelectedRowIndex].Value.ToString(), "Yes", "shown error");
        }

        [Test]
        public void GetNewStyleName()
        {
            ArrayList al = new ArrayList();
            al.Add("Sheet-1");
            al.Add("Sheet-4");
            al.Add("Sheet-8");
            _configTool._CToolBL.StyleName = "Sheet-1";
            string expected = "Copy of Sheet-1";
            string actual = _configTool._CToolBL.GetNewStyleName(al, "copy");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SelectRow()
        {
            DataGridView stylesGrid = PopulateGrid();

            _configTool._CToolBL.SelectRow(stylesGrid, "Style3");
            bool fail = stylesGrid.Rows[1].Selected;

            _configTool._CToolBL.SelectRow(stylesGrid, "Style2");
            bool pass = stylesGrid.Rows[1].Selected;

            Assert.IsTrue(!fail && pass, "SelectRow test is failed");
        }
        [Test]
        public void IsNameExists()
        {
            DataGridView stylesGrid = PopulateGrid();
            stylesGrid.Rows[1].Selected = true;
            bool exist = _configTool._CToolBL.IsNameExists(stylesGrid, "Style2");
            Assert.IsFalse(exist);

            exist = _configTool._CToolBL.IsNameExists(stylesGrid, "Style3");
            Assert.IsTrue(exist);
        }

        [Test]
        public void WriteCssClass()
        {
            string fileName = "WriteCss.css";
            string outputFileWithPath = Common.PathCombine(_outputBasePath, fileName);
            StreamWriter writeCss = new StreamWriter(outputFileWithPath);
            var value = new Dictionary<string, string>();
            value["column-gap"] = "5pt";
            _configTool._CToolBL.WriteCssClass(writeCss, "letData", value);
            writeCss.Close();
            string expectedFileWithPath = Common.PathCombine(_expectBasePath, fileName);
            TextFileAssert.AreEqual(expectedFileWithPath, outputFileWithPath, "WriteCss Test fails");
        }

        /// <summary>
        ///A test for RemoveXMLNode
        ///</summary>
        [Test]
        public void RemoveXMLNode()
        {
            //LoadParam();

            string styleName = "Draft";  // Style Name
            string xPath = "//styles/" + "paper" + "/style[@name='" + styleName + "']";
            XmlNode removableNode = Param.GetItem(xPath);
            if (removableNode == null)
                Assert.Fail("DictionaryStyleSettings.xml - The Xpath - " + xPath + " for Draft not found");


            string fileName = "Layout_01.css"; // File Name
            NunitFileCopy(fileName); // copy to outputFolder
            string fileWithPath = Common.PathCombine(_outputBasePath, fileName);
            if (!File.Exists(fileWithPath))
                Assert.Fail("Please add the input file Draft.css");

            _configTool._CToolBL.RemoveXMLNode(styleName);

            removableNode = Param.GetItem(xPath);
            if (removableNode != null)
                Assert.Fail("Unable to delete the node Draft in DictionaryStyleSettings.xml");

            fileWithPath = Common.PathCombine(_outputBasePath, fileName);
            if (File.Exists(fileWithPath))
                Assert.Fail("Unable to delete the input file Draft.css");
        }

        /// <summary>
        ///A test for RemoveXMLNode
        ///</summary>
        [Test]
        public void AddNew()
        {
            //LoadParam();

            string styleName = "MyyStyyle98765";
            _configTool._CToolBL.MediaType = "paper";
            _configTool._CToolBL.StyleName = styleName;
            _configTool._CToolBL.FileName = styleName + ".css";
            _configTool._CToolBL.AddNew(styleName);
            string xPath = "//styles/" + "paper" + "/style[@name='" + styleName + "']";

            XmlNode removableNode = Param.GetItem(xPath);
            if (removableNode == null)
                Assert.Fail("DictionaryStyleSettings.xml - The Xpath - " + xPath + "  not found in ADD New");
        }

        /// <summary>
        ///A test for Tab Controls
        ///</summary>
        [Test]
        public void ClearPropertyTab()
        {
            TabPage tabDisplay = new TabPage("tabDisplay");

            TextBox tb1 = new TextBox();
            tabDisplay.Controls.Add(tb1);
            tb1.Name = "txtStyleName1";
            tb1.Text = "MyStyleName1";

            TextBox tb2 = new TextBox();
            tabDisplay.Controls.Add(tb2);
            tb2.Name = "txtStyleName2";
            tb2.Text = "MyStyleName2";

            ComboBox dd1 = new ComboBox();
            tabDisplay.Controls.Add(dd1);
            dd1.Name = "chStyleName1";
            dd1.Items.Add("a");
            dd1.Items.Add("b");
            dd1.Items.Add("c");

            ComboBox dd2 = new ComboBox();
            tabDisplay.Controls.Add(dd2);
            dd2.Name = "chStyleName2";
            dd2.Items.Add("1");
            dd2.Items.Add("2");
            dd2.Items.Add("3");

            _configTool._CToolBL.ClearPropertyTab(tabDisplay);

            Assert.IsTrue(tb1.Text.Length == 0 && tb2.Text.Length == 0, "Text Box Value is not cleared");
            Assert.IsTrue(dd1.Items.Count == 0 && dd2.Items.Count == 0, "Combo Box Value is not cleared");
        }

        /// <summary>
        ///A test for PageSizeTest
        ///</summary>
        [Test]
        public void PageSizeTest()
        {
            string[] width = { "595", "420", "459", "298", "612", "396", "378", "418", "432" };
            string[] height = { "842", "595", "649", "420", "792", "612", "594", "626", "648" };
            string[] expected = { "A4", "A5", "C5", "A6", "Letter", "Half letter", "5.25in x 8.25in", "5.8in x 8.7in", "6in x 9in" };
            for (int i = 0; i < expected.Length; i++)
            {
                string actual = _configTool._CToolBL.PageSize1(width[i], height[i]);
                Assert.AreEqual(actual, expected[i]);
            }
        }

        /// <summary>
        ///A test for SaveColumnWidthTest and GridColumnWidth
        ///</summary>
        [Test]
        public void SaveColumnWidthTest()
        {
            //LoadParam();

            DataGridView grid = PopulateGrid2();
            string[] value = { "5", "10", "15", "20", "25" };

            for (int i = 0; i < value.Length; i++)
            {
                _configTool._CToolBL.SaveColumnWidth(i.ToString(), value[i]);
            }

            _configTool._CToolBL.GridColumnWidth(grid);

            bool result = true;
            for (int i = 0; i < value.Length; i++)
            {
                if (grid.Columns[i].Width.ToString() != value[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsTrue(result, "Error in  Grid ColumnWidth");
        }

        /// <summary>
        ///A test for SaveColumnWidthTest and GridColumnWidth
        ///</summary>
        [Test]
        public void CopyStyle()
        {
            ArrayList cssNames = new ArrayList();
            cssNames.Add("style-1");
            cssNames.Add("style-2");
            DataGridView grid = PopulateGrid2();
            _configTool._CToolBL.StyleName = "Sheet-1";
            _configTool._CToolBL.CopyStyle(grid, cssNames);
            string sName = "Copy of Sheet-1";
            string xPath = "//styles/" + "paper" + "/style[@name='" + sName + "']";
            XmlNode removableNode = Param.GetItem(xPath);
            if (removableNode == null)
                Assert.Fail("DictionaryStyleSettings.xml - The Xpath - " + xPath + "  not found in CopyStyle");
        }

        /// <summary>
        ///A test for SaveColumnWidthTest and GridColumnWidth
        ///</summary>
        [Test]
        public void WriteMedia()
        {
            _configTool._CToolBL.MediaType = "Web";
            _configTool._CToolBL.WriteMedia();
            string media = Param.GetAttrByName("//categories/category", "Media", "select");
            Assert.AreEqual(media, _configTool._CToolBL.MediaType, "Media writing failed");
        }


        /// <summary>
        ///A test for WriteAttribTest
        ///</summary>
        [Test]
        public void WriteAttribTest()
        {
            _configTool._CToolBL.PreviousValue = "Draft";
            string key = "StyleName";
            TextBox tb = new TextBox();
            tb.Text = _configTool._CToolBL.PreviousValue;
            object sender = tb;

            bool result = _configTool._CToolBL.WriteAttrib(key, sender);
            Assert.IsFalse(result, "Previous value not utilized properly");

            _configTool._CToolBL.StyleName = "no";
            _configTool._CToolBL.MediaType = "no";
            _configTool._CToolBL.PreviousValue = "Draft";
            key = "StyleName";
            string newStyleName = "NewStyleName";
            tb.Text = newStyleName;
            sender = tb;

            result = _configTool._CToolBL.WriteAttrib(key, sender);
            Assert.IsFalse(result, "Empty Media & Empty Style not handled");

            //Writing StyleName and FileName 
            key = _configTool._CToolBL.AttribName;
            _configTool._CToolBL.StyleName = "LikeBuangPNG";
            _configTool._CToolBL.MediaType = "paper";
            _configTool._CToolBL.PreviousValue = "Draft";
            _configTool._CToolBL.FileName = newStyleName + ".Css";
            tb.Text = newStyleName;
            sender = tb;

            result = _configTool._CToolBL.WriteAttrib(key, sender);

            string xPath = "//styles/" + "paper" + "/style[@name='" + newStyleName + "']";
            XmlNode removableNode = Param.GetItem(xPath);
            if (removableNode == null)
                Assert.Fail("DictionaryStyleSettings.xml - The Xpath - " + xPath + "  not found in WriteAttribTest");

            //Writing Shown
            key = _configTool._CToolBL.AttribShown;
            CheckBox checkBox = new CheckBox();
            checkBox.Checked = true;
            sender = checkBox;
            _configTool._CToolBL.StyleName = newStyleName;

            result = _configTool._CToolBL.WriteAttrib(key, sender);
            removableNode = Param.GetItem(xPath);
            if (removableNode != null)
            {
                string desc = removableNode.Attributes["shown"].Value;
                Assert.AreEqual("Yes", desc, "Shown not added");
            }

            //Writing Description 
            key = _configTool._CToolBL.ElementDesc;
            tb.Text = "New Description Added";
            sender = tb;

            result = _configTool._CToolBL.WriteAttrib(key, sender);
            removableNode = Param.GetItem(xPath);
            if (removableNode != null)
            {
                string desc = removableNode.FirstChild.InnerText;
                Assert.AreEqual(tb.Text, desc, "Description not added");
            }

            //Writing Comment
            key = _configTool._CToolBL.ElementComment;
            tb.Text = "New Comment Added";
            sender = tb;

            result = _configTool._CToolBL.WriteAttrib(key, sender);
            removableNode = Param.GetItem(xPath);
            if (removableNode != null)
            {
                string desc = removableNode.FirstChild.NextSibling.InnerText;
                Assert.AreEqual(tb.Text, desc, "Comment not added");
            }
        }

        /// <summary>
        ///A test for SaveColumnWidthTest and GridColumnWidth
        ///</summary>
        [Test]
        public void GetValue()
        {
            _configTool._CToolBL.SetCssDictionartyToTest();
            string task = "letData";
            string key = "column-count";
            string actual = _configTool._CToolBL.GetValue(task, key, "");
            string expected = "2";
            Assert.AreEqual(expected, actual, "GetValue Failed");

            task = "entry";
            key = "text-align";
            actual = _configTool._CToolBL.GetValue(task, key, "No");
            expected = "justify";
            Assert.AreEqual(expected, actual, "GetValue Failed");

        }

        [Test]
        public void WriteAtImport()
        {
            TreeView TvFeatures = new TreeView();
            _configTool._CToolBL.PopulateFeatureLists(TvFeatures);

            string fileName = "WriteAtImport.css";
            string outputFileWithPath = Common.PathCombine(_outputBasePath, fileName);
            StreamWriter writeCss = new StreamWriter(outputFileWithPath);

            string attrib = "Page Size";
            string key = "A4";
            _configTool._CToolBL.WriteAtImport(writeCss, attrib, key);
            writeCss.Close();
            string expectedFileWithPath = Common.PathCombine(_expectBasePath, fileName);
            TextFileAssert.AreEqual(expectedFileWithPath, outputFileWithPath, "WriteCss Test fails");
        }

        [Test]
        public void CreateCssFile()
        {
            string fileName = "CreateCssFile.css";
            _configTool._CToolBL.CreateCssFile(fileName);
            string expectedFileWithPath = Common.PathCombine(_expectBasePath, fileName);
            string outputFileWithPath = Common.PathCombine(_outputBasePath, fileName);
            TextFileAssert.AreEqual(expectedFileWithPath, outputFileWithPath, "CreateCssFile Test fails");
        }

        [Test]
        public void SetPreviousLayoutSelect()
        {
            DataGridView gridView = new DataGridView();
            string actual = _configTool._CToolBL.SetPreviousLayoutSelect(gridView);
            string expected = "FieldWorksStyles";
            Assert.AreEqual(expected, actual, "SetPreviousLayoutSelect Test failed");
        }

        [Test]
        public void LoadMediaStyle()
        {
            DataGridView gridView = new DataGridView();
            ArrayList arrayList = new ArrayList();
            _configTool._CToolBL.MediaType = "paper";
            _configTool._CToolBL.CreateGridColumn();
            _configTool._CToolBL.ShowStyleInGrid(gridView, arrayList);
            Assert.IsTrue(_configTool._CToolBL.DataSetForGrid.Tables[0].Rows.Count > 0, "ShowStyleInGrid Test failed");
        }

        /// <summary>
        ///A test for SaveColumnWidthTest and GridColumnWidth
        ///</summary>
        [Test]
        public void CopyCustomStyleToSend()
        {
            _configTool._CToolBL.MediaType = "paper";
            //LoadParam();
            string folderPath = Common.PathCombine(_outputBasePath, "NewFolderPath");
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);

            string fileName = "Layout_01.css"; // File Name
            NunitFileCopy(fileName); // copy to outputFolder

            fileName = "Layout_12.css"; // File Name
            NunitFileCopy(fileName); // copy to outputFolder

            Assert.IsTrue(_configTool._CToolBL.CopyCustomStyleToSend(folderPath), "CopyCustomStyleToSend test failed");
        }

        [Test]
        public void CreateGridColumn()
        {
            _configTool._CToolBL.CreateGridColumn();
            int columnCount = _configTool._CToolBL.DataSetForGrid.Tables[0].Columns.Count;
            Assert.AreEqual(9, columnCount, "Grid Column Count Error");

            bool result = true;
            string[] column = { "Name", "Description", "Comment", "Type", "Shown", "Approvedby", "File" };
            for (int i = 0; i < column.Length; i++)
            {
                if (_configTool._CToolBL.DataSetForGrid.Tables[0].Columns[i].ToString() != column[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsTrue(result, "Grid Column order is not proper");
        }

        [Test]
        [Category("SkipOnTeamCity")]
        public void GetUserInformationTest()
        {
            UserInformation userInfo = new UserInformation();
            userInfo.GetUserInformation(false);

            Assert.IsNotNullOrEmpty(userInfo.OSName);
            Assert.IsNotNullOrEmpty(userInfo.UserSystemGuid);
            Assert.IsNotNullOrEmpty(userInfo.OSServicePack);
            Assert.IsNotNullOrEmpty(userInfo.UserSystemName);
            Assert.IsNotNullOrEmpty(userInfo.Language);
            //Assert.IsNotNullOrEmpty(userInfo.FontLists);
            Assert.IsNotNullOrEmpty(userInfo.SystemCountry);
            Assert.IsNotNullOrEmpty(userInfo.UserIpAddress);
            Assert.IsNotNullOrEmpty(userInfo.JavaVersion);
            Assert.IsNotNullOrEmpty(userInfo.XelatexVersion);
            Assert.IsNotNullOrEmpty(userInfo.PathwayVersion);
            Assert.IsNotNullOrEmpty(userInfo.LibraofficeVersion);
            Assert.IsNotNullOrEmpty(userInfo.Paratext);
            Assert.IsNotNullOrEmpty(userInfo.TEVersion);
            Assert.IsNotNullOrEmpty(userInfo.Prince);
            Assert.IsNotNullOrEmpty(userInfo.IndesignVersion);
            Assert.IsNotNullOrEmpty(userInfo.BrowserList);
            Assert.IsNotNullOrEmpty(userInfo.FrameworkVersion);
            Assert.IsNotNullOrEmpty(userInfo.GeoLocation);
        }

        #endregion

        #region Methods

        private DataGridView PopulateGrid()
        {
            DataGridView stylesGrid = new DataGridView();

            stylesGrid.Columns.Add("Name", "Name");
            stylesGrid.Columns.Add("File", "File");

            string[] row1 = { "Style1", "Style1.css" };
            string[] row2 = { "Style2", "Style2.css" };
            string[] row3 = { "Style3", "Style3.css" };
            stylesGrid.Rows.Add(row1);
            stylesGrid.Rows.Add(row2);
            stylesGrid.Rows.Add(row3);
            return stylesGrid;
        }
        private DataGridView PopulateGrid2()
        {
            DataGridView stylesGrid = new DataGridView();

            stylesGrid.Columns.Add("Name", "Name");
            stylesGrid.Columns.Add("Description", "Description");
            stylesGrid.Columns.Add("Comment", "Comment");
            stylesGrid.Columns.Add("Type", "Type");
            stylesGrid.Columns.Add("Shown", "Shown");
            stylesGrid.Columns.Add("approvedBy", "approvedBy");
            string[] row1 = { "Style2", "Style2 is selected", "2 comment", "Custom", "Yes", "GPS" };
            string[] row2 = { "txtName123", "txtDesc123", "txtComment123", "Custom", "Yes", "GPS" };
            string[] row3 = { "Style3", "Style3 is selected", "3 comment", "Custom", "Yes", "GPS" };
            stylesGrid.Rows.Add(row1);
            stylesGrid.Rows.Add(row2);
            stylesGrid.Rows.Add(row3);
            return stylesGrid;
        }
        public static string NunitFileCopy(string fileName)
        {
            string fromFileName = Common.PathCombine(_inputBasePath, fileName);
            string toFileName = Common.PathCombine(_outputBasePath, fileName);
            if (File.Exists(fromFileName))
                File.Copy(fromFileName, toFileName, true);
            return toFileName;
        }

        private void LoadParam()
        {
            // Verifying the input setting file and css file - in Input Folder
            string settingFile = "DictionaryStyleSettings.xml";
            string sFileName = Common.PathCombine(_outputBasePath, settingFile);
            Common.ProgBase = _outputBasePath;
            Param.LoadValues(sFileName);
            Param.SetLoadType = "Dictionary";
            Param.Value["OutputPath"] = _outputBasePath;
            Param.Value["UserSheetPath"] = _outputBasePath;
        }

        #endregion

    }
}