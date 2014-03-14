﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace PdfLicense
{
    public class Program
    {
        public static int ExitCode;
        public static string RedirectOutput;
        public static string LastError;
        private static Process myProcess = new Process();
        private static int _elapsedTime;
        private static bool _eventHandled;
        static List<string> _readLicenseFilesBylines = new List<string>();

        static void Main(string[] args)
        {

            string allUserPath = GetAllUserPath();
            string licenseFileName = ReadPathinLicenseFile(allUserPath);
            if (_readLicenseFilesBylines.Count < 0)
            {
                return;
            }
            string creatorTool = _readLicenseFilesBylines[3];
            string getPsApplicationPath = GetPSApplicationPath();
            getPsApplicationPath = Path.Combine(getPsApplicationPath, "ApplyPDFLicenseInfo.exe");
            if (File.Exists(getPsApplicationPath))
            {
                if (creatorTool.ToLower() == "libreoffice")
                {
                    RunCommand(getPsApplicationPath, "", 0);
                }
                else
                {
                    RunCommand(getPsApplicationPath, "", 1);
                }
            }
        }

        #region GetPSApplicationPath()

        public static string ProgInstall = string.Empty;
        public static string SupportFolder = string.Empty;
        /// <summary>
        /// Return the Local setting Path+ "SIL\Dictionary" 
        /// </summary>
        /// <returns>Dictionary Setting Path</returns>
        public static string GetPSApplicationPath()
        {
            if (ProgInstall == string.Empty)
                ProgInstall = GetApplicationPath();
            return SupportFolder == "" ? ProgInstall : PathCombine(ProgInstall, SupportFolder);
        }
        #endregion

        public static string GetApplicationPath()
        {
            string pathwayDir = GetPathwayDir();
            if (string.IsNullOrEmpty(pathwayDir))
                return Directory.GetCurrentDirectory();
            return pathwayDir;
        }

        public static string GetPathwayDir()
        {
            string pathwayDir = string.Empty;
            object regObj;
            try
            {
                if (RegistryHelperLite.RegEntryExists(RegistryHelperLite.CompanyKeyCurrentUser,
                    "Pathway", "PathwayDir", out regObj))
                {
                    return (string)regObj;
                }
                if (Path.PathSeparator == '/') //Test for Linux (see: http://www.mono-project.com/FAQ:_Technical)
                {
                    const string myPathwayDir = "/usr/lib/pathway";
                    RegistryAccess.SetStringRegistryValue("PathwayDir", myPathwayDir);
                    return myPathwayDir;
                }
                if (RegistryHelperLite.RegEntryExists(RegistryHelperLite.CompanyKeyLocalMachine,
                    "Pathway", "PathwayDir", out regObj))
                {
                    return (string)regObj;
                }
                var fwKey = RegistryHelperLite.CompanyKeyLocalMachine.OpenSubKey("FieldWorks");
                if (fwKey != null)
                {
                    if (!RegistryHelperLite.RegEntryExists(fwKey, "8.0", "RootCodeDir", out regObj))
                        if (!RegistryHelperLite.RegEntryExists(fwKey, "7.0", "RootCodeDir", out regObj))
                            if (!RegistryHelperLite.RegEntryExists(fwKey, "", "RootCodeDir", out regObj))
                                regObj = string.Empty;
                    pathwayDir = (string)regObj;
                    // The next line helps those using the developer version of FieldWorks
                    pathwayDir = pathwayDir.Replace("DistFiles", @"Output\Debug").Replace("distfiles", @"Output\Debug");
                }
                if (!File.Exists(PathCombine(pathwayDir, "PsExport.dll")))
                    pathwayDir = string.Empty;
                if (pathwayDir == string.Empty)
                {
                    pathwayDir = Directory.GetCurrentDirectory();
                }
            }
            catch { }
            return pathwayDir;
        }

        public static void RunCommand(string instPath, string name, string arg, bool wait)
        {
            _elapsedTime = 0;
            _eventHandled = false;

            try
            {
                // Start a process to print a file and raise an event when done.
                myProcess.StartInfo.FileName = name;
                myProcess.StartInfo.Arguments = arg;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.EnableRaisingEvents = true;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.UseShellExecute = string.IsNullOrEmpty(RedirectOutput);
                myProcess.StartInfo.WorkingDirectory = instPath;

                myProcess.Exited += new EventHandler(myProcess_Exited);
                myProcess.Start();

            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(RedirectOutput))
                {
                    string result = string.Empty;
                    StreamWriter streamWriter = new StreamWriter(PathCombine(instPath, RedirectOutput));
                    var errorArgs = string.Format("An error occurred trying to print \"{0}\":" + "\n" + ex.Message, arg);
                    result += errorArgs;
                    streamWriter.Write(result);
                    streamWriter.Close();
                    RedirectOutput = null;
                }
                return;
            }

            // Wait for Exited event, but not more than 30 seconds. 
            const int SLEEP_AMOUNT = 100;
            while (!_eventHandled)
            {
                _elapsedTime += SLEEP_AMOUNT;
                if (_elapsedTime > 30000)
                {
                    break;
                }
                Thread.Sleep(SLEEP_AMOUNT);
            }
            myProcess.Close();
        }

        public static bool RunCommand(string szCmd, string szArgs, int wait)
        {
            if (szCmd == null) return false;
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            myproc.EnableRaisingEvents = false;
            myproc.StartInfo.CreateNoWindow = true;
            myproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myproc.StartInfo.FileName = szCmd;
            myproc.StartInfo.Arguments = szArgs;

            if (myproc.Start())
            {
                //Using WaitForExit( ) allows for the host program
                //to wait for the command its executing before it continues
                if (wait == 1) myproc.WaitForExit();
                else myproc.Close();

                return true;
            }
            else return false;
        }

        private static string ReadPathinLicenseFile(string allUserPath)
        {
            string fileLoc = PathCombine(allUserPath, "License.txt");
            string executePath = string.Empty;
            int countRead = 0;

            if (File.Exists(fileLoc))
            {
                using (StreamReader reader = new StreamReader(fileLoc))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        _readLicenseFilesBylines.Add(line);
                    }

                    reader.Close();

                    executePath = fileLoc;
                }
            }
            return executePath;
        }

        /// <summary>
        /// Get all user path
        /// </summary>
        /// <returns></returns>
        public static string GetAllUserPath()
        {
            string allUserPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            allUserPath += "/SIL/Pathway";
            return DirectoryPathReplace(allUserPath);
        }

        /// <summary>
        /// Make sure the path contains the proper / for the operating system.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns>normalized path</returns>
        public static string PathCombine(string path1, string path2)
        {
            path1 = DirectoryPathReplace(path1);
            path2 = DirectoryPathReplace(path2);
            if (path1 == null)
            {
                return path2;
            }
            else if (path2 == null)
            {
                return path1;
            }
            else
            {
                return Path.Combine(path1, path2);
            }
        }

        /// <summary>
        /// Make sure the path contains the proper / for the operating system.
        /// </summary>
        /// <param name="path">input path</param>
        /// <returns>normalized path</returns>
        public static string DirectoryPathReplace(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            string returnPath = path.Replace('/', Path.DirectorySeparatorChar);
            returnPath = returnPath.Replace('\\', Path.DirectorySeparatorChar);
            return returnPath;
        }

        // Handle Exited event and display process information. 
        private static void myProcess_Exited(object sender, System.EventArgs e)
        {
            _eventHandled = true;
        }

    }
}
