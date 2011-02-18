// --------------------------------------------------------------------------------------------
// <copyright file="ContentXMLTest.cs" from='2009' to='2009' company='SIL International'>
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
// </remarks>
// --------------------------------------------------------------------------------------------

#region Using
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using System.Windows.Forms;
using SIL.PublishingSolution;
using SIL.Tool;

#endregion Using

namespace Test.OpenOfficeConvert
{
    [TestFixture]
    [Category("BatchTest")]
    public class OOContentXMLTest
    {
        #region Private Variables
        //Styles _styleName;
        //Utility _util;
        string _errorFile;
        private string _inputPath;
        private string _outputPath;
        private string _expectedPath;
        ProgressBar _progressBar;
        private TimeSpan _totalTime;
        private PublicationInformation _projInfo;
        private ValidateXMLFile _validate;
        private string _styleFile;
        private string _contentFile;
        private int _index = 0;
        #endregion Private Variables

        #region SetUp

        //public Utility M_util
        //{
        //    get { return _util; }
        //}

        [TestFixtureSetUp]
        protected void SetUp()
        {
            Common.Testing = true;
            //_styleName = new Styles();
            //_util = new Utility();
            _projInfo = new PublicationInformation();
            _errorFile = Common.PathCombine(Path.GetTempPath(), "temp.odt");
            _progressBar = new ProgressBar();
            string testPath = PathPart.Bin(Environment.CurrentDirectory, "/OpenOfficeConvert/TestFiles");
            _inputPath = Common.PathCombine(testPath, "input");
            _outputPath = Common.PathCombine(testPath, "output");
            _expectedPath = Common.PathCombine(testPath, "expected");
            if (Directory.Exists(_outputPath))
            {
                Directory.Delete(_outputPath, true);
            }
            Directory.CreateDirectory(_outputPath);
            FolderTree.Copy(FileInput("Pictures"), FileOutput("Pictures"));
            _projInfo.ProgressBar = _progressBar;
            _projInfo.OutputExtension = "odm";
            _projInfo.ProjectInputType = "Dictionary";
            Common.SupportFolder = "";
            Common.ProgInstall = PathPart.Bin(Environment.CurrentDirectory, "/../PsSupport");
            _styleFile = "styles.xml";
            _contentFile = "content.xml";
        }
        #endregion

        #region Private Functions
        private string FileInput(string fileName)
        {
            return Common.PathCombine(_inputPath, fileName);
        }

        private string FileOutput(string fileName)
        {
            return Common.PathCombine(_outputPath, fileName);
        }

        private string FileExpected(string fileName)
        {
            return Common.PathCombine(_expectedPath, fileName);
        }

        private void InLineMethod()
        {
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            XmlNode x = _validate.GetOfficeNode();
            bool fail = false;

            int counter = 1;
            string exp;
            string inner;
            foreach (XmlNode item in x.ChildNodes)
            {
                switch (counter)
                {
                    case 3:
                        exp = "locator_dictionary";
                        inner = "parent text div div parent text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    case 4:
                        exp = "locator_locator_dictionary";
                        inner = "parent text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    case 5:
                        exp = "locator_dictionary";
                        inner = "parent text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    case 6:
                        exp = "topara_locator_dictionary";
                        inner = "text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    case 7:
                        exp = "topara_locator_dictionary";
                        inner = "text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    case 8:
                        exp = "locator_dictionary";
                        inner = "parent text";
                        fail = CheckStyleandInnerText(item, exp, inner);
                        break;

                    default:
                        break;

                }
                counter++;
                if (fail)
                {
                    Assert.Fail("InlineBlock Test Failed");
                }
            }
        }

        private bool CheckStyleandInnerText(XmlNode item, string exp, string inner)
        {
            string key = "style-name";
            string ns = "text";

            XmlNode y;
            bool fail = false;
            y = _validate.GetAttibute(item, key, ns);

            if (y.Value != exp)
            {
                fail = true;
            }
            string innerText = _validate.GetReplacedInnerText(item);
            if (!fail && innerText != inner)
            {
                fail = true;
            }
            return fail;
        }

        #endregion PrivateFunctions

        //#region Nodes_Test
        [Test]
        public void NestedDivCase1_Node()
        {

            const string file = "NestedDivCase1";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);


            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "T1 class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t2_t1_body";
            content = "T2 class";

            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t3_t2_t1_body";
            content = "T3 class";

            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);
        }
        [Test]
        public void NestedDivCase2_Node()
        {
            const string file = "NestedDivCase2";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "T1 class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t2_t1_body";
            content = "T2 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t3_t2_t1_body";
            content = "T3 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t4_t1_body";
            content = "T4 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

        }
        [Test]
        public void NestedDivCase3_Node()
        {
            const string file = "NestedDivCase3";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);


            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "T1 class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t3_t1_body";
            content = "T3 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t4_t3_t1_body";
            content = "T4 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);


        }
        [Test]
        public void NestedDivCase4_Node()
        {
            const string file = "NestedDivCase4";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);


            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "T1 class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t2_t1_body";
            content = "T2 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t3_t2_t1_body";
            content = "T3 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t4_t3_t2_t1_body";
            content = "T4 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t4_t2_t1_body";
            content = "T4 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t4_t1_body";
            content = "T4 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

        }

        ///<summary>
        ///em and % test
        /// <summary>
        /// </summary>      
        [Test]
        public void EMTest1_Node()
        {
            const string file = "EMTest1";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letData_body";
            string inner = "letdata <text:span text:style-name=\"slotname_letData_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">slotname </text:span><text:span text:style-name=\"name_letData_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">nameclass </text:span>";
            bool returnValue = _validate.ValidateNodeInnerXmlSubNode(inner);
            Assert.IsTrue(returnValue);
        }

        [Test]
        public void EMTest2_Node()
        {
            const string file = "EMTest2";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);


            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "a_body";
            string inner = "Class A <text:span text:style-name=\"b_a_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">Class B </text:span>";
            bool returnValue = _validate.ValidateNodeInnerXmlSubNode(inner);
            Assert.IsTrue(returnValue);
        }

        [Test]
        public void LetterSpacing_Node()
        {
            const string file = "EMTest3";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "entry_none";
            string inner = "Class A <text:span text:style-name=\"sense_entry_none\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">Class B </text:span>";
            bool returnValue = _validate.ValidateNodeInnerXmlSubNode(inner);
            Assert.IsTrue(returnValue);
        }

        [Test]
        public void Paragraph()
        {
            const string file = "para";
            _projInfo.ProjectInputType = "Scripture";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);

            string xpath = "//style:style[@style:name='p.first_body']/style:paragraph-properties";
            string content = "<style:paragraph-properties fo:padding-top=\"12pt\" fo:border-bottom=\"0.5pt solid #ffffff\" fo:border-top=\"0.5pt solid #ffffff\" fo:border-left=\"0.5pt solid #ffffff\" fo:border-right=\"0.5pt solid #ffffff\" fo:padding-bottom=\"12pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            _validate.GetOuterXml = true;
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference-ref failed");

            xpath = "//style:style[@style:name='p_body']/style:paragraph-properties";
            content = "<style:paragraph-properties fo:padding-top=\"12pt\" fo:border-bottom=\"0.5pt solid #ffffff\" fo:border-top=\"0.5pt solid #ffffff\" fo:border-left=\"0.5pt solid #ffffff\" fo:border-right=\"0.5pt solid #ffffff\" fo:padding-bottom=\"12pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            _validate.GetOuterXml = true;
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference-ref failed");
        }


        ///<summary>
        /// TD-90 .locator .letter
        /// <summary>
        /// </summary>      
        [Test]
        public void AncestorChildTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "AncestorChildTest";
            string styleOutput = GetStyleOutput(file);


            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letter.-locator_locator_dictionary";
            string content = "a";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "letter.-locator_xitem_locator_dictionary";
            content = "c";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "locator_dictionary";
            content = "j orange";
            returnValue1 = _validate.ValidateOfficeTextNode(11, content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "letter.-current_locator_dictionary";
            content = "w";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "letter.-locator_locator_dictionary";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "20pt");
            _validate.ClassProperty.Add("style:font-size-complex", "20pt");
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "letter.-locator_xitem_locator_dictionary";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "20pt");
            _validate.ClassProperty.Add("style:font-size-complex", "20pt");
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "letter.-current_locator_dictionary";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            _validate.ClassProperty.Add("fo:background-color", "#aaff00"); 

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);
        }

        ///<summary>
        /// TD-1525 Period after double quotes
        /// <summary>
        /// </summary>      
        [Test]
        public void PseudoQuotes_Node()
        {
            const string file = "PseudoQuotes";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xsensenumber..before_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "18pt");
            _validate.ClassProperty.Add("style:font-size-complex", "18pt");
            _validate.ClassProperty.Add("fo:color", "#008000");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "pronunciations..after_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "40pt");
            _validate.ClassProperty.Add("style:font-size-complex", "40pt");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


            _validate.ClassName = "translation..before_translations_xitem_examples_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "15pt");
            _validate.ClassProperty.Add("style:font-size-complex", "15pt");
            _validate.ClassProperty.Add("fo:color", "#008000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "translation..after_translations_xitem_examples_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "15pt");
            _validate.ClassProperty.Add("style:font-size-complex", "15pt");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "headword..after_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "50pt");
            _validate.ClassProperty.Add("style:font-size-complex", "50pt");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "headword..before_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "50pt");
            _validate.ClassProperty.Add("style:font-size-complex", "50pt");
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "letHead..before_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "20pt");
            _validate.ClassProperty.Add("style:font-size-complex", "20pt");
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


            _validate.ClassName = "definition..before_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "35pt");
            _validate.ClassProperty.Add("style:font-size-complex", "35pt");
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "definition..after_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "35pt");
            _validate.ClassProperty.Add("style:font-size-complex", "35pt");
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "sense..before_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "18pt");
            _validate.ClassProperty.Add("style:font-size-complex", "18pt");
            _validate.ClassProperty.Add("fo:color", "#000000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


            _validate.ClassName = "sense..after_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("style:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:font-size", "18pt");
            _validate.ClassProperty.Add("style:font-size-complex", "18pt");
            _validate.ClassProperty.Add("fo:color", "#800080");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            //Content Test - 
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "headword..before_entry_letData_dicBody";
            string content = "\"";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "headword..after_entry_letData_dicBody";
            content = "'";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "pronunciations..after_entry_letData_dicBody";
            content = "►";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "sense..before_senses_entry_letData_dicBody";
            content = "' ' ' '► ' '' ' multi singles with uni";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xsensenumber..before_sense_senses_entry_letData_dicBody";
            content = "►";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);


            _validate.ClassName = "definition..before_sense_senses_entry_letData_dicBody";
            content = "&lt;'-'&gt;";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "definition..after_sense_senses_entry_letData_dicBody";
            content = "&lt;\"-\"&gt;";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "translation..before_translations_xitem_examples_sense_senses_entry_letData_dicBody";
            content = "''╥'' two single with uni";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "translation..after_translations_xitem_examples_sense_senses_entry_letData_dicBody";
            content = "\"\"ℕ\"\" two double with uni";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "sense..after_senses_entry_letData_dicBody";
            content = "\" \" \" \"► \" \" \" multi double with uni";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);
        }

        [Test]
        public void PseudoQuotes1_Node()
        {
            const string file = "PseudoQuotes1";

            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "d..before_letHead_letHead1_dicBody";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "d..after_letHead_letHead1_dicBody";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            //Content Test 
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            string content = "\\U8658 A a \". ♦\" B b \". \" C c ' ** ' ** D d ► v \" v ►\"\" \\ ' E e \"♣ F f ♥♠ G g ♫ ";
            XmlNode officeNode = _validate.GetOfficeNode();
            Assert.IsNotNull(officeNode, "Office node is null");
            Assert.AreEqual(content, officeNode.InnerText, "PseudoQuotes1 Test failed");

        }

        ///<summary>
        /// TD-429 -  Handling Anchor Tag 
        /// <summary>
        /// </summary>      
        [Test]
        public void AnchorTag_Node()
        {
            const string file = "AnchorTag";
            _projInfo.ProjectInputType = "Scripture";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            //<text:span text:style-name="a_scrFootnoteMarker_Paragraph_scrSection_scrBook">
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            string xpath = "//text:span[@text:style-name='scrFootnoteMarker_Paragraph_scrSection_scrBook_scrBody']";
            string content = "<text:reference-ref text:reference-format=\"text\" text:ref-name=\"f7be51147-aa97-40a2-ba86-4df84849e9f9\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">a</text:reference-ref>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference-ref failed");
            // ="
            xpath = "//text:span[@text:style-name='scrFootnoteMarker_NoteGeneralParagraph_scrSection_scrBook_scrBody']";
            //content = "<text:reference-mark text:name=\"f7be51147-aa97-40a2-ba86-4df84849e9f9\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />a";
            content = "a";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference - mark failed");

            //TODO  - ANCHOR blue color should be merged
            //Style Test - Second
            //_validate = new ValidateXMLFile(styleOutput);
            //_validate.ClassName = "scrFootnoteMarker_Paragraph_scrSection_scrBook";
            //_validate.ClassProperty.Add("fo:color", "#0000ff");
            //_validate.ClassProperty.Add("style:text-underline-style", "solid");

            //returnValue1 = _validate.ValidateNodeAttributesNS(false);
            //Assert.IsTrue(returnValue1, "AnchorTag - Style property Failure");
        }

        [Test]
        public void CrossReference_Node()
        {
            const string file = "CrossRef";
            _projInfo.ProjectInputType = "Dictioanry";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            string xpath = "//text:span[@text:style-name='link_entry_letData_dicBody']";
            string content = "<text:reference-ref text:reference-format=\"text\" text:ref-name=\"nema1\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">nema1 source</text:reference-ref>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference-ref failed");

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            xpath = "//text:span[@text:style-name='headword_.bzh_a_entry_letData_dicBody']";
            content = "<text:reference-mark text:name=\"nema3\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />nema3 text";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "reference-ref failed");
        }


        ///<summary>
        /// TD-91 .letter[class~='current']
        /// <summary>
        /// </summary>      
        [Test]
        public void ClassNameValueTest_Node()
        {

            const string file = "ClassNameValueTest";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "letter.-current_locator_dictionary";

            _validate.ClassProperty.Add("fo:color", "#ff0000");
            _validate.ClassProperty.Add("fo:background-color","#aaff00");
            bool returnValue =_validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letter.-current_locator_dictionary";
            string content = "w";
            bool returnValue1 =_validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);
        }

        ///<summary>
        /// TD-1607. main.odm format - percentage test
        /// <summary>
        /// </summary>      
        [Test]
        public void ColumnGapPercent()
        {
            const string file = "ColumnGapPercent";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;

            string xpath = "//style:style[@style:name='Sect_letData']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letData']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "2");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#aa0000");
            _validate.ClassProperty.Add("style:width", "1pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);


            _validate.ClassProperty.Add("style:rel-width", "4*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.125in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "4*");
            _validate.ClassProperty.Add("fo:start-indent", "0.125in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            ////////////////////////////////
            /// 
            xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);


            xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "3");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#0000ff");
            _validate.ClassProperty.Add("style:width", "6pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);


            _validate.ClassProperty.Add("style:rel-width", "2.5*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.1666667in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.5*");
            _validate.ClassProperty.Add("fo:start-indent", "0.1666667in");
            _validate.ClassProperty.Add("fo:end-indent", "0.1666667in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.5*");
            _validate.ClassProperty.Add("fo:start-indent", "0.1666667in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(4, xpath);
            Assert.IsTrue(returnValue);

        }

        ///<summary>
        /// TD-83. Open Office column-gap: 1.5em;  
        /// <summary>
        /// </summary>      
        [Test]
        public void ColumnGap_Node()
        {
            const string file = "ColumnGap";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;

            string xpath = "//style:style[@style:name='Sect_letData']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letData']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "2");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#ff0000");
            _validate.ClassProperty.Add("style:width", "10pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);


            _validate.ClassProperty.Add("style:rel-width", "4*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.125in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "4*");
            _validate.ClassProperty.Add("fo:start-indent", "0.125in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_xitem']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "true");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);


            xpath = "//style:style[@style:name='Sect_xitem']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "3");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#0000ff");
            _validate.ClassProperty.Add("style:width", "6pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.333333*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.25in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.333333*");
            _validate.ClassProperty.Add("fo:start-indent", "0.25in");
            _validate.ClassProperty.Add("fo:end-indent", "0.25in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.333333*");
            _validate.ClassProperty.Add("fo:start-indent", "0.25in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(4, xpath);
            Assert.IsTrue(returnValue);

        }

        ///<summary>
        /// </summary>  
        [Test]    
        public void ColumnGapLong()
        {
            const string file = "ColumnGapLong";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;

            string xpath = "//style:style[@style:name='Sect_letData']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letData']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "2");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#aa0000");
            _validate.ClassProperty.Add("style:width", "1pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);


            _validate.ClassProperty.Add("style:rel-width", "4.24*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.005in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "4.24*");
            _validate.ClassProperty.Add("fo:start-indent", "0.005in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            ////////////////////////////////
            /// 
            xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "3");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:color", "#aa0000");
            _validate.ClassProperty.Add("style:width", "1pt");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);


            _validate.ClassProperty.Add("style:rel-width", "2.388889*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.2222222in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.388889*");
            _validate.ClassProperty.Add("fo:start-indent", "0.2222222in");
            _validate.ClassProperty.Add("fo:end-indent", "0.2222222in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "2.388889*");
            _validate.ClassProperty.Add("fo:start-indent", "0.2222222in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(4, xpath);
            Assert.IsTrue(returnValue);
            
        }

        ///<summary>
        /// TD-170 Open Office: triangle should appear before all senses
        /// TD-171 Open Office: sense # is wrong font size
        /// TD-172 open office: visibility  
        /// <summary>
        /// </summary>      
        [Test]
        public void Visibility_Node()
        {
            const string file = "VisibilityTest";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xsensenumber..before_sense_senses_entry_letData_dicBody";
            string content = "►";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xsensenumber_sense_senses_entry_letData_dicBody";
            content = "<text:s text:c=\"2\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />";
            _validate.ClassNameTrim = false;
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "VisibilityTest - Content 1 Failure");


            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xsensenumber_sense_senses_entry_letData_dicBody";

            _validate.ClassProperty.Add("fo:font-size", "2pt");
            _validate.ClassProperty.Add("fo:font-size-complex", "2pt");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "VisibilityTest - Style Failure");

            _validate.ClassName = "xsensenumber..before_sense_senses_entry_letData_dicBody";

            _validate.ClassProperty.Add("fo:font-size", "8pt");
            _validate.ClassProperty.Add("fo:font-size-complex", "8pt");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "VisibilityTest - Style Failure");
        }

        ///<summary>
        /// </summary>      
        [Test]
        public void Counter1_Node()
        {
            const string file = "Counter1";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";
            _validate.GetInnerText = true;
            string content = "1.2";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1,content, "span");
            Assert.IsTrue(returnValue1, "Counter1 - Content Failure");

            content = "1.4";
            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "span");
            Assert.IsTrue(returnValue1, "Counter1 - Content Failure");

            content = "2.2";
            returnValue1 = _validate.ValidateOfficeTextNodeList(3, content, "span");
            Assert.IsTrue(returnValue1, "Counter1 - Content Failure");

            content = "2.4";
            returnValue1 = _validate.ValidateOfficeTextNodeList(4, content, "span");
            Assert.IsTrue(returnValue1, "Counter1 - Content Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";

            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("fo:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:color", "#ff0000");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "Counter1 - Style Failure");
        }

        ///<summary>
        /// </summary>      
        [Test]
        public void Counter2_Node()
        {
            const string file = "Counter2";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);
            
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";
            _validate.GetInnerText = true;
            string content = "1.0.0";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1, content, "span");
            Assert.IsTrue(returnValue1, "Counter2 - Content Failure");

            content = "1.0.0";
            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "span");
            Assert.IsTrue(returnValue1, "Counter2 - Content Failure");

            content = "2.0.4";
            returnValue1 = _validate.ValidateOfficeTextNodeList(3, content, "span");
            Assert.IsTrue(returnValue1, "Counter2 - Content Failure");

            content = "2.0.5";
            returnValue1 = _validate.ValidateOfficeTextNodeList(4, content, "span");
            Assert.IsTrue(returnValue1, "Counter2 - Content Failure");

            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";

            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("fo:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:color", "#ff0000");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "Counter2 - Style Failure");
        }

        ///<summary>
        /// </summary>      
        [Test]
        public void Counter3_Node()
        {
            const string file = "Counter3";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";
            _validate.GetInnerText = true;
            string content = "1.1";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1, content, "span");
            Assert.IsTrue(returnValue1, "Counter3 - Content Failure");

            content = "1.2";
            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "span");
            Assert.IsTrue(returnValue1, "Counter3 - Content Failure");

            content = "2.1";
            returnValue1 = _validate.ValidateOfficeTextNodeList(3, content, "span");
            Assert.IsTrue(returnValue1, "Counter3 - Content Failure");

            content = "2.2";
            returnValue1 = _validate.ValidateOfficeTextNodeList(4, content, "span");
            Assert.IsTrue(returnValue1, "Counter3 - Content Failure");

            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "sense..before_article_sectionletter_dictionary";

            _validate.ClassProperty.Add("fo:font-weight", "700");
            _validate.ClassProperty.Add("fo:font-weight-complex", "700");
            _validate.ClassProperty.Add("fo:color", "#ff0000");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "Counter3 - Style Failure");
        }

        ///<summary>
        /// </summary>      
        [Test]
        public void FootNote_Node()
        {
            const string file = "Footnote";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            string xpath = "//text:note[@text:id='ftn1']";
            _validate.ClassName = string.Empty;
            _validate.GetOuterXml = true;
            string content = "<text:note text:id=\"ftn1\" text:note-class=\"footnote\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:note-citation text:label=\"‡ \">‡ </text:note-citation><text:note-body><text:p text:style-name=\"footnote\"><text:span text:style-name=\"footnote_p.first_section_div.scriptureText_scrBody\">1:1: Lii padhai ‘Ana Ama Lamatua’ abhu boe tu dara sasuri uuru sèra.</text:span></text:p></text:note-body></text:note>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "FootNote - Content Failure");

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "footnote..footnote-call";
            _validate.ClassProperty.Add("fo:color", "#800080");
            _validate.ClassProperty.Add("fo:font-size", "6pt");
            _validate.ClassProperty.Add("style:text-position", "super");
            _validate.ClassProperty.Add("text:display", "prince-footnote");
            _validate.ClassProperty.Add("fo:font-family", "Arial");
            _validate.ClassProperty.Add("fo:font-weight", "400");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "Footnote cal - Style Failure");

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "footnote..footnote-marker";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            _validate.ClassProperty.Add("fo:font-size", "10pt");
            _validate.ClassProperty.Add("text:display", "prince-footnote");
            _validate.ClassProperty.Add("fo:font-family", "Arial");
            _validate.ClassProperty.Add("fo:font-weight", "700");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "Footnote Marker - Style Failure");
        }

        ///<summary>
        /// TD-1909
        /// </summary>      
        [Test]
        public void FootnoteSpanContent_Node()
        {
            const string file = "FootnoteSpanContent";
            _projInfo.ProjectInputType = "Scripture";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            string xpath = "//text:note/text:note-body/text:p[@text:style-name='NoteGeneralParagraph']";
            _validate.ClassName = string.Empty;
            _validate.GetOuterXml = true;
            string content = "<text:p text:style-name=\"NoteGeneralParagraph\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"NoteTargetReference_NoteGeneralParagraph_Paragraph_scrSection_columns_scrBook_scrBody\">21:1 </text:span><text:span text:style-name=\"AlternateReading_NoteGeneralParagraph_Paragraph_scrSection_columns_scrBook_scrBody\">Nfɔ-nyíbʋ </text:span><text:span text:style-name=\"span_.zxx_NoteGeneralParagraph_Paragraph_scrSection_columns_scrBook_scrBody\">igyi obubwí kʋá ɩbʋ mantáa Yerusalem, bʋtɛtɩ́ mʋ́ Olifbʋ.</text:span></text:p>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "FootNote - Content Failure");
        }
 
        [Test]
        public void FootNoteFormat_Node()
        {
            const string file = "FootNoteFormat";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);
            //text:n ote text:id="ftn1"
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            string xpath = "//text:note[@text:id='ftn1']";
            _validate.ClassName = string.Empty;
            _validate.GetOuterXml = true;

            string content = "<text:note text:id=\"ftn1\" text:note-class=\"footnote\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:note-citation text:label=\"a \">a </text:note-citation><text:note-body><text:p text:style-name=\"NoteGeneralParagraph\">1:1 = <text:span text:style-name=\"AlternateReading_NoteGeneralParagraph_Paragraph_scrSection_scrBook_scrBody\">Les juges:</text:span><text:span text:style-name=\"span_.x-kal_NoteGeneralParagraph_Paragraph_scrSection_scrBook_scrBody\">À une époque de leur histoire, les Israélites ont été dirigés par des              juges. C'étaient des personnes envoyées par Dieu. Dieu les chargeait plus              particulièrement de délivrer une ou plusieurs tribus en guerre et de diriger le              peuple. Ils rendaient aussi la justice. </text:span><text:span text:style-name=\"AlternateReading_NoteGeneralParagraph_Paragraph_scrSection_scrBook_scrBody\">Moab:</text:span><text:span text:style-name=\"span_.x-kal_NoteGeneralParagraph_Paragraph_scrSection_scrBook_scrBody\">pays fertile situé à l'est de la mer Morte.</text:span></text:p></text:note-body></text:note>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath,content);
            Assert.IsTrue(returnValue1, "FootNoteFormat - Content Failure");
        }

        ///<summary>
        ///TD81 line-height: always syntax in Styles.xml
        /// <summary>
        /// </summary>      
        [Test]
        public void LineHeight_Node()
        {
            const string file = "LineHeight";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            _validate.ClassNameTrim = false;
            string content = "Text t1";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 1 Failure");

            _validate.ClassName = "inner1_t1_body";
            _validate.ClassNameTrim = false;
            content = "Text Inner1";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 2 Failure");

            _validate.ClassName = "inner2_t1_body";
            _validate.ClassNameTrim = false;
            content = "Text Inner2";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 3 Failure");

            _validate.ClassName = "inner3_t1_body";
            _validate.ClassNameTrim = false;
            content = "Text Inner3";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 4 Failure");

            _validate.ClassName = "inner4_t1_body";
            _validate.ClassNameTrim = false;
            content = "Text Inner4";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 5 Failure");

            _validate.ClassName = "noheight_t1_body";
            _validate.ClassNameTrim = false;
            content = "Line Height is None";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "LineHeight - Content 6 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "inner1_t1_body";
            _validate.ClassProperty.Add("style:line-spacing", "12pt");
            bool returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue, "LineHeight - Style 1 Failure");

            _validate.ClassProperty.Add("fo:font-size", "24pt");
            _validate.ClassProperty.Add("fo:font-size-complex", "24pt");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "LineHeight - Style 1a Failure");

            _validate.ClassName = "inner2_t1_body";
            _validate.ClassProperty.Add("style:line-spacing", "36pt");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue, "LineHeight - Style 2 Failure");

            _validate.ClassName = "inner3_t1_body";
            _validate.ClassProperty.Add("style:line-spacing", "24pt");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue, "LineHeight - Style 2 Failure");

            _validate.ClassProperty.Add("fo:font-size", "24pt");
            _validate.ClassProperty.Add("fo:font-size-complex", "24pt");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "LineHeight - Style 1a Failure");

            _validate.ClassName = "inner4_t1_body";
            _validate.ClassProperty.Add("style:line-spacing", "36pt");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue, "LineHeight - Style 2 Failure");

            _validate.ClassName = "noheight_t1_body";
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "LineHeight - Style 2 Failure");
        }

        ///<summary>
        ///TD-181 White-space:pre;
        /// <summary>
        /// </summary>      
        [Test]
        public void Whitespace_Node()
        {
            const string file = "Whitespace";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            //Note: without zero it should come. content is correct but same style (space) should come
            _validate.ClassName = "space0";
            _validate.ClassNameTrim = true;
            string content = "Data";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Whitespace - Content 2 Failure");

            _validate.ClassName = "nowrap_body";
            _validate.ClassNameTrim = true;
            content = "Goo (programming language), a programming language in the Lisp family";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Whitespace - Content 3 Failure");

            _validate.ClassName = "normal_body";
            _validate.ClassNameTrim = true;
            content = "Goo (programming language), a programming language in the Lisp family";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Whitespace - Content 4 Failure");

            _validate.ClassName = "space_body";
            _validate.ClassNameTrim = false;
            content = "This<text:s text:c=\"1\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />is<text:s text:c=\"1\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />a<text:s text:c=\"11\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" />sample\r";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Whitespace - Content 1 Failure");

        }

        ///<summary>
        /// TD-1776 TeluguGondwana_
        /// <summary>
        /// </summary>      
        [Test]
        public void TeluguGondwana_Node()
        {
            const string file = "TeluguGondwana";

            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "span_.ggo-Telu-IN_TitleMain_scrBook_scrBody";
            string content = "ఆదికాండము / గడెమాయ్వల్";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);


            ////Note - The Styles will be created while processing xhtml(content.xml)
            /// //Commented because of ldml files.
            ////Style Test - Second
            //_validate = new ValidateXMLFile(styleOutput);
            //_validate.ClassName = "span_.ggo-Telu-IN_TitleMain_scrBook_scrBody";
            //_validate.ClassProperty.Add("fo:font-name", "Gautami");
            //_validate.ClassProperty.Add("style:font-name-complex", "Gautami");
            //bool returnValue = _validate.ValidateNodeAttributesNS(false);
            //Assert.IsTrue(returnValue);

            //string xpath = "//office:font-face-decls";
            //_validate.ClassName = string.Empty;
            //_validate.ClassProperty.Add("svg:font-family", "Gautami");
            //returnValue = _validate.ValidateNodeAttributesNS(9, xpath);
            //Assert.IsTrue(returnValue);
        }
        ///<summary>
        ///TD130 (Remove AutoWidth and set Column Width for columns)
        /// <summary>
        /// .letData { column-fill: balance; }
        /// </summary>      
        [Test]
        public void RemoveAutoWidth_Node()
        {
            const string file = "RemoveAutoWidth";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            string xpath = "//style:style[@style:name='Sect_letData']";
            _validate.ClassName = string.Empty;
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);
        }


        ///<summary>
        ///TD130 (Remove AutoWidth and set Column Width for columns)
        /// <summary>
        /// </summary>      
        [Test]
        public void InlineBlock_Node()
        {
            const string file = "InlineBlock";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            InLineMethod();
        }


        ///<summary>
        ///Test pink and blue in output
        /// <summary>
        /// </summary>      
        [Test]
        public void LanguageColor_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "LanguageColor";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xlanguagetag_xitem_.pt_definitionL2_sense_senses_entry_letData_dicBody";
            _validate.ClassNameTrim = false;
            string content = "Por ";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "LanguageColor - Content 1 Failure");

            _validate.ClassName = "xitem_.en_definitionL2_sense_senses_entry_letData_dicBody";
            _validate.ClassNameTrim = true;
            content = "he";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "LanguageColor - Content 2 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xlanguagetag_xitem_.pt_definitionL2_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:color", "#2F60FF");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "LanguageColor - Style Failure");

            _validate.ClassName = "xitem_.en_definitionL2_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:color", "#FF00FF");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "LanguageColor - Style Failure");

        }

        /// <summary>
        /// </summary>      
        [Test]
        public void PseudoAfter_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PseudoAfter";

            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "headword..after_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:font-size", "12pt");
            _validate.ClassProperty.Add("style:font-size-complex", "12pt");
            _validate.ClassProperty.Add("fo:color", "#000000");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            //Content Test 
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "entry_letData_dicBody";
            string content = "<text:span text:style-name=\"headword_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">anon</text:span><text:span text:style-name=\"headword..after_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"></text:span><text:span text:style-name=\"pronunciation_pronunciations_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">a.ˈnon</text:span><text:span text:style-name=\"pronunciation..after_pronunciations_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">]</text:span><text:span text:style-name=\"pronunciations..after_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"></text:span>";

            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);
        }


        [Test]
        public void LanguageInlineBlock_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "LanguageInline";
            string styleOutput = GetStyleOutput(file);

            //Content Test 
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "partofspeech_entry_letData_dicBody";
            string content = "Select a destination";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1,content,"para");
            Assert.IsTrue(returnValue1);

            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "para");
            Assert.IsTrue(returnValue1);

            returnValue1 = _validate.ValidateOfficeTextNodeList(3, content, "para");
            Assert.IsTrue(returnValue1);
        }


        /// <summary>
        /// </summary>      
        [Test]
        public void PseudoAfter1_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PseudoAfter1";

            string styleOutput = GetStyleOutput(file);
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "letHead..after_div_dicBody";
            _validate.ClassProperty.Add("fo:font-size", "18pt");
            _validate.ClassProperty.Add("style:font-size-complex", "18pt");
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "letHead-letHead..after_div_dicBody";
            _validate.ClassProperty.Add("fo:font-size", "25pt");
            _validate.ClassProperty.Add("style:font-size-complex", "25pt");
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            //Content Test 
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letHead_div_dicBody";
            string content = "A a<text:span text:style-name=\"entry_letHead_div_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">entry</text:span><text:span text:style-name=\"letHead..after_div_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">###</text:span>";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1, content, "para");
            Assert.IsTrue(returnValue1, "PseudoAfter1 - Content Failure");

            content = "B b<text:span text:style-name=\"letHead-letHead..after_div_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">***</text:span>";
            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "para");
            Assert.IsTrue(returnValue1, "PseudoAfter1 - 2nd Content Failure");

        }

        ///<summary>
        /// TD-59  font-size: larger; and font-size: smaller;
        /// <summary>
        /// </summary>      
        [Test]
        public void Larger_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "Larger";

            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "p4_body";
            _validate.ClassNameTrim = true;
            string content = "<text:span text:style-name=\"child1_p4_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"> text </text:span><text:span text:style-name=\"child2_p4_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">text </text:span>";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Larger - Content 1 Failure");

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "child1_p4_body";
            _validate.ClassProperty.Add("fo:font-size", "120");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "child2_p4_body";
            _validate.ClassProperty.Add("fo:font-size", "53");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);
        }

        ///<summary>
        /// TD-222   Image width:50%
        /// <summary>
        /// </summary>      
        [Test]
        public void Picture_Width_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PictureWidth";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "entry1_letData_dicBody";
            _validate.ClassNameTrim = true;
            _validate.GetOuterXml = true;
            string content = "<text:p text:style-name=\"entry1_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><draw:frame draw:style-name=\"Graphics1\" draw:name=\"Graphics1\" text:anchor-type=\"paragraph\" draw:z-index=\"1\" svg:width=\"25%\" svg:height=\"17.8051500320435pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:text-box fo:min-height=\"0in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><draw:frame draw:style-name=\"Graphics1\" draw:name=\"Graphics1\" text:anchor-type=\"paragraph\" svg:width=\"100%\" svg:height=\"17.8051500320435pt\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/leftindexmacro1.jpg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>cala</svg:desc></draw:frame><text:p text:style-name=\"pictureCaption_pictureRight_entry1_letData_dicBody\"><text:span text:style-name=\"CmPicturepublishStemCaptionSenseNumber_pictureCaption_pictureRight_entry1_letData_dicBody\">1</text:span><text:span text:style-name=\"CmPicturepublishStemCaptionCaptionPub_.pt_pictureCaption_pictureRight_entry1_letData_dicBody\">cala</text:span></text:p></draw:text-box></draw:frame></text:p>";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "PictureWidth - Content 1 Failure");

            // Display:None
            //_validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            //_validate.ClassName = "pictureRightNone_entry2_letData_dicBody";
            //_validate.ClassNameTrim = true;
            //_validate.GetInnerText = true;
            //content = "cadu";
            //returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            //Assert.IsTrue(returnValue1, "PictureWidth - Content 2 Failure");
        }

        ///<summary>
        /// TD-1855 Picture / PictureBox error
        /// <summary>
        /// </summary>      
        [Test]
        public void Picture_Mrk_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "mrk";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "section_scriptureText_body";
            _validate.ClassNameTrim = true;
            _validate.GetOuterXml = true;
            string content = "<text:p text:style-name=\"section_scriptureText_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" draw:z-index=\"1\" svg:width=\"72pt\" svg:height=\"72pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:text-box fo:min-height=\"0in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" svg:width=\"72pt\" svg:height=\"72pt\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/lb00296c.png\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>Yohanis, Tuka Sarani Dhèu (Mark.1.4-6)</svg:desc></draw:frame><text:p text:style-name=\"div.caption_img_section_scriptureText_body\">Yohanis, Tuka Sarani Dhèu (Mark.1.4-6)</text:p></draw:text-box></draw:frame></text:p>";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "Picture_Mrk - Content 1 Failure");
        }

        ///<summary>
        /// TD-1855 Picture / PictureBox error
        /// <summary>
        /// </summary>      
        [Test]
        public void ImageNoCaption_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ImageNoCaption";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            _validate.GetOuterXml = true;
            _validate.XPath = "//draw:frame[@draw:style-name='Graphics2']";
            string content = "<draw:frame draw:style-name=\"Graphics2\" draw:name=\"Graphics2\" text:anchor-type=\"paragraph\" svg:width=\"72pt\" svg:height=\"72pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/c.jpg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>c.jpg</svg:desc></draw:frame>";
            bool returnValue = _validate.ValidateOfficeTextNode(0, content, "para");
            Assert.IsTrue(returnValue);
        }
        ///<summary>
        /// TD-227 Set language for data. 
        /// <summary>
        /// </summary>      
        [Test]
        [Category("SkipOnTeamCity")]
        public void Language_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "Language";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xitem_.en_main_body";
            string content = "sample text file";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem_.fr_main_body";
            content = "exemple de fichier texte";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem_.es_main_body";
            content = "Este es muestra texto";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem_.fr-FR_main_body";
            content = "exemple de fichier texte";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem_.es-ES_main_body";
            content = "Este es muestra texto";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);


            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xitem_.en_main_body";
            _validate.ClassProperty.Add("fo:language", "en");
            _validate.ClassProperty.Add("fo:country", "US");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem_.fr_main_body";
            _validate.ClassProperty.Add("fo:language", "fr");
            _validate.ClassProperty.Add("fo:country", "FR");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem_.es_main_body";
            _validate.ClassProperty.Add("fo:language", "es");
            _validate.ClassProperty.Add("fo:country", "ES");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem_.fr-FR_main_body";
            _validate.ClassProperty.Add("fo:language", "fr");
            _validate.ClassProperty.Add("fo:country", "FR");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem_.es-ES_main_body";
            _validate.ClassProperty.Add("fo:language", "es");
            _validate.ClassProperty.Add("fo:country", "ES");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

        }


        ///<summary>
        /// TD-204   unable to put tok / pisin. 
        /// <summary>
        /// </summary>      
        [Test]
        [Ignore]
        public void ClassContent_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ClassContent";

            string styleOutput = GetStyleOutput(file);
            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xlanguagetag_.en_xitem_.tpi_definition_.en_sense_senses_entry_letData";
            _validate.ClassNameTrim = true;
            string content = "/";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "ClassContent - Content 1 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xlanguagetag_.en_xitem_.tpi_definition_.en_sense_senses_entry_letData";
            _validate.ClassProperty.Add("fo:font-size", "38pt");
            _validate.ClassProperty.Add("style:font-size-complex", "38pt");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "ClassContent - Style 1 Failure");
        }


        ///<summary>
        /// TD-304   Open Office div.header
        /// <summary>
        /// </summary>      
        [Test]
        public void div_header_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "div_header";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "div.header_main";
            _validate.ClassNameTrim = true;
            string content = "Main Title";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "div_header - Content 1 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "div.header_main";
            _validate.ClassProperty.Add("fo:font-size", "24pt");
            _validate.ClassProperty.Add("style:font-size-complex", "24pt");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "div_header - Style 1 Failure");
        }


        ///<summary>
        /// TD-347  Implement content: normal;  
        /// <summary>
        /// </summary>      
        [Test]
        public void ContentNormalTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ContentNormalTest";

            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "entry_letData_dicBody";
            _validate.ClassNameTrim = false;
            _validate.GetInnerText = true;
            string content = "ba VPor roubarEng stealmwana wa FátimaPor criança de FátimaEng child of FatimaPor criança de FátimaEng child of Fatima11Por criança de FátimaPor criança de FátimaEng child of FatimaPor criança de FátimaPor criança de FátimaEng child of Fatima";

            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "ContentNormalTest - Content 1 Failure");

        }

        ///<summary>
        /// TD-343 Implement lists
        /// <summary>
        /// </summary>      
        [Test]
        public void ListOlUl_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ListOlUl";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            _validate.ClassNameTrim = false;
            string content = "one1 ";
            string xpath = "//text:list[@text:style-name='ol.a2']/text:list-item/text:p[@text:style-name='li.ol_ol.a2_section_body']";
            _validate.GetInnerText = true;
            bool returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a3']/text:list-item/text:p[@text:style-name='li.ol_ol.a3_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a4']/text:list-item/text:p[@text:style-name='li.ol_ol.a4_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a5']/text:list-item/text:p[@text:style-name='li.ol_ol.a5_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a6']/text:list-item/text:p[@text:style-name='li.ol_ol.a6_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a7']/text:list-item/text:p[@text:style-name='li.ol_ol.a7_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a8']/text:list-item/text:p[@text:style-name='li.ol_ol.a8_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            xpath = "//text:list[@text:style-name='ol.a9']/text:list-item/text:p[@text:style-name='li.ol_ol.a9_section_body']";
            _validate.GetInnerText = true;
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Content 1 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = string.Empty;
            content = "<text:list-level-style-bullet text:level=\"1\" text:style-name=\"Bullet_20_Symbols\" style:num-suffix=\".\" text:bullet-char=\"•\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-bullet>";
            xpath = "//text:list-style[@style:name='ol.a2']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-bullet text:level=\"1\" text:style-name=\"Bullet_20_Symbols\" style:num-suffix=\".\" text:bullet-char=\"◦\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-bullet>";
            xpath = "//text:list-style[@style:name='ol.a3']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-bullet text:level=\"1\" text:style-name=\"Bullet_20_Symbols\" style:num-suffix=\".\" text:bullet-char=\"▪\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-bullet>";
            xpath = "//text:list-style[@style:name='ol.a4']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-number text:level=\"1\" text:style-name=\"Numbering_20_Symbols\" style:num-suffix=\".\" style:num-format=\"1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-number>";
            xpath = "//text:list-style[@style:name='ol.a5']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-number text:level=\"1\" text:style-name=\"Numbering_20_Symbols\" style:num-suffix=\".\" style:num-format=\"i\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-number>";
            xpath = "//text:list-style[@style:name='ol.a6']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-number text:level=\"1\" text:style-name=\"Numbering_20_Symbols\" style:num-suffix=\".\" style:num-format=\"I\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-number>";
            xpath = "//text:list-style[@style:name='ol.a7']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-number text:level=\"1\" text:style-name=\"Numbering_20_Symbols\" style:num-suffix=\".\" style:num-format=\"a\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-number>";
            xpath = "//text:list-style[@style:name='ol.a8']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");

            content = "<text:list-level-style-number text:level=\"1\" text:style-name=\"Numbering_20_Symbols\" style:num-suffix=\".\" style:num-format=\"A\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><style:list-level-properties text:list-level-position-and-space-mode=\"label-alignment\"><style:list-level-label-alignment text:label-followed-by=\"listtab\" text:list-tab-stop-position=\"0.5in\" fo:text-indent=\"-0.25in\" fo:margin-left=\"0.5in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:list-level-properties></text:list-level-style-number>";
            xpath = "//text:list-style[@style:name='ol.a9']";
            returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue, "ListOlUl - Style 1 Failure");
        }

        ///<summary>
        /// TD-362 Image Attribute
        /// <example img[src='Thomsons-gazelle1.jpg'] { float:left;}>
        /// <summary>
        /// </summary>      
        [Test]
        public void Picture_ImageAttrib_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ImageAttrib";

            _projInfo.DefaultCssFileWithPath = FileInput(file + ".css");
            _projInfo.DefaultXhtmlFileWithPath = FileInput(file + ".xhtml");

            PreExportProcess preProcessor = new PreExportProcess(_projInfo);
            preProcessor.GetTempFolderPath();
            preProcessor.ImagePreprocess();
            _projInfo.DefaultXhtmlFileWithPath = preProcessor.ProcessedXhtml;

            string styleOutput = GetStyleOutput(_projInfo);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            _validate.GetOuterXml = true;
            _validate.XPath = "//draw:frame[@draw:style-name='Graphics0']";
            string content = "<draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" draw:z-index=\"1\" svg:width=\"288pt\" svg:height=\"144pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:text-box fo:min-height=\"0in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" svg:width=\"288pt\" svg:height=\"144pt\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/1.jpg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>a waitress</svg:desc></draw:frame><text:p text:style-name=\"ForcedDiv\"><text:span text:style-name=\"image_firstoftype sense_article_sectionletter_dictionary\"> Flea </text:span></text:p></draw:text-box></draw:frame>";
            bool returnValue = _validate.ValidateOfficeTextNode(0, content, "para");
            Assert.IsTrue(returnValue);

            _validate.XPath = "//draw:frame[@draw:style-name='Graphics1']";
            content = "<draw:frame draw:style-name=\"Graphics1\" draw:name=\"Graphics1\" text:anchor-type=\"paragraph\" draw:z-index=\"1\" svg:width=\"72pt\" svg:height=\"72pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:text-box fo:min-height=\"0in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><draw:frame draw:style-name=\"Graphics1\" draw:name=\"Graphics1\" text:anchor-type=\"paragraph\" svg:width=\"72pt\" svg:height=\"72pt\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/2.jpg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>Thomsons-gazelle1.jpg</svg:desc></draw:frame><text:p text:style-name=\"ForcedDiv\"><text:span text:style-name=\"image_article_sectionletter_dictionary\"> Gazelle </text:span></text:p></draw:text-box></draw:frame>";
            returnValue = _validate.ValidateOfficeTextNode(0, content, "para");
            Assert.IsTrue(returnValue);

            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "Graphics0";

            _validate.ClassProperty.Add("style:vertical-pos", "from-top");
            _validate.ClassProperty.Add("style:vertical-rel", "paragraph");
            _validate.ClassProperty.Add("style:horizontal-pos", "left");
            _validate.ClassProperty.Add("style:horizontal-rel", "paragraph");

            _validate.ClassProperty.Add("run-through", "foreground");
            _validate.ClassProperty.Add("number-wrapped-paragraphs", "no-limit");
            _validate.ClassProperty.Add("wrap-contour", "false");

            _validate.ClassProperty.Add("text:anchor-type", "paragraph");
            _validate.ClassProperty.Add("svg:x", "0in");
            _validate.ClassProperty.Add("svg:y", "0in");
            _validate.ClassProperty.Add("style:mirror", "none");
            _validate.ClassProperty.Add("fo:clip", "rect(0in 0in 0in 0in)");
            _validate.ClassProperty.Add("draw:luminance", "0%");

            _validate.ClassProperty.Add("draw:red", "0%");
            _validate.ClassProperty.Add("draw:green", "0%");
            _validate.ClassProperty.Add("draw:blue", "0%");
            _validate.ClassProperty.Add("draw:gamma", "100%");
            _validate.ClassProperty.Add("draw:color-inversion", "false");

            _validate.ClassProperty.Add("draw:image-opacity", "100%");
            _validate.ClassProperty.Add("draw:color-mode", "standard");
            _validate.ClassProperty.Add("style:wrap", "dynamic");

            returnValue = _validate.ValidateNodeAttributesNS(1, string.Empty);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "Graphics1";
            _validate.ClassProperty.Add("style:vertical-pos", "from-top");
            _validate.ClassProperty.Add("style:vertical-rel", "paragraph");
            _validate.ClassProperty.Add("style:horizontal-pos", "right");
            _validate.ClassProperty.Add("style:horizontal-rel", "paragraph");

            _validate.ClassProperty.Add("run-through", "foreground");
            _validate.ClassProperty.Add("number-wrapped-paragraphs", "no-limit");
            _validate.ClassProperty.Add("wrap-contour", "false");

            _validate.ClassProperty.Add("text:anchor-type", "paragraph");
            _validate.ClassProperty.Add("svg:x", "0in");
            _validate.ClassProperty.Add("svg:y", "0in");
            _validate.ClassProperty.Add("style:mirror", "none");
            _validate.ClassProperty.Add("fo:clip", "rect(0in 0in 0in 0in)");
            _validate.ClassProperty.Add("draw:luminance", "0%");

            _validate.ClassProperty.Add("draw:red", "0%");
            _validate.ClassProperty.Add("draw:green", "0%");
            _validate.ClassProperty.Add("draw:blue", "0%");
            _validate.ClassProperty.Add("draw:gamma", "100%");
            _validate.ClassProperty.Add("draw:color-inversion", "false");

            _validate.ClassProperty.Add("draw:image-opacity", "100%");
            _validate.ClassProperty.Add("draw:color-mode", "standard");
            _validate.ClassProperty.Add("style:wrap", "dynamic");

            returnValue = _validate.ValidateNodeAttributesNS(1, string.Empty);
            Assert.IsTrue(returnValue);
        }


        ///<summary>
        /// TD-416 chapter number should align with top of text not with bottom
        /// <summary>
        /// </summary>      
        [Test]
        public void DropCap_Node()
        {
            const string file = "DropCap";
            _projInfo.ProjectInputType = "Scripture";

            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "ChapterNumber1";
            _validate.ChildClassName = "ChapterNumber_Paragraph_scrSection_scrBook_scrBody";
            _validate.ChildClassType = "span";
            string content = "1";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            ////Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            string xpath = "//style:style[@style:name='ChapterNumber1']";
            _validate.ClassName = string.Empty;
            string inner =
                "<style:paragraph-properties xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\">" +
                "<style:drop-cap style:lines=\"2\" style:distance=\"0.20cm\" style:length=\"1\" />" +
                "</style:paragraph-properties>";

            bool returnValue = _validate.ValidateNodeInnerXml(xpath, inner);
            Assert.IsTrue(returnValue);
        }

        ///<summary>
        /// TD-453 - Index doesn't have three columns in this data
        /// <summary>
        /// </summary>      
        [Test]
        public void ColumnCountTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ColumnCount";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;

            string xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letHead']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "1");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letData']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "true");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_letData']/style:section-properties/style:columns";
            _validate.ClassProperty.Add("fo:column-count", "4");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "1.875*");
            _validate.ClassProperty.Add("fo:start-indent", "0in");
            _validate.ClassProperty.Add("fo:end-indent", "0.125in");
            returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "1.875*");
            _validate.ClassProperty.Add("fo:start-indent", "0.125in");
            _validate.ClassProperty.Add("fo:end-indent", "0.125in");
            returnValue = _validate.ValidateNodeAttributesNS(2, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "1.875*");
            _validate.ClassProperty.Add("fo:start-indent", "0.125in");
            _validate.ClassProperty.Add("fo:end-indent", "0.125in");
            returnValue = _validate.ValidateNodeAttributesNS(3, xpath);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("style:rel-width", "1.875*");
            _validate.ClassProperty.Add("fo:start-indent", "0.125in");
            _validate.ClassProperty.Add("fo:end-indent", "0in");
            returnValue = _validate.ValidateNodeAttributesNS(4, xpath);
            Assert.IsTrue(returnValue);
        }

        ///<summary>
        /// </summary>      
        [Test]
        public void SpanStyleTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "spanstyle";

            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "span_.tpi_definition_.en_sense_senses_entry_letData_dicBody";
            _validate.ClassNameTrim = false;
            string content = "spirit/ tewel em i lusim bodi long nait long driman";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "spacebefore - Content 1 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "span_.tpi_definition_.en_sense_senses_entry_letData_dicBody";
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "spacebefore - Style Failure");
        }
         
        ///<summary>
        /// TD-458 - Index sense divider spacing
        /// <summary>
        /// </summary>      
        [Test]
        public void SpaceBeforeTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "spacebefore";

            //StyleXML
            string styleOutput = GetStyleOutput(file);
            //revSense_.bss_revEntry_revData
            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "revEntry_revData_revAppendix";
            _validate.ClassNameTrim = false;
            string content =
                "<text:span text:style-name=\"headword_.en_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">??</text:span><text:span text:style-name=\"revSense_.bss_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">booy</text:span><text:span text:style-name=\"revSense-revSense..before_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">; </text:span><text:span text:style-name=\"revSense_.bss_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">béchôlaan</text:span><text:span text:style-name=\"revSense-revSense..before_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">; </text:span><text:span text:style-name=\"revSense_.bss_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">âchumtéd</text:span><text:span text:style-name=\"revSense-revSense..before_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">; </text:span><text:span text:style-name=\"revSense_.bss_revEntry_revData_revAppendix\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">meháa</text:span>";

            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "spacebefore - Content 1 Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "revSense-revSense..before_revEntry_revData_revAppendix";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "spacebefore - Style Failure");
        }

        ///<summary>
        /// TD-479 -  create property to substitute quote characters
        /// <summary>
        /// </summary>      
        [Test]
        public void ReplacePrinceQuoteTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "ReplacePrinceQuote";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "span_Line1_columns_scrBook_scrBody";
            _validate.ClassNameTrim = true;
            string content = "“I Jerusalem, inau ku tutua sina ghahira kori hidigna na vathe.";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "ReplacePrinceQuote - Content 1 Failure");

            _validate.ClassName = "span_Line2_columns_scrBook_scrBody";
            _validate.ClassNameTrim = true;
            content = "Ahai ke vaututunia, imanea teo keda toatogha ke boi toke ke vaututunia.”";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "ReplacePrinceQuote - Content 2 Failure");
        }

        ///<summary>
        /// TD-518 -  divider line missing after introduction
        /// <summary>
        /// </summary>      
        [Test]
        public void EmptyDivTag_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "EmptyDivTag";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "border";
            _validate.ClassNameTrim = false;
            bool returnValue1 = _validate.ValidateOfficeTextNode("para");
            Assert.IsTrue(returnValue1, "EmptyDivTag - Content  Failure");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "border";

            _validate.ClassProperty.Add("fo:border-top", ".5pt solid #000000");
            _validate.ClassProperty.Add("fo:text-align", "center");
            _validate.ClassProperty.Add("fo:margin-left", "1in");
            _validate.ClassProperty.Add("fo:margin-right", "1in");
            _validate.ClassProperty.Add("fo:margin-top", "9pt");
            _validate.ClassProperty.Add("fo:margin-bottom", "18pt");
            //TODO AFTER FINISHING BORDER PROPERTY CHECK THIS.
            //bool returnValue = _validate.ValidateNodeAttributesNS(true);
            //Assert.IsTrue(returnValue, "EmptyDivTag - Style Failure");

        }
        ///<summary>
        /// TD-654 
        /// <summary>
        /// </summary>      
        [Test]
        [Category("SkipOnTeamCity")]
        public void VerseNumber_Node()
        {

            _projInfo.ProjectInputType = "Dictionary";
            const string file = "VerseNumber";

            string styleOutput = GetStyleOutput(file);
            //
            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "VerseNumber_Paragraph_scrSection_columns_scrBook_scrBody";
            string content = "1";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "VerseNumber_Paragraph_scrSection_columns_scrBook_scrBody";

            _validate.ClassProperty.Add("fo:font-family", "Charis SIL");
            _validate.ClassProperty.Add("style:font-name-complex", "Charis SIL");
            _validate.ClassProperty.Add("style:text-position", "super");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            _validate.ClassProperty.Add("fo:font-style", "italic");
            _validate.ClassProperty.Add("fo:background-color", "#0000ff");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "VerseNumber - Style Failure");
        }


        ///<summary>
        /// TD-349 -  width: auto
        /// <summary>
        /// </summary>      
        [Test]
        public void AutoWidth_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "AutoWidth";

            string styleOutput = GetStyleOutput(file);

            //First Node
            string xpath = "//text:p[@text:style-name='pictureCaption_scrBody']";
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            _validate.ClassProperty.Add("fo:min-width", "111.529411764706pt");

            bool returnValue = _validate.ValidateNodeAttributesNS(1, xpath);
            Assert.IsTrue(returnValue);
        }

        /// <summary>
        /// Remove color if the background color is white. This is done for
        /// pdf output thru odt output.
        /// </summary>
        [Test]
        public void BackgroundWhiteColor_Removal_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "BackgroundWhiteRemoval";

            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "T1 class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t2_t1_body";
            content = "T2 class";

            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "t3_t2_t1_body";
            content = "T3 class";

            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "t1_body";
            _validate.ClassProperty.Add("fo:text-align", "left");
            _validate.ClassProperty.Add("fo:background-color", "#008000");
            bool returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("fo:font-size", "12pt");
            _validate.ClassProperty.Add("style:font-size-complex", "12pt");
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            //_validate.ClassProperty.Add("fo:background-color", "#008000");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "t2_t1_body";
            _validate.ClassProperty.Add("fo:text-align", "left");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("fo:color", "#0000ff");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassProperty.Add("fo:background-color", "#ffffff");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsFalse(returnValue);  // white color should not be there.
        }

        ///<summary>
        /// TD-1239 -  Two column output from TE can conflict with two column stylesheets, not wrap correctly at section breaks.
        /// <summary>
        /// </summary>      
        [Test]
        public void RemoveScrSectionClass_Node()
        {
            //TODO NEGATIVE TEST FOR SCRSECTION IS NOT CREATED, SHOULD BE DONE
            _projInfo.ProjectInputType = "Scripture";
            const string file = "RemoveScrSectionClass";

            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;

            string xpath = "//style:style[@style:name='Sect_columns2']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "false");
            bool returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

            xpath = "//style:style[@style:name='Sect_columns1']/style:section-properties";
            _validate.ClassProperty.Add("text:dont-balance-text-columns", "true");
            returnValue = _validate.ValidateNodeAttributesNS(0, xpath);
            Assert.IsTrue(returnValue);

        }

        ///<summary>
        /// TD-89	Open Office .slots:before
        /// <summary>
        /// </summary>      
        [Test]
        public void SignificantSpace()
        {
            const string file = "SignificantSpace";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "entry_letData_dicBody";
            //_validate.GetInnerText = true;
            string content = "<text:span text:style-name=\"partofspeech_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">No space between the spans -------------------------&gt; v</text:span><text:span text:style-name=\"definition_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">light </text:span><text:span text:style-name=\"blue_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">(COMING TOGETHER) </text:span>";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1, content, "para");
            Assert.IsTrue(returnValue1, "SignificantSpace - Content Failure");

            content = "<text:span text:style-name=\"partofspeech_.en_gram_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">Two end span and start span -------------------------&gt; v</text:span><text:span text:style-name=\"definition_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">light (COMING WITH ONE SPACE)</text:span>";
            returnValue1 = _validate.ValidateOfficeTextNodeList(2, content, "para");
            Assert.IsTrue(returnValue1, "SignificantSpace - Content Failure");

            content = "<text:span text:style-name=\"partofspeech_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"> One end span and start span -------------------------&gt; v</text:span><text:span text:style-name=\"definition_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">light (COMING WITH ONE SPACE)</text:span>";
            returnValue1 = _validate.ValidateOfficeTextNodeList(3, content, "para");
            Assert.IsTrue(returnValue1, "SignificantSpace - Content Failure");

            content = "<text:span text:style-name=\"partofspeech_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">Horizontal continues span without cr lf - long space ---&gt; v</text:span><text:span text:style-name=\"definition_.en_sense_senses_entry_letData_dicBody\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">light (COMING WITH ONE SPACE)</text:span>";
            returnValue1 = _validate.ValidateOfficeTextNodeList(4, content, "para");
            Assert.IsTrue(returnValue1, "SignificantSpace - Content Failure");
        }
        ///<summary>
        /// TD96 text-indent
        /// <summary>
        /// </summary>      
        [Test]
        public void Textindent_Node()
        {
            const string file = "Textindent";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "t1_body";
            string content = "You and all the people of Israel: It is by the name of Jesus Christ of Nazareth, whom you crucified but whom God raised from the dead, that this man stands before you healed. <text:span text:style-name=\"verse_t1_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">11</text:span>He is \" 'the stone you builders rejected, which has become the capstone.[a]'[b] <text:span text:style-name=\"verse_t1_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">12</text:span><text:span text:style-name=\"salvation_t1_body\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"> Salvation is found in no one else, for there is no other name under heaven given to men by which we must be saved.\" </text:span>";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "t1_body";
            _validate.ClassProperty.Add("fo:text-indent", "100pt");
            bool returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue);
        }
        ///<summary>
        /// BackgroundColor
        /// <summary>
        /// </summary>      
        [Test]
        public void BackgroundColor_Node()
        {
            const string file = "BackgroundColor";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "divColor_body";
            _validate.GetInnerText = true;
            string content = "Span Color class Div Color class";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "div color failed");


            _validate.ClassName = "spanColor_divColor_body";
            _validate.GetInnerText = true;
            content = "Span Color class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1,"span color failed");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "divColor_body";
            _validate.ClassProperty.Add("fo:background-color", "#008000");
            bool returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsTrue(returnValue,"div does not have background color");

            _validate.ClassName = "spanColor_divColor_body";
            _validate.ClassProperty.Add("fo:background-color", "#ffff00");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue,"span does not have background color ");

            _validate.ClassName = "spanColor_divColor_body";
            _validate.ClassProperty.Add("fo:background-color", "#ffff00");
            returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsFalse(returnValue,"para node has background color");
        }
        ///<summary>
        /// DisplayNone
        /// <summary>
        /// </summary>      
        [Test]
        public void DisplayNone_Node()
        {
            const string file = "DisplayNone";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "divColor1_body";
            _validate.GetInnerText = true;
            string content = "Divcolor 1";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1, "div color failed");


            _validate.ClassName = "spanColor2_divColor2_body";
            _validate.GetInnerText = true;
            content = "Span Color2 class";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "span color failed");

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "divColor1_body";
            _validate.ClassProperty.Add("fo:background-color", "#008000");
            bool returnValue = _validate.ValidateNodeAttributesNS(true);
            Assert.IsFalse(returnValue, "div does not have background color");

            _validate.ClassName = "spanColor2_divColor2_body";
            _validate.ClassProperty.Add("fo:background-color", "#ff0000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue, "span does not have background color ");

        }
        ///<summary>
        /// Display Inline
        /// <summary>
        /// </summary>      
        [Test]
        public void DisplayInline_Node()
        {
            const string file = "DisplayInline";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letter_letHead_dicBody";
            _validate.GetInnerText = true;
            string content = "A a";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1, "DisplayInline failed");
        }

        [Test]
        public void PageTest1()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 1;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"First top Left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"Right top Center\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"Right top Center\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#0000ff\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath,content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter0\">First top Left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter1\">Right top Center</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter2\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter19\">Right top Center</text:span><text:tab /></text:p></style:header></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath,content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }


        [Test]
        public void PageTest2()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 2;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"Page left - Top Left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#0000ff\" fo:font-size=\"14pt\" fo:font-style=\"italic\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#0000ff\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#008000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter12\">Page left - Top Left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter13\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:header></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest3()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 3;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffa500\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#0000ff\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ee82ee\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter12\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:header></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest4()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 4;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"Right top left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-size=\"16pt\" fo:font-style=\"italic\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffa500\" fo:font-size=\"8pt\" fo:font-style=\"italic\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-size=\"16pt\" fo:font-style=\"italic\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffa500\" fo:font-size=\"8pt\" fo:font-style=\"italic\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"Right top left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-size=\"16pt\" fo:font-style=\"italic\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffa500\" fo:font-size=\"8pt\" fo:font-style=\"italic\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#0000ff\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#008000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ee82ee\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter0\">Right top left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter1\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter2\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter3\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:footer></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:tab /><text:span text:style-name=\"PageHeaderFooter14\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter15\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:footer></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter18\">Right top left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter19\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter20\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter21\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:footer></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest5()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 5;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#0000ff\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top right \" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom right \" /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#0000ff\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top right \" /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom right \" /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#0000ff\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"top right \" /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom left\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"bottom right \" /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter0\">top left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter1\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter2\">top right </text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter3\">bottom left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter4\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter5\">bottom right </text:span></text:p></style:footer></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter12\">top left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter13\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter14\">top right </text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter15\">bottom left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter16\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter17\">bottom right </text:span></text:p></style:footer></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter18\">top left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter19\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter20\">top right </text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter21\">bottom left</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter22\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter23\">bottom right </text:span></text:p></style:footer></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest6()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 6;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"***\" /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"@@@\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"***\" /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"@@@\" fo:font-weight=\"700\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ee82ee\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter0\">***</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter1\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter2\">@@@</text:span></text:p></style:header></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter18\">***</text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter19\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter20\">@@@</text:span></text:p></style:header></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest7()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 7;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ff0000\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#008000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter1\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:header></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter13\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter16\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:footer></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter19\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:header></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        [Test]
        public void PageTest8()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PageTest";
            _index = 8;
            string styleOutput = GetStyleOutput(file);
            _index = 0; /// reset

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ff0000\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ff0000\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ff0000\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ff0000\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:color=\"#ffff00\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#008000\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ee82ee\" fo:page-width=\"419.5276pt\" fo:page-height=\"595.2756pt\" fo:margin-top=\"85.03937pt\" fo:margin-right=\"85.03937pt\" fo:margin-bottom=\"85.03937pt\" fo:margin-left=\"85.03937pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:tab /><text:span text:style-name=\"PageHeaderFooter2\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter3\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:footer></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:tab /><text:span text:style-name=\"PageHeaderFooter14\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:span text:style-name=\"PageHeaderFooter16\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /></text:p></style:footer></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:tab /><text:tab /><text:span text:style-name=\"PageHeaderFooter20\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter21\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:tab /></text:p></style:footer></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");
        }

        ///<summary>
        /// PageTest9
        /// <summary>
        /// </summary>
        [Ignore]
        [Test]
        [Category("SkipOnTeamCity")]
        public void PageTest9()
        {
            const string file = "PageTest9";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            string xpath = "//office:automatic-styles";
            _validate = new ValidateXMLFile(styleOutput);
            string content = "<style:style style:name=\"PageHeaderFooter0\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" style:writing-mode=\"lr-tb\" fo:font-family=\"Verdana\" style:font-name-complex=\"Verdana\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"100%\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter1\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" fo:margin-top=\"100%\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter2\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" style:writing-mode=\"lr-tb\" fo:font-family=\"Verdana\" style:font-name-complex=\"Verdana\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"100%\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter3\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter4\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter5\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter12\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"string(guidewordfirst)\" style:writing-mode=\"lr-tb\" fo:font-family=\"Charis SIL\" style:font-name-complex=\"Charis SIL\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter13\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter14\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"string(guidewordlast)\" style:writing-mode=\"lr-tb\" fo:font-family=\"Charis SIL\" style:font-name-complex=\"Charis SIL\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter15\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter16\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter17\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter18\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"string(guidewordfirst)\" style:writing-mode=\"lr-tb\" fo:font-family=\"Charis SIL\" style:font-name-complex=\"Charis SIL\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter19\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"|counter(page)|\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter20\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"string(guidewordlast)\" style:writing-mode=\"lr-tb\" fo:font-family=\"Charis SIL\" style:font-name-complex=\"Charis SIL\" fo:font-weight=\"700\" fo:font-size=\"12pt\" fo:margin-top=\"28.34646pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:style><style:style style:name=\"PageHeaderFooter21\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter22\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:style style:name=\"PageHeaderFooter23\" style:family=\"text\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:text-properties content=\"\" /></style:style><style:page-layout style:name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style /><style:footer-style /></style:page-layout><style:page-layout style:name=\"pm2\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ff0000\" fo:page-width=\"595.2756pt\" fo:page-height=\"841.8898pt\" fo:margin-top=\"57pt\" fo:margin-right=\"113.3858pt\" fo:margin-bottom=\"57pt\" fo:margin-left=\"113.3858pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#0000ff\" fo:page-width=\"595.2756pt\" fo:page-height=\"841.8898pt\" fo:margin-top=\"57pt\" fo:margin-right=\"113.3858pt\" fo:margin-bottom=\"57pt\" fo:margin-left=\"113.3858pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm4\" style:page-usage=\"left\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#008000\" fo:page-width=\"595.2756pt\" fo:page-height=\"841.8898pt\" fo:margin-top=\"57pt\" fo:margin-right=\"113.3858pt\" fo:margin-bottom=\"57pt\" fo:margin-left=\"113.3858pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout><style:page-layout style:name=\"pm5\" style:page-usage=\"right\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:page-layout-properties fo:background-color=\"#ffa500\" fo:page-width=\"595.2756pt\" fo:page-height=\"841.8898pt\" fo:margin-top=\"57pt\" fo:margin-right=\"113.3858pt\" fo:margin-bottom=\"57pt\" fo:margin-left=\"113.3858pt\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><style:background-image /><style:footnote-sep style:distance-before-sep=\"0.0398in\" style:distance-after-sep=\"0.0398in\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" /></style:page-layout-properties><style:header-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:header-style><style:footer-style><style:header-footer-properties fo:margin-bottom=\"14.21pt\" fo:min-height=\"28.42pt\" fo:margin-left=\"0pt\" fo:margin-right=\"0pt\" style:dynamic-spacing=\"false\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\" /></style:footer-style></style:page-layout>";
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);

            Assert.IsTrue(returnValue1, "PageTest failed");

            xpath = "//office:master-styles";
            _validate = new ValidateXMLFile(styleOutput);
            content = "<style:master-page style:name=\"Standard\" style:page-layout-name=\"pm1\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"XHTML\" style:page-layout-name=\"pm2\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" /><style:master-page style:name=\"First_20_Page\" style:display-name=\"First Page\" style:next-style-name=\"Left_20_Page\" style:page-layout-name=\"pm3\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter0\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter1\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter2\" /></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter3\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter4\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter5\" /></text:p></style:footer></style:master-page><style:master-page style:name=\"Left_20_Page\" style:display-name=\"Left Page\" style:page-layout-name=\"pm4\" style:next-style-name=\"Right_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter12\"><text:chapter text:display=\"name\" text:outline-level=\"9\" /></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter13\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter14\"><text:chapter text:display=\"name\" text:outline-level=\"10\" /></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter15\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter16\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter17\" /></text:p></style:footer></style:master-page><style:master-page style:name=\"Right_20_Page\" style:display-name=\"Right Page\" style:page-layout-name=\"pm5\" style:next-style-name=\"Left_20_Page\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"><style:header><text:p text:style-name=\"Header\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter18\"><text:chapter text:display=\"name\" text:outline-level=\"9\" /></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter19\"><text:page-number text:select-page=\"current\">4</text:page-number></text:span><text:tab /><text:span text:style-name=\"PageHeaderFooter20\"><text:chapter text:display=\"name\" text:outline-level=\"10\" /></text:span></text:p></style:header><style:footer><text:p text:style-name=\"Footer\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><text:span text:style-name=\"PageHeaderFooter21\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter22\" /><text:tab /><text:span text:style-name=\"PageHeaderFooter23\" /></text:p></style:footer></style:master-page>";
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "PageTest-master failed");

        }
        ///<summary>
        /// DictionaryT9Test
        /// <summary>
        /// </summary>      
        [Test]
        [Category("SkipOnTeamCity")]
        public void DictionaryT9Test()
        {
            const string file = "t9";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = string.Empty;
            string xpath = "//draw:frame";
            //_validate.GetInnerText = true;
            _validate.GetOuterXml = true;
            string content = "<draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" draw:z-index=\"1\" svg:width=\"100%\" svg:height=\"72pt\" xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"><draw:text-box fo:min-height=\"0in\" xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"><draw:frame draw:style-name=\"Graphics0\" draw:name=\"Graphics0\" text:anchor-type=\"paragraph\" svg:width=\"100%\" svg:height=\"72pt\"><draw:image xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuatet=\"onLoad\" xlink:href=\"Pictures/nowaitress.jpg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" /><svg:desc>a waitress</svg:desc></draw:frame><text:p text:style-name=\"ForcedDiv\"><text:span text:style-name=\"caption_image_sense_article_sectionletter_dictionary\">a waitress</text:span></text:p></draw:text-box></draw:frame>";
            bool returnValue = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue);
        }
        ///<summary>
        /// TD96 text-indent
        /// <summary>
        /// </summary>      
        [Test]
        public void MasterDocument()
        {
            ExportOpenOffice exportOpenOffice = new ExportOpenOffice();
            var projInfo = new PublicationInformation();

            CopyInputToOutput();

            string lexiconFull = FileOutput("main.xhtml");
            string revFull = FileOutput("flexrev.xhtml");
            string lexiconCSS = FileOutput("main.css");
            string revCSS = FileOutput("flexrev.css");

            ProgressBar pb = new ProgressBar();
            projInfo.ProgressBar = pb;
            //projInfo.ProjectFileWithPath = _projectFile;
            projInfo.IsLexiconSectionExist = File.Exists(lexiconFull);
            projInfo.IsReversalExist = File.Exists(revFull);
            //projInfo.IsReversalExist = Param.Value[Param.ReversalIndex] == "True";
            projInfo.SwapHeadword = false;
            projInfo.FromPlugin = true;
            projInfo.DefaultCssFileWithPath = lexiconCSS;
            projInfo.DefaultRevCssFileWithPath = revCSS;
            projInfo.DefaultXhtmlFileWithPath = lexiconFull;
            projInfo.ProjectInputType = "Dictionary";
            projInfo.DictionaryPath = Path.GetDirectoryName(lexiconFull);
            projInfo.ProjectName = Path.GetFileNameWithoutExtension(lexiconFull);

            exportOpenOffice.Export(projInfo);
            Assert.IsTrue(CheckFileExist(),"Master Document Failed.");
        }

        [Test]
        public void SuperVerticalAlign()
        {
            const string file = "SuperVErticalAlign";
            _projInfo.ProjectInputType = "Dictionary";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "b_div_a_body";
            _validate.ClassProperty.Add("fo:font-size", "20");
            _validate.ClassProperty.Add("style:font-size-complex", "20");
            _validate.ClassProperty.Add("style:text-position", "super");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "c_div_a_body";
            _validate.ClassProperty.Add("fo:font-size", "10pt");
            _validate.ClassProperty.Add("style:font-size-complex", "10pt");
            _validate.ClassProperty.Add("style:text-position", "super");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "d_div_a_body";
            _validate.ClassProperty.Add("fo:font-size", "10");
            _validate.ClassProperty.Add("style:font-size-complex", "10");
            _validate.ClassProperty.Add("style:text-position", "sub");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "e_div_a_body";
            _validate.ClassProperty.Add("fo:font-size", "40");
            _validate.ClassProperty.Add("style:font-size-complex", "40");
            _validate.ClassProperty.Add("style:text-position", "super");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);
        }

        ///TD-1857 - Footnote ruler is short
        [Test]
        public void FootnoteSeperator_Node()
        {
            const string file = "FootNoteSeperator";
            _projInfo.ProjectInputType = "Scripture";
            string styleOutput = GetStyleOutput(file);

            _validate = new ValidateXMLFile(styleOutput);

            string xpath = "//office:automatic-styles/style:page-layout[@style:name='pm1']/style:page-layout-properties/style:footnote-sep";
            string content = "<style:footnote-sep style:width=\"0.0071in\" style:line-style=\"solid\" style:distance-before-sep=\"30%\" style:distance-after-sep=\"30%\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            _validate.GetOuterXml = true;
            bool returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "FootnoteSeperator test failed");

            xpath = "//office:automatic-styles/style:page-layout[@style:name='pm5']/style:page-layout-properties/style:footnote-sep";
            content = "<style:footnote-sep style:width=\"0.0071in\" style:line-style=\"solid\" style:distance-before-sep=\"30%\" style:distance-after-sep=\"30%\" style:color=\"#000000\" style:adjustment=\"centre\" style:rel-width=\"100%\" xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\" />";
            _validate.GetOuterXml = true;
            returnValue1 = _validate.ValidateNodeInnerXml(xpath, content);
            Assert.IsTrue(returnValue1, "FootnoteSeperator test failed");

        }

        ///<summary>
        /// prince-text-replace
        /// <summary>
        /// </summary>      
        [Test]
        public void TextReplaceTest_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "TextReplace";
            string styleOutput = GetStyleOutput(file);

            //Content Test
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "span_NoteGeneralParagraph_scrSection_scrBook_scrBody";
            string content = "Kori haghore Grik “Oti veikisighi.” Na puhi kena eia mara i hau bali tateli aua nidia na dotho, imarea kena kisia na bakodia ara kuladia kiloau.";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);
        }

        #region Indesign TestCases
        ///<summary>
        /// Indesign PseudoBefore
        /// <summary>
        /// </summary>      
        [Test]
        public void PseudoBefore_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PseudoBefore";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "letHead..before_dicBody";
            string content = "♦";
            bool returnValue1 = _validate.ValidateOfficeTextNodeList(1,content,"span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "letData..before_dicBody";
            content = ":";
            returnValue1 = _validate.ValidateOfficeTextNodeList(1, content, "span");
            Assert.IsTrue(returnValue1);
        }

        ///<summary>
        /// Indesign Parent1
        /// <summary>
        /// </summary>      
        [Test]
        public void Parent1_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "Parent1";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xitem_main_main_body";
            string content = "sample text file";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xitem_main_main_body";
            _validate.ClassProperty.Add("fo:color", "#ff0000");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);
        }

        ///<summary>
        /// Indesign Precede1
        /// <summary>
        /// </summary>      
        [Test]
        public void Precede1_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "Precede1";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xitem-xitem_main_body";
            string content = "sample text file";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem_.en-xitem_main_body";
            content = "sample text file";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);
            _validate.ClassName = "xitem-xitem_main_body";
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem_.en-xitem_main_body";
            _validate.ClassProperty.Add("fo:color", "#008000");
            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

        }

        ///<summary>
        /// Indesign PseudoContains
        /// <summary>
        /// </summary>      
        [Test]
        public void PseudoContains_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "PseudoContains";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "xitem..contains_main_body";
            string content = "contain";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "xitem-xitem..contains_main_body";
            content = "XXcontain";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "span");
            Assert.IsTrue(returnValue1);

            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);

            _validate.ClassName = "xitem-xitem..contains_main_body";
            _validate.ClassProperty.Add("fo:color", "#008000");
            _validate.ClassProperty.Add("fo:font-size","18pt");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "xitem..contains_main_body";
            _validate.ClassProperty.Add("fo:color", "#ff0000");
            _validate.ClassProperty.Add("fo:font-size", "36pt");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

        }

        ///<summary>
        /// Indesign MultiClass
        /// <summary>
        /// </summary>      
        [Test]
        public void MultiClass_Node()
        {
            _projInfo.ProjectInputType = "Dictionary";
            const string file = "MultiClass";
            string styleOutput = GetStyleOutput(file);

            //Content Test - First
            _validate = new ValidateXMLFile(_projInfo.TempOutputFolder);
            _validate.ClassName = "a.-b.-c_main_body";
            string content = "sample text file b a c";
            bool returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "a_main_body";
            content = "sample text file a";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "b_main_body";
            content = "sample text file b";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);

            _validate.ClassName = "a.-c_main_body";
            content = "sample text file a c";
            returnValue1 = _validate.ValidateOfficeTextNode(content, "para");
            Assert.IsTrue(returnValue1);


            //Note - The Styles will be created while processing xhtml(content.xml)
            //Style Test - Second
            _validate = new ValidateXMLFile(styleOutput);


            _validate.ClassName = "a.-b.-c_main_body";
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            _validate.ClassProperty.Add("fo:font-size", "24pt");

            bool returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);

            _validate.ClassName = "a_main_body";
            _validate.ClassProperty.Add("fo:color", "#0000ff");
            _validate.ClassProperty.Add("fo:font-size", "18pt");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


            _validate.ClassName = "b_main_body";
            _validate.ClassProperty.Add("fo:color", "#ff0000");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


            _validate.ClassName = "a.-c_main_body";
            _validate.ClassProperty.Add("fo:color", "#ffa500");
            _validate.ClassProperty.Add("fo:font-size", "18pt");

            returnValue = _validate.ValidateNodeAttributesNS(false);
            Assert.IsTrue(returnValue);


        }

        #endregion

        private void CopyInputToOutput()
        {
            string[] files = new[] { "main.odt", "flexrev.odt", "main.odm", "flexrev.css", "main.xhtml", "flexrev.xhtml", "main.css", "flexrev.css" };
            foreach (string file in files)
            {
                string outputfile = FileOutput(file);
                File.Delete(outputfile);
            }
           
            files = new[] {"main.xhtml","flexrev.xhtml","main.css","flexrev.css"};
            foreach (string file in files)
            {
                CopyExistingFile(file);
            }
        }

        private bool CheckFileExist()
        {
            bool returnValue = true;
            string[] files = new[] {"main.odt", "flexrev.odt", "main.odm"};
            foreach (string file in files)
            {
                string outputfile = FileOutput(file);
                if (!File.Exists(outputfile))
                {
                    returnValue = false;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Copies a file if it exists from the input test path to the output
        /// </summary>
        /// <param name="fileName">file to be copied if it exists</param>
        private void CopyExistingFile(string fileName)
        {
            if (File.Exists(FileInput(fileName)))
                File.Copy(FileInput(fileName), FileOutput(fileName), true);
        }

        private string GetStyleOutput(string file)
        {
            OOContent contentXML = new OOContent();
            OOStyles stylesXML = new OOStyles();
            string fileOutput = _index > 0 ? file + _index + ".css" : file + ".css";

            //string input = FileInput(file + ".css");
            string input = FileInput(fileOutput);

            _projInfo.DefaultCssFileWithPath = input;
            _projInfo.TempOutputFolder = _outputPath;

            Dictionary<string, Dictionary<string, string>> cssClass = new Dictionary<string, Dictionary<string, string>>();
            CssTree cssTree = new CssTree();
            cssClass = cssTree.CreateCssProperty(input, true);

            //StyleXML
            string styleOutput = FileOutput(file + _styleFile);
            Dictionary<string, Dictionary<string, string>> idAllClass = stylesXML.CreateStyles(_projInfo, cssClass, styleOutput);

            // ContentXML
            _projInfo.DefaultXhtmlFileWithPath = FileInput(file + ".xhtml");
            _projInfo.TempOutputFolder = FileOutput(file);
            contentXML.CreateStory(_projInfo, idAllClass, cssTree.SpecificityClass, cssTree.CssClassOrder);
            _projInfo.TempOutputFolder = _projInfo.TempOutputFolder + _contentFile;
            return styleOutput;
        }

        private string GetStyleOutput(PublicationInformation projInfo)
        {
            OOContent contentXML = new OOContent();
            OOStyles stylesXML = new OOStyles();
            projInfo.TempOutputFolder = _outputPath;
            string file = Path.GetFileNameWithoutExtension(_projInfo.DefaultXhtmlFileWithPath);

            Dictionary<string, Dictionary<string, string>> cssClass = new Dictionary<string, Dictionary<string, string>>();
            CssTree cssTree = new CssTree();
            cssClass = cssTree.CreateCssProperty(projInfo.DefaultCssFileWithPath, true);

            //StyleXML
            string fileOutput = _index > 0 ? file + _index + _styleFile : file + _styleFile;
            //string styleOutput = FileOutput(file + _styleFile);
            string styleOutput = FileOutput(fileOutput);
            Dictionary<string, Dictionary<string, string>> idAllClass = stylesXML.CreateStyles(_projInfo, cssClass, styleOutput);

            // ContentXML
            _projInfo.TempOutputFolder = FileOutput(file);
            contentXML.CreateStory(_projInfo, idAllClass, cssTree.SpecificityClass, cssTree.CssClassOrder);
            _projInfo.TempOutputFolder = _projInfo.TempOutputFolder + _contentFile;
            return styleOutput;
        }

        /*


                /////<summary>
                ///// TD98 (Clear:both)
                ///// <summary>
                ///// </summary>      
                /////[Test]
                //public void ClearTest_Clarification()
                //{
                //    //Note - CLARIFIATION - no input files available
                //    var contentXML = new ContentXML();
                //    var stylesXML = new StylesXML();
                //    _projInfo.ProjectInputType = "Dictionary";
                //    const string file = "ClearTest";

                //    //StyleXML
                //    string input = FileInput(file + ".css");
                //    string styleOutput = FileOutput(file +_styleFile);
                //    _styleName = stylesXML.CreateStyles(input, styleOutput, _errorFile, true);
                //    string styleExpected = FileExpected(file +_styleFile);

                //    // ContentXML
                //    _projInfo.DefaultXhtmlFileWithPath = FileInput(file + ".xhtml");
                //    _projInfo.TempOutputFolder = FileOutput(file);
                //    contentXML.CreateContent(_projInfo, _styleName);
                //    _projInfo.TempOutputFolder = _projInfo.TempOutputFolder + _contentFile;
                //    string contentExpected = FileExpected(file + _contentFile);

                //    XmlAssert.AreEqual(styleExpected, styleOutput, file + " in styles.xml");
                //    XmlAssert.AreEqual(contentExpected, _projInfo.TempOutputFolder, file + " in content.xml");
                //}

         * */
    }
}