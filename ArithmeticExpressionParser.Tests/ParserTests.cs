using System;
using NUnit.Framework;

namespace ArithmeticExpressionParser.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [TestCase("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM version = Dimension Pro Vers.3.9.R1 For NT Version 7", ExpectedResult = "3.9.R1")]
        public string Version(string expression)
        {
            var parser = new Parser(expression);
            return parser.Parse();
        }

        [Test]
        public string MultipleNumbersWithParenthesis(string expression)
        {
            var parser = new Parser(expression);
            return parser.Parse();
        }
    }
}

