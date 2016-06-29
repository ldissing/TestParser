using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;
using ArithmeticExpressionParser;
using System.Linq;
using System.IO;

using Tokens;

namespace UnitTestProject1
{
    static class Extension
    {
        public static IEnumerable<TSource> IndexRange<TSource>(
           this IList<TSource> source,
           int fromIndex,
           int toIndex)
        {
            int currIndex = fromIndex;
            while (currIndex <= toIndex)
            {
                yield return source[currIndex];
                currIndex++;
            }
        }
    }
     [TestClass]
    public class UnitTest1
    {
         [TestMethod]
         public void TestNews()
         {
             string[] files = Directory.GetFiles(@"C:\DimensionTrader\EDR.Daily\News\ForexFactory", "160629.htm");
             ForexFactoryParser.Parser p = null;
             try
             {
                 foreach (string f in files)
                 {
                     if (p == null)
                         p = new ForexFactoryParser.Parser(f);
                     else
                         p = new ForexFactoryParser.Parser(f);
                 }
             }
             catch { }
             if (p != null)
                 foreach (string ni in p.NewsDict.Keys)
                 {
                     string test = p.NewsDict[ni].ToString();
                     if (test != "" && p.NewsDict[ni].time > DateTime.Now)
                         Console.WriteLine(test);
                 }
             
         }
         [TestMethod]
         public void TestParser()
         {
             string[] files = Directory.GetFiles(@"C:\temp", "*.txt");
             Parser p = null;
             try
             {
                 foreach (string f in files)
                 {
                     if (p == null)
                         p = new Parser(f);
                     else
                         p = new Parser(f, p.OrderDict);
                 }
             }
             catch { }
             if (p != null)
                 p.OutputDailyProfit();
         }
        [TestMethod]
        public void TestMethod1()
        {

            var tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 PM version = Dimension Pro Vers.3.9.R1 For NT Version 7").ToList();
            var expected = new List<Token>() { new TradeDateTimeToken(), new VersionToken("Vers.3.9.R1") };

            CompareTokens(expected, tokens);
            // name does not contain limit, stop, sg exit
            tokens = new Tokenizer().Scan("2016_05_02 09:08:16 - 5/2/2016 9:06:19 AM order: Order='f2bdbc5f1d8d4d459ea1e8b91b72e7ac/Mohan' Name='DTMOJOAE.CL.0.1' State=Filled Instrument='CL 06-16' Action=SellShort Limit price=0 Stop price=0 Quantity=1 Type=Market Tif=Day OverFill=False Oco='' Filled=1 Fill price=45.07 Token='f2bdbc5f1d8d4d459ea1e8b91b72e7ac' Gtd='1/1/0001 12:00:00 AM'").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new OrderEntryToken(new Tokenizer().OrderScan("Order='f2bdbc5f1d8d4d459ea1e8b91b72e7ac/Mohan' Name='DTMOJOAE.CL.0.1' State=Filled Instrument='CL 06-16' Action=SellShort Limit price=0 Stop price=0 Quantity=1 Type=Market Tif=Day OverFill=False Oco='' Filled=1 Fill price=45.07 Token='f2bdbc5f1d8d4d459ea1e8b91b72e7ac' Gtd='1/1/0001 12:00:00 AM'").ToList()) };
            CompareTokens(expected, tokens);
            tokens = new Tokenizer().Scan("2016_05_02 09:08:16 - 5/2/2016 9:06:19 AM order: Order='8226222b456f4ecebc4b311b18d3548c/Mohan' Name='Limit DTMOJOAE.CL.0.1' State=Working Instrument='CL 06-16' Action=BuyToCover Limit price=44.91 Stop price=0 Quantity=1 Type=Limit Tif=Day OverFill=False Oco='c9a8fdf3-3295-4495-be4d-ab5ad40ce7a0DTMOJOAE.CL.0.1' Filled=0 Fill price=0 Token='8226222b456f4ecebc4b311b18d3548c' Gtd='1/1/0001 12:00:00 AM'").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new OrderEntryToken(new Tokenizer().OrderScan("Order='f2bdbc5f1d8d4d459ea1e8b91b72e7ac/Mohan' Name='DTMOJOAE.CL.0.1' State=Filled Instrument='CL 06-16' Action=SellShort Limit price=0 Stop price=0 Quantity=1 Type=Market Tif=Day OverFill=False Oco='' Filled=1 Fill price=45.07 Token='f2bdbc5f1d8d4d459ea1e8b91b72e7ac' Gtd='1/1/0001 12:00:00 AM'").ToList()) };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM Account Name = Mohan").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new AccountToken("Mohan") };
            CompareTokens(expected, tokens);


            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM profit targets = 16,16,16").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new ProfitTargetsToken("16,16,16") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM catastrophic loss = 11").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new CatastrophicLossToken("11") };
            CompareTokens(expected, tokens);


            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM sgTargets = 8,12,16").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new SGTargetsToken("8,12,16") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM move stop to BE at 6").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new BEStopToken("6") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM move stop at = 4,8,12").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new StopsToken("4,8,12") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM ProcessHistorical = True").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new ProcessHistoricalToken("True") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:22 - 4/29/2016 10:01:10 AM US start end time 1= 0, 1500").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new StartEndTimeToken("US start end time 1", "0, 1500") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:25 - 4/29/2016 10:01:25 AM US start end time 2= 0, 0").ToList();
            expected = new List<Token>() { new TradeDateTimeToken(), new StartEndTimeToken("US start end time 2", "0, 0") };
            CompareTokens(expected, tokens);

            tokens = new Tokenizer().Scan("2016_05_02 08:54:25 - 4/29/2016 10:01:25 AM Daily goal/risk = 400/800").ToList();
            expected = new List<Tokens.Token>() { new Tokens.TradeDateTimeToken(), new Tokens.DailyGoalRiskToken("400/800") };
            CompareTokens(expected, tokens);


        }
        private void CompareTokens(List<Tokens.Token> expected, List<Tokens.Token> parsed)
        {
            if (expected.Count != parsed.Count)
                Assert.Fail("expected and parsed token count different");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(parsed[i].GetType() == expected[i].GetType());
                if (expected[i] is Tokens.NumberConstantToken)
                {
                    var ct = (expected[i] as Tokens.NumberConstantToken).Value;
                    var pt = ((Tokens.NumberConstantToken)parsed[i]).Value;

                    Assert.AreEqual(ct, pt);
                }
                if (expected[i] is Tokens.WordToken)
                {
                    var ct = (expected[i] as Tokens.WordToken).Value;
                    var pt = ((WordToken)parsed[i]).Value;

                    Assert.AreEqual(ct, pt);
                    
                }
                if (expected[i] is Tokens.OrderEntryToken)
                {
                    Tokens.OrdersToken parsedT = parsed[i] as Tokens.OrdersToken;
                    Tokens.OrdersToken expectedT = parsed[i] as Tokens.OrdersToken;
                    var ct = expectedT.Name;
                    var pt = parsedT.Name;
                    Assert.AreEqual(ct, pt);
                    pt = parsedT.BaseName;
                    if (parsedT.IsEntryOrder)
                        Assert.AreEqual(ct, pt);
                }
                
            }
            foreach (Tokens.Token t in parsed)
                Console.WriteLine(t.GetType() + " " + t.ToString());
        }
    }
}
