using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ParallelCount
{
    public class DataStock<CountData>where CountData:ICountDataStruct<CountData>,new()
    {
        public string FileName { get; set; }
        public string Folder { get; set; }
        Dictionary<int, CountData> tmpDic = new Dictionary<int, CountData>();

        public void AddData(string key, CountData count)
        {
            var id = GetId(key);

            {
                if (tmpDic.ContainsKey(id))
                {
                    tmpDic[id] = tmpDic[id].Add( count);
                }
                else
                {
                    tmpDic.Add(id, count);
                }
            }

        }

        public HashNameData HashNameData { get; set; }








        int idCount = 0;

        public int GetIdCount()
        {
            object obj = new object();
            lock (obj)
            {
                idCount++;
            }
            return idCount;
        }

        public int GetId(string key)
        {
            return HashNameData.GetId(key);
        }



        public void AddData(string key)
        {
            AddData(key, new CountData());
        }

        DateTime lastTime = DateTime.Now;

        StockType stockType = StockType.Memory;

        public StockType StockType
        {
            get { return stockType; }
            set { stockType = value; }
        }


        public string Reduce()
        {
            string reduceFileName = Folder + "/Reduce/" + FileName;

            if (System.IO.Directory.Exists(Folder + "/Reduce") == false)
            {
                System.IO.Directory.CreateDirectory(Folder + "/Reduce");
            }




            if (stockType == StockType.Memory)
            {
                using (var file = System.IO.File.CreateText(reduceFileName))
                {
                    foreach (var item in tmpDic.OrderBy(n => n.Key))
                    {
                        file.WriteLine(HashNameData.GetName(item.Key) + "\t" + item.Value);
                    }
                }

            }
            else if (stockType == StockType.MapReduce)
            {
                tmpDic.Clear();
                foreach (var item in System.IO.File.ReadLines(Folder + "/" + FileName))
                {
                    var data = KeyCountStruct<CountData>.FromText(item);
                    if (tmpDic.ContainsKey(HashNameData.GetId(data.Key)))
                    {
                        tmpDic[HashNameData.GetId(data.Key)] = tmpDic[HashNameData.GetId(data.Key)].Add( data.Count);
                    }
                    else
                    {
                        tmpDic.Add(HashNameData.GetId(data.Key), data.Count);
                    }
                }

                using (var file = System.IO.File.CreateText(reduceFileName))
                {
                    foreach (var item in tmpDic.OrderBy(n => n.Key))
                    {
                        file.WriteLine(HashNameData.GetName(item.Key) + "\t" + item.Value);

                    }
                }
                tmpDic.Clear();

            }
            return reduceFileName;
        }

        public List<KeyCountStruct<CountData>> GetReduceData()
        {
            List<KeyCountStruct<CountData>> list = new List<KeyCountStruct<CountData>>();
            foreach (var item in tmpDic)
            {
                string key = HashNameData.GetName(item.Key);
                if (key != null)
                {
                    list.Add(new KeyCountStruct<CountData>() { Key = key, Count = item.Value });
                }
                else
                {
                    System.Console.WriteLine("エラー：GetReduceData()でKeyがNull");
                }
            }

            return list;
        }
    }

    public struct KeyCountStruct<CountStruct>where CountStruct:ICountDataStruct<CountStruct>,new()
    {
        private string key;
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private CountStruct count;
        public CountStruct Count
        {
            get { return count; }
            set { count = value; }
        }

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }




        public static KeyCountStruct<CountStruct> FromText(string keyvalue)
        {
            var d = keyvalue.Split('\t');
            int c;
            if (d.Length > 1)
            {
                var s = new KeyCountStruct<CountStruct>();
                s.Count.TryParse(d[1]);
                return s;
            }
            return new KeyCountStruct<CountStruct>() { Key = d[0], Count = new CountStruct() };
        }
    }



    public class KeyCountData
    {
        private string key;
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private int count;
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public KeyCountData()
        {

        }

        public KeyCountData(string key)
        {
            this.key = key;
            this.count = 1;
        }

        public static KeyCountData FromText(string keyvalue)
        {
            var d = keyvalue.Split('\t');
            int c;
            if (d.Length > 1)
            {
                if (int.TryParse(d[1], out c))
                {
                    return new KeyCountData() { Key = d[0], Count = c };
                }
            }
            return new KeyCountData() { Key = d[0], Count = 1 };
        }

        public string ToText()
        {
            return Key + "\t" + Count.ToString();
        }

        public static string ToText(string key, int value)
        {
            return key + "\t" + value.ToString();
        }
    }

    public enum StockType
    {
        Memory, MapReduce
    }

    public enum SyncType
    {
        Sync, Async
    }
}
