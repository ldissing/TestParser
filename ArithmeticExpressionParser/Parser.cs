using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orders
{
    using Tokens;
    public class CompletedOrders
    {
        public bool IsLong = false;
        public double entry = 0;
        public int quantity = 0;
        public double exit = 0;
        public string orderName = "";
        public double profit = 0;
        public DateTime dateTime = DateTime.MinValue;
        public DateTime exitTime = DateTime.MinValue;
        public string Instrument = String.Empty;

        public override string ToString()
        {
            return string.Format("{5}  long trade = {6}  name = {4}, entry = {0}, entryTime = {7}, quantity = {1}, exit = {2}, exitTime={8}, profit = {3}", entry, quantity, exit, profit, orderName, dateTime, IsLong, dateTime, exitTime);
        }
        /// <summary>
        /// must use ProcessOrder to fill in fields
        /// </summary>
        public CompletedOrders()
        {
        }
        public CompletedOrders(string[] headerSplit, string[] valuesSplit)
        {
            for(int i = 0; i < headerSplit.Length; i++)
            {
                switch(headerSplit[i])
                {
                    case "Market pos.":
                        IsLong = valuesSplit[i].Contains("Long");
                        break;
                    case "Quantity":
                        quantity = int.Parse(valuesSplit[i]);
                        break;
                    case "Entry price":
                        entry = double.Parse(valuesSplit[i]);
                        break;
                    case "Exit price":
                        exit = double.Parse(valuesSplit[i]);
                        break;
                    case "Entry time":
                        dateTime = DateTime.Parse(valuesSplit[i]);
                        break;
                    case "Exit time":
                        exitTime = DateTime.Parse(valuesSplit[i]);
                        break;
                    case "Entry name":
                        orderName = (valuesSplit[i]);
                        break;
                    case "Instrument":
                        Instrument = (valuesSplit[i]);
                        break;
                }
                
            }
            if (IsLong)
            {
                profit = (exit - entry) / .25;
            }
            else
            {
                profit = (entry - exit) / .25;
            }
        }
        public CompletedOrders ProcessOrder(Token time, OrdersToken ot, Dictionary<string, CompletedOrders> orderDict)
        {
            if (ot.Filled > 0)
            {
                if (ot.IsEntryOrder)
                {
                    if (ot.Action != "SellShort")
                    {
                        IsLong = true;
                    }
                    entry = ot.Price;
                    quantity = ot.Quantity;
                    orderName = ot.Name;
                    dateTime = DateTime.Parse(time.ToString());
                }
                else if (!ot.IsEntryOrder)
                {
                    if (ot.Name.Contains(orderName))
                    {
                        exit = ot.Price;
                        if (ot.Action == "BuyToCover")
                        {
                            profit = entry - exit;
                        }
                        else
                            profit = exit - entry;

                        if (ot.Quantity == quantity)
                        {
                            if (orderDict.ContainsKey(dateTime.ToString()))
                            {
                                orderDict[dateTime.ToString()] = this;
                            }
                            else
                                orderDict.Add(dateTime.ToString(), this);
                            return new CompletedOrders();
                        }
                    }
                }
            }
            return this;
        }
    }

}
namespace ArithmeticExpressionParser
{
    using Tokens;
    using Orders;
    public class Parser
    {
        private string _expression;
        private TokensWalker _walker;

        Dictionary<string, CompletedOrders> orderDict = new Dictionary<string, CompletedOrders>();

        public Dictionary<string, CompletedOrders> OrderDict
        {
            get { return orderDict; }
            set { orderDict = value; }
        }
        //public Parser(StreamReader file)
        //{
        //    string line="";
        //    while (file.EndOfStream == false)
        //    {
        //        line = file.ReadLine();

        //        _walker = Parse(line);
        //    }
        //}
        public Parser(string file)
        {
            ParseFile(file);
        }
        protected void ParseFile(string file)
        {
            CompletedOrders co = new CompletedOrders();
            using (StreamReader sr = new StreamReader(file))
            {
                while (sr.EndOfStream == false)
                {
                    string expression = sr.ReadLine();
                    _expression = expression;
                    var tokens = new Tokenizer().Scan(_expression);
                    var list = tokens.ToList();
                    if (list.Count >= 2)
                    {
                        TokensWalker walker = new TokensWalker(tokens);
                        bool bPrinted = false;
                        
                        while (walker.ThereAreMoreTokens)
                        {
                            Token t = walker.GetNext();
                            if (walker.ThereAreMoreTokens)
                            {
                                Token nextT = walker.PeekNext();
                                //if ((nextT as WordToken) != null || (nextT as OrdersToken) != null)
                                {
                                    OrdersToken ot = nextT as OrdersToken;
                                    string t1 = nextT.GetType().ToString();
                                    switch(t1)
                                    {
                                        case "Tokens.OrderEntryToken":
                                            co = co.ProcessOrder(t, ot, orderDict);
                                            break;
                                        case "Tokens.NumberConstantToken":
                                            break;
                                        case "Tokens.DashToken":
                                            break;
                                        case "Tokens.UnderlineToken":
                                            break;
                                        case "Tokens.ColonToken":
                                            break;
                                        case "Tokens.VersionToken":
                                            //OutputDailyProfit();
                                            orderDict.Clear();
                                            Console.WriteLine(string.Format("Version {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.PMTFToken":
                                            Console.WriteLine(string.Format("PMTF {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.AccountToken":
                                            Console.WriteLine(string.Format("Account {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.ProfitTargetsToken":
                                            Console.WriteLine(string.Format("Targets {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.CatastrophicLossToken":
                                            Console.WriteLine(string.Format("Catestrophic Loss = {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.BEStopToken":
                                            Console.WriteLine(string.Format("Move to Break Even at {0} ticks profit", nextT.ToString()));
                                            break;
                                        case "Tokens.SGTargetsToken":
                                            Console.WriteLine(string.Format("Move stop at these profit targets {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.StopsToken":
                                            Console.WriteLine(string.Format("Move stop to here {0}", nextT.ToString()));
                                            break;
                                        case "Tokens.ProcessHistoricalToken":
                                        case "Tokens.StartEndTimeToken":
                                        case "Tokens.DailyRiskExceededToken":
                                        case "Tokens.DailyGoalExceededToken":
                                            Console.WriteLine(string.Format("{0}", nextT.ToString()));
                                            break;
                                        case "Tokens.DailyGoalRiskToken":
                                            Console.WriteLine(string.Format("{0}", nextT.ToString()));
                                            break;
                                        default:
                                            break;
                                    }
                                    
                                }
                                //else
                                //    break;
                            }
                            bPrinted = true;
                            //Console.Write(t.ToString());
                            //Console.Write(" ");
                        }
                        //if (bPrinted)
                        //    Console.Write("\n");
                    }
                }
            }

        }
        public Parser(string file, Dictionary<string, CompletedOrders> orderDictIn)
        {
            if (orderDictIn != null)
                orderDict = orderDictIn;
            ParseFile(file);
        }
        public void OutputDailyProfit()
        {
            CompletedOrders co = new CompletedOrders();
            double totalProfitinPoints = 0;
            DateTime begin = DateTime.MinValue;
            int count = 0;
            foreach (string k in OrderDict.Keys)
            {
                co = OrderDict[k];
                if (begin == DateTime.MinValue || begin.Date == co.dateTime.Date)
                {
                    begin = co.dateTime;
                    totalProfitinPoints += co.profit;
                    count += co.quantity;
                    Console.WriteLine(string.Format("{5}  long trade = {6}  name = {4}, entry = {0}, quantity = {1}, exit = {2}, profit = {3} trade count = {7}", co.entry, co.quantity, co.exit, co.profit, co.orderName, co.dateTime, co.IsLong, count));
                    
                }
                else
                {
                    Console.WriteLine(string.Format("TOTAL PROFIT in points  = {0}, with commissions {1:C}", totalProfitinPoints, totalProfitinPoints * 1000 - count * 5));
                    count = co.quantity;
                    begin = co.dateTime;
                    totalProfitinPoints = co.profit;
                    Console.WriteLine(string.Format("{5}  long trade = {6}  name = {4}, entry = {0}, quantity = {1}, exit = {2}, profit = {3} trade count = {7}", co.entry, co.quantity, co.exit, co.profit, co.orderName, co.dateTime, co.IsLong, count));
                }

            }
            Console.WriteLine(string.Format("TOTAL PROFIT in points  = {0}, with commissions {1:C}", totalProfitinPoints, totalProfitinPoints * 1000 - count * 5));

        }
        // EBNF Grammar:
        // Expression := [ "-" ] Term { ("+" | "-") Term }
        // Term       := Factor { ( "*" | "/" ) Factor }
        // Factor     := RealNumber | "(" Expression ")"
        // RealNumber := Digit{Digit} | [Digit] "." {Digit}
        // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

        // Expression := [ "-" ] Term { ("+" | "-") Term }
        public string Parse()
        {
            //var isNegative = NextIsMinus();
            //if (isNegative)
            //    GetNext();
            //var valueOfExpression = TermValue();
            //if (isNegative)
            //    valueOfExpression = -valueOfExpression;
            //while (NextIsMinusOrPlus())
            //{
            //    var op = GetTermOperand();
            //    var nextTermValue = TermValue();
            //    if (op is PlusToken)
            //        valueOfExpression += nextTermValue;
            //    else
            //        valueOfExpression -= nextTermValue;
            //}
            return "";
        }

        // Term       := Factor { ( "*" | "/" ) Factor }
        private string TermValue()
        {
            //var totalVal = FactorValue();
            //while (NextIsMultiplicationOrDivision())
            //{
            //    var op = GetFactorOperand();
            //    var nextFactor = FactorValue();

            //    if (op is DivideToken)
            //        totalVal /= nextFactor;
            //    else
            //        totalVal *= nextFactor;
            //}

            return "";// totalVal;
        }

        // Factor     := RealNumber | "(" Expression ")"
        private string FactorValue()
        {
            //if (NextIsDigit())
            //{
            //    var nr = GetNumber();
            //    return nr;
            //}
            //if (!NextIsOpeningBracket())
            //    throw new Exception("Expecting Real number or '(' in expression, instead got : " + (PeekNext() != null ? PeekNext().ToString()  : "End of expression"));          
            //GetNext();

            //var val = Parse();
            
            //if (!(NextIs(typeof(ClosedParenthesisToken))))
            //    throw new Exception("Expecting ')' in expression, instead got: " + (PeekNext() != null ? PeekNext().ToString() : "End of expression"));           
            //GetNext();
            return ""; //val;
        }

        private bool NextIsMinus()
        {
            return _walker.ThereAreMoreTokens && _walker.IsNextOfType(typeof(DashToken));
        }

        private bool NextIsOpeningBracket()
        {
            return NextIs(typeof(OpenParenthesisToken));
        }

        private Token GetTermOperand()
        {
            var c = GetNext();
            
            if (c is DashToken)
                return c;

            throw new Exception("Expected term operand '+' or '-' but found" + c);
        }

        private Token GetFactorOperand()
        {
            var c = GetNext();
            if (c is ForwardSlashToken)
                return c;
            

            throw new Exception("Expected factor operand '/' or '*' but found" + c);
        }

        private Token GetNext()
        {
            return _walker.GetNext();
        }

        private Token PeekNext()
        {
            return _walker.ThereAreMoreTokens ? _walker.PeekNext() : null;
        }

        private double GetNumber()
        {
            var next = _walker.GetNext();

            var nr = next as NumberConstantToken;
            if (nr == null)
                throw new Exception("Expecting Real number but got " + next);

            return nr.Value;
        }

        private bool NextIsDigit()
        {
            if (!_walker.ThereAreMoreTokens)
                return false;
            return _walker.PeekNext() is NumberConstantToken;
        }

        private bool NextIs(Type type)
        {
            return _walker.ThereAreMoreTokens && _walker.IsNextOfType(type);
        }

        private bool NextIsDash()
        {
            return _walker.ThereAreMoreTokens && (NextIs(typeof(DashToken)));
        }

    }
}