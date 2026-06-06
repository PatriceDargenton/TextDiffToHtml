
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
            var result = Program.Demo(showHtml: false);
            var normalizedResult = NormalizeLineEndings(result);

            string path = AppContext.BaseDirectory;
            var samplesExpectedPath = Path.Combine(path, @"..\..\..\..", "Samples.html");

            Assert.IsTrue(File.Exists(samplesExpectedPath), $"File not found: {samplesExpectedPath}");

            var expected = File.ReadAllText(samplesExpectedPath);
            var normalizedExpected = NormalizeLineEndings(expected);

            Assert.AreEqual(
                normalizedExpected,
                normalizedResult,
                "Difference in content (excluding line ending style).");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}