﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeStylesTest : TokenizeTestBase<MdToken, MdTokenType>
    {
        [TestMethod]
        public void TokenizeMd_Bold01()
        {
            var actualTokens = this.Tokenize(@"**bold** text **a**b**c**", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Bold, 0, 8),
                new TokenData<MdTokenType>(MdTokenType.Bold, 14, 5),
                new TokenData<MdTokenType>(MdTokenType.Bold, 20, 5)
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeMd_Bold02()
        {
            var actualTokens = this.Tokenize(@"**bold*", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Bold03()
        {
            var actualTokens = this.Tokenize(@"** bold**", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Italic01()
        {
            var actualTokens = this.Tokenize(@"*italic* text *a*b*c*", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Italic, 0, 8),
                new TokenData<MdTokenType>(MdTokenType.Italic, 14, 3),
                new TokenData<MdTokenType>(MdTokenType.Italic, 18, 3)
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeMd_Italic02()
        {
            var actualTokens = this.Tokenize(@"*italic", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Italic03()
        {
            var actualTokens = this.Tokenize(@"_ italic_", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Italic04()
        {
            var actualTokens = this.Tokenize(@"_italic_ text _a_b_c_", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Italic, 0, 8),
                new TokenData<MdTokenType>(MdTokenType.Italic, 14, 3),
                new TokenData<MdTokenType>(MdTokenType.Italic, 18, 3)
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeMd_Italic05()
        {
            var actualTokens = this.Tokenize(@"_italic", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Italic06()
        {
            var actualTokens = this.Tokenize(@"_ italic_", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Monospace01()
        {
            var actualTokens = this.Tokenize(@"`italic` text `a`b`c`", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Monospace, 0, 8),
                new TokenData<MdTokenType>(MdTokenType.Monospace, 14, 3),
                new TokenData<MdTokenType>(MdTokenType.Monospace, 18, 3)
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeMd_Monospace02()
        {
            var actualTokens = this.Tokenize(@"`italic", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Monospace03()
        {
            var actualTokens = this.Tokenize(@"` italic_", new MdTokenizer());
            Assert.AreEqual(0, actualTokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Mixed01()
        {
            var actualTokens = this.Tokenize(@"**bold _text_ *a*b_c_**", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Bold, 0, 7),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 7, 6),
                new TokenData<MdTokenType>(MdTokenType.Bold, 13, 1),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 14, 3),
                new TokenData<MdTokenType>(MdTokenType.Bold, 17, 1),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 18, 3),
                new TokenData<MdTokenType>(MdTokenType.Bold, 21, 2),
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeMd_Mixed02()
        {
            var actualTokens = this.Tokenize(@"_italic **text** **a**b**c**_", new MdTokenizer());
            var expectedTokens = new TokenData<MdTokenType>[]
            {
                new TokenData<MdTokenType>(MdTokenType.Italic, 0, 8),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 8, 8),
                new TokenData<MdTokenType>(MdTokenType.Italic, 16, 1),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 17, 5),
                new TokenData<MdTokenType>(MdTokenType.Italic, 22, 1),
                new TokenData<MdTokenType>(MdTokenType.BoldItalic, 23, 5),
                new TokenData<MdTokenType>(MdTokenType.Italic, 28, 1),
            };

            TokensCompare<MdTokenType, MdToken>.Compare(expectedTokens, actualTokens);
        }
    }
}