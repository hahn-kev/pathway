﻿// --------------------------------------------------------------------------------------------
// <copyright file="OOMetaXML.cs" from='2009' to='2014' company='SIL International'>
//      Copyright (c) 2014, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Creates the Meta file
// </remarks>
// --------------------------------------------------------------------------------------------

#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using SIL.Tool;

#endregion Using
namespace SIL.PublishingSolution
{
    #region Class LOMetaXML
    public class LOMetaXML
    {
        public LOMetaXML(string projectInputType)
        {
            _projectInputType = projectInputType;
        }
        #region Private Variable
        XmlTextWriter _writer;
        private static string _projectInputType;
        private static Dictionary<string, string> _metaDataDic = null;
        #endregion

        #region Public Methods
        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate Meta.xml 
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="projInfo">projInfo</param>
        /// <returns>None </returns>
        /// -------------------------------------------------------------------------------------------
        public void CreateMeta(PublicationInformation projInfo)
        {
            try
            {
                _metaDataDic = Common.GetMetaData(_projectInputType, string.Empty);
                CreateFile(projInfo.TempOutputFolder);
                CloseFile();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateFile(string targetPath)
        {
            string userName = Environment.UserName;
            string createDate = DateTime.Now.ToString("s");
            string createBy = Application.ProductName + " " + Application.ProductVersion;
            string[] assemblyInfo = Assembly.GetExecutingAssembly().FullName.Split(',');
            string assemblyVersion = assemblyInfo[0] + assemblyInfo[1].Replace("Version=","");

            string targetXML = Common.PathCombine(targetPath, "meta.xml");
            _writer = new XmlTextWriter(targetXML, null) { Formatting = Formatting.Indented };
            _writer.WriteStartDocument();

            _writer.WriteStartElement("office:document-meta");
            _writer.WriteAttributeString("xmlns:office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            _writer.WriteAttributeString("xmlns:xlink", "http://www.w3.org/1999/xlink");
            _writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
            _writer.WriteAttributeString("xmlns:meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
            _writer.WriteAttributeString("xmlns:ooo", "http://openoffice.org/2004/office");
            _writer.WriteAttributeString("xmlns:grddl", "http://www.w3.org/2003/g/data-view#");
            _writer.WriteAttributeString("office:version", "1.2");
            _writer.WriteAttributeString("grddl:transformation", "http://docs.oasis-open.org/office/1.2/xslt/odf2rdf.xsl");
            _writer.WriteStartElement("office:meta");
            _writer.WriteStartElement("meta:generator");
            _writer.WriteString("LibreOffice/3.3$Win32 LibreOffice_project/330m19$Build-202");
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:initial-creator");
            _writer.WriteString(userName);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:creation-date");
            _writer.WriteString(createDate);
            _writer.WriteEndElement();
            _writer.WriteStartElement("dc:creator");
            _writer.WriteString(userName);
            _writer.WriteEndElement();
            _writer.WriteStartElement("dc:date");
            _writer.WriteString(createDate);
            _writer.WriteEndElement();

            _writer.WriteStartElement("dc:description");
            _writer.WriteString(_metaDataDic["Description"]);
            _writer.WriteEndElement();
            _writer.WriteStartElement("dc:subject");
            _writer.WriteString(_metaDataDic["Subject"]);
            _writer.WriteEndElement();
            _writer.WriteStartElement("dc:title");
            _writer.WriteString(_metaDataDic["Title"]);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:editing-cycles");
            _writer.WriteString("1");
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:editing-duration");
            _writer.WriteString("PT2M27S");
            _writer.WriteEndElement();

            _writer.WriteStartElement("meta:user-defined");
            _writer.WriteAttributeString("meta:name", "Author");
            _writer.WriteString(_metaDataDic["Creator"]);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:user-defined");
            _writer.WriteAttributeString("meta:name", "Publisher");
            _writer.WriteString(_metaDataDic["Publisher"]);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:user-defined");
            _writer.WriteAttributeString("meta:name", "Copyright Holder");
            _writer.WriteString(Common.UpdateCopyrightYear(_metaDataDic["Copyright Holder"]));
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:user-defined");
            _writer.WriteAttributeString("meta:name", "Assembly-Version");
            _writer.WriteString(assemblyVersion);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:user-defined");
            _writer.WriteAttributeString("meta:name", "Created-By");
            _writer.WriteString(createBy);
            _writer.WriteEndElement();
            _writer.WriteStartElement("meta:document-statistic");
            _writer.WriteAttributeString("meta:table-count", "0");
            _writer.WriteAttributeString("meta:image-count", "0");
            _writer.WriteAttributeString("meta:object-count", "0");
            _writer.WriteAttributeString("meta:page-count", "1");
            _writer.WriteAttributeString("meta:paragraph-count", "1");
            _writer.WriteAttributeString("meta:word-count", "5");
            _writer.WriteAttributeString("meta:character-count", "22");
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _writer.WriteEndElement();

        }

        void CloseFile()
        {
            _writer.WriteEndDocument();
            _writer.Flush();
            _writer.Close();
        }

        #endregion
    }
    #endregion
}
