using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParallelCount.Extend;

namespace ParallelCountLib
{
    public class CountDictionary
    {
        Dictionary<string, Dictionary<string, IntCount>> dic = new Dictionary<string, Dictionary<string, IntCount>>();

        public Dictionary<string, Dictionary<string, IntCount>> Dic
        {
            get { return dic; }
  //          set { dic = value; }
        }
        int hashLength = 2;

        public int HashLength
        {
            get { return hashLength; }
            set { hashLength = value; }
        }

        public void AddCount(string text)
        {
            AddCount(text, IntCount.Default);
        }

        public Func<string, string> HashFunc { get; set; }

        private string GetHashString(string text)
        {
            if(HashFunc !=null)
            {
                return HashFunc(text);
            }
            else
            {
                if (text.Length > 1) return text.Substring(0, 2);
                return text;
            }
        }

        public void AddCount(string text,IntCount iCount)
        {
            string hash = GetHashString(text);
            Dictionary<string, IntCount> tDic = new Dictionary<string,IntCount>();
            IntCount intCount = new IntCount();
         
            if (dic.TryGetValue(hash, out tDic))
            {
                if( tDic.TryGetValue(text,out intCount))
                {
                    tDic[text] = intCount.Add(iCount);
                }
                else
                {
                    tDic.Add(text, iCount);
                }
            }
            else
            {
                dic.Add(hash, new Dictionary<string, IntCount>());
                dic[hash].Add(text, iCount);
            }
        }

        public Dictionary<string,IntCount> GetDictionary(int min)
        {
            return dic.SelectMany(n => n.Value).Where(n=>n.Value.Value >=min).ToDictionary(n=> n.Key,n=>n.Value);
        }

        public Dictionary<string,IntCount> GetDictionarySkip(int min)
        {
             var d = dic.SelectMany(n => n.Value).Where(n=>n.Value.Value >=min).GroupBy(n=>n.Value.Value);
             List<KeyValuePair<string, IntCount>> list2 = new List<KeyValuePair<string, IntCount>>(dic.Count/2);

            var d2 =  ParallelCount.ForEach<IGrouping<int, KeyValuePair<string, IntCount>>,List<KeyValuePair<string, IntCount>>>(d,
                 item => {
                     List<KeyValuePair<string, IntCount>> list = new List<KeyValuePair<string, IntCount>>(item.OrderByDescending(n => n.Key.Length));
                     KeyValuePair<string, IntCount> c = list.First();
                     int count = 0;
                     while (true)
                     {
                         foreach (var item2 in list.ToArray())
                         {
                             if (item2.Key != c.Key && c.Key.Contains(item2.Key))
                             {
                                 list.Remove(item2);
                             }
                         }
                         count++;
                         if (list.Count <= count) break;
                         c = list[count];
                     }
                     return list;                 
                 });
            return d2.SelectMany(n => n).ToDictionary(n => n.Key, n => n.Value);
             //foreach (var item in d)
             //{
             //    List<KeyValuePair<string, IntCount>> list = new List<KeyValuePair<string, IntCount>>(item.OrderByDescending(n => n.Key.Length));
             //    KeyValuePair<string, IntCount> c = list.First();
             //    int count = 0;
             //    while(true)
             //    {
             //        foreach (var item2 in list.ToArray())
             //        {
             //            if(item2.Key != c.Key && c.Key.Contains(item2.Key) )
             //            {
             //                list.Remove(item2);
             //            }
             //        }
             //        count++;
             //        if (list.Count < count) break;
             //        c = list[count];
             //    }
             //    list2.AddRange(list);
             //}
             //return list2.ToDictionary(n => n.Key, n => n.Value);
        }


        public void Save(string fileName,int min)
        {
            using (var file = System.IO.File.CreateText(fileName))
            {
                var utf8 = System.Text.UTF8Encoding.UTF8;
                    
                foreach (var item in this.GetDictionarySkip(min).OrderByDescending(n=>n.Value.Value))
                {
                    try
                    {
                        var d = utf8.GetBytes(item.Key);
                        file.WriteLine( item.Key + "\t" + item.Value.ToString());
                    }
                    catch(Exception e)
                    {

                    }
                }
            }
        }

        public static CountDictionary Load(string fileName)
        {
            CountDictionary cdic = new CountDictionary();

            foreach (var item in System.IO.File.ReadLines(fileName))
            {
                var d = item.Split('\t');
                if (d.Length > 1) cdic.AddCount(d[0], IntCount.Parse(d[1]));
            }
            return cdic;
        }
    }
}
