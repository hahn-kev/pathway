﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using SIL.Tool;
using epubConvert;

namespace SIL.PublishingSolution
{
    public class Epub3Transformation
    {
        private readonly Exportepub _parent;
        private readonly EpubFont _epubFont;
        public bool IsUnixOs;
        public string Epub3Directory = string.Empty;

        public Epub3Transformation()
        {
            _parent = null;
            _epubFont = null;
        }

        public Epub3Transformation(Exportepub exportepub, EpubFont epubFont)
        {
            _parent = exportepub;
            _epubFont = epubFont;
        }

        /// <summary>
        /// Entry point for epub 3 converter
        /// </summary>
        /// <param name="projInfo">values passed including epub2 exported files and changed to epub3 support</param>
        /// <returns>true if succeeds</returns>
        public bool Export(PublicationInformation projInfo)
        {
            bool epub3Export = false;
            string oebpsPath = Common.PathCombine(Epub3Directory, "OEBPS");
            string cssFile = Common.PathCombine(oebpsPath, "book.css");

            var preProcessor = new PreExportProcess();
            preProcessor.ReplaceStringInFile(cssFile, "{direction:ltr}", "{direction:ltr;}");
            //preProcessor.RemoveStringInCss(cssFile, "direction:");


            var xhmltohtml5Space = Loadxhmltohtml5Xslt(projInfo.ProjectInputType.ToLower());

            Convertxhtmltohtml5(oebpsPath, xhmltohtml5Space);

            ModifyContainerXML();

            ModifyContentOPF(projInfo, oebpsPath);

            ModifyTocContent(oebpsPath);

            ModifyCoverpage(oebpsPath);

            epub3Export = true;

            return epub3Export;
        }

        private static void ModifyCoverpage(string oebpsPath)
        {
            var epub3CoverPage = LoadEpub3CoverPage();
            string epub3CoverPageFile = Common.PathCombine(oebpsPath, "File0Cvr00000_.html");
            if (File.Exists(epub3CoverPageFile))
            {
                Common.ApplyXslt(epub3CoverPageFile, epub3CoverPage);
            }
        }

        private static void ModifyTocContent(string oebpsPath)
        {
            var epub3Toc = LoadEpub3Toc();

            string ncxTempFile = Common.PathCombine(oebpsPath, "toctemp.ncx");
            string ncxfile = Common.PathCombine(oebpsPath, "toc.ncx");
            File.Copy(ncxfile, ncxTempFile, true);
            string epub3TocFile = Common.PathCombine(oebpsPath, "toc.html");
            if (File.Exists(ncxfile))
            {
                Common.ApplyXslt(ncxfile, epub3Toc);
                File.Copy(ncxfile, epub3TocFile, true);
                if (File.Exists(ncxTempFile))
                {
                    File.Copy(ncxTempFile, ncxfile, true);
                    var preProcessor = new PreExportProcess();
                    preProcessor.ReplaceStringInFile(ncxfile, ".xhtml", ".html");
                    File.Delete(ncxTempFile);
                }
            }
        }

        private void ModifyContentOPF(PublicationInformation projInfo, string oebpsPath)
        {
            string contentOPFFile = Common.PathCombine(oebpsPath, "content.opf");

            if (File.Exists(contentOPFFile))
                File.Delete(contentOPFFile);

            var epubManifest = new EpubManifest(_parent, _epubFont);
            var bookId = Guid.NewGuid(); // NOTE: this creates a new ID each time Pathway is run. 
            epubManifest.CreateOpfV3(projInfo, oebpsPath, bookId);
        }

        private void ModifyContainerXML()
        {
            string modifyContainerFile = Common.PathCombine(Epub3Directory, "META-INF");
            modifyContainerFile = Common.PathCombine(modifyContainerFile, "container.xml");
            if (File.Exists(modifyContainerFile))
            {
                ModifyContainerXMLForEpub3(modifyContainerFile);
            }
        }

        private void Convertxhtmltohtml5(string oebpsPath, XslCompiledTransform xhmltohtml5Space)
        {
            string[] filesList = null;
            //XslCompiledTransform _xhtmlXelatexXslProcess = new XslCompiledTransform();
            //_xhtmlXelatexXslProcess.Load(XmlReader.Create(Common.UsersXsl("AddBidi.xsl")));
            if (Directory.Exists(oebpsPath))
            {
                filesList = Directory.GetFiles(oebpsPath);
                foreach (var curFile in filesList)
                {
                    FileInfo fileInfo = new FileInfo(curFile);
                    if (fileInfo.Extension == ".xhtml")
                    {
                        Common.ApplyXslt(curFile, xhmltohtml5Space);
                        //Common.ApplyXslt(curFile, _xhtmlXelatexXslProcess);

                        if (File.Exists(curFile))
                        {
                            string fileName = Path.GetFileName(curFile);
                            File.Copy(curFile, curFile.Replace(".xhtml", ".html"), true);
                            File.Delete(curFile);
                        }
                    }
                }
            }
        }

        private void ModifyContainerXMLForEpub3(string containerXmlFile)
        {
            Common.StreamReplaceInFile(containerXmlFile, "<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
        }

        public static XslCompiledTransform Loadxhmltohtml5Xslt(string projectInputType)
        {
            string xsltName = "epubConvert.xhtmltohtml5.xslt";
            //xsltName = (projectInputType == "dictionary")
            //               ? "epubConvert.xhtmltohtml5.xslt"
            //              : "epubConvert.xhtmltohtml5.xslt";

            var xhmltohtml5Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xsltName);
            Debug.Assert(xhmltohtml5Stream != null);
            var xhmltohtml5 = new XslCompiledTransform();
            xhmltohtml5.Load(XmlReader.Create(xhmltohtml5Stream));
            return xhmltohtml5;
        }

        public static XslCompiledTransform LoadEpub3Toc()
        {
            var xhmltohtml5Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("epubConvert.Epub3Toc.xsl");
            Debug.Assert(xhmltohtml5Stream != null);
            var xhmltohtml5 = new XslCompiledTransform();
            xhmltohtml5.Load(XmlReader.Create(xhmltohtml5Stream));
            return xhmltohtml5;
        }

        public static XslCompiledTransform LoadEpub3CoverPage()
        {
            var xhmltohtml5Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("epubConvert.Epub3CoverPage.xsl");
            Debug.Assert(xhmltohtml5Stream != null);
            var xhmltohtml5 = new XslCompiledTransform();
            xhmltohtml5.Load(XmlReader.Create(xhmltohtml5Stream));
            return xhmltohtml5;
        }
    }
}
