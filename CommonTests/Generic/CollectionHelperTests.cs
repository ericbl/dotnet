using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Common.Generic.Tests
{
    [TestClass()]
    public class CollectionHelperTests
    {
        /// <summary>
        /// Splits the into chunks test.
        /// </summary>
        [TestMethod()]
        public void SplitIntoChunksTest()
        {
            var src = new[] { 1, 2, 3, 4, 5, 6 };

            var c3 = src.SplitIntoChunks(3);      // {{1, 2, 3}, {4, 5, 6}};     
            foreach (var sub in c3)
            {
                var list = sub.ToList();
            }
                              
            Assert.AreEqual(2, c3.Count());       // 2
            var sum = c3.Select(c => c.Sum()).ToArray(); // {6, 15}
            Assert.AreEqual(15, sum[1]);

            var arrays = c3.Select(c => c.ToArray()).ToArray();
            Assert.AreEqual(arrays[0][2], 3);
            Assert.AreEqual(arrays[1][0], 4);

            var take2 = c3.Select(c => c.Take(2)); // {{1, 2}, {4, 5}}
            var sum2 = take2.Select(c => c.Sum()).ToArray(); // {3, 9}
            Assert.AreEqual(9, sum2[1]);

            var c4 = src.SplitIntoChunks(4);      // {{1, 2, 3, 4}, {5, 6}}; 
            var arrays4 = c4.Select(c => c.ToArray()).ToArray();
            Assert.AreEqual(arrays4[0][3], 4);
            Assert.AreEqual(arrays4[1][0], 5);
        }
    }
}