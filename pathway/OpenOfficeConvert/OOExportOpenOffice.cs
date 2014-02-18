// --------------------------------------------------------------------------------------------
// <copyright file="ExportLibreOffice.cs" from='2009' to='2014' company='SIL International'>
//      Copyright � 2014, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Export process used to Export the ODT and Prince PDF output
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using SIL.PublishingSolution.Sort;
using SIL.Tool;
using SIL.Tool.Localization;

namespace SIL.PublishingSolution
{
    public class ExportLibreOffice : IExportProcess
    {
        #region Public Functions

        public string ExportType
        {
            get
            {
                return "OpenOffice/LibreOffice";
            }
        }

        public bool Handle(string inputDataType)
        {
            bool returnValue = inputDataType.ToLower() == "dictionary" || inputDataType.ToLower() == "scripture";
            return returnValue;
        }

        public string _refFormat = "Genesis 1";
        public string GeneratedPdfFileName = string.Empty;


        private static PublicationInformation publicationInfo;
        Dictionary<string, string> _dictLexiconPrepStepsFilenames = new Dictionary<string, string>();
        Dictionary<string, string> _dictSectionNames = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> _dictStepFilenames = new Dictionary<string, Dictionary<string, string>>();
        SortedDictionary<int, Dictionary<string, string>> _dictSorderSection = new SortedDictionary<int, Dictionary<string, string>>();
        ArrayList _odtFiles = new ArrayList();
        string _endQuote = ")";
        string _singleQuote = "'";
        string _comma = ",";
        private bool isMultiLanguageHeader = false;
        private bool _isFromExe = false;
        private bool _isFirstODT = true;

        private void SplitFile(PublicationInformation publicationInformation)
        {
            Dictionary<string, Dictionary<string, string>> cssClass = new Dictionary<string, Dictionary<string, string>>();
            CssTree cssTree = new CssTree();
            cssClass = cssTree.CreateCssProperty(publicationInformation.DefaultCssFileWithPath, true);

            if (cssClass.ContainsKey("@page") && cssClass["@page"].ContainsKey("-ps-fileproduce"))
            {
                publicationInformation.FileToProduce = cssClass["@page"]["-ps-fileproduce"];
            }
        }

        private Dictionary<string, string> SplitXhtmlAsMultiplePart(PublicationInformation publicationInformation, Dictionary<string, string> dictSecName)
        {
            var fileNameWithPath = new List<string>();
            if (publicationInformation.FileToProduce.ToLower() != "one")
            {

                if (publicationInformation.FileToProduce.ToLower() == "one per book")
                {
                    fileNameWithPath = Common.SplitXhtmlFile(publicationInfo.DefaultXhtmlFileWithPath,
                                        "scrbook", false);
                }
            }
            else if (publicationInformation.SplitFileByLetter != null && publicationInformation.SplitFileByLetter.ToLower() == "true")
            {
                var fs = new FileSplit();
                fileNameWithPath = fs.SplitFile(publicationInformation.DefaultXhtmlFileWithPath);
                string flexRevFile =
                    Common.PathCombine(Path.GetDirectoryName(publicationInformation.DefaultXhtmlFileWithPath), "FlexRev.xhtml");
                if(File.Exists(flexRevFile))
                    fileNameWithPath.Add(flexRevFile);
            }

            if (fileNameWithPath.Count > 0)
            {
                dictSecName = CreateJoiningForSplited(fileNameWithPath);
                publicationInformation.DictionaryOutputName = null;
                publicationInformation.MainLastFileName = fileNameWithPath[fileNameWithPath.Count - 1];
            }
            return dictSecName;
        }

        public void SetPublication(PublicationInformation pubInfo)
        {
            publicationInfo = pubInfo;
        }

        public string TransformLiftToXhtml(PublicationInformation pubInfo)
        {
            string entryFilterXpath = "true()";
            string senseFilterXpath = "true()";
            string languageFilterXpath = "true()";
            string liftSupportPath = Common.PathCombine(Common.GetPSApplicationPath(), "LiftSupport");

            string inputFile = pubInfo.DefaultXhtmlFileWithPath;
            string xhtmlFile = inputFile;
            string transFormfile = Common.PathCombine(liftSupportPath, "liftTransform.xsl");
            string filterFile = Common.PathCombine(liftSupportPath, "liftEntryAndSenseFilter.xsl");
            string newTempXslfile = Common.PathCombine(Path.GetTempPath(), "tempFilter.xsl");
            string newTempXslfile1 = Common.PathCombine(Path.GetTempPath(), "tempFilter1.xsl");


            string languageSortFile = Common.PathCombine(Path.GetTempPath(), "langSortedFile.xhtml");
            try
            {
                // Transformation files

                // Filter starts
                if (pubInfo.IsSenseFilter)
                {
                    senseFilterXpath = GetSenseFilterXpath();
                }
                if (pubInfo.IsEntryFilter)
                {
                    entryFilterXpath = GetEntryFilterXpath();
                }
                if (pubInfo.IsLanguageFilter)
                {
                    languageFilterXpath = GetLanguageFilterXpath();
                }

                string xmlFile = Path.ChangeExtension(inputFile, "xml");
                string lXmlFile = Path.ChangeExtension(inputFile, "lxml");

                xhtmlFile = Path.ChangeExtension(inputFile, "xhtml");
                File.Copy(filterFile, newTempXslfile, true);

                //Replace the filter key strings and Transformation takes place
                ReplaceFilters("entry", newTempXslfile, entryFilterXpath);
                ReplaceFilters("sense", newTempXslfile, senseFilterXpath);

                Common.XsltProcess(inputFile, newTempXslfile, ".xml");

                if (pubInfo.IsLanguageFilter)
                {
                    ReplaceNamespace(xmlFile);
                    string langFilterFile = Common.PathCombine(liftSupportPath, "liftLangFilter.xsl");
                    File.Copy(langFilterFile, newTempXslfile1, true);

                    ReplaceFilters("language", newTempXslfile1, languageFilterXpath);
                    Common.XsltProcess(xmlFile, newTempXslfile1, ".lxml");
                    xmlFile = lXmlFile;
                }

                ReplaceNamespace(xmlFile);
                Common.XsltProcess(xmlFile, transFormfile, ".xhtml");

                if (pubInfo.IsLanguageSort)
                {
                    bool sorted = LiftSortWritingSys(xhtmlFile, languageSortFile);
                    if (sorted)
                    {
                        xhtmlFile = languageSortFile;
                    }
                }

                if (pubInfo.IsEntrySort)
                {
                    LiftPreparer lp = new LiftPreparer();
                    lp.loadLift(pubInfo.DefaultXhtmlFileWithPath);
                    lp.applySort();
                    lp.getCurrentLift();
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return xhtmlFile;
        }

        public bool LiftSortWritingSys(string fileName, string outputfile)
        {
            bool returnValue = true;
            try
            {
                var liftDoc = new LiftDocument(fileName);
                var sorter = new LiftLangSorter(liftDoc);
                sorter.sortWritingSystems();
                var writer = new LiftWriter(outputfile);
                liftDoc.Save(writer);
                writer.Close();
            }
            catch (Exception)
            {
                returnValue = false;
            } 
            return returnValue;
        }
        private void ReplaceFilters(string key,string newTempXslfile, string filterXpath)
        {
            const string entryFilterParameter = "EntFltParam";
            const string senseFilterParameter = "SenseFltParam";
            const string langFilterParameter = "LangFltParam";

            string filterParameter = string.Empty;
            switch (key.ToLower())
            {
                case "entry":
                    filterParameter = entryFilterParameter;
                    break;
                case "sense":
                    filterParameter = senseFilterParameter;
                    break;
                case "language":
                    filterParameter = langFilterParameter;
                    break;

            }
            Common.StreamReplaceInFile(newTempXslfile, filterParameter, filterXpath);
        }

        private void ReplaceNamespace(string xmlFile)
        {
            string nameSpace = "xmlns=\"http://www.w3.org/1999/XSL/Transform\"";
            Common.StreamReplaceInFile(xmlFile, nameSpace, ""); // replacing the namespace with empty
        }


        private string GetFilterXpath(string mid, string forEntry)
        {
            string filterString = string.Empty;
            string searchKey = string.Empty;
            bool matchCase = false;
            string comma = _comma;
            string endQuote = _endQuote;
            if (forEntry.ToLower() == "entry" && publicationInfo.IsEntryFilter)
            {
                filterString = publicationInfo.EntryFilterString;
                searchKey = publicationInfo.EntryFilterKey;
                matchCase = publicationInfo.IsEntryFilterMatchCase;

            }
            else if (forEntry.ToLower() == "sense" && publicationInfo.IsSenseFilter)
            {
                filterString = publicationInfo.SenseFilterString;
                searchKey = publicationInfo.SenseFilterKey;
                matchCase = publicationInfo.IsSenseFilterMatchCase;
            }
            else if (forEntry.ToLower() == "language" && publicationInfo.IsLanguageFilter)
            {
                filterString = publicationInfo.LanguageFilterString;
                searchKey = publicationInfo.LanguageFilterKey;
                matchCase = publicationInfo.IsLanguageFilterMatchCase;
            }

            string searchKeyValue;

            if (!matchCase)
            {
                mid = SubstitueTranslateWithLowerCase(mid);
                string filterStringwithQuote = _singleQuote + filterString + _singleQuote;
                searchKeyValue = SubstitueTranslateWithLowerCase(filterStringwithQuote);
            }
            else // as it is - Match Case
            {
                searchKeyValue = _singleQuote + filterString + _singleQuote;
            }

            switch (searchKey.ToLower())
            {
                case "at start":
                    searchKey = "starts-with(";
                    break;
                case "at end": // not working in 1.0 works in 2.0
                    mid = "substring(" + mid + ", string-length(" + mid + ") - string-length(" + searchKeyValue + ") +1,string-length(" +
                          searchKeyValue + "))";
                    searchKey = "starts-with(";
                    break;
                case "anywhere":
                    searchKey = "contains(";
                    break;
                case "not equal":
                    endQuote = string.Empty;
                    searchKey = string.Empty;
                    comma = " != "; 
                    break;

                case "whole item":
                    endQuote = string.Empty;
                    searchKey = string.Empty;
                    comma = " = ";
                    break;
            }
            string filterXpath = searchKey + mid + comma + searchKeyValue + endQuote;
            return filterXpath;
        }

        private string GetSenseFilterXpath()
        {
            string mid = "$pentry/sense[$senseno]/definition//text"; // using variable for sense search
            string senseFilterXpath = GetFilterXpath(mid, "sense");
            return senseFilterXpath;
        }

        private string GetEntryFilterXpath()
        {
            const string mid = "lexical-unit/form/text";
            string filterXpath = GetFilterXpath(mid, "entry");
            return filterXpath;
        }

        private string GetLanguageFilterXpath()
        {
            const string mid = "@lang";
            string filterXpath = GetFilterXpath(mid, "language");
            return filterXpath;
        }

        private string SubstitueTranslateWithLowerCase(string input)
        {
            const string translateCommand = "translate(";
            string lowercase = _comma + "$lowercase";
            string uppercase = _comma + "$uppercase";
            input = translateCommand + input + uppercase + lowercase + _endQuote;
            return input;
        }

        private Dictionary<string, string> CreatePageDictionary(PublicationInformation projInfo)
        {
            Dictionary<string, string> dictSecName = new Dictionary<string, string>();
            if (projInfo.IsLexiconSectionExist && projInfo.IsReversalExist)
            {
                _dictSorderSection.Clear();
                dictSecName["Main"] = "main.xhtml";
                dictSecName["Reversal"] = "FlexRev.xhtml";
                _dictSorderSection[1] = dictSecName;
            }
            return dictSecName;
        }

        private Dictionary<string, string> CreateJoiningForSplited(List<string> fileNames)
        {
            Dictionary<string, string> dictSecName = new Dictionary<string, string>();
              if (fileNames.Count> 0)
            
            {
                bool mainAdded = false;
                int fileCount = 1;
                foreach (string s in fileNames)
                {
                    if (!mainAdded)
                    {
                        dictSecName["Main"] = s;
                        mainAdded = true;
                    }
                    else
                    { dictSecName["Main" + fileCount++] = s; }
                }
                _dictSorderSection[1] = dictSecName;
            }
            return dictSecName;
        }
        /// <summary>
        /// Get the Document Sections 
        /// </summary>
        private Dictionary<string, string> GetPageSectionSteps()
        {
            XmlDocument projXml = new XmlDocument();
            Dictionary<string, string> dictSecName = new Dictionary<string, string>();
            string xPath = "";
            XmlElement root = null;
            XmlNode tempNodeSearch = null;

            if (publicationInfo.ProjectFileWithPath == null)
                return dictSecName;
            projXml.Load(publicationInfo.ProjectFileWithPath);
            root = projXml.DocumentElement;

            xPath = "DocumentSettings/Sections";
            tempNodeSearch = root.SelectSingleNode(xPath);   // Default to Document Settings
            if (tempNodeSearch != null)
            {
                for (int i = 0; i < tempNodeSearch.ChildNodes.Count; i++)
                {
                    string attName = "";
                    string attValue = "";
                    int attNo = 0;
                    for (int j = 0; j < tempNodeSearch.ChildNodes[i].Attributes.Count; j++)
                    {
                        string attString = tempNodeSearch.ChildNodes[i].Attributes[j].Name.ToString();
                        if (attString == "Name")
                        {
                            attName = tempNodeSearch.ChildNodes[i].Attributes[j].Value.ToString(); ;
                        }
                        else if (attString == "Value")
                        {
                            attValue = tempNodeSearch.ChildNodes[i].Attributes[j].Value.ToString();
                        }
                        else if (attString == "No")
                        {
                            attNo = int.Parse(tempNodeSearch.ChildNodes[i].Attributes[j].Value);
                        }


                        if (tempNodeSearch.ChildNodes[i].HasChildNodes)
                        {
                            XmlNode childNodes = tempNodeSearch.ChildNodes[i];
                            for (int ii = 0; ii < childNodes.ChildNodes.Count; ii++)
                            {
                                string attChildName = "";
                                string attChildValue = "";
                                for (int jj = 0; jj < childNodes.ChildNodes[ii].Attributes.Count; jj++)
                                {
                                    string attChildString = childNodes.ChildNodes[ii].Attributes[jj].Name.ToString();
                                    if (attChildString == "Name")
                                    {
                                        attChildName = childNodes.ChildNodes[ii].Attributes[jj].Value.ToString(); ;
                                    }
                                    else if (attChildString == "Value")
                                    {
                                        attChildValue = childNodes.ChildNodes[ii].Attributes[jj].Value.ToString();
                                    }
                                }
                                if (attName == "Main")
                                {
                                    _dictLexiconPrepStepsFilenames[attChildName] = attChildValue;
                                }
                                else
                                {
                                    Dictionary<string, string> tempDict = new Dictionary<string, string>();
                                    tempDict[attChildName] = attChildValue;
                                    _dictStepFilenames[attName] = tempDict;
                                }
                            }
                        }
                    }
                    _dictSectionNames = new Dictionary<string, string>();
                    _dictSectionNames[attName] = attValue;
                    _dictSorderSection[attNo] = _dictSectionNames;
                    if (attValue.ToLower() != "none")
                    {
                        dictSecName[attName] = attValue;
                    }
                }
            }
            return dictSecName;
        }

        private void ExportODM(ProgressBar statusProgressBar)
        {
            Common.ShowMessage = false;
            _odtFiles.Clear();
            var exportProcess = new ExportLibreOffice();
            string LexiconFileName = string.Empty;
            publicationInfo.IsODM = true;
            foreach (KeyValuePair<int, Dictionary<string, string>> keyvalue in _dictSorderSection)
            {
                
                var Sections = keyvalue.Value;
                foreach (var subSection in Sections)
                {
                    if (subSection.Value != "none")
                    {
                        //string fileName = Path.GetFileName(subSection.Value);
                        string fileName = subSection.Value;
                        publicationInfo.AddFileToXML(fileName, "False", true, "", false, true);
                        fileName = Common.PathCombine(publicationInfo.DictionaryPath, Path.GetFileName(fileName));
                        string fileExt = Path.GetExtension(fileName);
                        string xslFileName = "";
                        bool generated = false;

                        switch (fileExt)
                        {
                            case ".xml":
                                if (subSection.Key != "Main")
                                {
                                    XslProcess(subSection, fileName, xslFileName, exportProcess, statusProgressBar);
                                }
                                break;
                            case ".xhtml":
                                if (subSection.Key == "Main")
                                {
                                    LexiconFileName = fileName;
                                }
                                publicationInfo.DefaultXhtmlFileWithPath = fileName;
                                publicationInfo.ProgressBar = statusProgressBar;
                                publicationInfo.IsOpenOutput = false;
                                generated = ExportODT(publicationInfo);
                                if (generated)
                                {
                                    string returnFileName = Path.GetFileName(fileName);
                                    _odtFiles.Add(Path.ChangeExtension(returnFileName, ".odt"));
                                }
                                break;

                            case ".lift":
                                if (subSection.Key == "Main")
                                {
                                    LexiconFileName = fileName;
                                }
                                XslProcess(subSection, fileName, xslFileName, exportProcess, statusProgressBar);

                                publicationInfo.DefaultXhtmlFileWithPath = fileName;
                                break;

                            case ".odt":
                                _odtFiles.Add(Path.GetFileName(fileName));
                                break;

                            default:
                                break;
                        }
                    }
                    else if (subSection.Key == "Blank")
                    {
                        _odtFiles.Add("Blank.odt");  // Blank ODT Files
                    }
                }
            }

            // Finally run the ODM file 
            Common.ShowMessage = true; // used to control MessageBox;
            publicationInfo.DefaultXhtmlFileWithPath = LexiconFileName;
            publicationInfo.DictionaryOutputName = publicationInfo.ProjectName;
            publicationInfo.ProgressBar = statusProgressBar;
            publicationInfo.FileSequence = _odtFiles;
            publicationInfo.IsOpenOutput = true;
            ExportODT(publicationInfo);
        }

        private void XslProcess(KeyValuePair<string, string> subSection, string fileName, string xslFileName, ExportLibreOffice exportLibreOffice, ProgressBar statusProgressBar)
        {
            bool generated;
            if (_dictStepFilenames.ContainsKey(subSection.Key))
            {
                var tempDict1 = new Dictionary<string, string>();
                tempDict1 = _dictStepFilenames[subSection.Key];
                foreach (KeyValuePair<string, string> tempString in tempDict1)
                {
                    xslFileName = tempString.Value;
                }
            }

            if (_dictLexiconPrepStepsFilenames.Count > 0)
            {
                xslFileName = _dictLexiconPrepStepsFilenames["Filter Entries"];
            }
            string returnFileName = Common.XsltProcess(fileName, xslFileName, ".xhtml");
            if (!Path.IsPathRooted(returnFileName))
            {
                var msg = new[] { returnFileName };
                LocDB.Message("defErrMsg", returnFileName, msg, LocDB.MessageTypes.Error, LocDB.MessageDefault.First);
            }

            if (returnFileName != "")
            {
                publicationInfo.ProgressBar = statusProgressBar;
                publicationInfo.IsOpenOutput = false;
                generated = ExportODT(publicationInfo);

                if (generated)
                {
                    returnFileName = Path.GetFileName(returnFileName);
                    _odtFiles.Add(Path.ChangeExtension(returnFileName, ".odt"));
                }
            }
        }

        /// <summary>
        /// Convert XHTML to ODT
        /// </summary>
        public bool Export(PublicationInformation projInfo)
        {
            publicationInfo = projInfo;
            string defaultXhtml = projInfo.DefaultXhtmlFileWithPath;
            GeneratedPdfFileName = defaultXhtml;
            projInfo.OutputExtension = "odt";
            Common.OdType = Common.OdtType.OdtChild;
            bool returnValue = false;
            VerboseClass verboseClass = VerboseClass.GetInstance();
            _isFromExe = Common.CheckExecutionPath();

            Common.DeleteDirectoryWildCard(Path.GetTempPath(), "SilPathwaytmp*"); 

            string strFromOfficeFolder = Common.FromRegistry("OfficeFiles" + Path.DirectorySeparatorChar + projInfo.ProjectInputType);
            projInfo.TempOutputFolder = Common.PathCombine(Path.GetTempPath(), "OfficeFiles" + Path.DirectorySeparatorChar + projInfo.ProjectInputType);
            CopyOfficeFolder(strFromOfficeFolder, projInfo.TempOutputFolder);
            Dictionary<string, string> dictSecName = new Dictionary<string, string>();

            Dictionary<string, Dictionary<string, string>> cssClass = new Dictionary<string, Dictionary<string, string>>();
            CssTree cssTree = new CssTree();
            cssClass = cssTree.CreateCssProperty(projInfo.DefaultCssFileWithPath, true);

            if (cssClass.ContainsKey("@page") && cssClass["@page"].ContainsKey("-ps-fileproduce"))
            {
                projInfo.FileToProduce = cssClass["@page"]["-ps-fileproduce"];
            }

            if (projInfo.FromPlugin && projInfo.IsReversalExist)
            {
                dictSecName = CreatePageDictionary(projInfo);
            }
            else
            {
                dictSecName = GetPageSectionSteps();
            }
            dictSecName = SplitXhtmlAsMultiplePart(projInfo, dictSecName);
            
            if (dictSecName.Count > 1)
            {
                GeneratedPdfFileName = dictSecName["Main"];
                ExportODM(publicationInfo.ProgressBar);
            }
            else
            {
                publicationInfo.DictionaryOutputName = publicationInfo.ProjectName;
                returnValue = ExportODT(publicationInfo);
            }

            if (publicationInfo.FinalOutput.ToLower() == "pdf")
            {
                publicationInfo.OutputExtension = "pdf";
                Common.InsertCopyrightInPdf(defaultXhtml, "LibreOffice", projInfo.ProjectInputType);
            }
            else
            {
                Common.CleanupExportFolder(publicationInfo.DefaultXhtmlFileWithPath, ".tmp,.de,.exe,.jar,.xml", "layout", string.Empty);
                CreateRAMP();
                Common.CleanupExportFolder(publicationInfo.DefaultXhtmlFileWithPath, ".css,.xhtml,.xml", String.Empty, String.Empty);
            }
            return returnValue;
        }

        private void InsertFrontMatter(PublicationInformation projInfo)
        {
            if (_isFromExe && _isFirstODT)
            {
                PreExportProcess preProcessor = new PreExportProcess(projInfo);
                //Preprocess XHTML & CSS for FrontMatter
                preProcessor.InsertLoFrontMatterContent(projInfo.DefaultXhtmlFileWithPath, projInfo.IsODM);
                _isFirstODT = false;
            }
        }

        /// <summary>
        /// Convert XHTML to ODT and ODM
        /// </summary>
        public bool ExportODT(PublicationInformation projInfo)
        {
            string defaultXhtml = projInfo.DefaultXhtmlFileWithPath;
            projInfo.OutputExtension = "odt";
            Common.OdType = Common.OdtType.OdtChild;
            bool returnValue = false;
            string strFromOfficeFolder = Common.PathCombine(Common.GetPSApplicationPath(), "OfficeFiles" + Path.DirectorySeparatorChar + projInfo.ProjectInputType);
            projInfo.TempOutputFolder = Common.PathCombine(Path.GetTempPath(), "OfficeFiles" + Path.DirectorySeparatorChar + projInfo.ProjectInputType);
            Common.DeleteDirectory(projInfo.TempOutputFolder);
            string strStylePath = Common.PathCombine(projInfo.TempOutputFolder, "styles.xml");
            string strContentPath = Common.PathCombine(projInfo.TempOutputFolder, "content.xml");
            CopyOfficeFolder(strFromOfficeFolder, projInfo.TempOutputFolder);
            string strMacroPath = Common.PathCombine(projInfo.TempOutputFolder, "Basic/Standard/Module1.xml");
            string outputFileName;
            string outputPath = Path.GetDirectoryName(projInfo.DefaultXhtmlFileWithPath);
            VerboseClass verboseClass = VerboseClass.GetInstance();
            if (projInfo.FileSequence != null && projInfo.FileSequence.Count > 1)
            {
                projInfo.OutputExtension = "odm";  // Master Document
                Common.OdType = Common.OdtType.OdtMaster;
                if (projInfo.DictionaryOutputName == null)
                    projInfo.DictionaryOutputName = projInfo.DefaultXhtmlFileWithPath.Replace(Path.GetExtension(projInfo.DefaultXhtmlFileWithPath), "");
                outputFileName = Common.PathCombine(outputPath, projInfo.DictionaryOutputName); // OdtMaster is created in Dictionary Name
            }
            else
            {
                // All other OdtChild files are created in the name of Xhtml or xml file Names.
                if (projInfo.DictionaryOutputName == null)
                {
                    outputFileName = projInfo.DefaultXhtmlFileWithPath.Replace(Path.GetExtension(projInfo.DefaultXhtmlFileWithPath), "");
                }
                else
                {
                    string inputFileName = Path.GetFileNameWithoutExtension(projInfo.DefaultXhtmlFileWithPath);
                    outputFileName = Common.PathCombine(outputPath, inputFileName);
                    Common.OdType = Common.OdtType.OdtNoMaster; // to all the Page property process
                }
            }

            string cssFile = projInfo.DefaultCssFileWithPath;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(defaultXhtml);
            if(projInfo.DefaultRevCssFileWithPath != null && projInfo.DefaultRevCssFileWithPath.Trim().Length > 0)
            {
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.ToLower() == "flexrev")
                {
                    cssFile = projInfo.DefaultRevCssFileWithPath;
                }
            }
            

            PreExportProcess preProcessor = new PreExportProcess(projInfo);
            if (fileNameWithoutExtension != null && fileNameWithoutExtension.ToLower() == "flexrev")
            {
                preProcessor.InsertEmptyDiv(preProcessor.ProcessedXhtml);
            }
            preProcessor.GetTempFolderPath();
            preProcessor.GetDefaultLanguage(projInfo);
            projInfo.DefaultXhtmlFileWithPath = preProcessor.PreserveSpace();
            InsertFrontMatter(projInfo);
            preProcessor.GetfigureNode();
            preProcessor.InsertKeepWithNextOnStyles(cssFile);
            preProcessor.InsertBookPageBreak();
            preProcessor.ArrangeImages();
            isMultiLanguageHeader = preProcessor.GetMultiLanguageHeader();

            Dictionary<string, Dictionary<string, string>> cssClass = new Dictionary<string, Dictionary<string, string>>();
            CssTree cssTree = new CssTree();
            cssTree.OutputType = Common.OutputType.ODT;
            cssClass = cssTree.CreateCssProperty(cssFile, true);
            HandledInCss(ref projInfo, ref cssClass);
            int pageWidth = GetPictureWidth(cssClass);
            // BEGIN Generate Styles.Xml File
            Dictionary<string, Dictionary<string, string>> idAllClass = new Dictionary<string, Dictionary<string, string>>();
            LOStyles inStyles = new LOStyles();
            inStyles._multiLanguageHeader = isMultiLanguageHeader;
            idAllClass = inStyles.CreateStyles(projInfo, cssClass, "styles.xml");
            projInfo.IncludeFootnoteSymbol = inStyles._customFootnoteCaller;
            projInfo.IncludeXRefSymbol = inStyles._customXRefCaller;
            projInfo.SplitFileByLetter = inStyles._splitFileByLetter;
            projInfo.HideSpaceVerseNumber = inStyles._hideSpaceVerseNumber;
            //To set Constent variables for User Desire
            string fname = Common.GetFileNameWithoutExtension(projInfo.DefaultXhtmlFileWithPath);
            string macroFileName = Common.PathCombine(projInfo.DictionaryPath, fname);
            string isCoverImageInserted = "false";
            if (idAllClass.ContainsKey("cover"))
                isCoverImageInserted = "true";

            string isToc = "false";
            if (idAllClass.ContainsKey("TableOfContentLO"))
                isToc = "true";

            _refFormat = GetReferenceFormat(idAllClass, _refFormat);
            IncludeTextinMacro(strMacroPath, _refFormat, macroFileName, projInfo.IsExtraProcessing, isCoverImageInserted, isToc);

            // BEGIN Generate Meta.Xml File
            var metaXML = new LOMetaXML(projInfo.ProjectInputType);
            metaXML.CreateMeta(projInfo);
            // BEGIN Generate Content.Xml File 
            var cXML = new LOContent();
            preProcessor.ImagePreprocess(false);
            preProcessor.ReplaceSlashToREVERSE_SOLIDUS();
            if (projInfo.SwapHeadword)
                preProcessor.SwapHeadWordAndReversalForm();

            Dictionary<string, string> pageSize = new Dictionary<string, string>();
            pageSize["height"] = cssClass["@page"]["page-height"];
            pageSize["width"] = cssClass["@page"]["page-width"];

            projInfo.DefaultXhtmlFileWithPath = preProcessor.ProcessedXhtml;

            AfterBeforeProcess afterBeforeProcess = new AfterBeforeProcess();
            afterBeforeProcess.RemoveAfterBefore(projInfo, cssClass, cssTree.SpecificityClass, cssTree.CssClassOrder);

            projInfo.TempOutputFolder += Path.DirectorySeparatorChar;
            cXML._multiLanguageHeader = isMultiLanguageHeader;
            cXML.RefFormat = this._refFormat;

            SetHeaderFontName(projInfo, idAllClass);
            cXML.CreateStory(projInfo, idAllClass, cssTree.SpecificityClass, cssTree.CssClassOrder, pageWidth, pageSize);
            PostProcess(projInfo);

            if (projInfo.FileSequence != null && projInfo.FileSequence.Count > 1)
            {
                var ul = new OOUtility();
                ul.CreateMasterContents(strContentPath, projInfo.FileSequence);
            }

            if (projInfo.MoveStyleToContent)
                MoveStylesToContent(strStylePath, strContentPath);

            var mODT = new ZipFolder();
            string fileNameNoPath = outputFileName + "." + projInfo.OutputExtension;
            mODT.CreateZip(projInfo.TempOutputFolder, fileNameNoPath, verboseClass.ErrorCount);

            projInfo.DictionaryOutputName = fileNameNoPath;
            try
            {
                if (File.Exists(fileNameNoPath))
                {
                    returnValue = true;
                    if (projInfo.IsOpenOutput)
                    {
                        Common.OpenOutput(fileNameNoPath);
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1155)
                {
                    string installedLocation = string.Empty;
                    if (File.Exists(fileNameNoPath))
                    {
                        installedLocation = fileNameNoPath;
                        var msg = new[] { "LibreOffice application from http://www.libreoffice.org site.\nAfter downloading and installing Libre Office, please consult release notes about how to change the macro security setting to enable the macro that creates the headers." };
                        LocDB.Message("errInstallFile", "The output has been save in " + installedLocation + ".", "Please install " + msg, msg, LocDB.MessageTypes.Error, LocDB.MessageDefault.First);
                        return false;
                    }
                    else
                    {
                        var msg = new[] { "LibreOffice application from http://www.libreoffice.org site.\nAfter downloading and installing Libre Office, please consult release notes about how to change the macro security setting to enable the macro that creates the headers." };
                        LocDB.Message("errInstallFile", "Please install " + msg, msg, LocDB.MessageTypes.Error, LocDB.MessageDefault.First);
                        return false;
                    }
                }
            }
            finally
            {
                projInfo.DefaultXhtmlFileWithPath = defaultXhtml;
                if (preProcessor != null)
                {
                    DirectoryInfo di = new DirectoryInfo(preProcessor.GetCreatedTempFolderPath);
                    Common.CleanDirectory(di);
                }
                if (projInfo.TempOutputFolder != null)
                {
                    DirectoryInfo di = new DirectoryInfo(projInfo.TempOutputFolder);
                    Common.CleanDirectory(di);
                }
            }
            return returnValue;
        }

        private void CreateRAMP()
        {
            string outputExtn = ".odt";
            if (publicationInfo.OutputExtension == "odm")
            {
                outputExtn = ".odm," + outputExtn;
            }
            else if (publicationInfo.OutputExtension == "pdf")
            {
                outputExtn = ".pdf";
            }
            Ramp ramp = new Ramp();
            ramp.Create(publicationInfo.DefaultXhtmlFileWithPath, outputExtn, publicationInfo.ProjectInputType);
        }

        private void HandledInCss(ref PublicationInformation projInfo, ref Dictionary<string, Dictionary<string, string>> cssClass)
        {
            if (projInfo.IsReversalExist)
            {
                if (cssClass.ContainsKey("headref") && cssClass["headref"].ContainsKey("font-family"))
                {
                    cssClass["headref"].Remove("font-family");
                }
            }

            if (projInfo.ProjectInputType.ToLower() == "scripture")
            {
                if (cssClass.ContainsKey("ipi") && cssClass["ipi"].ContainsKey("font-size")) //TD-3281  
                {
                    if (cssClass.ContainsKey("IntroParagraph") && cssClass["IntroParagraph"].ContainsKey("font-size"))
                    {
                        cssClass["ipi"]["font-size"] = cssClass["IntroParagraph"]["font-size"];
                    }
                }

                if (cssClass.ContainsKey("li") && cssClass["li"].ContainsKey("font-size")) //TD-3299  
                {
                    if (cssClass.ContainsKey("Paragraph") && cssClass["Paragraph"].ContainsKey("font-size"))
                    {
                        cssClass["li"]["font-size"] = cssClass["Paragraph"]["font-size"];
                    }
                }
            }
        }

        private static void SetHeaderFontName(PublicationInformation projInfo, Dictionary<string, Dictionary<string, string>> idAllClass)
        {
            if (projInfo.ProjectInputType == "Dictionary")
            {
                if (idAllClass.ContainsKey("headword") && idAllClass["headword"].ContainsKey("font-family"))
                {
                    projInfo.HeaderFontName = idAllClass["headword"]["font-family"];
                }
            }
        }

        private static void PostProcess(PublicationInformation projInfo)
        {
            if (projInfo.ProjectInputType.ToLower() == "dictionary")
            {
                InsertKeepWithNextinEntryStyle(projInfo.TempOutputFolder, "styles.xml");
            }
            else if (projInfo.ProjectInputType.ToLower() == "scripture")
            {
                InsertChapterNumber(projInfo.TempOutputFolder);
                ContentPostProcess(projInfo.TempOutputFolder);
            }
        }

        public static void InsertFirstGuidewordForReversal(string tempOutputFolder)
        {
            string filename = Common.PathCombine(tempOutputFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            //CopyChapterVariableBeforeSectionHead
            string xpath = "//text:p[@text:style-name='letter_letHead_dicBody']"; //letter_letHead_body

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list != null)
            {
                foreach (XmlNode xmlNode in list)
                {
                    if (xmlNode.ParentNode != null)
                    {
                        XmlNode letDataNode = xmlNode.ParentNode.NextSibling;
                        xpath = ".//text:variable-set[@text:name='Left_Guideword_L']";
                        if (letDataNode == null)
                        {
                            continue;
                        }
                        XmlNodeList leftGuidewordList = letDataNode.SelectNodes(xpath, nsmgr1);
                        if (leftGuidewordList.Count > 0)
                        {
                            string xpath1 = "//text:p[@text:style-name='hideDiv_dicBody']";//hideDiv_body

                            XmlNodeList list1 = xdoc.SelectNodes(xpath1, nsmgr1);
                            if (list1 != null && list1.Count > 0)
                            {
                                list1[0].AppendChild(leftGuidewordList[0].Clone());
                            }
                            break;
                        }
                    }
                }
            }

            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        public static void InsertGuidewordAfterLetter(string tempOutputFolder)
        {

            string filename = Common.PathCombine(tempOutputFolder, "content.xml");
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(filename);

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            //CopyChapterVariableBeforeSectionHead
            string xpath = "//text:p[@text:style-name='letter_letHead_body']";

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list != null)
            {
                foreach (XmlNode xmlNode in list)
                {
                    if (xmlNode.ParentNode != null)
                    {
                        XmlNode letDataNode = xmlNode.ParentNode.NextSibling;
                        xpath = ".//text:variable-set[@text:name='Left_Guideword_L']";
                        XmlNodeList leftGuidewordList = letDataNode.SelectNodes(xpath, nsmgr1);
                        if (leftGuidewordList.Count > 0)
                        {
                            xmlNode.AppendChild(leftGuidewordList[0].Clone());
                            continue;
                        }
                    }
                }
                xdoc.PreserveWhitespace = true;
                xdoc.Save(filename);
            }
        }

        public static void ContentPostProcess(string tempOutputFolder)
        {
            
            string filename = Common.PathCombine(tempOutputFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            //CopyChapterVariableBeforeSectionHead
            string xpath = "//text:p[@text:style-name='ChapterNumber1']";

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list != null)
            {
                foreach (XmlNode xmlNode in list)
                {
                    if (xmlNode.PreviousSibling != null)
                    {
                        XmlNode prevNode = xmlNode.PreviousSibling;
                        if (prevNode.Attributes != null)
                        {
                            string value = prevNode.Attributes["text:style-name"].Value;
                            if (value.ToLower().IndexOf("sectionhead") == 0)
                            {
                                xpath = ".//text:span";
                                XmlNodeList spanList = xmlNode.SelectNodes(xpath, nsmgr1);
                                int cnt = 0;
                                for (int i = 0; i < spanList.Count; i++)
                                {
                                    if (spanList[i].InnerXml.Contains("_Guideword_"))
                                    {
                                        cnt++;
                                        prevNode.InsertBefore(spanList[i].CloneNode(true), prevNode.FirstChild);
                                        if (cnt != 2) continue;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        public static void InsertPublisherOnTitlePage(string tempOutputFolder)
        {
            string filename = Common.PathCombine(tempOutputFolder, "content.xml");
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(filename);

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            nsmgr1.AddNamespace("draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");

            string xpath = "//text:p[@text:style-name='logo_div_dicBody']";
            if (publicationInfo.ProjectInputType.ToLower() == "scripture")
            {
                xpath = "//text:p[@text:style-name='logo_div_scrBody']";
            }

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list != null)
            {
                foreach (XmlNode xmlNode in list)
                {
                    XmlNode FirstNode = xmlNode.FirstChild.CloneNode(true);
                    xmlNode.RemoveChild(xmlNode.FirstChild);
                    xpath = "//draw:frame/draw:text-box";
                    XmlNode list1 = xdoc.SelectSingleNode(xpath, nsmgr1);
                    if (list1 != null)
                    {
                        list1.InnerXml = FirstNode.OuterXml.Replace("text:span", "text:p") +
                                         @"<text:p text:style-name=""Illustration"">" + list1.InnerXml + "</text:p>";
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        public static void ChangeTitleNameasBookName(string tempOutputFolder)
        {
            string filename = Common.PathCombine(tempOutputFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            if (publicationInfo.ProjectInputType.ToLower() == "scripture")
            {
                string xx = "//text:section[@text:style-name='Sect_scrBook']";
                XmlNodeList bookList = xdoc.SelectNodes(xx, nsmgr1);
                if(bookList != null && bookList.Count > 0)
                {
                    for (int i = 0; i < bookList.Count; i++)
                    {
                        string xpath = ".//text:span[@text:style-name='scrBookName_scrBook_scrBody']";
                        XmlNode bookNameNode = bookList[i].SelectSingleNode(xpath, nsmgr1);
                        if (bookNameNode != null)
                        {
                            string bookName = bookNameNode.InnerText;
                            xpath = ".//text:span[@text:style-name='span_TitleMain_scrBook_scrBody']";
                            XmlNode titleNode = bookList[i].SelectSingleNode(xpath, nsmgr1);
                            if (titleNode != null) titleNode.InnerText = bookName;
                        }
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        private static void RenameContentStyleOnCondition(string tempFolder)
        {
            string filename = Common.PathCombine(tempFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            string xpath = "//text:p[@text:style-name='DictionarySubentry_a_subentries_entry_letData_dicBody']";
            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list != null)
            {
                foreach (XmlNode xmlNode in list)
                {
                    XmlNode prevEntryNode = xmlNode.PreviousSibling;
                    if (prevEntryNode.Attributes != null)
                    {
                        string copyAttr = prevEntryNode.Attributes["text:style-name"].Value;
                        prevEntryNode.Attributes["text:style-name"].Value = copyAttr.Replace("entry", "entry1");
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        /// <summary>
        /// TD-2488
        /// </summary>
        /// <param name="directoryPath">File Directory path</param>
        public static void InsertKeepWithNextinEntryStyle(string directoryPath, string styleFilename)
        {
            string filename = Common.PathCombine(directoryPath, styleFilename);
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            string xpath = "//style:style[@style:name='letter_letHead_dicBody']";
            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list.Count > 0)
            {
                XmlNode copyNode = list[0].Clone();
                if (copyNode.Attributes != null)
                {
                    string copyAttr = copyNode.Attributes["style:name"].Value;
                    copyNode.Attributes["style:name"].Value = copyAttr.Replace("entry", "entry1");
                }
                string xPath = "//style:paragraph-properties";
                XmlAttribute attribute = xdoc.CreateAttribute("keep-with-next", nsmgr1.LookupNamespace("fo"));
                attribute.Value = "always";
                XmlNode paraAttriblist = copyNode.SelectSingleNode(xPath, nsmgr1);
                if (paraAttriblist != null && paraAttriblist.Attributes != null)
                    paraAttriblist.Attributes.Append(attribute);

                attribute = xdoc.CreateAttribute("widows", nsmgr1.LookupNamespace("fo"));
                attribute.Value = "3";
                paraAttriblist = copyNode.SelectSingleNode(xPath, nsmgr1);
                if (paraAttriblist != null && paraAttriblist.Attributes != null)
                    paraAttriblist.Attributes.Append(attribute);

                attribute = xdoc.CreateAttribute("page-number", nsmgr1.LookupNamespace("style"));
                attribute.Value = "auto";
                paraAttriblist = copyNode.SelectSingleNode(xPath, nsmgr1);
                if (paraAttriblist != null && paraAttriblist.Attributes != null)
                    paraAttriblist.Attributes.Append(attribute);


                attribute = xdoc.CreateAttribute("auto-update", nsmgr1.LookupNamespace("style"));
                attribute.Value = "true";
                if (copyNode != null && copyNode.Attributes != null)
                    copyNode.Attributes.Append(attribute);

                attribute = xdoc.CreateAttribute("master-page-name", nsmgr1.LookupNamespace("style"));
                attribute.Value = "";
                if (copyNode.Attributes != null)
                    copyNode.Attributes.Append(attribute);

                var parentNode = list[0].ParentNode;
                if (parentNode != null) parentNode.AppendChild(copyNode);
            }

            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        private int GetPictureWidth(Dictionary<string, Dictionary<string, string>> cssClass)
        {
            double pageMarginLeft = 0;
            double pageMarginRight = 0;
            double pageWidth = 325;
            int pictureWidth = 325;
            if (cssClass.ContainsKey("@page"))
            {
                try
                {
                    if (cssClass["@page"].ContainsKey("page-width"))
                        pageWidth = Convert.ToDouble(cssClass["@page"]["page-width"], CultureInfo.GetCultureInfo("en-US"));

                    if (cssClass["@page"].ContainsKey("margin-left"))
                        pageMarginLeft = Convert.ToDouble(cssClass["@page"]["margin-left"], CultureInfo.GetCultureInfo("en-US"));

                    if (cssClass["@page"].ContainsKey("margin-right"))
                        pageMarginRight = Convert.ToDouble(cssClass["@page"]["margin-right"], CultureInfo.GetCultureInfo("en-US"));

                    pageWidth = pageWidth - (pageMarginLeft + pageMarginRight);
                    pictureWidth = Convert.ToInt32(pageWidth);
                }
                catch (Exception)
                {
                    pictureWidth = 325;
                }
            }
            return pictureWidth;
        }


        /// <summary>
        /// TD-2488
        /// </summary>
        /// <param name="tempFolder">Temp folder path</param>
        private static void InsertChapterNumber(string tempFolder)
        {

            string filename = Common.PathCombine(tempFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");



            string xpath = "//text:p[@text:style-name=\"IntroListItem1_scrIntroSection_scrBook_scrBody\"]";

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list.Count > 0)
            {
                foreach (XmlNode VARIABLE in list)
                {
                    string xpath1 = "//text:span/text:variable-set[@text:name=\"Left_Guideword_L\"]";
                    XmlNodeList list1 = xdoc.SelectNodes(xpath1, nsmgr1);
                    if (list1.Count > 0)
                    {
                        VARIABLE.AppendChild(list1[0].CloneNode(true));
                    }

                    xpath1 = "//text:span/text:variable-set[@text:name=\"Left_Guideword_R\"]";
                    list1 = xdoc.SelectNodes(xpath1, nsmgr1);
                    if (list1.Count > 0)
                    {
                        VARIABLE.AppendChild(list1[0].CloneNode(true));
                        break;
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        /// <summary>
        /// TD-2488
        /// </summary>
        /// <param name="tempFolder">Temp folder path</param>
        private static void InsertVariableOnLetHead(string tempFolder)
        {

            string filename = Common.PathCombine(tempFolder, "content.xml");
            XmlDocument xdoc = Common.DeclareXMLDocument(true);
            FileStream fs = File.OpenRead(filename);
            xdoc.Load(fs);
            fs.Close();
            xdoc.Load(filename);

            var nsmgr1 = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr1.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            string xpath = "//text:p[@text:style-name=\"letter_letHead_dicBody\"]";

            XmlNodeList list = xdoc.SelectNodes(xpath, nsmgr1);
            if (list.Count > 0)
            {
                foreach (XmlNode VARIABLE in list)
                {
                    if (VARIABLE.ParentNode.NextSibling != null)
                    {
                        XmlNode nde = VARIABLE.ParentNode.NextSibling.CloneNode(true);


                        string xpath1 = "//text:span/text:variable-set[@text:name=\"Left_Guideword_L\"]";
                        XmlNodeList list1 = nde.SelectNodes(xpath1, nsmgr1);
                        if (list1 != null)
                            if (list1.Count > 0)
                            {
                                VARIABLE.AppendChild(list1[0].CloneNode(true));
                            }
                    }
                }
            }
            xdoc.PreserveWhitespace = true;
            xdoc.Save(filename);
        }

        private string GetReferenceFormat(Dictionary<string, Dictionary<string, string>> idAllClass, string refFormat)
        {
            if (idAllClass.ContainsKey("ReferenceFormat"))
                if (idAllClass["ReferenceFormat"].ContainsKey("@page"))
                {
                    refFormat = idAllClass["ReferenceFormat"]["@page"];
                }
                else if (idAllClass["ReferenceFormat"].ContainsKey("@page:left"))
                {
                    refFormat = idAllClass["ReferenceFormat"]["@page:left"];
                }
                else if (idAllClass["ReferenceFormat"].ContainsKey("@page:right"))
                {
                    refFormat = idAllClass["ReferenceFormat"]["@page:right"];
                }
            return refFormat;
        }

        private static void MoveStylesToContent(string strStylePath, string strContentPath)
        {
            string[] s =  {
             "GuideL",
             "GuideR",
             "headword",
             "headwordminor",
              "entry_letData",
             "letter",
             "homographnumber"
            };

            List<string> macroList = new List<string>();
            macroList.AddRange(s);

            var doc = new XmlDocument();
            doc.Load(strStylePath);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("st", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr.AddNamespace("of", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");

            // if new stylename exists
            XmlElement root1 = doc.DocumentElement;
            string officeNode = "//of:styles";
            string allStyles = string.Empty;

            XmlNode node = root1.SelectSingleNode(officeNode, nsmgr); // work

            if (node != null)
            {
                int nodeCount = node.ChildNodes.Count;
                for (int i = 0; i < nodeCount; i++)
                {

                    XmlNode name = node.ChildNodes[i].Attributes.GetNamedItem("name", nsmgr.LookupNamespace("st"));
                    if (name != null)
                    {
                        if (name.Value.ToLower() != "entry_letdata" && name.Value.ToLower() != "Text_20_body")
                        {
                            allStyles += node.ChildNodes[i].OuterXml;
                            node.ChildNodes[i].ParentNode.RemoveChild(node.ChildNodes[i]);

                            i--;
                            nodeCount--;
                        }
                    }
                }
            }
            doc.Save(strStylePath);

            var docContent = new XmlDocument();
            docContent.Load(strContentPath);
            var nsmgr1 = new XmlNamespaceManager(docContent.NameTable);
            nsmgr1.AddNamespace("st", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr1.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            nsmgr1.AddNamespace("of", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");


            // if new stylename exists
            XmlElement root2 = docContent.DocumentElement;
            string officeNode1 = "//of:automatic-styles";
            if (root2 != null)
            {
                XmlNode node1 = root2.SelectSingleNode(officeNode1, nsmgr); 
                if (node1 != null)
                {
                    node1.InnerXml = node1.InnerXml + allStyles;
               }
                docContent.Save(strContentPath);
            }
        }

        private static void IncludeTextinMacro(string strMacroPath, string ReferenceFormat, string saveAsPath, bool runMacroFirstTime, string isCoverImageInserted, string isToc)
        {
    	    var xmldoc = Common.DeclareXMLDocument(true);
            xmldoc.Load(strMacroPath);
            XmlElement ele = xmldoc.DocumentElement;
            string autoMacro = "False";
            if (Param.Value.Count > 0)
            {
                if (Param.Value[Param.ExtraProcessing] != "ExtraProcessing")
                {
                    autoMacro = Param.Value[Param.ExtraProcessing];
                }
            }
            if (ele != null)
            {
                
                string seperator = "\n";
                string line1 = string.Empty;
                if (publicationInfo.ProjectInputType.ToLower() == "scripture")
                {
                    line1 = "\n'Constant ReferenceFormat for User Desire\nConst ReferenceFormat = \"" +
                            ReferenceFormat + "\"";
                }
                line1 = line1 + "\nConst AutoMacro = \"" + autoMacro + "\"";
                string line2 = "\nConst OutputFormat = \"" + publicationInfo.FinalOutput + "\"" +
                               "\nConst FilePath = \"" + saveAsPath + "\"" + "\nConst IsPreview = \"" +
                               publicationInfo.JpgPreview + "\"";
                string line3 = "\nConst RunMacroFirstTime = \"" + runMacroFirstTime + "\"";
                string line4 = "\nConst IsCoverImageInserted = \"" + isCoverImageInserted + "\"";
                string line5 = "\nConst IsTOC = \"" + isToc + "\"";
                string combined = line1 + line2 + line3 + line4 + line5 + seperator;

                ele.InnerText = combined + ele.InnerText;
            }
            xmldoc.Save(strMacroPath);
        }


        
        #endregion

        #region Private Functions
        /// <summary>
        /// To copy Temporary Office files to Environmental Temp Folder instead of keeping changes in Application Itself.
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        private void CopyOfficeFolder(string sourceFolder, string destFolder)
        {
            if (Directory.Exists(destFolder))
            {
                DirectoryInfo di = new DirectoryInfo(destFolder);
                Common.CleanDirectory(di);
            }
            Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            try
            {
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Common.PathCombine(destFolder, name);
                    File.Copy(file, dest, true);
                }

                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Common.PathCombine(destFolder, name);
                    if (name != ".svn")
                    {
                        CopyOfficeFolder(folder, dest);
                    }
                }
            }
            catch
            {
                return;
            }
        }
        #endregion
    }
}
