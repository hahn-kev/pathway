﻿// --------------------------------------------------------------------------------------------
// <copyright file="OdtTest.cs" from='2009' to='2009' company='SIL International'>
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
// ODT Test Support
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using SIL.Tool;

namespace Test
{
    public class ODet // Output Details
    {
        public const int Def = 1;
        public const int Chk = 0;
        public const string Main = "main.odt";
        public const string Rev = "FlexRev.odt";
        public const string Mast = "main.odm";
        public const string Content = "content.xml";
        public const string Styles = "styles.xml";
        public int Op = Chk;
        public string File = string.Empty;
        public string Part = string.Empty;
        public string XPath = string.Empty;
        public string Res = string.Empty;
        public string Msg = string.Empty;

        public ODet(int pOp, string pMsg, string pFile, string pPart, string pXPath, string pRes)
        {
            Op = pOp;
            Msg = pMsg;
            File = pFile;
            Part = pPart;
            XPath = pXPath;
            Res = pRes;
        }
    }

    public static class OdtTest
    {
        /// <summary>
        /// Compares all odt's in outputPath to make sure the content.xml and styles.xml are the same
        /// </summary>
        /// <param name="expectPath">expected output path</param>
        /// <param name="outputPath">output path</param>
        /// <param name="msg">message to display if mismatch</param>
        public static void AreEqual(string expectPath, string outputPath, string msg)
        {
            var outDi = new DirectoryInfo(outputPath);
            var expDi = new DirectoryInfo(expectPath);
            FileInfo[] outFi = outDi.GetFiles("*.od*");
            FileInfo[] expFi = expDi.GetFiles("*.od*");
            Assert.AreEqual(outFi.Length, expFi.Length, string.Format("{0} {1} odt found {2} expected", msg, outFi.Length, expFi.Length));
            foreach (FileInfo fi in outFi)
            {
                var outFl = new ZipFile(fi.FullName);
                var expFl = new ZipFile(Common.PathCombine(expectPath, fi.Name));
                foreach (string name in "content.xml,styles.xml".Split(','))
                {
                    string outputEntry = new StreamReader(outFl.GetInputStream(outFl.GetEntry(name).ZipFileIndex)).ReadToEnd();
                    string expectEntry = new StreamReader(expFl.GetInputStream(expFl.GetEntry(name).ZipFileIndex)).ReadToEnd();
                    XmlDocument outputDocument = new XmlDocument();
                    outputDocument.XmlResolver = new XmlUrlResolver();
                    outputDocument.LoadXml(outputEntry);
                    XmlDocument expectDocument = new XmlDocument();
                    expectDocument.XmlResolver = new XmlUrlResolver();
                    expectDocument.LoadXml(expectEntry);
                    XmlDsigC14NTransform outputCanon = new XmlDsigC14NTransform();
                    outputCanon.Resolver = new XmlUrlResolver();
                    outputCanon.LoadInput(outputDocument);
                    XmlDsigC14NTransform expectCanon = new XmlDsigC14NTransform();
                    expectCanon.Resolver = new XmlUrlResolver();
                    expectCanon.LoadInput(expectDocument);
                    Stream outputStream = (Stream)outputCanon.GetOutput(typeof(Stream));
                    Stream expectStream = (Stream)expectCanon.GetOutput(typeof(Stream));
                    string errMessage = string.Format("{0}: {1} {2} doesn't match", msg, fi.Name, name);
                    Assert.AreEqual(expectStream.Length, outputStream.Length, errMessage);
                    FileAssert.AreEqual(expectStream, outputStream, errMessage);
                }
            }
        }

        public static void DoTests(string outputPath, ArrayList tests)
        {
            var parts = new Dictionary<string, XmlDocument>();
            var variables = new Dictionary<string, string>();
            var outDi = new DirectoryInfo(outputPath);
            var errorCount = 0;

            foreach (ODet detail in tests)
            {
                var fileKey = Path.Combine(detail.File, detail.Part);
                if (!parts.ContainsKey(fileKey))
                    parts[fileKey] = LoadXml(Path.Combine(outDi.FullName, detail.File), detail.Part);
                var spath = SubstituteVariables(detail.XPath, variables);
                var xDoc = parts[fileKey];
                var nsMgr = XmlDocumentNamespaceManager(xDoc);
                var res = xDoc.SelectSingleNode(spath, nsMgr);
                if (res == null)
                {
                    Console.WriteLine(string.Format("Missing property {0}", detail.Msg));
                    errorCount += 1;
                    continue;
                }
                Assert.IsNotNull(res, detail.Msg);
                var resultStr = res.NodeType == XmlNodeType.Element ? res.InnerText : res.Value;
                if (detail.Op == ODet.Def)
                    variables[detail.Res] = resultStr;
                else if (ToPt(resultStr) != ToPt(detail.Res))
                {
                    Console.WriteLine(string.Format("{0} {1} failure. Expected {2} but was {3}", fileKey, detail.Msg, ToPt(detail.Res), resultStr));
                    errorCount += 1;
                }
            }
            Assert.AreEqual(0,errorCount, "Error Count");
        }

        private static string ToPt(string p)
        {
            if (p.EndsWith("cm"))
            {   // convert centimeters to points and round
                var v = double.Parse(p.Substring(0, p.Length - 2));
                v *= 72.0 / 2.54;
                return string.Format("{0:0.}pt", v);
            }
            if (p.EndsWith("mm"))
            {   // convert milimeters to points and round
                var v = double.Parse(p.Substring(0, p.Length - 2));
                v *= 72.0 / 25.4;
                return string.Format("{0:0.}pt", v);
            }
            if (p.EndsWith("in"))
            {   // convert centimeters to points and round
                var v = double.Parse(p.Substring(0, p.Length - 2));
                v *= 72.0;
                return string.Format("{0:0.}pt", v);
            }
            if (p.EndsWith("pt"))
            {   // return value rounded to 4 places
                var v = double.Parse(p.Substring(0, p.Length - 2));
                return string.Format("{0:0.}pt", v);
            }
            return p.Trim();
        }

        private static string SubstituteVariables(string p, Dictionary<string, string> variables)
        {
            var q = "";
            int b = 0;
            int i = p.IndexOf('{');
            while (i != -1)
            {
                int j = p.IndexOf('}', i);
                var name = p.Substring(i + 1, j - i - 1);
                if (variables.ContainsKey(name))
                {
                    q += p.Substring(b, i - b);
                    q += variables[name];
                    b = j + 1;
                }
                i = p.IndexOf('{', j);
            }
            q += p.Substring(b);
            return q;
        }

        private static XmlNamespaceManager XmlDocumentNamespaceManager(XmlDocument xDoc)
        {
            var nsMgr = new XmlNamespaceManager(xDoc.NameTable);
            Assert.IsNotNull(xDoc.DocumentElement);
            foreach (XmlAttribute attribute in xDoc.DocumentElement.Attributes)
            {
                nsMgr.AddNamespace(attribute.LocalName, attribute.Value);
            }
            return nsMgr;
        }

        /// <summary>
        /// Load Xml data from part of ODT file (usually content.xml or styles.xml)
        /// </summary>
        /// <param name="odtPath">full path to odt</param>
        /// <param name="sectionName">section name (usually content.xml or styles.xml)</param>
        /// <returns>xmlDocument with <paramref name="sectionName">sectionName</paramref> loaded</returns>
        public static XmlDocument LoadXml(string odtPath, string sectionName)
        {
            var odtFile = new ZipFile(odtPath);
            var reader = new StreamReader(odtFile.GetInputStream(odtFile.GetEntry(sectionName).ZipFileIndex));
            var text = reader.ReadToEnd();
            reader.Close();
            odtFile.Close();
            var xmlDocument = new XmlDocument();
            xmlDocument.XmlResolver = new XmlUrlResolver();
            xmlDocument.LoadXml(text);
            return xmlDocument;
        }

        public static XmlNamespaceManager NamespaceManager(XmlDocument xmlDocument)
        {
            var root = xmlDocument.DocumentElement;
            Assert.IsNotNull(root, "Missing xml document");
            var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            foreach (XmlAttribute attribute in root.Attributes)
            {
                var namePart = attribute.Name.Split(':');
                if (namePart[0] == "xmlns")
                    nsManager.AddNamespace(namePart[1], attribute.Value);
            }
            return nsManager;
        }
    }
}
