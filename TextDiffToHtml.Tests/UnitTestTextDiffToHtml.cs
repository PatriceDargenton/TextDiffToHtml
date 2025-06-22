
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace TextDiffToHtml.Tests
{
    [TestClass]
    public class UnitTestTextDiffToHtml
    {
        [TestMethod]
        public void TestAllSamples()
        {
            var result = Program.Demo(showHtml:false);
            string path = AppContext.BaseDirectory; // Application.StartupPath() equivalent in .Net8;
            var samplesPath = Path.Combine(path, @"..\..\..\..", "Samples.html");
            var fullPath = Path.GetFullPath(samplesPath);
            var exists = File.Exists(samplesPath);
            Assert.AreEqual(exists, true);
            var expected = File.ReadAllText(samplesPath);
            Assert.AreEqual(result, expected);
        }
    }
}