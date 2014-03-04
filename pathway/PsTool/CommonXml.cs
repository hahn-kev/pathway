﻿// --------------------------------------------------------------------------------------------
// <copyright file="CommonXml.cs" from='2009' to='2014' company='SIL International'>
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
// Library for Pathway
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.Win32;

namespace SIL.Tool
{
    public static partial class Common
    {
        #region SetProgressBarValue(ProgressBar pb, string sourcefile)
        /// <summary>
        /// Find the numbe of tags in a given XML
        /// </summary>
        /// <param name="pb">Progress bar</param>
        /// <param name="sourcefile">Source XML File</param>
        public static void SetProgressBarValue(ProgressBar pb, string sourcefile)
        {
            if (pb == null) return;
            int setValue = 0;
            if (File.Exists(sourcefile))
            {
                var streamReader = new StreamReader(sourcefile);
                string text = string.Empty;
                try
                {
                    //throw new System.OutOfMemoryException(" ");
                    text = streamReader.ReadToEnd();
                }
                catch (OutOfMemoryException ex)
                {
                    Console.Write(ex.Message);
                }
                streamReader.Close();
                var reg = new Regex("</", RegexOptions.Multiline);
                MatchCollection mat = reg.Matches(text);
                setValue = mat.Count;
            }
            pb.Minimum = 0;
            pb.Maximum = setValue;
            pb.Visible = true;
        }
        #endregion

        #region ExportedCss(string xhtmlName)
        /// <summary>
        /// Return the name of the stylesheet name in the link tag.
        /// </summary>
        /// <param name="xhtmlName">full path name of XHTML file to process</param>
        public static string GetLinkedCSS(string xhtmlName)
        {
            if (!File.Exists(xhtmlName))
            {
                return "";
            }

            string cssFile = string.Empty;
            StreamReader reader = File.OpenText(xhtmlName);
            if (!reader.EndOfStream)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = Regex.Match(line, "<link([^>]*)>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        Match match1 = Regex.Match(match.Groups[1].Value, "stylesheet", RegexOptions.IgnoreCase);
                        if (match1.Success)
                        {
                            Match match2 = Regex.Match(match.Groups[1].Value, "href=\"([^\"]*)", RegexOptions.IgnoreCase);
                            if (match2.Success)
                            {
                                reader.Dispose();
                                cssFile = match2.Groups[1].Value;
                                break;
                            }
                        }
                        else
                        {
                            Match match3 = Regex.Match(match.Groups[1].Value, "type=\"text/css\"", RegexOptions.IgnoreCase);
                            if (match3.Success)
                            {
                                Match match4 = Regex.Match(match.Groups[1].Value, "rel=\"([^\"]*)", RegexOptions.IgnoreCase);
                                if (match4.Success)
                                {
                                    reader.Dispose();
                                    cssFile = match4.Groups[1].Value;
                                    break;
                                }
                            }
                        }
                    }
                    Match match5 = Regex.Match(line, "</head>", RegexOptions.IgnoreCase);
                    if (match5.Success)
                    {
                        break;
                    }
                }
                reader.Dispose();
            }
            return cssFile;
        }
        #endregion

        /// <summary>
        /// Meta Value returning Function
        /// </summary>
        /// <param name="fileName">Source File to Read</param>
        /// <returns>The Meta External Link Root Director Name</returns>
        public static string GetMetaValue(string fileName)
        {
            if (!File.Exists(fileName)) return string.Empty;
            string metaName = string.Empty;
            XmlTextReader reader = Common.DeclareXmlTextReader(fileName, true);
            if (Common.IsUnixOS())
            {
                while (reader.Read())
                {
                    if (reader.Name == "meta")
                    {
                        if (reader.GetAttribute("name") != null)
                        {
                            metaName = reader.GetAttribute("name");
                            if (metaName.IndexOf("RootDir") >= 0)
                            {
                                if (reader.GetAttribute("content") != null)
                                {
                                    metaName = reader.GetAttribute("content");
                                    if (Common.UnixVersionCheck() && metaName == null)
                                    {
                                        metaName = Common.GetAllUserAppPath();
                                        metaName = Common.RightRemove(metaName, "sil");
                                        metaName = Common.PathCombine(metaName, "fieldworks");
                                    }
                                    else if (metaName.IndexOf("file://") >= 0) // from the file access
                                    {
                                        const int designatorLength = 7;
                                        metaName = metaName.Substring(designatorLength,
                                                                      metaName.Length - designatorLength);
                                    }
                                    break;
                                }

                            }
                        }
                    }
                    // Check only in the header and retrun 
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "head")
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                while (reader.Read())
                {
                    if (reader.IsEmptyElement)
                    {
                        if (string.Compare(reader.Name, "meta", true) >= 0)
                        {
                            if (reader.GetAttribute("name") != null)
                            {
                                metaName = reader.GetAttribute("name");
                                if (metaName.IndexOf("RootDir") >= 0)
                                {
                                    if (reader.GetAttribute("content") != null)
                                    {
                                        metaName = reader.GetAttribute("content");
                                        if (Common.UnixVersionCheck())
                                        {
                                            metaName = Common.GetAllUserAppPath();
                                            metaName = Common.RightRemove(metaName, "sil");
                                            metaName = Common.PathCombine(metaName, "fieldworks");
                                        }
                                        else if (metaName.IndexOf("file://") >= 0) // from the file access
                                        {
                                            const int designatorLength = 7;
                                            metaName = metaName.Substring(designatorLength,
                                                                          metaName.Length - designatorLength);
                                        }
                                        break;
                                    }

                                }
                            }
                        }
                    }
                    // Check only in the header and retrun 
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (string.Compare(reader.Name, "head", true) >= 0)
                        {
                            break;
                        }
                    }
                }
            }
            reader.Close();
            return metaName;
        }


        private static Dictionary<string, string> _metaDataDic = null;
        /// <summary>
        /// Get MetaData information from DictionaryStyleSettings.xml/ScriptureStyleSettings.xml
        /// </summary>
        /// <param name="_projectInputType">Dictionary / Scripture</param>
        /// <param name="path"></param>
        /// <returns>The metaDataList information</returns>
        public static Dictionary<string, string> GetMetaData(string _projectInputType, string metaDataFull)
        {
            if (_projectInputType.Length == 0)
                _projectInputType = "Dictionary";

            List<string> metaDataList = new List<string>();
            metaDataList.Add("Title");
            metaDataList.Add("Creator");
            metaDataList.Add("Publisher");
            metaDataList.Add("Description");
            metaDataList.Add("Copyright Holder");
            metaDataList.Add("Subject");

            _metaDataDic = new Dictionary<string, string>();
            foreach (string meta in metaDataList)
            {
                _metaDataDic[meta] = string.Empty;
            }

            string metaData = _projectInputType.ToLower() == "scripture" ? "ScriptureStyleSettings.xml" : "DictionaryStyleSettings.xml";

            if (metaDataFull == string.Empty)
            {
                metaDataFull = Common.PathCombine(Common.PathCombine(Common.PathCombine(Common.PathCombine(GetAllUserAppPath(), "SIL"), "Pathway"), _projectInputType), metaData);
            }

            if (!File.Exists(metaDataFull)) return _metaDataDic;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(metaDataFull);
            XmlNode root = xmlDocument.DocumentElement;
            if (root == null) return _metaDataDic;
            string xPath = "//Metadata/meta";
            XmlNodeList returnNode = root.SelectNodes(xPath);

            foreach (XmlNode node in returnNode)
            {
                string metaName = node.Attributes[0].Value;
                if (metaDataList.Contains(metaName))
                {
                    _metaDataDic[metaName] = XmlConvert.DecodeName(node.LastChild.InnerText.Trim());
                }
            }
            return _metaDataDic;
        }

        /// <summary>
        /// Extracts the HTML img Tag from the Tag and returns it
        /// </summary>
        /// <param name="stringContent">The stringContent holds the string to be extracted</param>
        /// <returns>String Array that contains the img Tags.</returns>
        public static string[] ReturnImageSource(string stringContent)
        {
            const string pattern = @"<img(.|\n)*?/>";
            var reg = new Regex(pattern, RegexOptions.Multiline);
            MatchCollection mc = reg.Matches(stringContent);
            var results = new string[mc.Count];
            for (int i = 0; i < mc.Count; i++)
            {
                string sub = mc[i].Value;
                if (sub.IndexOf("src") >= 0)
                {
                    try
                    {
                        sub = sub.Substring(sub.IndexOf("src=") + 4);
                        sub = sub.Substring(sub.IndexOf("\"") + 1);
                        sub = sub.Substring(0, sub.IndexOf("\""));
                    }
                    catch
                    {
                        sub = string.Empty;
                    }
                    results[i] = sub;
                }
            }
            return results;
        }

        /// <summary>
        /// Base Value returning Function
        /// </summary>
        /// <param name="fileName">Source File to Read</param>
        /// <returns>The Meta External Link Root Director Name</returns>
        public static string GetBaseValue(string fileName)
        {
            string baseValue = string.Empty;
            try
            {
                var doc = Common.DeclareXMLDocument(true);
                if (!File.Exists(fileName)) return string.Empty;
                doc.Load(fileName);
                XmlElement root = doc.DocumentElement;
                if (root != null)
                    if (root.HasChildNodes)
                    {
                        foreach (XmlNode xmlNode in root.ChildNodes)
                        {
                            if (xmlNode.Name == "head")
                            {
                                foreach (XmlNode child in xmlNode.ChildNodes)
                                {
                                    if (child.Name == "base")
                                    {
                                        baseValue = child.Attributes["href"].ToString();
                                        if (baseValue.IndexOf("file://") >= 0)  // from the file access
                                        {
                                            const int designatorLength = 7;
                                            baseValue = baseValue.Substring(designatorLength, baseValue.Length - designatorLength);
                                        }
                                        if (baseValue.Length > 0)
                                        {
                                            xmlNode.ParentNode.RemoveChild(child); // Remove the base Node, to avoid the conflict
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                doc.Save(fileName);
            }
            catch
            {
            }
            return baseValue;
        }

        /// <summary>
        /// Removing the empty anchor tags <a href="sample"/>
        /// </summary>
        /// <param name="sourceFile">The Xhtml File</param>
        private static string RemoveEmptyAnchorTag(string sourceFile)
        {
            const string tag = "a";
            var xDoc = new XmlDocument { XmlResolver = null };
            xDoc.Load(sourceFile);
            XmlNodeList nodeList = xDoc.GetElementsByTagName(tag);
            if (nodeList.Count > 0)
            {
                while (true)
                {
                    bool removed = false;
                    foreach (XmlNode item in nodeList)
                    {
                        item.ParentNode.RemoveChild(item);
                        removed = true;
                        break;
                    }
                    if (!removed)
                        break;
                }
                xDoc.Save(sourceFile);
            }
            return sourceFile;
        }

        /// <summary>
        /// Checks all the Paths of image and returns the Path
        /// </summary>
        /// <param name="src">Image Source </param>
        /// <param name="metaname">Metaname from XHTML</param>
        /// <param name="sourcePicturePath">Source Dictionary path</param>
        /// <returns>The Path where picture exist</returns>
        public static string GetPictureFromPath(string src, string metaName, string sourcePicturePath)
        {
            string fileName;
            const int designatorLength = 7;
            string dictPath = sourcePicturePath;
            string fromPath = ImageSource(src, designatorLength, dictPath);

            //if not found in any Path then search in Meta Data Path.
            //Meta Value - Substitution
            if (fromPath == string.Empty && metaName.Length > 0)
            {
                string rootPath = Path.GetPathRoot(metaName);
                if (rootPath.Length > 0)
                {
                    //Server Path ex: \\servername\foldername and from the Drive ex: c:\\ 
                    fileName = PathCombine(metaName, src);
                    if (File.Exists(fileName))
                    {
                        fromPath = fileName;
                    }
                }
                else  // from the Folder 
                {
                    string srcFileName = src;
                    if (src.IndexOf("file://") >= 0)  // File contains
                    {
                        srcFileName = Path.GetFileName(src);
                    }

                    fileName = PathCombine(dictPath, PathCombine(metaName, srcFileName));  // local +  meta folder
                    if (File.Exists(fileName))
                    {
                        fromPath = fileName;
                    }
                }
            }
            //For SIL / Fieldworks Path.
            if (fromPath == string.Empty)
            {
                if (!File.Exists(fromPath))
                {
                    fileName = Path.GetFileName(src); // sil + fileName
                    string flexPict = PathCombine(GetFiledWorksPath(), fileName);
                    if (File.Exists(flexPict))
                    {
                        fromPath = flexPict;
                    }
                    else
                    {   // sil + picture(folder) + fileName
                        flexPict = PathCombine(GetFiledWorksPath(), PathCombine("Pictures", fileName));
                        if (File.Exists(flexPict))
                        {
                            fromPath = flexPict;
                        }
                        else
                        {   // sil + fileName with exact sourceFolder path
                            flexPict = PathCombine(GetFiledWorksPath(), src);
                            if (File.Exists(flexPict))
                            {
                                fromPath = flexPict;
                            }
                        }

                    }
                }
            }

            if (Testing) return fromPath; // Linux Test, Registry error

            //For ParaText Path.
            if (fromPath == string.Empty)
            {
                if (!Common.UnixVersionCheck())
                {
                    string databaseNamePara = databaseName; // "NKOu1"; // todo substitute for database name
                    string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\ScrChecks\1.0\Settings_Directory";
                    object paraPath1 = Registry.GetValue(key, "", "") ?? string.Empty;
                    string paraPath = paraPath1.ToString();
                    string dataPath = PathCombine(paraPath, databaseNamePara);
                    fileName = Path.GetFileName(src); // para + database + fileName
                    string flexPict = PathCombine(dataPath, fileName);
                    if (File.Exists(flexPict))
                    {
                        fromPath = flexPict;
                    }
                    else
                    {
                        // Note: In ParaText original files are stored in {any Drive\My Paratext Projects\{project name}\local\figures}
                        // Note: The converted jpg files are stored under figures folder.

                        // para + database + figures(folder) + fileName
                        flexPict = PathCombine(dataPath, PathCombine("figures", fileName));
                        string flexJPGPath = flexPict;
                        if (Path.GetExtension(flexJPGPath).ToLower() != "jpg") // jpg files need no conversion
                        {
                            flexJPGPath = Path.ChangeExtension(flexPict, "jpg");
                        }
                        if (File.Exists(flexJPGPath))
                        {
                            fromPath = flexJPGPath;
                        }
                        else
                        {
                            if (File.Exists(flexPict))
                            {
                                fromPath = flexPict;
                            }
                            else
                            {
                                // para + database + local(folder) + figures(folder) + fileName
                                flexPict = PathCombine(dataPath, PathCombine("local", PathCombine("figures", fileName)));
                                if (File.Exists(flexPict))
                                {
                                    fromPath = flexPict;
                                }
                                else
                                {
                                    // para + database + fileName with exact sourceFolder path
                                    flexPict = PathCombine(dataPath, src);
                                    if (File.Exists(flexPict))
                                    {
                                        fromPath = flexPict;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string dataPath = Path.GetDirectoryName(src);
                    fileName = Path.GetFileName(src); // para + database + fileName
                    dataPath = Common.PathCombine(dataPath, databaseName);
                    string pictureFileName = PathCombine(dataPath, fileName);
                    if (File.Exists(pictureFileName))
                    {
                        fromPath = pictureFileName;
                    }
                    else
                    {
                        // Note: In ParaText original files are stored in {any Drive\My Paratext Projects\{project name}\local\figures}
                        // Note: The converted jpg files are stored under figures folder.

                        // para + database + figures(folder) + fileName
                        pictureFileName = PathCombine(dataPath, PathCombine("figures", fileName));
                        string flexJPGPath = pictureFileName;
                        if (Path.GetExtension(flexJPGPath).ToLower() != "jpg") // jpg files need no conversion
                        {
                            flexJPGPath = Path.ChangeExtension(pictureFileName, "jpg");
                        }
                        if (File.Exists(flexJPGPath))
                        {
                            fromPath = flexJPGPath;
                        }
                        else
                        {
                            if (File.Exists(pictureFileName))
                            {
                                fromPath = pictureFileName;
                            }
                            else
                            {
                                // para + database + local(folder) + figures(folder) + fileName
                                pictureFileName = PathCombine(dataPath, PathCombine("local", PathCombine("figures", fileName)));
                                if (File.Exists(pictureFileName))
                                {
                                    fromPath = pictureFileName;
                                }
                                else
                                {
                                    // para + database + fileName with exact sourceFolder path
                                    pictureFileName = PathCombine(dataPath, src);
                                    if (File.Exists(pictureFileName))
                                    {
                                        fromPath = pictureFileName;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return fromPath;
        }

        private static string ImageSource(string src, int designatorLength, string dictPath)
        {
            string fileName = string.Empty;
            string fromPath = string.Empty;
            if (src.IndexOf("file://") >= 0)  // from the file access
            {
                fileName = PathCombine(dictPath, Path.GetFileName(src));
                // search in local Folder
                if (File.Exists(fileName))
                {
                    fromPath = fileName;
                }
                else
                {
                    // search in local Pictures Folder
                    fileName = PathCombine("Pictures", Path.GetFileName(src));
                    fileName = PathCombine(dictPath, fileName);
                    // search in local Folder
                    if (File.Exists(fileName))
                    {
                        fromPath = fileName;
                    }
                    else
                    {
                        // search in Exact folder 
                        fileName = src.Substring(designatorLength, src.Length - designatorLength);
                        if (File.Exists(fileName))
                        {
                            fromPath = fileName;
                        }
                    }
                }
            }
            else // search in local exact folder
            {
                string localPath = PathCombine(dictPath, src);
                if (File.Exists(localPath))
                {
                    fromPath = localPath;
                }
                else
                {
                    string srcFileName = Path.GetFileName(src);
                    fileName = PathCombine(dictPath, PathCombine("Pictures", srcFileName));  // local +  "picture" + fileName
                    if (File.Exists(fileName))
                    {
                        fromPath = fileName;
                    }
                }
            }
            return fromPath;
        }

        /// <summary>
        /// Set .CSS file in .XHTML file
        /// </summary>
        /// <param name="xhtmlFile">XHTML Filename with path</param>
        /// <param name="defaultCSS">CSS name</param>
        public static void SetDefaultCSS(string xhtmlFile, string defaultCSS)
        {
            if (!File.Exists(xhtmlFile) || string.IsNullOrEmpty(defaultCSS))
            {
                return;
            }

            string cssFileName = GetLinkedCSS(xhtmlFile);
            if (string.Compare(cssFileName, defaultCSS, true) == 0)
            {
                return;
            }
            var xmldoc = Common.DeclareXMLDocument(true);
            xmldoc.Load(xhtmlFile);
            XmlNodeList headnodes = xmldoc.GetElementsByTagName("head");
            XmlNode headnode = headnodes[0];
            XmlNode newNode;
            XmlNodeList findnodes = xmldoc.GetElementsByTagName("link");
            if (findnodes.Count > 0)
            {
                newNode = findnodes[0].Clone();
                if (newNode.Attributes.Count > 0)
                    newNode.Attributes.RemoveAll();
                if (newNode.ChildNodes.Count > 0)
                    newNode.RemoveAll();

                int countChild = findnodes.Count;
                for (int i = 0; i < countChild; i++)
                {
                    headnode.RemoveChild(findnodes[0]);
                    break;
                }
            }
            else
            {

                newNode = xmldoc.CreateElement("link");
            }

            XmlAttribute xmlAttrib = xmldoc.CreateAttribute("type");
            xmlAttrib.Value = "text/css";
            newNode.Attributes.Append(xmlAttrib);

            xmlAttrib = xmldoc.CreateAttribute("rel");
            xmlAttrib.Value = "stylesheet";
            newNode.Attributes.Append(xmlAttrib);

            xmlAttrib = xmldoc.CreateAttribute("href");
            xmlAttrib.Value = defaultCSS;
            newNode.Attributes.Append(xmlAttrib);

            headnode.AppendChild(newNode);

            xmldoc.Save(xhtmlFile);

        }

        #region GetXmlNode
        /// <summary>
        /// Returns XML Node in the file based on the xpath
        /// XmlNode = GetXmlNode("c:\en.xml", "\\book[id = 10]")
        /// </summary>
        /// <param name="xmlFileNameWithPath">File Name</param>
        /// <param name="xPath">Xpath for the XML Node</param>
        /// <returns></returns>
        public static XmlNode GetXmlNode(string xmlFileNameWithPath, string xPath)
        {
            XmlDocument xmlDoc = Common.DeclareXMLDocument(true);
            xmlDoc.PreserveWhitespace = false;
            xmlFileNameWithPath = DirectoryPathReplace(xmlFileNameWithPath);
            if (!File.Exists(xmlFileNameWithPath))
            {
                return null;
            }
            xmlDoc.Load(xmlFileNameWithPath);
            XmlElement root = xmlDoc.DocumentElement;
            if (root != null)
            {
                XmlNode returnNode = root.SelectSingleNode(xPath);
                return returnNode;
            }
            return null;
        }
        #endregion

        #region GetXmlNodes
        /// <summary>
        /// Returns XmlNodeList 
        /// </summary>
        /// <param name="xmlFileNameWithPath">File Name</param>
        /// <param name="xPath">Xpath for the XML Node</param>
        /// <returns>Returns XmlNodeList</returns>
        public static XmlNodeList GetXmlNodes(string xmlFileNameWithPath, string xPath)
        {
            XmlNodeList resultNodeList = null;
            XmlNode resultNode = GetXmlNode(xmlFileNameWithPath, xPath);
            if (resultNode != null)
            {
                resultNodeList = resultNode.ChildNodes;
            }
            return resultNodeList;


        }
        #endregion

        #region CreateXMLFile
        /// <summary>
        /// Returns XmlTextWriter 
        /// </summary>
        /// <param name="xmlFileNameWithPath">File Name</param>
        /// <returns>Returns XmlTextWriter</returns>
        public static XmlTextWriter CreateXMLFile(string xmlFileNameWithPath)
        {
            XmlTextWriter writer = new XmlTextWriter(xmlFileNameWithPath, null) { Formatting = Formatting.Indented };
            return writer;
        }
        #endregion

        #region GetXmlNode
        /// <summary>
        /// Returns ArrayList Example: Apple, Ball
        /// </summary>
        /// <param name="xmlFileNameWithPath">File Name</param>
        /// <param name="xPath">Xpath for the XML Node</param>
        /// <returns>Returns ArrayList Example: Apple, Ball</returns>
        public static ArrayList GetXmlNodeList(string xmlFileNameWithPath, string xPath)
        {
            ArrayList dataList = new ArrayList();
            XmlNode resultNode = GetXmlNode(xmlFileNameWithPath, xPath);
            if (resultNode != null)
            {
                foreach (XmlNode node in resultNode.ChildNodes)
                {
                    if (node.NodeType.ToString() == "Whitespace") continue;
                    dataList.Add(node.InnerText);
                }
            }
            return dataList;
        }
        #endregion

        #region XsltProcess(string inputFile, string xsltFile, string ext)
        public static ProgressBar XsltProgressBar = null;
        public static Formatting XsltFormat = Formatting.None;
        public static bool IncludeUtf8BomIdentifier = true;

        public static string XsltProcess(string inputFile, string xsltFile, string ext)
        {
            return XsltProcess(inputFile, xsltFile, ext, null);
        }
        /// <summary>
        /// Process Input file with xslt and produce result with same name as input but extension changed.
        /// </summary>
        /// <param name="inputFile">input file</param>
        /// <param name="xsltFile">xslt file name. It is assumed to be in Dictionary Express folder</param>
        /// <param name="ext">new extension</param>
        /// <param name="myParams">pass a dictionary of parameters and values</param>
        /// <returns>results or error message</returns>
        public static string XsltProcess(string inputFile, string xsltFile, string ext, Dictionary<string, string> myParams)
        {
            if (!File.Exists(inputFile))
                return string.Empty;

            if (!File.Exists(xsltFile))
                return inputFile;

            if (string.IsNullOrEmpty(ext))
                ext = ".xhtml";

            try
            {
                string path = GetPSApplicationPath();
                string outputPath = Path.GetDirectoryName(inputFile);
                string result = PathCombine(outputPath, Path.GetFileNameWithoutExtension(inputFile) + ext);

                if (File.Exists(result))
                {
                    File.Delete(result);
                }

                string xsltPath = PathCombine(path, xsltFile);

                //Create the XslCompiledTransform and load the stylesheet.
                var xsltReader = XmlReader.Create(xsltPath);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xsltReader.NameTable);
                namespaceManager.AddNamespace("x", "http://www.w3.org/1999/xhtml");
                namespaceManager.AddNamespace("fn", "http://www.w3.org/2005/xpath-functions");
                var xslt = new XslCompiledTransform();
                var xsltTransformSettings = new XsltSettings { EnableDocumentFunction = true };
                xslt.Load(xsltReader, xsltTransformSettings, null);
                xsltReader.Close();

                //Create an XsltArgumentList.
                var xslArg = new XsltArgumentList();

                //Add an object 
                var obj = new FlexString();
                var fun = new XmlFun(); // string-length replaed with stringLength
                xslArg.AddExtensionObject("urn:reversal-conv", obj);
                xslArg.AddExtensionObject("http://www.w3.org/2005/xpath-functions", fun);

                if (myParams != null)
                    foreach (string param in myParams.Keys)
                    {
                        xslArg.AddParam(param, "", myParams[param]);
                    }

                //Transform the file. and writing to temporary File
                var setting = new XmlReaderSettings { ProhibitDtd = false, XmlResolver = null };
                XmlReader reader = XmlReader.Create(inputFile, setting);
                var writerSettings = new XmlWriterSettings();
                if (!IncludeUtf8BomIdentifier || !ext.ToLower().Contains("xhtml"))
                {
                    writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
                }
                
                if (IncludeUtf8BomIdentifier)
                {
                    writerSettings.Encoding = Encoding.UTF8;
                }
                else
                {
                    writerSettings.Encoding = new UTF8Encoding(IncludeUtf8BomIdentifier);
                    IncludeUtf8BomIdentifier = true;   // reset to true for next time if it has been changed
                }
                
                if (XsltFormat == Formatting.Indented)
                {
                    writerSettings.Indent = true;
                    XsltFormat = Formatting.None;       // reset to None for next time if it has been changed
                }
                
                var writer = XmlWriter.Create(result, writerSettings);
                if (ext.ToLower().Contains("xhtml"))
                {
                    writer.WriteStartDocument();
                }
               
                xslt.Transform(reader, xslArg, writer);
                writer.Close();
                reader.Close();
                return result;
            }
            catch (FileNotFoundException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }
        }
        #endregion

        #region GetProjectType
        /// <summary>
        /// To find the type of input file(Dictionary or Scripture) based on the Keywords.
        /// </summary>
        /// <param name="xhtmlPath">XHTML file path name</param>
        /// <returns>returns the Project type</returns>
        public static string GetProjectType(string xhtmlPath)
        {
            string projectType = "Dictionary";
            if (!File.Exists(xhtmlPath))
                return projectType;

            string cssPath = GetLinkedCSS(xhtmlPath);
            if (!File.Exists(cssPath))
            {
                string xhtmlDirectory = Path.GetDirectoryName(xhtmlPath);
                cssPath = PathCombine(xhtmlDirectory, cssPath);
            }
            if (!string.IsNullOrEmpty(cssPath) && File.Exists(cssPath))
            {
                string fstr = ReadFiletoEnd(cssPath);
                if (fstr.Contains(".footnote") || fstr.Contains(".chapter_number") || fstr.Contains(".chapternumber") || fstr.Contains(".verse_number") || fstr.Contains(".versenumber"))
                {
                    projectType = "Scripture";
                }
            }
            else
            {
                string fstr = ReadFiletoEnd(xhtmlPath);
                if (fstr.Contains("chapter_number") || fstr.Contains("verse_number"))
                {
                    projectType = "Scripture";
                }
            }
            return projectType;
        }

        private static string ReadFiletoEnd(string filePath)
        {
            var fsRead = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(fsRead);
            string fstr = reader.ReadToEnd().ToLower();
            reader.Close();
            return fstr;
        }

        #endregion GetProjectType

        #region PicturePathAssign
        /// <summary>
        /// PicturePathAssign is used for preview of the xhtml files
        /// </summary>
        /// <param name="xn">XML Node</param>
        /// <param name="sourcePath">Source Path</param>
        public static void PicturePathAssign(XmlNode xn, string sourcePath)
        {
            foreach (XmlNode childNode in xn.ChildNodes)
            {
                if (childNode.Name == "img" && childNode.Attributes.Count > 0)
                {
                    for (int i = 0; i < childNode.Attributes.Count; i++)
                    {
                        if (childNode.Attributes[i].Name == "src")
                        {
                            string fromPath;
                            string source = childNode.Attributes[i].Value;
                            if (source.IndexOf("file://") >= 0)
                            {
                                fromPath = source;
                            }
                            else
                            {
                                fromPath = PathCombine(sourcePath, source);
                                if (!File.Exists(fromPath))
                                {
                                    string flexPict = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"/SIL/FieldWorks/" + source;
                                    if (File.Exists(flexPict))
                                    {
                                        fromPath = flexPict;
                                    }
                                }
                            }
                            childNode.Attributes[i].Value = fromPath;
                        }
                    }
                }
                if (childNode.HasChildNodes)
                {
                    PicturePathAssign(childNode, sourcePath);
                }
            }
        }
        #endregion PicturePathAssign

        #region GetCountryCode(string language, string country, string langCountry, Dictionary spellCheck)
        /// <summary>
        /// GetCountryCode(out language, out country, "en_US", Library.spellCheck)
        /// </summary>
        /// <param name="language">Return Language</param>
        /// <param name="country">Return Country</param>
        /// <param name="langCountry">Language and Country or Languaue Ex: en_US or en</param>
        /// <param name="spellCheck">OO Spelling Dictionary</param>
        public static void GetCountryCode(out string language, out string country, string langCountry, Dictionary<string, ArrayList> spellCheck)
        {
            country = "none";
            language = "zxx";   //Disable spelling checking in Libre Office - this code means language is unknown.
            string[] langCoun = langCountry.Split('-');

            try
            {
                if (langCoun.Length < 2)
                {
                    var wsPath = PathCombine(GetAllUserAppPath(), "SIL/WritingSystemStore/" + langCoun[0] + ".ldml");
                    if (File.Exists(wsPath))
                    {
                        var ldml = Common.DeclareXMLDocument(true);
                        ldml.Load(wsPath);
                        var nsmgr = new XmlNamespaceManager(ldml.NameTable);
                        nsmgr.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
                        var node = ldml.SelectSingleNode("//palaso:spellCheckingId/@value", nsmgr);
                        if (node != null)
                            langCoun = node.Value.Split('_');
                    }
                }
                if (spellCheck.ContainsKey(langCoun[0]))
                {
                    language = langCoun[0];
                    if (langCoun.Length == 1)
                    {
                        string path = PathCombine(GetFiledWorksPath(), "Languages");
                        string f7Path = GetAllUserAppPath() + @"/SIL/FieldWorks 7";
                        string xPath = "//SpellCheckDictionary24/Uni";
                        if (Directory.Exists(f7Path))
                        {
                            path = PathCombine(f7Path, "Projects\\" + databaseName + "\\WritingSystemStore");
                            path = PathCombine(path, language + ".ldml");
                            xPath = "/ldml/special[1]/*[namespace-uri()='urn://palaso.org/ldmlExtensions/v1' and local-name()='spellCheckingId'][1]/@value";
                        }
                        else
                        {
                            path = PathCombine(path, language + ".xml");
                        }
                        if (File.Exists(path))
                        {
                            XmlNode node = GetXmlNode(path, xPath);
                            if (node != null)
                            {
                                country = node.InnerText.ToUpper();
                                country = RightString(country, "_");
                                if (country == "EN")
                                {
                                    var cultureName = Application.CurrentCulture.TextInfo.CultureName.Split('-');
                                    if (cultureName[0].ToUpper() == "EN")
                                        country = cultureName[1];
                                }
                            }
                        }
                        if (country.ToLower() == "none" || country == "<None>")
                        {
                            ArrayList countryList = spellCheck[language];
                            string currentCulture = RightString(Application.CurrentCulture.IetfLanguageTag, "-");
                            if (countryList != null && countryList.Contains(currentCulture))
                            {
                                country = currentCulture;
                            }
                            else if (countryList != null)
                            {
                                country = countryList[0].ToString();
                            }
                        }
                    }
                    else
                    {
                        country = langCoun[1].ToUpper();
                    }
                }

            }
            catch
            {
            }
        }
        #endregion


        /// <summary>
        /// This method splits the input xhtml into different files 
        /// and stores in the temp path using the class name to split the file
        /// 
        /// </summary>
        /// <param name="xhtmlFileWithPath">The input Xhtml File </param>
        /// <param name="bookSplitterClass">The class name to split the class</param>
        /// <param name = "adjacentClass"></param>
        /// <returns>The entire path of the splitted files are retured as List</returns>
        public static List<string> SplitXhtmlFile(string xhtmlFileWithPath, string bookSplitterClass, bool adjacentClass)
        {
            return SplitXhtmlFile(xhtmlFileWithPath, bookSplitterClass, "PartFile", adjacentClass);
        }


        /// <summary>
        /// This method splits the input xhtml into different files 
        /// and stores in the temp path using the class name to split the file. This override allows you
        /// to specify the resulting filename prefix (e.g., "PartFile" in "PartFile1.xhtml").
        /// 
        /// </summary>
        /// <param name="xhtmlFileWithPath">The input Xhtml File </param>
        /// <param name="bookSplitterClass">The class name to split the class</param>
        /// <param name="filenamePrefix"></param>
        /// <param name = "adjacentClass"></param>
        /// <returns>The entire path of the splitted files are retured as List</returns>
        public static List<string> SplitXhtmlFile(string xhtmlFileWithPath, string bookSplitterClass, string filenamePrefix, bool adjacentClass)
        {
            List<string> books = new List<string>();
            XmlTextReader _reader;
            bookSplitterClass = bookSplitterClass.ToLower();


            var reader = new StreamReader(xhtmlFileWithPath);
            string content = reader.ReadToEnd().ToLower();
            reader.Close();

            var match = Regex.Matches(content, "\"" + bookSplitterClass + "\"");
            int counter = match.Count;

            if (counter <= 0)
                return books;

            try
            {
                _reader = Common.DeclareXmlTextReader(xhtmlFileWithPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return books;
            }

            Dictionary<string, XmlWriter> writers = new Dictionary<string, XmlWriter>();

            string allUserPath = Path.GetTempPath();
            for (int i = 0; i < counter; i++)
            {
                string fileName = Common.PathCombine(allUserPath, filenamePrefix + (i + 1) + ".xhtml");
                DeleteFile(fileName);
                XmlTextWriter writer = null;
                try
                {
                    writer = new XmlTextWriter(fileName, null) { Formatting = Formatting.Indented };
                }
                catch (Exception ex)
                {

                    Console.Write(ex.Message);
                }
                writers[fileName] = writer;
            }

            try
            {

                SplitXhtmlFileAdjacent(_reader, writers, bookSplitterClass, adjacentClass);
                _reader.Close();

                XmlWriter writerClose;
                foreach (KeyValuePair<string, XmlWriter> pair in writers)
                {
                    writerClose = pair.Value;
                    writerClose.Flush();
                    writerClose.Close();
                }
            }
            catch (XmlException ex)
            {
                var msg = new[] { ex.Message, xhtmlFileWithPath };
            }
            books.AddRange(writers.Keys);
            return books;
        }

        /// <summary>
        /// From the one xmlreader multiple xmlwriter are ued to split the class
        /// using dictionary
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writers"></param>
        /// <param name="bookSplitterClass"></param>
        static void SplitXhtmlFileAdjacent(XmlReader reader, Dictionary<string, XmlWriter> writers, string bookSplitterClass, bool adjacentClass)
        {
            int srcCount = 0;
            bool isLetData = false;
            XmlWriter writer;

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            while (reader.Read())
            {
            srcWritten:
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        string prefix = reader.Prefix;
                        string localName = reader.LocalName;
                        string nameSpace = reader.NamespaceURI;
                        string className = string.Empty;
                        className = reader.GetAttribute("class") ?? "";
                        if (className == "letData" && !isLetData)
                        {
                            isLetData = true;
                        }
                        if (className.ToLower() == bookSplitterClass)
                        {
                            isLetData = false;
                            srcCount++;
                            string f1 = reader.ReadOuterXml();  // current node - Ex: LetHead

                            string f2 = string.Empty;
                            if (adjacentClass)
                            {
                                f2 = reader.ReadOuterXml();  // Adjacent node - Ex: LetData
                                f1 += f2;
                            }

                            int writerCount1 = 1;
                            foreach (KeyValuePair<string, XmlWriter> pair in writers)
                            {
                                if (srcCount == writerCount1)
                                {
                                    writer = pair.Value;
                                    writer.WriteRaw(f1);
                                }
                                writerCount1++;
                            }
                            goto srcWritten;  // Note - The reader already points to next node.
                            // Note - so need of reading again. start process the node.
                        }
                        int writerCount2 = 1;
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            if (!isLetData || writerCount2 == srcCount)
                            {
                                writer = pair.Value;
                                writer.WriteStartElement(prefix, localName, nameSpace);
                                writer.WriteAttributes(reader, true);
                                if (reader.IsEmptyElement)
                                {
                                    writer.WriteEndElement();
                                }
                            }
                            writerCount2++;
                        }
                        break;
                    case XmlNodeType.Text:
                        int writerCount3 = 1;
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            if (!isLetData || writerCount3 == srcCount)
                            {
                                writer = pair.Value;
                                writer.WriteString(reader.Value);
                            }
                            writerCount3++;
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        int writerCount4 = 1;
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            if (!isLetData || writerCount4 == srcCount)
                            {
                                writer = pair.Value;
                                writer.WriteWhitespace(reader.Value);
                            }
                            writerCount4++;
                        }
                        break;
                    case XmlNodeType.CDATA:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteCData(reader.Value);
                        }
                        break;
                    case XmlNodeType.EntityReference:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteEntityRef(reader.Name);
                        }
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteProcessingInstruction(reader.Name, reader.Value);
                        }
                        break;
                    case XmlNodeType.DocumentType:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"),
                                                reader.GetAttribute("SYSTEM"), reader.Value);
                        }
                        break;
                    case XmlNodeType.Comment:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteComment(reader.Value);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        int writerCount5 = 1;
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            if (!isLetData || writerCount5 == srcCount)
                            {
                                try
                                {
                                    writer = pair.Value;
                                    writer.WriteFullEndElement();
                                }
                                catch
                                {
                                    return;
                                }
                            }
                            writerCount5++;
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// From the one xmlreader multiple xmlwriter are ued to split the class
        /// using dictionary
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writers"></param>
        /// <param name="bookSplitterClass"></param>
        static void SplitXhtmlFileAdjacent_Old(XmlReader reader, Dictionary<string, XmlWriter> writers, string bookSplitterClass, bool adjacentClass)
        {

            int srcCount = 0;
            XmlWriter writer;

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            while (reader.Read())
            {
            srcWritten:
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        string prefix = reader.Prefix;
                        string localName = reader.LocalName;
                        string nameSpace = reader.NamespaceURI;
                        string className = string.Empty;
                        className = reader.GetAttribute("class") ?? "";
                        if (className.ToLower() == bookSplitterClass)
                        {
                            srcCount++;
                            string f1 = reader.ReadOuterXml();  // current node - Ex: LetHead

                            string f2 = string.Empty;
                            if (adjacentClass)
                            {
                                f2 = reader.ReadOuterXml();  // Adjacent node - Ex: LetData
                                f1 += f2;
                            }

                            int writerCount = 1;
                            foreach (KeyValuePair<string, XmlWriter> pair in writers)
                            {
                                if (srcCount == writerCount)
                                {
                                    writer = pair.Value;
                                    writer.WriteRaw(f1);
                                }
                                writerCount++;
                            }
                            goto srcWritten;  // Note - The reader already points to next node.
                            // Note - so need of reading again. start process the node.
                        }
                        {
                            foreach (KeyValuePair<string, XmlWriter> pair in writers)
                            {
                                writer = pair.Value;
                                writer.WriteStartElement(prefix, localName, nameSpace);
                                writer.WriteAttributes(reader, true);
                                if (reader.IsEmptyElement)
                                {
                                    writer.WriteEndElement();
                                }
                            }
                        }
                        break;
                    case XmlNodeType.Text:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteString(reader.Value);
                        }
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteWhitespace(reader.Value);
                        }
                        break;
                    case XmlNodeType.CDATA:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteCData(reader.Value);
                        }
                        break;
                    case XmlNodeType.EntityReference:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteEntityRef(reader.Name);
                        }
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteProcessingInstruction(reader.Name, reader.Value);
                        }
                        break;
                    case XmlNodeType.DocumentType:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"),
                                                reader.GetAttribute("SYSTEM"), reader.Value);
                        }
                        break;
                    case XmlNodeType.Comment:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            writer = pair.Value;
                            writer.WriteComment(reader.Value);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        foreach (KeyValuePair<string, XmlWriter> pair in writers)
                        {
                            try
                            {
                                writer = pair.Value;
                                writer.WriteFullEndElement();
                            }
                            catch
                            {
                                return;
                            }
                        }
                        break;
                }
            }
        }
    }
}
