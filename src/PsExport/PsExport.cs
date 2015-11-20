// --------------------------------------------------------------------------------------------
// <copyright file="PsExport.cs" from='2009' to='2014' company='SIL International'>
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
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using JWTools;
using L10NSharp;
using SilTools;
using SIL.Tool;
using SIL.Tool.Localization;

namespace SIL.PublishingSolution
{
    /// <summary>
    /// Implements Publishing Solutins for Translation Editor
    /// </summary>
    public class PsExport: IExporter
    {
        public bool _fromNUnit = false;
        private string _selectedCssFromTemplate = string.Empty;
        private Progress pb;

        #region Properties
        /// <summary>Gets or sets Output format (ODT, PDF, INX, TeX, HTM, PDB, etc.)</summary>
        public string Destination
        {
            get
            {
                return Param.Value[Param.PrintVia];
            }
            set
            {
                Param.SetValue(Param.PrintVia, value);
            } 
        }

        /// <summary>Gets or sets data type (Scripture, Dictionary)</summary>
        public string DataType { get; set; }

        /// <summary>UI progress bar</summary>
        public ProgressBar ProgressBar { get; set; }
        #endregion Properties

        private string _projectFile;

        #region Export
        /// <summary>
        /// Have the utility do what it does.
        /// </summary>
        public void Export(string outFullName)
        {
			Common.SetupLocalization();
            Debug.Assert(DataType == "Scripture" || DataType == "Dictionary", "DataType must be Scripture or Dictionary");
            Debug.Assert(outFullName.IndexOf(Path.DirectorySeparatorChar) >= 0, "full path for output must be given");
			string caption = LocalizationManager.GetString("PsExport.ExportClick.Caption", "Pathway Export", "");
            try

            {
                pb = new Progress();
                //get xsltFile from ExportThroughPathway.cs
                XsltPreProcess(outFullName);

                string supportPath = GetSupportPath();
				Backend.Load(Common.ProgInstall);
                LoadProgramSettings(supportPath);
                LoadDataTypeSettings();
                var outDir = Path.GetDirectoryName(outFullName);
                DefaultProjectFileSetup(outDir);
                SubProcess.BeforeProcess(outFullName);

                var mainXhtml = Path.GetFileNameWithoutExtension(outFullName) + ".xhtml";
                var mainFullName = Common.PathCombine(outDir, mainXhtml);
                Debug.Assert(mainFullName.IndexOf(Path.DirectorySeparatorChar) >= 0, "Path for input file missing");
                if (string.IsNullOrEmpty(mainFullName) || !File.Exists(mainFullName))
                {
                    var msg = new[] { "Input File(main.xhtml) is not Found" };
                    LocDB.Message("errFnFound", "Input File(main.xhtml) is not Found.", msg, LocDB.MessageTypes.Error, LocDB.MessageDefault.First);
                    return;
                }
                string cssFullName = GetCssFullName(outDir, mainFullName);
                if (cssFullName == null) return;
				SetPageCenterTitle(cssFullName);
                _selectedCssFromTemplate = Path.GetFileNameWithoutExtension(cssFullName);
                string fluffedCssFullName;
                string revFileName = string.Empty;
                if (Path.GetFileNameWithoutExtension(outFullName) == "FlexRev")
                {
                    fluffedCssFullName = GetFluffedCssFullName(GetRevFullName(outFullName), outDir, cssFullName);
                }
                else
                {
                    fluffedCssFullName = GetFluffedCssFullName(outFullName, outDir, cssFullName);
                    revFileName = GetRevFullName(outDir);
                }
                string fluffedRevCssFullName = string.Empty;
                if (revFileName.Length > 0)
                {
                    fluffedRevCssFullName = GetFluffedCssFullName(revFileName, outDir, cssFullName);
                }
                DestinationSetup();
                SetDefaultLanguageFont(fluffedCssFullName, mainFullName, fluffedRevCssFullName);
                if (DataType == "Scripture")
                {
                    SeExport(mainXhtml, Path.GetFileName(fluffedCssFullName), outDir);
                }
                else if (DataType == "Dictionary")
                {
                    
                    string revFullName = GetRevFullName(outDir);
                    DeExport(outFullName, fluffedCssFullName, revFullName, fluffedRevCssFullName);
                }
            }
            catch (InvalidStyleSettingsException err)
            {
                if (_fromNUnit)
                {
                    Console.WriteLine(string.Format(err.ToString(), err.FullFilePath), "Pathway Export");
                }
                else
                {
					Utils.MsgBox(string.Format(err.ToString(), err.FullFilePath), caption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            catch (UnauthorizedAccessException err)
            {
                if (_fromNUnit)
                {
                    Console.WriteLine(string.Format(err.ToString(), "Sorry! You might not have permission to use this resource."));
                }
                else
                {
                    var msg = LocalizationManager.GetString("PsExport.ExportClick.Message", "Sorry! You might not have permission to use this resource.", "");
					Utils.MsgBox(string.Format(err.ToString(), msg), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            catch (Exception ex)
            {
                if (_fromNUnit)
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    if (ex.ToString().IndexOf("Module1.xml") == -1)
                    {
						Utils.MsgBox(ex.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }
            finally
            {
                if (pb != null)
                {
                    pb.Close();
                }
            }
        }

        /// <summary>
        /// The constant "HeaderTitleLable" will replaced by the Title text in the DictionarySettingsXmlFIle.
        /// </summary>
        /// <param name="cssFullName">Css file before export</param>
        private static void SetPageCenterTitle(string cssFullName)
        {
	        string pathwayInstallerPath = GetSupportPath();
			if (cssFullName.Contains(pathwayInstallerPath))
			{
				return;
			}

            string fileDir = Path.GetDirectoryName(cssFullName);
            string fileName = Path.GetFileName(cssFullName);
			
	        if (fileName != null && fileName.IndexOf("Layout", System.StringComparison.Ordinal) == 0) 
				return; //For PathwayB Test fail

            fileName = "Preserve" + fileName;
            var fs = new FileStream(cssFullName, FileMode.Open);
            var stream = new StreamReader(fs);
            string modifiedFile = Common.PathCombine(fileDir, fileName);
            var fs2 = new FileStream(modifiedFile, FileMode.Create, FileAccess.Write);
            var sw2 = new StreamWriter(fs2);
            string line, prevLine = string.Empty;
            while ((line = stream.ReadLine()) != null)
            {
                if (prevLine.IndexOf("@top-center{") == 0 &&   line.IndexOf("content:") == 0)
                {
                    const string titlePath = "//Metadata/meta[@name='Title']/defaultValue";
                    string titleText = Param.GetItem(titlePath).InnerText;
                    line = line.Replace(line, "content:\"" + titleText + "\";");
                    sw2.WriteLine(line);
                }
                else
                {
                    sw2.WriteLine(line);
                }
                prevLine = line;
            }
            sw2.Close();
            fs.Close();
            fs2.Close();
            File.Copy(modifiedFile, cssFullName, true);
            File.Delete(modifiedFile);
        }

        /// <summary>
        /// Preprocess the xhtml file using xsl file.
        /// </summary>
        /// <param name="outFullName">input xhtml file</param>
        protected void XsltPreProcess(string outFullName)
        {
            if (!Param.Value.ContainsKey(Param.Preprocessing)) return;
            var preprocessing = Param.Value[Param.Preprocessing];
            if (preprocessing == string.Empty) return;
            var preProcessList = preprocessing.Split(",".ToCharArray());
            var curInput = AdjustNameExt(outFullName, "_.xhtml");
            File.Copy(outFullName, curInput, true);
            for (int i = 0; i < preProcessList.Length; i++)
            {

                var processName = Common.PathCombine(DataType, preProcessList[i] + ".xsl");
                string xsltFullName = Common.PathCombine(Common.GetAllUserPath(), processName);
                if (!File.Exists(xsltFullName))
                {
                    xsltFullName = Common.PathCombine(Common.PathCombine(Common.GetPSApplicationPath(), "Preprocessing"), processName);
                }
                Debug.Print("xsltFullName: {0}", xsltFullName);
                string resultExtention = string.Format("{0}.xhtml", i);
                Common.XsltProcess(curInput, xsltFullName, resultExtention);
                curInput = AdjustNameExt(curInput, resultExtention);
            }
            File.Copy(curInput, outFullName, true);
            File.Delete(curInput);
        }

        private static string AdjustNameExt(string fullName, string extension)
        {
            return Common.PathCombine(Path.GetDirectoryName(fullName),
                                      Path.GetFileNameWithoutExtension(fullName) + extension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fluffedCssFullName"></param>
        /// <param name="mainFullName"></param>
        /// <param name="fluffedCssReversal"></param>
        private void SetDefaultLanguageFont(string fluffedCssFullName, string mainFullName, string fluffedCssReversal)
        {
            string fileName = Path.GetFileName(mainFullName);

            if (AppDomain.CurrentDomain.FriendlyName.ToLower() == "paratext.exe")
            {
                Common.ParaTextFontName(fluffedCssFullName);
            }
            else if (DataType == "Dictionary" && fileName == "main.xhtml")
            {
                Common.LanguageSettings(mainFullName, fluffedCssFullName, DataType == "Dictionary", fluffedCssReversal);
            }
            else
            {
                Common.TeLanguageSettings(fluffedCssFullName);
            }
        }


        /// <summary>
        /// Returns reversal name with path (empty string if file doesn't exist)
        /// </summary>
        /// <param name="outDir">Folder which contains reversal file if it exists</param>
        /// <returns>Reversal name with path</returns>
        protected static string GetRevFullName(string outDir)
        {
            if(File.Exists(outDir))
            {
                outDir = Path.GetDirectoryName(outDir);
            }
            string revFullName = Common.PathCombine(outDir, "FlexRev.xhtml");
            if (!File.Exists(revFullName))
                revFullName = "";
            else
            {
                Common.StreamReplaceInFile(revFullName, "<ReversalIndexEntry_Self>", "");
                Common.StreamReplaceInFile(revFullName, "</ReversalIndexEntry_Self>", "");
                Common.StreamReplaceInFile(revFullName, "class=\"headword\"", "class=\"headref\"");
                string revCssFullName = revFullName.Substring(0, revFullName.Length - 6) + ".css";
                Common.StreamReplaceInFile(revCssFullName, ".headword", ".headref");
                AddHomographAndSenseNumClassNames.Execute(revFullName, revFullName);
            }
            return revFullName;
        }

        /// <summary>
        /// Combines all css files into one and adds expected css to beginning
        /// </summary>
        /// <param name="outputFullName">Name used to calculate default css name</param>
        /// <param name="outDir">where results will be stored</param>
        /// <param name="cssFullName">name and path of css file</param>
        /// <returns></returns>
        public string GetFluffedCssFullName(string outputFullName, string outDir, string cssFullName)
        {
            var mc = new MergeCss();
            string fluffedCssFullName;

            try
            {
                if (!File.Exists(cssFullName))
                {
                    var layout = Param.Value[Param.LayoutSelected];
                    cssFullName = Param.StylePath(Param.StyleFile[layout]);
                }
                string myCss = Common.PathCombine(outDir, Path.GetFileName(cssFullName));
                if (cssFullName != myCss)
                    File.Copy(cssFullName, myCss, true);
                var expCss = Path.GetFileNameWithoutExtension(outputFullName) + ".css";
                string expCssLine = "@import \"" + expCss + "\";";
                Common.FileInsertText(myCss, expCssLine);
                string outputCSSFileName = "merged" + expCss;
                var tmpCss = mc.Make(myCss, outputCSSFileName);
                fluffedCssFullName = Common.PathCombine(outDir, Path.GetFileName(tmpCss));
                File.Copy(tmpCss, fluffedCssFullName, true);
                File.Delete(tmpCss);
                Common.StreamReplaceInFile(fluffedCssFullName, "string(verse) ' = '", "string(verse) ' '"); //TD-1945
            }
            catch (Exception)
            {
                fluffedCssFullName = string.Empty;
            }
            return fluffedCssFullName;
        }

        /// <summary>
        /// Return full css name
        /// </summary>
        /// <param name="outDir">where to find css name</param>
        /// <param name="mainFullName">export name used to calculate css name</param>
        /// <returns>name and path of css</returns>
        protected string GetCssFullName(string outDir, string mainFullName)
        {
            var cssFullName = Param.StylePath(Param.Value[Param.LayoutSelected]);
            if (string.IsNullOrEmpty(cssFullName))
            {
                var stylePick = new PublicationTask { InputPath = outDir, CurrentInput = mainFullName, InputType = DataType };
                if (!Common.Testing)
                    stylePick.ShowDialog();
                else
                {
                    stylePick.DoLoad();
                    stylePick.DoAccept();
                }
                cssFullName = stylePick.cssFile;
            }
            return cssFullName;
        }

        /// <summary>
        /// Determines destination back end.
        /// </summary>
        protected void DestinationSetup()
        {
            if (string.IsNullOrEmpty(Destination))
            {
                var edlg = new ExportDlg { ExportType = DataType };
                edlg.ShowDialog();
                Destination = edlg.ExportType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outDir"></param>
        protected void DefaultProjectFileSetup(string outDir)
        {
            _projectFile = Common.PathCombine(outDir, DataType + ".de");
            string deFile = Common.FromRegistry(Common.PathCombine(Param.Value[Param.SamplePath], Path.GetFileName(_projectFile)));
            if (!File.Exists(_projectFile) && File.Exists(deFile))
                File.Copy(deFile, _projectFile);
        }

        protected void LoadDataTypeSettings()
        {
            Param.Value[Param.InputType] = DataType;
            Param.LoadSettings();
        }

        protected static void LoadProgramSettings(string supportPath)
        {
			Common.ProgBase = supportPath;
            Param.LoadSettings();
        }

        protected static string GetSupportPath()
        {
            return Common.GetPSApplicationPath();
        }

        #endregion Export

        #region Protected Function
        #region DeExport
        /// <summary>
        /// Exports the input files to the chosen destination
        /// </summary>
        /// <param name="lexiconFull">main dictionary content</param>
        /// /// <param name="lexiconCSS">main css file with style info</param>
        /// <param name="revFull">reversal content</param>
        /// <param name="revCSS">rev CSS file with style info</param>
        public void DeExport(string lexiconFull, string lexiconCSS, string revFull, string revCSS)
        {
            var projInfo = new PublicationInformation();
            projInfo.ProjectFileWithPath = _projectFile;
            projInfo.IsLexiconSectionExist = File.Exists(lexiconFull);
            SetReverseExistValue(projInfo);
            projInfo.SwapHeadword = false;
            projInfo.FromPlugin = true;
            projInfo.DefaultCssFileWithPath = lexiconCSS;
            projInfo.DefaultRevCssFileWithPath = revCSS;
            projInfo.DefaultXhtmlFileWithPath = lexiconFull;
            projInfo.ProjectInputType = "Dictionary";
            projInfo.DictionaryPath = Path.GetDirectoryName(lexiconFull);
            projInfo.ProjectName = Path.GetFileNameWithoutExtension(lexiconFull);
            projInfo.SelectedTemplateStyle = _selectedCssFromTemplate;

            string lexiconFileName = Path.GetFileName(lexiconFull);
            string revFileName = Path.GetFileName(revFull);
            if (lexiconFileName == revFileName)
                projInfo.IsLexiconSectionExist = false;
            if (projInfo.IsLexiconSectionExist && !projInfo.IsReversalExist)
            {
                projInfo.ProjectName = Path.GetFileNameWithoutExtension(lexiconFull);
            }
            else if (!projInfo.IsLexiconSectionExist && projInfo.IsReversalExist)
            {
                projInfo.ProjectName = Path.GetFileNameWithoutExtension(revFull);
            }
            SetExtraProcessingValue(projInfo);

            Backend.Launch(Destination, projInfo);
        }

        private void SetReverseExistValue(PublicationInformation projInfo)
        {
            if (_fromNUnit)
            {
                projInfo.IsReversalExist = !_fromNUnit;
            }
            else
            {
                projInfo.IsReversalExist = Param.Value[Param.ReversalIndex] == "True";
            }
        }

        private void SetExtraProcessingValue(PublicationInformation projInfo)
        {
            if (_fromNUnit)
            {
                projInfo.IsExtraProcessing = _fromNUnit;
            }
            else
            {
                projInfo.IsExtraProcessing = Param.Value[Param.ExtraProcessing] == "True";
            }
        }

        #endregion DeExport

        #region SeExport
        /// <summary>
        /// Exports the input files to the chosen destination
        /// </summary>
        /// <param name="mainXhtml">main dictionary content</param>
        /// <param name="jobFileName">css file with style info</param>
        /// <param name="outPath">destination path</param>
        public void SeExport(string mainXhtml, string jobFileName, string outPath)
        {
            Debug.Assert(mainXhtml.IndexOf(Path.DirectorySeparatorChar) < 0, mainXhtml + " should be just name");
            Debug.Assert(jobFileName.IndexOf(Path.DirectorySeparatorChar) < 0, jobFileName + " should be just name");
            Common.ShowMessage = !Common.Testing;

            var projInfo = new PublicationInformation();
            var mainSection = File.Exists(Common.PathCombine(outPath, mainXhtml));
            projInfo.DefaultCssFileWithPath = Common.PathCombine(outPath, jobFileName);
            projInfo.ProjectInputType = "Scripture";
            projInfo.FromPlugin = true;
            projInfo.DictionaryPath = outPath;
            projInfo.SelectedTemplateStyle = _selectedCssFromTemplate;
            if (mainSection)
            {
                projInfo.DefaultXhtmlFileWithPath = Common.PathCombine(outPath, mainXhtml);
                string DictionaryName = Common.PathCombine(Path.GetDirectoryName(projInfo.DefaultXhtmlFileWithPath), Path.GetFileNameWithoutExtension(projInfo.DefaultXhtmlFileWithPath));
                projInfo.DictionaryOutputName = DictionaryName;
                projInfo.IsOpenOutput = !Common.Testing;
                projInfo.ProjectName = Path.GetFileNameWithoutExtension(mainXhtml);
                SetExtraProcessingValue(projInfo);
                Backend.Launch(Destination, projInfo);
            }
        }
        #endregion SeExport
        #endregion Protected Function

    }
}