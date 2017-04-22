using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Strings.Tests
{
    [TestClass()]
    public class StringCharacterFilterTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        /// Test whether Alpha Comparer works well
        /// </summary>
        [TestMethod, Owner("Eric")]
        public void StringAlphaComparerTest()
        {
            bool actual = StringCharacterFilter.StringsContainsCommonCharacters("23459BCDEHIJKLNRSTZ", "afGMUVW");
            Assert.AreEqual(false, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("23459BCDEHIJKLNRSTZ", "afGMeUVW");
            Assert.AreEqual(true, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("23459BCDEHIJKLNRSTZ", "eUVW");
            Assert.AreEqual(true, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("23459BCDEHIJKLNRSTZ", "UVWc");
            Assert.AreEqual(true, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("23459BCDEHIJKLNRSTZ", "259");
            Assert.AreEqual(true, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("", "afGMeUVW");
            Assert.AreEqual(false, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("afGMeUVW", "");
            Assert.AreEqual(false, actual);

            actual = StringCharacterFilter.StringsContainsCommonCharacters("", "");
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        public void LatinizeAndConvertToASCIITest()
        {
            string europeanStr = "Bonjour ça va? C'est l'été! Ich möchte ä Ä á à â ê é è ë Ë É ï Ï î í ì ó ò ô ö Ö Ü ü ù ú û Û ý Ý ç Ç ñ Ñ";
            string expected = "Bonjour ca va? C'est l'ete! Ich moechte ae Ae a a a e e e e E E i I i i i o o o oe Oe Ue ue u u u U y Y c C n N";
            string expected2 = "Bonjourcava?C'estl'ete!IchmoechteaeAeaaaeeeeEEiIiiiooooeOeUeueuuuUyYcCnN";
            string actual = europeanStr.LatinizeAndConvertToASCII(true);
            string actual2 = europeanStr.LatinizeAndConvertToASCII();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected2, actual2);
        }

        [TestMethod()]
        public void RemoveOrReplaceSpecialCharactersSharePointTest()
        {
            string testStr = "Bonjour ça va? C'est l'été! Ich möcht fe {} fek \\ fe / f > Fe< fe";
            string expected = "Bonjour ça va C'est l'été! Ich möcht fe  fek  fe  f  Fe fe";
            string expected2 = "Bonjour ça va- C'est l'été! Ich möcht fe -- fek - fe - f - Fe- fe";
            string actual = testStr.RemoveOrReplaceSpecialCharactersSharePoint();
            string actual2 = testStr.RemoveOrReplaceSpecialCharactersSharePoint('-');
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected2, actual2);
        }
    }
}