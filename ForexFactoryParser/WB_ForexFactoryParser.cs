using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ForexFactoryParser
{
    public class NewsItem
    {
        public string tr;
        public DateTime time = DateTime.MinValue;
        public List<string> td = new List<string>();
        public Dictionary<string, string> data = new Dictionary<string, string>();
        public string title;
        public override string ToString()
        {
            string ret = "";
            string value = string.Format("{0:hh:mm tt}", time);
            //if (data.ContainsKey("Time"))
                ret += value + "  ";
            if (data.ContainsKey("Impact"))
                ret += data["Impact"][0] + "\t";
            if (data.ContainsKey("Currency"))
                ret += data["Currency"] + "\t";
            if (data.ContainsKey("Event"))
                ret += data["Event"] + " ";
            if (data.ContainsKey("Previous"))
            {
                ret += "(" + data["Previous"];
                if (data.ContainsKey("Forecast"))
                {
                    ret += " / " + data["Forecast"];
                }
                ret += ")";
            }

            return ret;
        }
    }
    /// <summary>
    /// This parser sucks.  Could be done much better, but I'm not getting paid for it.
    /// </summary>
    public class ForexFactoryParser
    {
        private string _expression;
        private TokensWalker _walker;
        private int _timeOffsetFromEastern = 0;
        Dictionary<string, NewsItem> _newsDict = new Dictionary<string, NewsItem>();

        public Dictionary<string, NewsItem> NewsDict
        {
            get { return _newsDict; }
            set { _newsDict = value; }
        }
        public ForexFactoryParser(string expression, bool isData)
        {
            ParseFile(expression);
        }
        /// <summary>
        /// pass in a file name
        /// </summary>
        /// <param name="file"></param>
        public ForexFactoryParser(string file)
        {
            string expression = "";
            using (StreamReader sr = new StreamReader(file))
            {
                expression = sr.ReadToEnd();
                
            }
            ParseFile(expression);
        }
        /// <summary>
        /// pass in the data (string) to parse
        /// </summary>
        /// <param name="expression"></param>
        protected void ParseFile(string expression)
        {
            _timeOffsetFromEastern = TimeZoneDiff();
            //using (StreamReader sr = new StreamReader(file))
            {
                //while (sr.EndOfStream == false)
                {
                    //string expression = sr.ReadToEnd();
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
                                    NewsItemToken ot = nextT as NewsItemToken;
                                    string t1 = nextT.Name;
                                    switch(t1)
                                    {
                                        case "ForexFactoryParser.NewsItemToken":
                                            if (!NewsDict.ContainsKey(ot.ToString()))
                                                NewsDict.Add(ot.ToString(), ot.Value);
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
        public ForexFactoryParser(string file, Dictionary<string, NewsItem> newsDictIn)
        {
            if (newsDictIn != null)
                _newsDict = newsDictIn;
            ParseFile(file);
        }
        
        // EBNF Grammar:  NOT THE RIGHT GRAMMER
        // Expression := [ "<tr" ] Term { ("+" | "-") Term }
        // Term       := Factor { ( "*" | "/" ) Factor }
        // Factor     := RealNumber | "(" Expression ")"
        // RealNumber := Digit{Digit} | [Digit] "." {Digit}
        // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

        // Expression := [ "-" ] Term { ("+" | "-") Term }
        public string Parse()
        {
            
            return "";
        }
        /// <summary>
        /// Get the difference in hours between EST and current time zone
        /// </summary>
        /// <returns></returns>
        int TimeZoneDiff()
        {
            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTime convertedTime = DateTime.Now;
            TimeSpan offset;
            TimeSpan offset1;
            convertedTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, est);
            offset = est.GetUtcOffset(DateTime.Now);
            offset1 = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            return offset1.Hours - offset.Hours;
        }
        
        /// <summary>
        /// Get the next token
        /// </summary>
        /// <returns></returns>
        private Token GetNext()
        {
            return _walker.GetNext();
        }

        /// <summary>
        /// See if there are more tokens and if so, return it
        /// </summary>
        /// <returns></returns>
        private Token PeekNext()
        {
            return _walker.ThereAreMoreTokens ? _walker.PeekNext() : null;
        }
        /// <summary>
        /// Get the next token type if available
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool NextIs(Type type)
        {
            return _walker.ThereAreMoreTokens && _walker.IsNextOfType(type);
        }


    }

    public abstract class Token
    {
        public virtual string Name
        {
            get { return this.GetType().ToString(); }
        }
        public override string ToString()
        {
            return typeof(Token).ToString();
        }
        public virtual string Value
        {
            get
            {
                return "";
            }
        }
    }
    public class TypeToken : DataToken
    {
        public TypeToken(string type) : base(type)
        {
        }
    }
   
    public class TRToken : Token
    {
        public override string Value
        {
            get { return "tr"; }
        }
    }
    public class LessThanToken : Token
    {
        public override string Value
        {
            get { return "<"; }
        }
    }
    public class ForwardSlashToken : Token
    {
        public override string Value
        {
            get { return "/"; }
        }
    }
    public class GreaterThanToken : Token
    {
        public override string Value
        {
            get { return ">"; }
        }
    }
    public class KeyToken : Token
    {
        public KeyToken(string value)
        {
            _value = value;
        }
        string _value;
        public override string Value
        {
            get { return _value; }
        }
    }
    public class DataToken : Token
    {
        public DataToken(string value)
        {
            _value = value;
        }
        string _value;
        public override string Value
        {
            get { return _value; }
        }
    }
    public class NewsItemToken : Token
    {
        private NewsItem _value;

        public NewsItemToken(NewsItem value)
        {
            _value = value;
        }

        public NewsItem Value
        {
            get { return _value; }
        }
        public override string ToString()
        {
            string ret = "";
            if (_value.data.ContainsKey("Time"))
                ret += _value.data["Time"];
            if (_value.data.ContainsKey("Event"))
                ret += _value.data["Event"];

            return ret;
        }

    }
    
    public class NumberConstantToken : Token
    {
        private readonly double _value;

        public NumberConstantToken(double value)
        {
            _value = value;
        }

        public double Value
        {
            get { return _value; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

public class TokensWalker
    {
        private readonly List<Token> _tokens = new List<Token>();
        private int _currentIndex = -1;

        public bool ThereAreMoreTokens
        {
            get { return _currentIndex < _tokens.Count - 1; }
        }

        public TokensWalker(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();
        }

        public Token GetNext()
        {
            MakeSureWeDontGoPastTheEnd();
            return _tokens[++_currentIndex];
        }

        private void MakeSureWeDontGoPastTheEnd()
        {
            if (!ThereAreMoreTokens)
                throw new Exception("Cannot read pass the end of tokens list");
        }

        public Token PeekNext()
        {
            MakeSureWeDontPeekPastTheEnd();
            return _tokens[_currentIndex + 1];
        }

        private void MakeSureWeDontPeekPastTheEnd()
        {
            var weCanPeek = (_currentIndex + 1 < _tokens.Count);
            if (!weCanPeek)
                throw new Exception("Cannot peek pass the end of tokens list");
        }

        public bool IsNextOfType(Type type)
        {
            return PeekNext().GetType() == type;
        }
    }
    public class Tokenizer
    {
        private List<Token> _newstokens = new List<Token>();
        private List<Token> tokens = new List<Token>();
        private DateTime prevTime = DateTime.MinValue;
        private StringReader _reader;
        private int _offsetToEst = 0;
        private string _expression;
        
        string ReadUpToText(StringReader _reader, string textToFind)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < textToFind.Length; i++)
            {
                if (_reader.Peek() == -1)
                    break;

                if ((char)_reader.Peek() == textToFind[i])
                {
                    //if (textToFind.Length != 1)
                    sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
                    if (i == textToFind.Length - 1)
                        break;
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
                    i = -1;
                }
            }
            return sb.ToString();
        }
        int TimeZoneDiff()
        {
            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTime convertedTime = DateTime.Now;
            TimeSpan offset;
            TimeSpan offset1;
            convertedTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, est);
            offset = est.GetUtcOffset(DateTime.Now);
            offset1 = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            return offset1.Hours - offset.Hours;
        }

        NewsItem ParseCalendarRow(StringReader MyReader)
        {
            
            for (int i = 0; i < tokens.Count; i++ )
            {
                if (tokens[i].Name.Contains("DataToken") || tokens[i].Name.Contains("KeyToken"))
                {

                }
                else
                {
                    tokens.RemoveAt(i--);
                }
            }
            string key="";
            string value = "";
            NewsItem item1 = new NewsItem();
            item1.time = prevTime;
            value = string.Format("{0:hh:mm tt}", prevTime);
                while (tokens.Count > 0)
                {
                    if (tokens[0].Name.Contains("KeyToken"))
                    {
                        if (tokens[0].Value.Contains("calendar"))
                        key = tokens[0].Value;
                    }
                        
                    if (key.Contains("calendar__event-title"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            item1.data.Add("Event", value);
                        }
                    }
                    if (key.Contains("calendar__actual"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            item1.data.Add("Actual", value);
                        }
                    }
                    if (key.Contains("calendar__forecast"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            item1.data.Add("Forecast", value);
                        }
                    }
                    if (key.Contains("calendar__impact"))
                    {
                        if (item1.data.ContainsKey("Impact") == false)
                        {
                            if (key.Contains("medium"))
                                item1.data.Add("Impact", "Medium");
                            else if (key.Contains("high"))
                                item1.data.Add("Impact", "High");
                            else if (key.Contains("low"))
                                item1.data.Add("Impact", "Low");
                        }
                        key = "";
                    }
                    if (key.Contains("calendar__previous"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            item1.data.Add("Previous", value);
                        }
                    }
                    if (key.Contains("calendar__currency"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            item1.data.Add("Currency", value);
                        }
                    }
                    if (key.Contains("calendar__time"))
                    {
                        if (tokens[0].Name.Contains("DataToken"))
                        {
                            value = tokens[0].Value;
                            value = value.ToLower();
                            if (value.Contains("am") || value.Contains("pm"))
                            {
                                // figure out where we should be timewise for our timezone
                                string temp = "";
                                for (int i = 0; i < value.Length; i++)
                                    if (Char.IsDigit(value[i]))
                                        temp += value[i];

                                string leftOver = value.Substring(temp.Length + 1);
                                Regex regexTime = new Regex("[^0-9]+");
                                int time = -1;
                                try
                                {
                                    time = Convert.ToInt16(regexTime.Replace(value, ""));
                                }
                                catch (Exception)
                                {
                                }
                                if (time != -1)
                                {
                                    if (value.IndexOf("pm") > 0)
                                    {
                                        if (time < 1200)
                                        {
                                            time += 1200;
                                        }
                                    }
                                    else if (time > 1200)
                                    {
                                        time -= 1200;
                                    }
                                    DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, time / 100, time % 100, 0);
                                    dt = dt.AddHours(_offsetToEst);
                                    value = string.Format("{0:hh:mm tt}", dt);
                                    prevTime = dt;
                                    item1.time = dt;
                                }
                                item1.data.Add("Time", value);
                            }
                            else
                            {
                                if (prevTime != DateTime.MinValue && value == "")
                                {
                                    value = string.Format("{0:hh:mm tt}", prevTime);
                                    item1.data.Add("Time", value);
                                    item1.time = prevTime;
                                }
                                else
                                {
                                    DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                    value = string.Format("{0:hh:mm tt}", dt);
                                    item1.data.Add("Time", value);
                                }
                            }
                        }
                    }
                    tokens.RemoveAt(0);
                }
                return item1;
                NewsItem item = new NewsItem();
            //<tr class=\"calendar__row
            //</tr>
            var sb = new StringBuilder();

            while (((char)MyReader.Peek() == '<' || (char)MyReader.Peek() == 't') || (char)MyReader.Peek() == 'r')
            {
                sb.AppendFormat("{0}", ((char)MyReader.Read()).ToString());
                char next = (char)MyReader.Peek();
            }
            char c = (char)MyReader.Peek();
            string tr = string.Empty;
            if (sb.ToString().Contains("<tr"))
            {
                string trType = ReadUntilWhiteSpace(MyReader);
                if (trType == "")
                {
                    char next = (char)MyReader.Read();
                    string classType = ReadAllButEqual(MyReader);
                    if (classType.ToLower().Contains("class"))
                    {
                        MyReader.Read(); // read =
                        string type = ReadUntilWhiteSpace(MyReader);
                        if (type.ToLower().Contains("calendar__row"))
                        {
                            string name = ReadUpToText(MyReader, ">"); // read until >,  pull off the crap we don't need
                            ReadWhiteSpace(MyReader);
                            tr = ReadToEnd(MyReader);
                            if (tr.Contains("<tr"))
                            {
                                tr += ReadUpToText(MyReader, "</tr>");
                            }
                            item.tr = tr;
                            StringReader trreader = new StringReader(tr);
                            // parse the tr
                            bool bContinue = true;
                            while(bContinue)
                            {
                                string td = ReadUpToText(trreader, "<td");
                                td += ReadUpToText(trreader, "</td>");
                                
                                if (td.Contains("</tr>") || td == "")
                                    break;
                                else
                                {
                                    item.td.Add(td);
                                    // process the td
                                    if (td.Contains("class=") || td.Contains("<span") || td.Contains("<a"))
                                    using(StringReader tdreader = new StringReader(td))
                                    {
                                        while (tdreader.Peek() != -1)
                                        {
                                            string tdClass = ReadUpToText(tdreader, "class=\"calendar__cell ");
                                            tdClass = ReadUntilWhiteSpace(tdreader);
                                            value = "";
                                            string span = "";
                                            if (tdreader.Peek() == -1 || tdClass == "")
                                                continue;
                                            switch(tdClass)
                                            {
                                                case "calendar__previous":
                                                    if (td.Contains("span"))
                                                    {
                                                        span = ReadUpToText(tdreader, "<span ");
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    else
                                                    {
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    if (value != "")
                                                    item.data.Add("Previous", value);
                                                    break;
                                                case "calendar__forecast":
                                                    if (td.Contains("span"))
                                                    {
                                                        span = ReadUpToText(tdreader, "<span ");
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    else
                                                    {
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    if (value != "")
                                                        item.data.Add("Forecast", value);
                                                    break;
                                                case "calendar__actual":
                                                    if (td.Contains("span"))
                                                    {
                                                        span = ReadUpToText(tdreader, "<span ");
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    else
                                                    {
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    if (value != "")
                                                        item.data.Add("Actual", value);
                                                    break;
                                                //case "calendar__date":
                                                //    span = ReadUpToText(tdreader, "<span>");
                                                //    value = ReadUpToText(tdreader, ">");
                                                //    item.data.Add("Date", value);
                                                //    break;
                                                case "calendar__event":
                                                    span = ReadUpToText(tdreader, "<span ");
                                                    value = ReadUpToText(tdreader, ">");
                                                    value = ReadUntil(tdreader, '<');
                                                    item.data.Add("Event", value);
                                                    break;
                                                case "calendar__time":
                                                    if (td.Contains("span"))
                                                    {
                                                        span = ReadUpToText(tdreader, "<span ");
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    else
                                                    {
                                                        value = ReadUpToText(tdreader, ">");
                                                        value = ReadUntil(tdreader, '<');
                                                    }
                                                    value = value.ToLower();
                                                    if (value.Contains("am") || value.Contains("pm"))
                                                    {
                                                        // figure out where we should be timewise for our timezone
                                                        string temp = "";
                                                        for(int i =0; i < value.Length; i++)
                                                            if (Char.IsDigit(value[i]))
                                                                temp += value[i];

                                                        string leftOver = value.Substring(temp.Length+1);
                                                        Regex regexTime = new Regex("[^0-9]+");	
                                                        int time = -1;
                                                        try {
											                time = Convert.ToInt16(regexTime.Replace(value, ""));
										                }
										                catch (Exception ) {
										                }
										                if (time != -1) {
											                if (value.IndexOf("pm") > 0) {
												                if (time < 1200) {
													                time += 1200;
												                }
											                }
											                else if (time > 1200) {
												                time -= 1200;
											                }
											                DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, time / 100, time % 100, 0);
											                dt = dt.AddHours(_offsetToEst);
                                                            value = string.Format("{0:hh:mm tt}", dt);
                                                            prevTime = dt;
                                                            item.time = dt;
										                }
                                                        item.data.Add("Time", value);
                                                    }
                                                    else
                                                    {
                                                        if (prevTime != DateTime.MinValue && value == "")
                                                        {
                                                            value = string.Format("{0:hh:mm tt}", prevTime);
                                                            item.data.Add("Time", value);
                                                            item.time = prevTime;
                                                        }
                                                        else
                                                        {
                                                            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                                            value = string.Format("{0:hh:mm tt}", dt);
                                                            item.data.Add("Time", value);
                                                        }
                                                    }
                                                    
                                                    break;
                                                case "calendar__currency":
                                                    value = ReadUpToText(tdreader, ">");
                                                    value = ReadUntil(tdreader, '<');
                                                    item.data.Add("Currency", value);
                                                    break;
                                                case "calendar__impact":
                                                    value = ReadUpToText(tdreader, "<");
                                                    if (value.Contains("medium"))
                                                        item.data.Add("Impact", "Medium");
                                                    else if (value.Contains("high"))
                                                        item.data.Add("Impact", "High");
                                                    else if (value.Contains("low"))
                                                        item.data.Add("Impact", "Low");
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            
                            
                        }
                    }

                }
                sb.AppendFormat("{0}", trType);
            }
            sb.AppendFormat("{0}", c.ToString());
            return item;
        }
        public IEnumerable<Token> Scan(string expression)
        {
            _offsetToEst = TimeZoneDiff();
            _expression = expression;
            _reader = new StringReader(expression);
            string word = "";
            
            string data = "";
            int trCount = 0;
            while (_reader.Peek() != -1)
            {

                var c = (char)_reader.Peek();
                if (c == '<')
                {
                    _reader.Read();
                    if (data.Length > 1 && data[0] != '&')
                    {
                        tokens.Add(new DataToken(data));
                    }
                    data = "";

                    char ch = (char)_reader.Peek();
                    if (ch == '/')
                    {
                        tokens.Add(new LessThanToken());
                        continue;
                    }
                    word = ParseWord(_reader);
                    if (word != "")
                    {
                        tokens.Add(new LessThanToken());
                        switch(word.ToLower())
                        {
                            case "tr":
                                trCount++;
                                tokens.Add(new TRToken());
                                //NewsItem newitem = ParseCalendarRow(_reader);
                                //if (newitem.td.Count > 0)
                                //{
                                ////    //foreach(string s in newitem.data.Keys)
                                ////    //{
                                ////    //    Console.WriteLine(s + ", " + newitem.data[s]);

                                ////    //}

                                ////    //foreach(string s in newitem.td)
                                ////    //{
                                ////    //    Console.WriteLine(s);
                                ////    //}
                                ////    //Console.WriteLine();
                                //    tokens.Add(new NewsItemToken(newitem));
                                //}
                                break;
                            case "td":
                                tokens.Add(new TypeToken(word));
                                break;
                            case "span":
                                tokens.Add(new TypeToken(word));
                                break;
                            case "a":
                                tokens.Add(new TypeToken(word));
                                break;
                            case "div":
                                tokens.Add(new TypeToken(word));
                                break;
                            default:
                                tokens.Add(new TypeToken(word));
                                break;
                        }
                        data = "";
                        
                    }
                }
                else if (c == '>')
                {
                    if (data.Length > 1 && data[0] != '&')
                    {
                        if (tokens.Count > 0)
                        {
                            if (tokens[tokens.Count - 1].Name.Contains("TRToken") || tokens[tokens.Count - 1].Name.Contains("TypeToken"))
                                tokens.Add(new KeyToken(data));
                            else if (tokens[tokens.Count - 1].Name.Contains("ForwardSlash"))
                                tokens.Add(new TypeToken(data));
                            else
                                tokens.Add(new DataToken(data));
                        }
                        else if (data[0] != '&')
                            tokens.Add(new DataToken(data));
                    }
                    data = "";
                    tokens.Add(new GreaterThanToken());
                    if (tokens.Count > 0)
                    {
                        int i = 0;
                        
                        if (tokens.Count >= 4)
                        {
                            if (tokens[tokens.Count-3] as ForwardSlashToken != null)
                            {
                                // we have ended this one
                                DataToken myData = tokens[tokens.Count - 2] as DataToken;
                                if (myData != null)
                                {
                                    if (myData.Value.ToLower() == "tr" && trCount > 0)
                                        trCount--;
                                    if (myData.Value.ToLower() == "tr" && trCount == 0)
                                    // process data
                                    for(i = 0; i < tokens.Count; i++)
                                    {
                                        if (tokens[i].Name.Contains("TRToken"))
                                        {
                                            // do something
                                            string temp = "<";
                                            for (; i < tokens.Count; i++)
                                            {
                                                temp += tokens[i].Value;
                                                DataToken dt = tokens[i] as DataToken;
                                                //if (dt != null)
                                                //    Console.WriteLine(dt.Value);

                                            }
                                            using (StringReader newReader = new StringReader(temp))
                                            {
                                                NewsItem newitem = ParseCalendarRow(newReader);
                                                if (newitem.ToString() != "")
                                                {
                                                    _newstokens.Add(new NewsItemToken(newitem));
                                                }
                                            }
                                            tokens.Clear();
                                            i = 0;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (tokens.Count > 0 && (!tokens[tokens.Count - 1].Name.Contains("LessThanToken")))
                            {
                                
                                tokens.RemoveAt(tokens.Count - 1);
                            }
                            if (tokens.Count > 0)
                                tokens.RemoveAt(tokens.Count - 1);
                        }

                    }
                    _reader.Read();
                }
                else if (c == '/')
                {
                    if (tokens.Count > 0 && tokens[tokens.Count - 1].Name.Contains("LessThanToken"))
                    {
                        if (data.Length > 1 && data[0] != '&')
                            tokens.Add(new DataToken(data));
                        data = "";
                        tokens.Add(new ForwardSlashToken());
                    }
                    else
                        data += c;

                    _reader.Read();
                   
                }
                else
                {
                    data += (char)_reader.Read();
                    // throw it away
                    //throw new Exception("Unknown character in expression: " + c);
                }
            }
            tokens.Clear();
            foreach (Token t in _newstokens)
                tokens.Add(t);
            return tokens;
        }
        
        private bool NextAvailable(StringReader _reader, out char c)
        {
            int i = _reader.Peek();
            if (i != -1)
            {
                c = (char)_reader.Peek();
                return true;
            }
            else
            {
                c = ' ';
                return false;
            }
        }
        private void RemoveTokens(List<Token> tokens, Type t)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i].GetType() != t)
                    tokens.RemoveAt(i);
            }
        }
        private string ReadToEnd(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1)
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ReadAllButEqual(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '='))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }

        private string ReadAllButSingleQuote(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '\''))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private void ReadJustPastSingleQuote(StringReader _reader)
        {

            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '\''))
                _reader.Read();
            _reader.Read();
            while (_reader.Peek() != -1 && Char.IsWhiteSpace((char)_reader.Peek()))
                _reader.Read();
        }
        private void ReadJustPastEqual(StringReader _reader)
        {

            while (_reader.Peek() != -1 && ((char)_reader.Peek() != '='))
                _reader.Read();
            _reader.Read();
            while (_reader.Peek() != -1 && Char.IsWhiteSpace((char)_reader.Peek()))
                _reader.Read();
        }
        private string ReadUntil(StringReader _reader, char c)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && (char)_reader.Peek() != c)
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        
        private string ReadUntilWhiteSpace(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && !Char.IsWhiteSpace((char)_reader.Peek()))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
       
        private string ReadWhiteSpace(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (_reader.Peek() != -1 && Char.IsWhiteSpace((char)_reader.Peek()))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ParseWord(StringReader _reader)
        {
            var sb = new StringBuilder();
            while ((Char.IsLetter((char)_reader.Peek())))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            return sb.ToString();
        }
        private string ParseAMPM(StringReader _reader)
        {
            var sb = new StringBuilder();
            while (((char)_reader.Peek() == 'A' || (char)_reader.Peek() == 'P'))
                sb.AppendFormat("{0}", ((char)_reader.Read()).ToString());
            char c = (char)_reader.Peek();
            sb.AppendFormat("{0}", c.ToString());
            return sb.ToString();
        }
        private double ParseNumber(StringReader _reader)
        {
            var sb = new StringBuilder();
            var decimalExists = false;
            while (Char.IsDigit((char)_reader.Peek()) || ((char)_reader.Peek() == '.'))
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
