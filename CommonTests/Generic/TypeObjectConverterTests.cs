using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Generic.Tests
{
    [TestClass()]
    public class TypeObjectConverterTests
    {
        [TestMethod()]
        public void ConvertTypeChangeTypeNullableTest()
        {
            ModelTest model = new ModelTest();
            model.myInt = 5;
            ModelTest model2 = new ModelTest();
            model2.myInt = 5;
            object actualValue = TypeObjectConverter.ConvertTypeChangeTypeNullable<int>(model.myInt.ToString());
            object expectedValue = model2.myInt;
            bool refEq = ReferenceEquals(actualValue, expectedValue);

            Assert.AreEqual(expectedValue, actualValue);
        }

        private class ModelTest
        {
            internal int myInt { get; set; }
            internal int? myNullableInt { get; set; }
            internal string myString { get; set; }
            internal DateTime myDate { get; set; }
            internal DateTime? myNullableDate { get; set; }
        }

        [TestMethod()]
        public void ToInt64NullableTest()
        {
            string testNr = "+41 79 626 74 78";            
            long? testNrValInt = testNr.ToInt64Nullable();
            Assert.IsTrue(testNrValInt.HasValue);
            long expectedNr = 41796267478;
            Assert.AreEqual(expectedNr, testNrValInt.Value);
        }
    }
}

