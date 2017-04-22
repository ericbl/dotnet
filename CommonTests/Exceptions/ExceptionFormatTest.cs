using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Logging;

namespace Common.Exceptions.Tests
{
    /// <summary>
    ///This is a test class for ExceptionFormatTest and is intended
    ///to contain all ExceptionFormatTest Unit Tests
    ///</summary>
    [TestClass]
    public class ExceptionFormatTest
    {
        private static readonly ILogger logger = LoggerGenerator.CreateLogger(typeof(ExceptionFormatTest));

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        /// <value>The test context.</value>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///A test for FormatKey
        ///</summary>
        [TestMethod]
        public void FormatKeyTest()
        {
            logger.WriteInfo("Logging FormatKeyTest - begin");
            object[] key = new object[] { 1, "Test" };
            string expected = string.Format("{0}; {1}", key[0], key[1]);
            string actual;
            actual = ExceptionFormat.FormatKey(key);
            Assert.AreEqual(expected, actual, "Ausgabe des Schlüssels erfolg nicht in dem erwarteten Format");
            logger.WriteInfo("Logging FormatKeyTest - end");
        }
    }
}
