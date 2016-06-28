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
    public class TokenizerTests
    {

        [TestCaseSource("TestExpressions")]
        public void CanParseExpressions(KeyValuePair<string, List<Token>> sampleExpressionAndExpectedResult)
        {
            var tokens = new Tokenizer().Scan(sampleExpressionAndExpectedResult.Key).ToList();
            var expected = sampleExpressionAndExpectedResult.Value;

            CompareTokens(expected, tokens);
        }

        private void CompareTokens(List<Token> expected, List<Token> parsed)
        {
            if (expected.Count != parsed.Count)
                Assert.Fail("expected and parsed token count different");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.True(parsed[i].GetType() == expected[i].GetType());
                if (expected[i] is NumberConstantToken)
                {
                    var ct = (expected[i] as NumberConstantToken).Value;
                    var pt = ((NumberConstantToken) parsed[i]).Value;

                    Assert.AreEqual(ct, pt);
                }
            }
        }

        private static List<KeyValuePair<string, List<Token>>> TestExpressions()
        {
            return new List<KeyValuePair<string, List<Token>>>
            {
                    new KeyValuePair<string, List<Token>>("1+2-3*4/5", new List<Token>
                    {
                        new NumberConstantToken(1),
                        new UnderlineToken(),
                        new NumberConstantToken(3),
                        new MultiplyToken(),
                        new NumberConstantToken(4),
                        new ForwardSlashToken(),
                        new NumberConstantToken(5)
                    })
            };

        }

    }


}
