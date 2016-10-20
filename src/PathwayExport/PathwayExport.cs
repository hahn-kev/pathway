﻿// --------------------------------------------------------------------------------------------
// <copyright file="PathwayExport.cs" from='2009' to='2014' company='SIL International'>
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
// 
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;
using SIL.Tool;
using System.Collections.Generic;

namespace PathwayExport
{
    internal class Program
    {
        private static bool _showHelp;
        private enum InputFormat
        {
            XHTML,
            USFM,
            USX,
            PTBUNDLE
        }

        private enum ExportFormat
        {
            Dictionary,
            Scripture
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("PathwayExport: ");
                Console.WriteLine("Try 'PathwayExport --help' for more information.");
                Console.Read();
                Environment.Exit(0);
            }
            else if (args.Length == 1 && args[0].ToLower() == "--help")
            {
                Usage();
                Console.Read();
                Environment.Exit(0);
            }
            else
            {
                ArgumentUsage(args);
            }
        }

        private static void ArgumentUsage(string[] args)
        {
            string exportType = string.Empty;
            string exportDirectory = string.Empty;
            List<string> files = new List<string>();
            int i = 0;
            while (i < args.Length)
            {
                switch (args[i++])
                {
                    case "--target":
                    case "-t":
                        exportType = args[i++];
                        if (!CheckExportType(exportType))
                        {
                            Console.Write(@"PathwayExport : Unknown Export Type");
                            Console.WriteLine(@"Try 'PathwayExport --help' for more information.");
                            Console.Read();
                            Environment.Exit(0);
                        }
                        break;
                    case "--directory":
                    case "-d":
                        exportDirectory = args[i++];
                        if (!Directory.Exists(exportDirectory))
                        {
                            Console.Write(@"PathwayExport : Export directory not exists");
                            Console.WriteLine(@"Try 'PathwayExport --help' for more information.");
                            Console.Read();
                            Environment.Exit(0);
                        }
                        break;
                    case "--files":
                    case "-f":
                        i = CaptureFileList(args, i, files);
                        break;
                    case "--css":
                    case "-c":
                        files.Add(args[i++]);
                        break;
                }
            }
        }

        private static bool CheckExportType(string exportType)
        {
            bool isCorrectExportType = false;
            switch (exportType.ToLower())
            {
                case "e-book (.epub)":
                case "gobible":
                case "pdf (using openoffice/libreoffice":
                case "pdf (using prince)":
                case "indesign":
                case "openoffice/libreoffice":
                case "xelatex":
                case "sword":
                case "theword/mysword":
                    isCorrectExportType = true;
                    break;
            }
            return isCorrectExportType;
        }

        private static int CaptureFileList(string[] args, int i, List<string> files)
        {
            // store the files in our internal list for now 
            // (single filenames and * will end up as a single element in the list)
            while (args[i].EndsWith(","))
            {
                files.Add(args[i].Substring(0, args[i].Length - 1));
                i++;
            }
            files.Add(args[i++]);
            return i;
        }

        private static void Usage()
        {
            string val = "The usage is PathwayExport[--target<format>][--directory <filePath>] inputfile(s)";
            Console.Write(val);
            val = "   --target | -t <format>    (required) desired output format. Can be one of:\r\n";
            Console.Write(val);
            val = "                             \"E-Book (.epub)\"           .epub format.\r\n";
            Console.Write(val);
            val = "                             \"GoBible\"                  GoBible .jar format.\r\n";
            Console.Write(val);
            val = "                             \"PDF (using OpenOffice/LibreOffice)\" \r\n";
            Console.Write(val);
            val = "                                                          .pdf format.\r\n";
            Console.Write(val);
            val = "                             \"PDF (using Prince)\"       .pdf format.\r\n";
            Console.Write(val);
            val = "                             \"InDesign\"                 .idml format.\r\n";
            Console.Write(val);
            val = "                             \"OpenOffice/LibreOffice\"   .odt format.\r\n";
            Console.Write(val);
            val = "                             \"XeLaTex\"                  .tex format.\r\n";
            Console.Write(val);
            val = "                             \"sword\"                    format.\r\n";
            Console.Write(val);
            val = "                             \"theword/mysword\"          format.\r\n";
            Console.Write(val);
            val = "   --directory | -d <path>   (required) full path to the content.\r\n";
            Console.Write(val);
            val = "   --files | -f {<file>[, <file>] | *}\r\n";
            Console.Write(val);
            val = "                             (required) files to process, or * for all files in\r\n";
            Console.Write(val);
            val = "                             the directory specified by the -d flag.\r\n";
            Console.Write(val);
            val = "   --css | -c                stylesheet file name (required for xhtml only).\r\n";
            Console.Write(val);
            val = "   --showdialog | -s         Show the Export Through Pathway dialog, and take\r\n";
            Console.Write(val);
            val = "                             the values for target format, style, etc. from\r\n";
            Console.Write(val);
            val = "                             the user's input on the dialog.\r\n";
            Console.Write(val);
            val = "   --launch | -l             launch resulting output in target back end.\r\n";
            Console.Write(val);
            val = "   --name | -n               [main] Project name.\r\n\r\n\r\n";
            Console.Write(val);
            val = "Examples:\r\n\r\n";
            Console.Write(val);
            val = "   PathwayB.exe -d \"D:\\MyProject\" -if usfm -f * -t \"E-Book (.epub)\" \r\n";
            Console.Write(val);
            val = "                             -i \"Scripture\" -n \"SEN\" \r\n";
            Console.Write(val);
            val = "      Creates an .epub file from the USFM project found in D:\\MyProject.\r\n\r\n";
            Console.Write(val);
            val = "   PathwayB.exe -d \"D:\\MyDict\" -if xhtml -c \"D:\\MyDict\\main.css\" \r\n";
            Console.Write(val);
            val = "                             -f \"main.xhtml\", \"FlexRev.xhtml\" \r\n";
            Console.Write(val);
            val = "                             -t \"E-Book (.epub)\" -i \"Dictionary\" \r\n";
            Console.Write(val);
            val = "                             -n \"Sena 3-01\" \r\n";
            Console.Write(val);
            val = "      Creates an .epub file from the xhtml dictionary found in D:\\MyDict.\r\n";
            Console.Write(val);
            val = "      Both main and reversal index files are included in the output.\r\n\r\n";
            Console.Write(val);
            val = "   PathwayB.exe -d \"D:\\Project2\" -if usfm -f * -i \"Scripture\" -n \"SEN\"-s\r\n";
            Console.Write(val);
            val = "      Displays the Export Through Pathway dialog, then generates output from\r\n";
            Console.Write(val);
            val = "      the USFM project found in D:\\Project2 to the user-specified output \r\n";
            Console.Write(val);
            val = "      format and style.\r\n\r\n";
            Console.Write(val);
            val = "Notes:\r\n\r\n";
            Console.Write(val);
            val = "-  Not all output types may be available, depending on your installation\r\n";
            Console.Write(val);
            val = "   Package. To verify the available output types, open the Configuration\r\n";
            Console.Write(val);
            val = "   Tool, click the Defaults button and click on the Destination drop-down.\r\n";
            Console.Write(val);
            val = "   The available outputs match the selections in this list.\r\n\r\n";
            Console.Write(val);
            val = "-  For dictionary output, the reversal index file needs to be named\r\n";
            Console.Write(val);
            val = "   \"FlexRev.xhtml\". this is to maintain consistency with the file naming\r\n";
            Console.Write(val);
            val = "   convention used in Pathway.\r\n\r\n";
            Console.Write(val);
        }

        private InputFormat GetFileFormat(string format)
        {
            InputFormat inFormat = InputFormat.XHTML;
            switch (format.ToLower())
            {
                case "xhtml":
                    inFormat = InputFormat.XHTML;
                    break;
                case "usfm":
                    inFormat = InputFormat.USFM;
                    break;
                case "usx":
                    inFormat = InputFormat.USX;
                    break;
                case "ptb":
                    inFormat = InputFormat.PTBUNDLE;
                    break;
            }
            return inFormat;
        }


    }
}
