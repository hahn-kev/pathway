﻿#region // Copyright (C) 2014, SIL International. All Rights Reserved.
// --------------------------------------------------------------------------------------------
// <copyright file="EpubManifest.cs" from='2009' to='2014' company='SIL International'>
//      Copyright (C) 2014, SIL International. All Rights Reserved.   
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
#endregion

#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using SIL.PublishingSolution;
using SIL.Tool;

#endregion using

namespace epubConvert
{
    public class EpubManifest
    {
        #region Setup & Class data
        private readonly Exportepub _parent;
        private readonly EpubFont _epubFont;

        public string Title { get; private set; }
        private string Creator { get; set; }
        private string Description { get; set; }
        private string Publisher { get; set; }
        private string Coverage { get; set; }
        private string Rights { get; set; }
        private string Format { get; set; }
        private string Source { get; set; }

        public EpubManifest(Exportepub exportepub, EpubFont epubFont)
        {
            _parent = exportepub;
            _epubFont = epubFont;
        }
        #endregion Setup & Class data

        #region void CreateOpf(PublicationInformation projInfo, string contentFolder, Guid bookId)
        /// <summary>
        /// Generates the manifest and metadata information file used by the .epub reader
        /// (content.opf). For more information, refer to <see cref="http://www.idpf.org/doc_library/epub/OPF_2.0.1_draft.htm#Section2.0"/> 
        /// </summary>
        /// <param name="projInfo">Project information</param>
        /// <param name="contentFolder">Content folder (.../OEBPS)</param>
        /// <param name="bookId">Unique identifier for the book we're generating.</param>
        public void CreateOpf(PublicationInformation projInfo, string contentFolder, Guid bookId)
        {
            XmlWriter opf = XmlWriter.Create(Common.PathCombine(contentFolder, "content.opf"));
            opf.WriteStartDocument();
            // package name
            opf.WriteStartElement("package", "http://www.idpf.org/2007/opf");
            opf.WriteAttributeString("version", "2.0");
            opf.WriteAttributeString("unique-identifier", "BookId");
            // metadata - items defined by the Dublin Core Metadata Initiative:
            Metadata(projInfo, bookId, opf);
            StartManifest(opf);
            if (_parent.EmbedFonts)
            {
                ManifestFontEmbed(opf);
            }
            string[] files = Directory.GetFiles(contentFolder);
            ManifestContent(opf, files);
            Spine(opf, files);
            Guide(projInfo, opf, files);
            opf.WriteEndElement(); // package
            opf.WriteEndDocument();
            opf.Close();
        }

        private void Metadata(PublicationInformation projInfo, Guid bookId, XmlWriter opf)
        {
            // (http://dublincore.org/documents/2004/12/20/dces/)
            opf.WriteStartElement("metadata");
            opf.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
            opf.WriteAttributeString("xmlns", "opf", null, "http://www.idpf.org/2007/opf");
            opf.WriteElementString("dc", "title", null,
                                   (Title == "") ? (Common.databaseName + " " + projInfo.ProjectName) : Title);
            opf.WriteStartElement("dc", "creator", null); //<dc:creator opf:role="aut">[author]</dc:creator>
            opf.WriteAttributeString("opf", "role", null, "aut");
            opf.WriteValue((Creator == "") ? Environment.UserName : Creator);
            opf.WriteEndElement();
            opf.WriteElementString("dc", "subject", null, _parent.InputType == "dictionary" ? "Reference" : "Religion & Spirituality");
            if (Description.Length > 0)
                opf.WriteElementString("dc", "description", null, Description);
            if (Publisher.Length > 0)
                opf.WriteElementString("dc", "publisher", null, Publisher);
            opf.WriteStartElement("dc", "contributor", null); // authoring program as a "contributor", e.g.:
            opf.WriteAttributeString("opf", "role", null, "bkp");
            // <dc:contributor opf:role="bkp">FieldWorks 7</dc:contributor>
            opf.WriteValue(Common.GetProductName());
            opf.WriteEndElement();
            opf.WriteElementString("dc", "date", null, DateTime.Today.ToString("yyyy-MM-dd"));
            // .epub standard date format (http://www.idpf.org/2007/opf/OPF_2.0_final_spec.html#Section2.2.7)
            opf.WriteElementString("dc", "type", null, "Text"); // 
            if (Format.Length > 0)
                opf.WriteElementString("dc", "format", null, Format);
            if (Source.Length > 0)
                opf.WriteElementString("dc", "source", null, Source);

            if (_epubFont.LanguageCount == 0)
            {
                opf.WriteElementString("dc", "language", null, "en");
            }

            foreach (var lang in _epubFont.LanguageCodes())
            {
                opf.WriteElementString("dc", "language", null, lang);
            }


            if (Coverage.Length > 0)
                opf.WriteElementString("dc", "coverage", null, Coverage);
            if (Rights.Length > 0)
                opf.WriteElementString("dc", "rights", null, Rights);
            opf.WriteStartElement("dc", "identifier", null); // <dc:identifier id="BookId">[guid]</dc:identifier>
            opf.WriteAttributeString("id", "BookId");
            opf.WriteValue(bookId.ToString());
            opf.WriteEndElement();
            // cover image (optional)
            if (Param.GetMetadataValue(Param.CoverPage).ToLower().Equals("true"))
            {
                opf.WriteStartElement("meta");
                opf.WriteAttributeString("name", "cover");
                opf.WriteAttributeString("content", "cover-image");
                opf.WriteEndElement(); // meta
            }
            opf.WriteEndElement(); // metadata
        }

        private static void StartManifest(XmlWriter opf)
        {
            // manifest
            opf.WriteStartElement("manifest");
            // (individual "item" elements in the manifest)
            opf.WriteStartElement("item");
            opf.WriteAttributeString("id", "ncx");
            opf.WriteAttributeString("href", "toc.ncx");
            opf.WriteAttributeString("media-type", "application/x-dtbncx+xml");
            opf.WriteEndElement(); // item
        }

        private void ManifestFontEmbed(XmlWriter opf)
        {
            int fontNum = 1;
            foreach (var embeddedFont in _epubFont.EmbeddedFonts())
            {
                if (embeddedFont.Filename == null)
                {
                    // already written out that this font doesn't exist in the CSS file; just skip it here
                    continue;
                }
                opf.WriteStartElement("item"); // item (charis embedded font)
                opf.WriteAttributeString("id", "epub.embedded.font" + fontNum);
                opf.WriteAttributeString("href", Path.GetFileName(embeddedFont.Filename));
                opf.WriteAttributeString("media-type", "font/opentype/");
                opf.WriteEndElement(); // item
                fontNum++;
                if (_parent.IncludeFontVariants)
                {
                    // italic
                    if (embeddedFont.HasItalic && String.Compare(embeddedFont.Filename, embeddedFont.ItalicFilename, StringComparison.Ordinal) != 0)
                    {
                        if (embeddedFont.ItalicFilename != string.Empty)
                        {
                            opf.WriteStartElement("item"); // item (charis embedded font)
                            opf.WriteAttributeString("id", "epub.embedded.font_i_" + fontNum);

                            opf.WriteAttributeString("href", Path.GetFileName(embeddedFont.ItalicFilename));

                            opf.WriteAttributeString("media-type", "font/opentype/");
                            opf.WriteEndElement(); // item
                            fontNum++;
                        }
                    }
                    // bold
                    if (embeddedFont.HasBold && String.Compare(embeddedFont.Filename, embeddedFont.BoldFilename, StringComparison.Ordinal) != 0)
                    {
                        if (embeddedFont.BoldFilename != string.Empty)
                        {
                            opf.WriteStartElement("item"); // item (charis embedded font)
                            opf.WriteAttributeString("id", "epub.embedded.font_b_" + fontNum);
                            opf.WriteAttributeString("href", Path.GetFileName(embeddedFont.BoldFilename));
                            opf.WriteAttributeString("media-type", "font/opentype/");
                            opf.WriteEndElement(); // item
                            fontNum++;
                        }
                    }
                }
            }
        }

        private void ManifestContent(XmlWriter opf, IEnumerable<string> files)
        {
            var listIdRef = new List<string>();
            int counterSet = 1;
            foreach (string file in files)
            {
                // iterate through the file set and add <item> elements for each xhtml file
                string name = Path.GetFileName(file);
                Debug.Assert(name != null);
                string nameNoExt = Path.GetFileNameWithoutExtension(file);

                if (name.EndsWith(".xhtml"))
                {
                    // is this the cover page?
                    if (name.StartsWith(PreExportProcess.CoverPageFilename.Substring(0, 8)))
                    {
                        // yup - write it out and go to the next item
                        opf.WriteStartElement("item");
                        opf.WriteAttributeString("id", "cover");
                        opf.WriteAttributeString("href", name);
                        opf.WriteAttributeString("media-type", "application/xhtml+xml");
                        opf.WriteEndElement(); // item
                        continue;
                    }

                    // if we can, write out the "user friendly" book name in the TOC
                    string fileId = _parent.GetBookId(file);

                    string idRefValue;
                    if (listIdRef.Contains(fileId))
                    {
                        listIdRef.Add(fileId + counterSet.ToString(CultureInfo.InvariantCulture));
                        idRefValue = fileId + counterSet.ToString(CultureInfo.InvariantCulture);
                        counterSet++;
                    }
                    else
                    {
                        listIdRef.Add(fileId);
                        idRefValue = fileId;
                    }

                    opf.WriteStartElement("item");
                    // the book ID can be wacky (and non-unique) for dictionaries. Just use the filename.
                    var itemId = _parent.InputType == "dictionary" ? nameNoExt : idRefValue;
                    opf.WriteAttributeString("id", itemId);
                    opf.WriteAttributeString("href", name);
                    opf.WriteAttributeString("media-type", "application/xhtml+xml");
                    opf.WriteEndElement(); // item
                }
                else if (name.EndsWith(".css"))
                {
                    opf.WriteStartElement("item"); // item (stylesheet)
                    opf.WriteAttributeString("id", "stylesheet");
                    opf.WriteAttributeString("href", name);
                    opf.WriteAttributeString("media-type", "text/css");
                    opf.WriteEndElement(); // item
                }
                else if (name.ToLower().EndsWith(".jpg") || name.ToLower().EndsWith(".jpeg"))
                {
                    opf.WriteStartElement("item"); // item (image)
                    opf.WriteAttributeString("id", "image" + nameNoExt);
                    opf.WriteAttributeString("href", name);
                    if (nameNoExt != null && nameNoExt.Contains("sil-bw-logo"))
                    {
                        opf.WriteAttributeString("media-type", "image/png");
                    }
                    else
                    {
                        opf.WriteAttributeString("media-type", "image/jpeg");
                    }

                    opf.WriteEndElement(); // item
                }
                else if (name.ToLower().EndsWith(".gif"))
                {
                    opf.WriteStartElement("item"); // item (image)
                    opf.WriteAttributeString("id", "image" + nameNoExt);
                    opf.WriteAttributeString("href", name);
                    opf.WriteAttributeString("media-type", "image/gif");
                    opf.WriteEndElement(); // item
                }
                else if (name.ToLower().EndsWith(".png"))
                {
                    opf.WriteStartElement("item"); // item (image)
                    opf.WriteAttributeString("id", "image" + nameNoExt);
                    opf.WriteAttributeString("href", name);
                    opf.WriteAttributeString("media-type", "image/png");
                    opf.WriteEndElement(); // item
                }
            }
            opf.WriteEndElement(); // manifest
        }

        private void Spine(XmlWriter opf, IEnumerable<string> files)
        {
            // spine
            opf.WriteStartElement("spine");
            opf.WriteAttributeString("toc", "ncx");
            // a couple items for the cover image
            if (Param.GetMetadataValue(Param.CoverPage).ToLower().Equals("true"))
            {
                opf.WriteStartElement("itemref");
                opf.WriteAttributeString("idref", "cover");
                opf.WriteAttributeString("linear", "yes");
                opf.WriteEndElement(); // itemref
            }

            var listIdRef = new List<string>();
            int counterSet = 1;
            foreach (string file in files)
            {
                // is this the cover page?
                var fileName = Path.GetFileName(file);
                Debug.Assert(fileName != null);
                if (fileName.StartsWith(PreExportProcess.CoverPageFilename.Substring(0, 8)))
                {
                    continue;
                }
                // add an <itemref> for each xhtml file in the set
                if (fileName.EndsWith(".xhtml"))
                {
                    string fileId = _parent.GetBookId(file);
                    string idRefValue;
                    if (listIdRef.Contains(fileId))
                    {
                        var counter = counterSet.ToString(CultureInfo.InvariantCulture);
                        listIdRef.Add(fileId + counter);
                        idRefValue = fileId + counter;
                        counterSet++;
                    }
                    else
                    {
                        listIdRef.Add(fileId);
                        idRefValue = fileId;
                    }


                    opf.WriteStartElement("itemref"); // item (stylesheet)
                    // the book ID can be wacky (and non-unique) for dictionaries. Just use the filename.
                    var idRef = _parent.InputType == "dictionary" ? Path.GetFileNameWithoutExtension(file) : idRefValue;
                    opf.WriteAttributeString("idref", idRef);
                    opf.WriteEndElement(); // itemref
                }
            }
            opf.WriteEndElement(); // spine
        }

        private static void Guide(PublicationInformation projInfo, XmlWriter opf, string[] files)
        {
            // guide
            opf.WriteStartElement("guide");
            // cover image
            if (Param.GetMetadataValue(Param.CoverPage).Trim().Equals("True"))
            {
                opf.WriteStartElement("reference");
                opf.WriteAttributeString("href", "File0Cvr00000_.xhtml");
                opf.WriteAttributeString("type", "cover");
                opf.WriteAttributeString("title", "Cover");
                opf.WriteEndElement(); // reference
            }
            // first xhtml filename
            opf.WriteStartElement("reference");
            opf.WriteAttributeString("type", "text");
            opf.WriteAttributeString("title", Common.databaseName + " " + projInfo.ProjectName);
            int index = 0;
            while (index < files.Length)
            {
                if (files[index].EndsWith(".xhtml"))
                {
                    break;
                }
                index++;
            }
            if (index == files.Length) index--; // edge case
            opf.WriteAttributeString("href", Path.GetFileName(files[index]));
            opf.WriteEndElement(); // reference
            opf.WriteEndElement(); // guide
        }
        #endregion void CreateOpf(PublicationInformation projInfo, string contentFolder, Guid bookId)

        #region Property persistence
        /// <summary>
        /// Loads the settings file and pulls out the values we look at.
        /// </summary>
        public void LoadPropertiesFromSettings()
        {
            // Load User Interface Collection Parameters
            Param.LoadSettings();
            string organization;
            try
            {
                // get the organization
                organization = Param.Value["Organization"];
            }
            catch (Exception)
            {
                // shouldn't happen (ExportThroughPathway dialog forces the user to select an organization), 
                // but just in case, specify a default org.
                organization = "SIL International";
            }
            // Title (book title in Configuration Tool UI / dc:title in metadata)
            Title = Param.GetMetadataValue(Param.Title, organization) ?? ""; // empty string if null / not found
            // Creator (dc:creator))
            Creator = Param.GetMetadataValue(Param.Creator, organization) ?? ""; // empty string if null / not found
            // information
            Description = Param.GetMetadataValue(Param.Description, organization) ?? ""; // empty string if null / not found
            // Source
            Source = Param.GetMetadataValue(Param.Source, organization) ?? ""; // empty string if null / not found
            // Format
            Format = Param.GetMetadataValue(Param.Format, organization) ?? ""; // empty string if null / not found
            // Publisher
            Publisher = Param.GetMetadataValue(Param.Publisher, organization) ?? ""; // empty string if null / not found
            // Coverage
            Coverage = Param.GetMetadataValue(Param.Coverage, organization) ?? ""; // empty string if null / not found
            // Rights (dc:rights)
            Rights = Param.GetMetadataValue(Param.CopyrightHolder, organization) ?? ""; // empty string if null / not found
            Rights = Common.UpdateCopyrightYear(Rights);
        }
        #endregion

    }
}
