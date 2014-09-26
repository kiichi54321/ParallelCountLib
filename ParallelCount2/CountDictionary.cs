using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void Save(string fileName,int min)
        {
            using (var file = System.IO.File.CreateText(fileName))
            {
                foreach (var item in this.GetDictionary(min).OrderByDescending(n=>n.Value.Value))
                {
                    try
                    {
                        file.WriteLine(item.Key + "\t" + item.Value.ToString());
                    }
                    catch
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
