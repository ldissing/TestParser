using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ArithmeticExpressionParser
{
    using Tokens;
    // Letter := [A-Z] | [a-z]
    // Word := [A-Z][a-z]*
                                //2016_04_19 15:38:46
    // CurrentDateTime := Digit(Digit) "_" Digit(Digit) "_" Digit(Digit) "-" Digit(Digit) ":" Digit(Digit) ":" Digit(Digit)
    // TradeDateTime := Digit(Digit) "/" Digit(Digit) "/" Digit(Digit) "-" Digit(Digit) ":" Digit(Digit) ":" Digit(Digit)
    // Version := CurrentDateTime TradeDateTime "Vers." Digit "." Digit [ "." ] [ Letter ] [ Digit ]
    // Expression := [ "-" ] Term { ("+" | "-") Term }
    // Term       := Factor> { ( "*" | "/" ) Factor }
    // Factor     := RealNumber | "(" Expression ")"
    // RealNumber := Digit{Digit} | [Digit] "." {Digit}
    // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

    public class Tokenizer
    {
        private StringReader _reader;

        public IEnumerable<Token> OrderScan(string expression)
        {
            _reader = new StringReader(expression);
            string key = "";
            string value = "";
            bool bFindingKey = true;
            bool bFoundFirstQuote = false;
            var tokens = new List<Token>();
            while (_reader.Peek() != -1)
            {
                var c = (char)_reader.Peek();
                if (Char.IsWhiteSpace(c) || c == '\'')
                {
                    // if a space or a second single quote, value has been found
                    if (c == '\'')
                    {
                        if (bFoundFirstQuote == true)
                        {
                            // value has been found
                            tokens.Add(new KeyValueToken(key, value));
                            key = "";
                            value = "";
                            bFindingKey = true;
                            bFoundFirstQuote = false;
                        }
                        else
                            bFoundFirstQuote = true;
                    }
                    if (bFindingKey == false && c == ' ')
                    {
                        if (bFoundFirstQuote == true)
                            value += System.Convert.ToString(c);
                        else
                        {
                            tokens.Add(new KeyValueToken(key, value));
                            key = "";
                            value = "";
                            bFindingKey = true;
                            bFoundFirstQuote = false;
                        }
                    }

                    _reader.Read();
                    continue;
                }
                if (c == '=')
                {
                    // key has ended, find value
                    bFindingKey = false;
                    _reader.Read();
                    continue;
                }
                //if (Char.IsLetter(c) || Char.IsDigit(c))
                {
                    if (bFindingKey == true)
                        key += System.Convert.ToString(c);
                    else
                        value += System.Convert.ToString(c);
                }
                _reader.Read();
            }
            return tokens;
        }
        public IEnumerable<Token> Scan(string expression)
        {
            _reader = new StringReader(expression);
            
            var tokens = new List<Token>();
            while (_reader.Peek() != -1)
            {
                var c = (char)_reader.Peek();
                if (Char.IsWhiteSpace(c))
                {
                    _reader.Read();
                    continue;
                }
                

                if (Char.IsDigit(c) || c == '.')
                {
                    var nr = ParseNumber();
                    tokens.Add(new NumberConstantToken(nr));
                }
                else if (c == '_')
                {
                    tokens.Add(new UnderlineToken());
                    _reader.Read();
                }
                else if (c == ':')
                {
                    tokens.Add(new ColonToken());
                    _reader.Read();
                }
                else if (c == '-')
                {
                    
                    tokens.Add(new DashToken());
                    _reader.Read();
                }
                else if ((c == 'A' || c == 'P') && tokens.Count >= 2 && tokens[tokens.Count - 1].GetType() == typeof(NumberConstantToken) && tokens[tokens.Count-2].GetType() == typeof(ColonToken))
                {
                    // do we have the stuff for a tradedate
                    string ampm =  ParseAMPM();
                    {
                        if (   tokens[tokens.Count - 1].GetType() == typeof(NumberConstantToken)
                            && tokens[tokens.Count - 2].GetType() == typeof(ColonToken)
                            && tokens[tokens.Count - 3].GetType() == typeof(NumberConstantToken)
                            && tokens[tokens.Count - 4].GetType() == typeof(ColonToken)
                            && tokens[tokens.Count - 5].GetType() == typeof(NumberConstantToken)
                            )
                        {
                            List<Token> list = new List<Token>();
                            for (int i = 10; i >= 1; i--)
                            {
                                list.Add(tokens[tokens.Count - 1]);
                                tokens.RemoveAt(tokens.Count - 1);
                                
                            }
                            tokens.Clear();
                            list.Reverse();
                            if (ampm == "AM")
                                tokens.Add(new TradeDateTimeToken(list, true));
                            else
                                tokens.Add(new TradeDateTimeToken(list, false));
                        }

                        
                    }
                    
                    _reader.Read();
                }
                else if (c == '/')
                {
                    tokens.Add(new ForwardSlashToken());
                    _reader.Read();
                }
                else if (c == '(')
                {
                    tokens.Add(new OpenParenthesisToken());
                    _reader.Read();
                }
                else if (c == ')')
                {
                    tokens.Add(new ClosedParenthesisToken());
                    _reader.Read();
                }
                else if (Char.IsLetter(c))
                {
                    string word = ParseWord();
                    switch(word)
                    {
                        case "PMtf":
                            tokens.Add(new PMTFToken(word));
                            return tokens;
                           
                        case "order":
                            if (((char)_reader.Peek() == ':'))
                                _reader.Read();  // reads the next ':'
                            else
                                break;
                            word = ReadToEnd().Trim();
                            List<Token> kvPairs = OrderScan(word).ToList();
                            RemoveTokens(tokens, typeof(TradeDateTimeToken));
                            tokens.Add(new OrderEntryToken(kvPairs));
                            return tokens;
                            
                        case "Vers":
                            // read the version
                            word += ReadUntilWhiteSpace();
                            RemoveTokens(tokens, typeof(TradeDateTimeToken));
                            tokens.Add(new VersionToken(word));
                            return tokens;
                        case "Account":
                            ReadJustPastEqual();
                            word = ReadUntilWhiteSpace();
                            RemoveTokens(tokens, typeof(TradeDateTimeToken));
                            tokens.Add(new AccountToken(word));
                            return tokens;
                            
                        case "profit":
                            _reader.Read();
                            word = ReadUntilWhiteSpace();
                            if (word == "targets")
                            {
                                ReadJustPastEqual();
                                word = ReadUntilWhiteSpace();
                                RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                tokens.Add(new ProfitTargetsToken(word));
                                return tokens;
                            }
                            else
                                return tokens;
                        case "catastrophic":
                            _reader.Read();
                            word = ReadUntilWhiteSpace();
                            if (word == "loss")
                            {
                                ReadJustPastEqual();
                                word = ReadUntilWhiteSpace();
                                RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                tokens.Add(new CatastrophicLossToken(word));
                                return tokens;
                            }
                            else
                                return tokens;
                        case "sgTargets":
                            {
                                ReadJustPastEqual();
                                word = ReadUntilWhiteSpace();
                                RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                tokens.Add(new SGTargetsToken(word));
                                return tokens;
                            }
                        case "BE":
                                _reader.Read();
                                word = ReadUntilWhiteSpace();
                                _reader.Read();
                                word = ReadUntilWhiteSpace();
                                RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                tokens.Add(new BEStopToken(word));
                                return tokens;
                        case "move":
                            {
                                _reader.Read();
                                word = ReadUntilWhiteSpace();
                                if (word == "stop")
                                {
                                    _reader.Read();
                                    word = ReadUntilWhiteSpace();
                                    if (word == "at")
                                    {
                                        
                                        ReadJustPastEqual();
                                        word = ReadUntilWhiteSpace();
                                        RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                        tokens.Add(new StopsToken(word));
                                        return tokens;
                                    }
                                }
                                break;
                            }
                        case "ProcessHistorical":
                            ReadJustPastEqual();
                            word = ReadUntilWhiteSpace();
                            RemoveTokens(tokens, typeof(TradeDateTimeToken));
                            tokens.Add(new ProcessHistoricalToken(word));
                            return tokens;
                           
                        case "US":
                            word += ReadAllButEqual();
                            string name = word;
                            _reader.Read();
                            word = ReadToEnd().Trim();
                            RemoveTokens(tokens, typeof(TradeDateTimeToken));
                            tokens.Add(new StartEndTimeToken(name, word));
                            return tokens;
                            
                        case "Daily":
                            _reader.Read();
                            string newword = ParseWord();
                            if (newword == "goal")
                            {
                                c = (char)_reader.Peek();
                                if (c == '/')
                                {
                                    word += ReadAllButEqual();
                                    ReadJustPastEqual();
                                    word = ReadUntilWhiteSpace();
                                    RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                    tokens.Add(new DailyGoalRiskToken(word));
                                    return tokens;
                                }
                                else
                                {
                                    newword += ReadAllButEqual();
                                    ReadJustPastEqual();
                                    word = ReadUntilWhiteSpace();
                                    RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                    tokens.Add(new DailyGoalExceededToken(word));
                                    return tokens;
                                }
                            }
                            if (newword == "risk")
                            {
                                word += ReadAllButEqual();
                                ReadJustPastEqual();
                                word = ReadUntilWhiteSpace();
                                RemoveTokens(tokens, typeof(TradeDateTimeToken));
                                tokens.Add(new DailyRiskExceededToken(word));
                                return tokens;

                            }
                            break;
                        default:
                            //Console.WriteLine(word);
                            break;
                    }
                    _reader.Read();
                }
                else
                {
                    _reader.Read();
                    // throw it away
                    //throw new Exception("Unknown character in expression: " + c);
                }
            }

            return tokens;
        }
        private void RemoveTokens(List<Token> tokens, Type t)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i].GetType() != t)
                    tokens.RemoveAt(i);
            }
        }
        private string ReadToEnd()
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1)
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ReadAllButEqual()
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '='))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        
        private string ReadAllButSingleQuote()
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '\''))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private void ReadJustPastSingleQuote()
        {

            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '\''))
                _reader.Read();
            _reader.Read();
            while (_reader.Peek() != -1 && Char.IsWhiteSpace((char)_reader.Peek()))
                _reader.Read();
        }
        private void ReadJustPastEqual()
        {
          
            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '='))
                _reader.Read();
            _reader.Read();
            while (_reader.Peek() != -1 && Char.IsWhiteSpace((char)_reader.Peek()))
                _reader.Read();
        }
        private string ReadUntilWhiteSpace()
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && !Char.IsWhiteSpace((char)_reader.Peek()))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ParseWord()
        {
            var sb = new StringBuilder();
            while ((Char.IsLetter((char)_reader.Peek())))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ParseAMPM()
        {
            var sb = new StringBuilder();
            while (((char)_reader.Peek() == 'A' || (char)_reader.Peek() == 'P'))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            char c = (char)_reader.Peek();
            sb.AppendFormat("{0}", c.ToString());
            return sb.ToString();
        }
        private double ParseNumber()
        {
            var sb = new StringBuilder();
            var decimalExists = false;
            while (Char.IsDigit((char)_reader.Peek()) || ((char) _reader.Peek() == '.'))
            {
                var digit = (char)_reader.Read();
                if (digit == '.')
                {
                    if (decimalExists) throw new Exception("Multiple dots in decimal number");
                    decimalExists = true;
                }
                sb.Append(digit);
            }

            double res;
            if (!double.TryParse(sb.ToString(), out res))
                throw new Exception("Could not parse number: " + sb);

           return res;
        }
    }
}
