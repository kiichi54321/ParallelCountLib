using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ParallelCountLib
{
    public class DataStockManage<CountData, ReadData>:IDisposable
        where CountData : ICountDataStruct<CountData>, new()
        where ReadData : BaseReadData<CountData>, new()
    {
        private ConcurrentDictionary<string, DataStock<CountData>> dicDataStock = new ConcurrentDictionary<string, DataStock<CountData>>();

        public void DataStockClear()
        {
            dicDataStock.Clear();
        }

        public Action<string> ReportAction { get; set; }
        public Action<ReadData> ReadLineAction { get; set; }


        protected void OnReport(string message)
        {
            if (ReportAction != null)
            {
                ReportAction(message);
            }
        }

        public DataStockManage()
        {
            readData = new ReadData();
            readData.AddCountAction = (string o, string c, CountData h) =>
            {
                this.Add(o,c,h);
            };
        }

        private ReadData readData;


        public Func<string, string> ConvertHash;

        public string BaseFolder { get; set; }
        public string BaseFileName { get; set; }

        bool addFlag = false;
        public void Add(string hashstr,string key, CountData count )
        {
            addFlag = true;
            string hash = string.Empty;

            if (hashstr == string.Empty)
            {
                if (ConvertHash != null)
                {
                    hash = ConvertHash(key);
                }
                else
                {
                    if (key.Length > 1)
                    {
                        hash = key.Substring(0, 2);
                    }
                    else
                    {
                        hash = key;
                    }
                }
            }
            else
            {
                hash = hashstr;
            }
            DataStock<CountData> ds;
            if (dicDataStock.TryGetValue(hash, out ds))
            {
                ds.AddData(key, count);
            }
            else
            {
                ds = new DataStock<CountData>() { FileName = BaseFileName + "_" + hash + ".tsv", Folder = BaseFolder, StockType = stockType, HashNameData = HashNameManage.GetHashNameData(hash) };
                if (dicDataStock.TryAdd(hash, ds))
                {
                    ds.AddData(key, count);
                }
                else
                {
                    Add(hashstr,key, count );
                }
            }
        }

        public HashNameManage HashNameManage { get; set; }


        public void FileRead(string fileName)
        {
            string key = null;
            readData.ReadLines.Clear();
            foreach (var item in System.IO.File.ReadLines(fileName))
            {
                key = readData.GetGroupByKey(item);
                if (key != null)
                {
                    if (readData.Key != key)
                    {
                        OnReadLineAction(readData);
                        readData.Key = key;
                        readData.ReadLines.Clear();
                    }
                    readData.ReadLines.Add(item);
                }
                else
                {
                    readData.ReadLines.Add(item);
                    OnReadLineAction(readData);
                    readData.ReadLines.Clear();
                }
            }
            OnReadLineAction(readData);
            readData.ReadLines.Clear();
            readData.Key = null;
        }

        protected void OnReadLineAction(ReadData readData )
        {
            readData.ReadLinesAction();
            if (ReadLineAction != null)
            {
                ReadLineAction(readData);
            }
        }

        public IEnumerable<string> HashList
        {
            get { return dicDataStock.Keys; }
        }

        public DataStock<CountData> GetDataStock(string hash)
        {
            if (dicDataStock.ContainsKey(hash))
            {
                return dicDataStock[hash];
            }
            else { return null; }
        }


        public void Add(string key)
        {
            Add(key, string.Empty, new CountData());
        }



        public string Reduce()
        {
            System.Collections.Concurrent.ConcurrentBag<string> list = new ConcurrentBag<string>();
            System.Threading.Tasks.Parallel.ForEach(dicDataStock, (item) =>
            {
                list.Add(item.Value.Reduce());
            });
            string reduceFile = BaseFolder + "/" + BaseFileName + "_reduce.tsv";
            using (var file = System.IO.File.CreateText(reduceFile))
            {
                foreach (var item in list)
                {
                    foreach (var line in System.IO.File.ReadLines(item))
                    {
                        file.WriteLine(line);
                    }
                }
            }
            return reduceFile;
        }

        public static void Reduce(string saveFileName, IEnumerable<string> fileList)
        {
            Dictionary<string, CountData> table = new Dictionary<string, CountData>();

            foreach (var item in fileList)
            {
                foreach (var line in System.IO.File.ReadLines(item))
                {
                    var data = KeyCountStruct<CountData>.FromText(line);
                    if (table.ContainsKey(data.Key))
                    {
                        table[data.Key] = table[data.Key].Add(data.Count);
                    }
                    else
                    {
                        table.Add(data.Key, data.Count);
                    }
                }
            }

            using (var file = System.IO.File.CreateText(saveFileName))
            {
                foreach (var item in table)
                {
                    file.WriteLine(item.Key + "\t" + item.Value);
                }
            }
        }

        public static void Reduce(string saveFileName, IEnumerable<DataStockManage<CountData, ReadData>> manageList)
        {
            List<string> hashList = new List<string>();

            foreach (var item in manageList)
            {
                hashList.AddRange(item.HashList);
            }
            hashList = hashList.Distinct().ToList();

            using (var file = System.IO.File.CreateText(saveFileName))
            {
                foreach (var item in hashList)
                {
                    Dictionary<string, CountData> dic = new Dictionary<string, CountData>();
                    foreach (var manage in manageList)
                    {
                        var dataStock = manage.GetDataStock(item);
                        if (dataStock != null)
                        {
                            var dataList = dataStock.GetReduceData();
                            foreach (var data in dataList)
                            {
                                if (dic.ContainsKey(data.Key))
                                {
                                    dic[data.Key] = dic[data.Key].Add(data.Count);
                                }
                                else
                                {
                                    dic.Add(data.Key, data.Count);
                                }
                            }
                        }
                    }

                    foreach (var item2 in dic.OrderBy(n => n.Key))
                    {
                        file.WriteLine(item2.Key + "\t" + item2.Value);
                    }

                }
            }
        }





        StockType stockType = StockType.Memory;

        public StockType StockType
        {
            get { return stockType; }
            set { stockType = value; }
        }


        public void Dispose()
        {
            foreach (var item in dicDataStock)
            {
                item.Value.Dispose();
            }
            dicDataStock.Clear();
        }
    }

}
