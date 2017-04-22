using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Strings.Tests
{
    [TestClass()]
    public class HelperTests
    {
        [TestMethod()]
        public void ToFileNameStringTest()
        {
            DateTime dateTime = new DateTime(2010, 1, 2, 3, 4, 5);
            string expected = "2010-01-02T03-04";
            string current = dateTime.ToFileNameString();
            Assert.AreEqual(expected, current);
        }

        [TestMethod()]
        public void ToUpperFirstWordTest()
        {
            string test = "hello world!";
            string expected = "HELLO world!";
            string current = test.ToUpperFirstWord();
            Assert.AreEqual(expected, current);
        }


    }
}