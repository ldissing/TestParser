using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using ArithmeticExpressionParser;
using Orders;
using DictionaryDataGridDemo;
using NinjaReader;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Data;

namespace DictionaryDataGridDemo
{
    public class Results
    {
        public int profitTarget = 4;
        public int stopLoss = 4;
        public int winners = 0;
        public int losers = 0;
        public double profit = 0;
        public double dd = 0;
        public override string ToString()
        {
            return string.Format("PT {0}, SL {1} : W {2}, L {3}, Profit {4}, % winners {5}", profitTarget, stopLoss, winners, losers, profit, PercentageWinners);
        }
        public string CSV
        {
            get
            {
                return string.Format("{6}, {0}, {1}, {2}, {3}, {4}, {5}, {7}", profitTarget, stopLoss, winners, losers, profit, PercentageWinners, profit*50.0, dd*50.0);
            }
        }
        public string CSVHeader
        {
            get
            {
                return string.Format("Profit in $, ProfitTarget, StopLoss, Winners, Losers, Profit(Points), % winners, DD");
            }
        }
        public double PercentageWinners
        {
            get
            {
                if (TotalTrades > 0)
                    return (100.0 * winners) / TotalTrades;
                else
                    return 0;
            }
        }
        public int TotalTrades
        {
            get
            {
                return winners + losers;
            }
        }

    }
    public class SingleDictViewModel : INotifyPropertyChanged
    {
        public DataTable dt { get; set; }
        public Dictionary<string, CompletedOrders> MyDictionary { get; set; }
        public Dictionary<string, Results> ProcessedTrades { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public SingleDictViewModel()
        {
                // Default INotifyPropertyChanged
            dt = new DataTable();
            Parser p = null;
            MyDictionary = new Dictionary<string, CompletedOrders>();
            ProcessedTrades = new Dictionary<string, Results>();
            //string[] files = Directory.GetFiles(@"C:\DimensionTrader\log", "*.txt");
            //p = null;
            //try
            //{
            //    foreach (string f in files)
            //    {
            //        if (p == null)
            //            p = new Parser(f);
            //        else
            //            p = new Parser(f, p.OrderDict);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //}
            //if (p != null)
            //{
            //    MyDictionary = p.OrderDict;
            //}
            using (StreamReader sr = new StreamReader(@"c:\results\NinjaTrader Trade List.csv"))
            {
                string header = "";
                string[] headerBits = null;
                if (sr.EndOfStream == false)
                {
                    header = sr.ReadLine();
                    headerBits = header.Split(',');
                }
                while(sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split(',');
                    CompletedOrders order = new CompletedOrders(headerBits, parts);
                    if (!MyDictionary.ContainsKey(order.dateTime.ToString()))
                        MyDictionary.Add(order.dateTime.ToString(), order);
                }

            }
            dt.Columns.Add("Profit", typeof(double));
            dt.Columns.Add("Target", typeof(int));
            dt.Columns.Add("StopLoss", typeof(int));
            dt.Columns.Add("Point Profit", typeof(double));
            dt.Columns.Add("Percenage Profitable", typeof(double));
            dt.Columns.Add("Winners", typeof(int));
            dt.Columns.Add("Losers", typeof(int));
            dt.Columns.Add("DD", typeof(double));
            
            Results r = new Results();
            //r = ProcessPTSL(4, 4);
            //ProcessPTSL(11, 4);
            using (StreamWriter sw = new StreamWriter(@"c:\results\Results.csv"))
            {
                sw.WriteLine(r.CSVHeader);
                for (int j = 4; j <= 16; j += 1)
                    for (int i = 4; i <= 18; i++)
                    {
                        r = ProcessPTSL(i, j);
                        sw.WriteLine(r.CSV);
                    }
                for (int j = 20; j <= 100; j += 4)
                    for (int i = 4; i <= 18; i++)
                    {
                        r = ProcessPTSL(i, j);
                        sw.WriteLine(r.CSV);
                    }
            }
        }
        private Results ProcessPTSL(int stopLoss, int target)
        {
            double tickSize = .25;
            
            NinjaReaderNinjaTickFileManager gfn = null;
            MarketDataType mdtlast = new MarketDataType();
            DateTime startDate = DateTime.MinValue;
            int winners = 0;
            int losers = 0;
            double profit = 0;
            double dd = 0;
            double maxprofit = 0;
            string instrument = string.Empty;
            string gfnInstrument = string.Empty;
            foreach (string k in MyDictionary.Keys)
            {
                CompletedOrders order = MyDictionary[k];
                if (gfn == null || gfnInstrument == string.Empty || gfnInstrument != order.Instrument)
                {
                    gfnInstrument = order.Instrument;
                    gfn = new NinjaReader.NinjaReaderNinjaTickFileManager(true, gfnInstrument, .25, false, NinjaReader.FileModeType.OnePerDay, NinjaReader.NinjaReaderNinjaTickFileManager.NinjaTickType.Last);
                    DateTime then = new DateTime(2013, 12, 1);
                }
                
                double tradeTarget = order.IsLong ? order.entry + target * tickSize : order.entry - target * tickSize;
                double tradeStop = order.IsLong ? order.entry - stopLoss * tickSize : order.entry + stopLoss * tickSize;

                gfn.SetCursorTime(order.dateTime.AddMilliseconds(1), ref mdtlast);
                bool brokeOut = false;
                do
                {
                    gfn.GetNextTick(ref mdtlast);
                    if (mdtlast.Time == DateTime.MinValue)
                    {
                        brokeOut = true;
                        break;
                    }

                } while (mdtlast.Price != order.entry);
                bool bContinue = true;
                do
                {
                    gfn.GetNextTick(ref mdtlast);
                    if (mdtlast.Time == DateTime.MinValue)
                    {
                        brokeOut = true;
                        break;
                    }
                    
                    
                    if (tradeTarget == mdtlast.Price
                        || tradeStop == mdtlast.Price)
                        bContinue = false;
                    
                    if (mdtlast.Time > order.exitTime)
                        bContinue = false;
                } while (bContinue);
                //while ((order.IsLong ? tradeTarget > mdtlast.Price : tradeTarget < mdtlast.Price)
                //        && (order.IsLong ? tradeStop < mdtlast.Price : tradeStop > mdtlast.Price)
                //        && mdtlast.Time < order.exitTime
                //        );

                CompletedOrders trade = new CompletedOrders();
                trade.entry = order.entry;
                trade.dateTime = order.dateTime;
                trade.exitTime = order.exitTime;
                trade.exit = order.exit;
                trade.quantity = order.quantity;
                if (mdtlast.Time >= order.exitTime || brokeOut == true)
                {

                }
                else
                {
                    trade.exit = mdtlast.Price;
                    trade.exitTime = mdtlast.Time;
                }
                if (trade.exit > 1.0)
                {
                    if (order.IsLong)
                        trade.profit = trade.exit - trade.entry;
                    else
                        trade.profit = trade.entry - trade.exit;

                    //commission
                    trade.profit -= 5 / 50.0;
                    if (trade.profit > 0)
                    {

                        // winning trade
                        winners++;
                    }
                    else
                    {
                        losers++;
                        // losing trade
                    }
                    profit += trade.profit;
                    if (maxprofit < profit)
                        maxprofit = profit;
                    else
                        dd = Math.Max(maxprofit - profit, dd);
                }
            }
            gfn.Dispose();
            Results r = new Results();
            r.profit = profit;
            r.winners = winners;
            r.losers = losers;
            r.stopLoss = stopLoss;
            r.profitTarget = target;
            r.dd = dd;
            var row = dt.NewRow();
            double myProfit = r.profit * 50.0;
            row["Profit"] = myProfit;
            row["Target"] = r.profitTarget;
            row["StopLoss"] = r.stopLoss;
            row["Point Profit"] = r.profit;
            row["Percenage Profitable"] = r.PercentageWinners;
            row["Winners"] = r.winners;
            row["Losers"] = r.losers;
            row["DD"] = dd * 50.0;
            dt.Rows.Add(row);
            return r;
        }
    }
}
namespace TestPTStopLosses
{
    /// <summary>
    /// Proxy class to easily take actions when a specific property in the "source" changed
    /// </summary>
    /// Last updated: 20.01.2015
    /// <typeparam name="TSource">Type of the source</typeparam>
    /// <typeparam name="TPropType">Type of the property</typeparam>
    public class PropertyChangedProxy<TSource, TPropType> where TSource : INotifyPropertyChanged
    {
        private readonly Func<TSource, TPropType> _getValueFunc;
        private readonly TSource _source;
        private readonly Action<TPropType> _onPropertyChanged;
        private readonly string _modelPropertyname;

        /// <summary>
        /// Constructor for a property changed proxy
        /// </summary>
        /// <param name="source">The source object to listen for property changes</param>
        /// <param name="selectorExpression">Expression to the property of the source</param>
        /// <param name="onPropertyChanged">Action to take when a property changed was fired</param>
        public PropertyChangedProxy(TSource source, Expression<Func<TSource, TPropType>> selectorExpression, Action<TPropType> onPropertyChanged)
        {
            _source = source;
            _onPropertyChanged = onPropertyChanged;
            // Property "getter" to get the value
            _getValueFunc = selectorExpression.Compile();
            // Name of the property
            var body = (MemberExpression)selectorExpression.Body;
            _modelPropertyname = body.Member.Name;
            // Changed event
            _source.PropertyChanged += SourcePropertyChanged;
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _modelPropertyname)
            {
                _onPropertyChanged(_getValueFunc(_source));
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,  INotifyPropertyChanged
    {
        private PropertyChangedProxy<SingleDictViewModel, string> _statusPropertyChangedProxy;
        private SingleDictViewModel _model;
        //public MainWindow(SingleDictViewModel model)
        //{
        //    _model = model;
        //    _statusPropertyChangedProxy = new PropertyChangedProxy<SingleDictViewModel, string>(
        //        _model, myModel => myModel.ProcessedTrades.Keys, s => OnPropertyChanged("ProcessedTrades")
        //    );
        //}

        // Default INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        Parser p = null;
        Dictionary<string, CompletedOrders> orderDict = new Dictionary<string, CompletedOrders>();
        public MainWindow()
        {
            //_model = model;
            InitializeComponent();
            SingleDictViewModel svm = ((SingleDictViewModel)this.DataContext);
            orderDict = svm.MyDictionary;

            Results r = new Results();
            r.winners = 1;
            r.losers = 1;


            
            

            
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ProcessPTSL_Click(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Trades_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //KeyValuePair<string, CompletedOrders> kvp = (KeyValuePair<string, CompletedOrders>)e.Row.Item;
            //if (orderDict.ContainsKey(kvp.Key) == false)
            //    orderDict.Add(kvp.Key, kvp.Value);
        }
    }
}
