using System;
using System.Collections.Generic;
namespace Tokens
{
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
    }
    public class WordToken : Token
    {
        private readonly string _value;

        public WordToken(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        
    }
    public class PMTFToken : WordToken
    {
        public PMTFToken(string value)
            : base(value)
        {
        }
    }
    public class VersionToken : WordToken
    {
        public VersionToken(string value) : base(value)
        {
        }
    }
    public class AccountToken : WordToken
    {
        public override string ToString()
        {
            return "Account = " + Value;
        }
        public AccountToken(string value)
            : base(value)
        {
        }
    }
    public class ProfitTargetsToken : WordToken
    {
        public ProfitTargetsToken(string value)
            : base(value)
        {

        }
        public override string ToString()
        {
            return "ProfitTargets = " + Value;
        }
    }
    public class CatastrophicLossToken : WordToken
    {
        public CatastrophicLossToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "Catastrophic Loss = " + Value;
        }
    }
    public class SGTargetsToken : WordToken
    {
        public SGTargetsToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "sgTargets = " + Value;
        }
    }
    public class BEStopToken : WordToken
    {
        public BEStopToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "BE stop = " + Value;
        }
    }
    public class StopsToken : WordToken
    {
        public StopsToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "sg stop movement " + Value;
        }
    }
    public class OrdersToken : Token
    {
        public override string ToString()
        {
            string ret = "";
            foreach(string k in _dict.Keys)
            {
                ret += k + "=" + _dict[k] + " ";
            }
            return ret;
        }
        protected Dictionary<string, string> _dict = new Dictionary<string, string>();
        public override string Name
        {
            get {
                if (_dict.ContainsKey("Name"))
                    return _dict["Name"];
                return "";
            }
        }
        public string BaseName
        {
            get
            {
                if (IsEntryOrder)
                    return Name;
                else
                {
                    string text = Name;
                    string[] split = text.Split(' ');
                    if (split.Length == 2)
                        return split[1].Trim();
                    return "error";
                }
            }
        }
        public string State
        {
            get
            {
                if (_dict.ContainsKey("State"))
                    return _dict["State"];
                return "";
            }
        }
        public string Action
        {
            get
            {
                if (_dict.ContainsKey("Action"))
                    return _dict["Action"];
                return "";
            }
        }
        public double Price
        {
            get
            {
                if (_dict.ContainsKey("Fillprice"))
                    return double.Parse(_dict["Fillprice"]);
                return 0;
            }
        }
        public string Type
        {
            get
            {
                if (_dict.ContainsKey("Type"))
                    return _dict["Type"];
                return "";
            }
        }
        public int Filled
        {
            get
            {
                if (_dict.ContainsKey("Filled"))
                    return int.Parse(_dict["Filled"]);
                return 0;
            }
        }
        public int Quantity
        {
            get
            {
                if (_dict.ContainsKey("Quantity"))
                    return int.Parse(_dict["Quantity"]);
                return 0;
            }
        }
        public bool IsEntryOrder
        {
            get
            {
                if (Name.StartsWith("Limit"))
                    return false;
                if (Name.StartsWith("Stop"))
                    return false;
                if (Name.StartsWith("SG EXIT"))
                    return false;
                return true;
            }
        }
        public bool IsTarget
        {
            get
            {
                if (Name.StartsWith("Limit"))
                    return true;
                return false;
            }
        }
        public bool IsStop
        {
            get
            {
                if (Name.StartsWith("Stop"))
                    return true;
                return false;
            }
        }
        public bool IsSGExit
        {
            get
            {
                if (Name.StartsWith("SG EXIT"))
                    return true;
                return false;
            }
        }
    }
    public class KeyValueToken : WordToken
    {
        private readonly string _key;
        public KeyValueToken(string key, string value) : base(value)
        {
            _key = key;
        }
        public string Key
        {
            get { return _key; }
        }
    }
    public class OrderEntryToken : OrdersToken
    {
        public OrderEntryToken(List < Token > tokens)
        {
            foreach(Token t in tokens)
            {
                KeyValueToken kv = t as KeyValueToken;
                if (kv != null)
                {
                    if (_dict.ContainsKey(kv.Key))
                        _dict[kv.Key] = kv.Value;
                    else
                        _dict.Add(kv.Key, kv.Value);
                }
            }
        }
    }
    public class DailyGoalRiskToken : WordToken
    {
        public DailyGoalRiskToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "Goal/Risk = " + Value;
        }
    }
    public class DailyGoalExceededToken : WordToken
    {
        public DailyGoalExceededToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "Daily Goal exceeded = " + Value;
        }
    }
    public class DailyRiskExceededToken : WordToken
    {
        public DailyRiskExceededToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "Daily Risk exceeded = " + Value;
        }
    }
    public class ProcessHistoricalToken : WordToken
    {
        public ProcessHistoricalToken(string value)
            : base(value)
        {
        }
        public override string ToString()
        {
            return "Process Historical = " + Value;
        }
    }
    public class StartEndTimeToken : WordToken
    {
        private readonly string _name;
        public StartEndTimeToken(string name, string value)
            : base(value)
        {
            _name = name;
        }
        public override string Name
        {
            get { return _name; }
        }
        public override string ToString()
        {
            return _name + " = " + Value;
        }
    }
    public class TradeDateTimeToken : Token
    {
        private readonly DateTime _value;
        public TradeDateTimeToken()
        { }
        public TradeDateTimeToken(List<Token> list, bool isAM)
        {
            // parse the date from the tokens passed in
            if (list.Count == 10)
            {
                int month = System.Convert.ToInt32((list[0] as NumberConstantToken).Value);
                int day = System.Convert.ToInt32((list[2] as NumberConstantToken).Value);
                int year = System.Convert.ToInt32((list[4] as NumberConstantToken).Value);
                int hour = System.Convert.ToInt32((list[5] as NumberConstantToken).Value);
                int min = System.Convert.ToInt32((list[7] as NumberConstantToken).Value);
                int sec = System.Convert.ToInt32((list[9] as NumberConstantToken).Value);
                if (isAM == true)
                {
                    _value = new DateTime(year, month, day, hour, min, sec);
                }
                else
                {
                    if (hour + 12 == 24)
                        hour = 0;
                    _value = new DateTime(year, month, day, hour + 12, min, sec);
                }
            }
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public DateTime Value
        {
            get { return _value; }
        }
    }
    public class UnderlineToken : Token
    {
        public override string ToString()
        {
                return "_";
        }
    }
    public class ColonToken : Token
    {
        public override string ToString()
        {
            return ":";
        }
    }
    public class AMToken : Token { }
    public class PMToken : Token { }
    public class OperatorToken : Token
    {
    }
    public class DashToken : OperatorToken
    {
        public override string ToString()
        {
            return "-";
        }
    }
    public class MultiplyToken : OperatorToken
    {
    }
    public class ForwardSlashToken : OperatorToken
    {
    }
    public class ParenthesisToken : Token
    {
        
    }
    public class OpenParenthesisToken : ParenthesisToken
    {
    }
    public class ClosedParenthesisToken : ParenthesisToken
    {
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
}