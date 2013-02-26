﻿// --------------------------------------------------------------------------------------------
// <copyright file="Dic4MidStreamWriter.cs" from='2013' to='2013' company='SIL International'>
//      Copyright © 2013, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Stylepick FeatureSheet
// </remarks>
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using SIL.Tool;

namespace SIL.PublishingSolution
{
    public class Dic4MidStreamWriter
    {
        protected StreamWriter StreamWriter { get; set; }

        public Dic4MidStreamWriter(PublicationInformation projInfo)
        {
            var name = Path.GetFileNameWithoutExtension(projInfo.DefaultXhtmlFileWithPath);
            var myPath = Path.GetDirectoryName(projInfo.DefaultXhtmlFileWithPath);
            Debug.Assert(myPath != null);
            StreamWriter = new StreamWriter(Path.Combine(myPath, name + ".txt"));
        }

        public void WriteLine(string value)
        {
            StreamWriter.WriteLine(value);
        }

        public void Close()
        {
            StreamWriter.Close();
        }
    }
}
