﻿// --------------------------------------------------------------------------------------------
#region // Copyright Â© 2009, SIL International. All Rights Reserved.
// <copyright file="FeatureSheetTest.cs" from='2009' to='2009' company='SIL International'>
//		Copyright Â© 2009, SIL International. All Rights Reserved.   
//    
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
#endregion
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using SIL.PublishingSolution;
using NUnit.Framework;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Test.CssDialog
{
    /// <summary>
    ///This is a test class for FeatureSheetTest and is intended
    ///to contain all FeatureSheetTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FeatureSheetTest
    {
        /// <summary>
        ///A test for Sheet
        ///</summary>
        [Test]
        public void SheetTest()
        {
            FeatureSheet target = new FeatureSheet(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Sheet = expected;
            actual = target.Sheet;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Features
        ///</summary>
        [Test]
        public void FeaturesTest()
        {
            FeatureSheet target = new FeatureSheet(); // TODO: Initialize to an appropriate value
            List<string> expected = null; // TODO: Initialize to an appropriate value
            List<string> actual;
            target.Features = expected;
            actual = target.Features;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [Test]
        public void WriteTest()
        {
            FeatureSheet target = new FeatureSheet(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            CommonTestMethod.DisableDebugAsserts();
            try
            {
                actual = target.Write();
                Assert.Fail("Write succeeded with no sheet name!");
            }
            catch (Exception e)
            {
                ArgumentNullException expectedException = new ArgumentNullException();
                Assert.AreEqual(expectedException.GetType(), e.GetType());
            }
        }

        /// <summary>
        ///A test for SaveFeatures
        ///</summary>
        [Test]
        public void SaveFeaturesTest()
        {
            FeatureSheet target = new FeatureSheet(); // TODO: Initialize to an appropriate value
            TreeView tv = null; // TODO: Initialize to an appropriate value
            // TODO: SaveFeatures depends on settings in Param class being loaded!
            CommonTestMethod.DisableDebugAsserts();
            try
            {
                target.SaveFeatures(tv);
                Assert.Fail("SaveFeatures returned when Param not loaded!");
            }
            catch (Exception e)
            {
                KeyNotFoundException expectedException = new KeyNotFoundException();
                Assert.AreEqual(expectedException.GetType(), e.GetType());
            }
        }

        /// <summary>
        ///A test for ReadToEnd
        ///</summary>
        [Test]
        public void ReadToEndTest()
        {
            FeatureSheet target = new FeatureSheet(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            // TODO: Sheet parameter must be set
            CommonTestMethod.DisableDebugAsserts();
            try
            {
                actual = target.ReadToEnd();
                Assert.Fail("ReadToEnd returned when Sheet not set!");
            }
            catch (Exception e)
            {
                NullReferenceException expectedException = new NullReferenceException();
                Assert.AreEqual(expectedException.GetType(), e.GetType());
            }
        }

        /// <summary>
        ///A test for FeatureSheet Constructor
        ///</summary>
        [Test]
        public void FeatureSheetConstructorTest1()
        {
            FeatureSheet target = new FeatureSheet();
            //TODO: Implement code to verify target
        }

        /// <summary>
        ///A test for FeatureSheet Constructor
        ///</summary>
        [Test]
        public void FeatureSheetConstructorTest()
        {
            string sheet = string.Empty; // TODO: Initialize to an appropriate value
            FeatureSheet target = new FeatureSheet(sheet);
            //TODO: Implement code to verify target
        }
    }
}