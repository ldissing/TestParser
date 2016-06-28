using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using Tokens;

namespace ArithmeticExpressionParser.Tests
{
    [TestFixture]
    public class TokensWalkerTests
    {

        [Test]
        public void CanHandleEmptyList()
        {
            var tokens = new List<Token>();
            var walker = new TokensWalker(tokens);
            Assert.IsFalse(walker.ThereAreMoreTokens);
        }

        [Test]
        
        public void CannotPeekEmpty()
        {
            var tokens = new List<Token>();
            var walker = new TokensWalker(tokens);
            walker.PeekNext();
        }

        [Test]
        
        public void CannotGetNextEmpty()
        {
            var tokens = new List<Token>();
            var walker = new TokensWalker(tokens);
            walker.GetNext();
        }

        [Test]
        public void CanPeekExisting()
        {
            Token firstToken = new DashToken();
            var tokens = new List<Token>()
            {
                firstToken
            };
            var walker = new TokensWalker(tokens);
            Assert.IsTrue(walker.ThereAreMoreTokens);
            var first = walker.PeekNext();
            Assert.AreEqual(firstToken, first);
        }

        [Test]
        public void CanGetExisting()
        {
            Token firstToken = new DashToken();
            var tokens = new List<Token>()
            {
                firstToken
            };
            var walker = new TokensWalker(tokens);
            Assert.IsTrue(walker.ThereAreMoreTokens);
            var first = walker.GetNext();
            Assert.AreEqual(firstToken, first);
        }

        [Test]
        public void AfterGettingLastThereAreNoMoreIsSet()
        {
            Token firstToken = new DashToken();
            var tokens = new List<Token>()
            {
                firstToken
            };
            var walker = new TokensWalker(tokens);
            walker.GetNext();
            Assert.IsFalse(walker.ThereAreMoreTokens);
        }
    }


}
