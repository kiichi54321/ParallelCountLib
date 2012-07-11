using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ParallelCountLib
{
    public class HashNameManage:IDisposable
    {
        ConcurrentDictionary<string, HashNameData> dic = new ConcurrentDictionary<string, HashNameData>();

        public HashNameData GetHashNameData(string hash)
        {
            HashNameData hashData;
            if (dic.TryGetValue(hash, out hashData) == false)
            {
                hashData = new HashNameData();
                if (dic.TryAdd(hash, hashData)==false)
                {
                    return GetHashNameData(hash);
                }
            }
            return hashData;
        }

        public void Clear()
        {
            dic.Clear();
        }

        public void Dispose()
        {
            foreach (var item in dic.Values)
            {
                item.Dispose();
            }
            dic.Clear();
        }
    }

    public class HashNameData:IDisposable
    {
        ConcurrentDictionary<string, int> table = new ConcurrentDictionary<string, int>();
        ConcurrentDictionary<int, string> table2 = new ConcurrentDictionary<int, string>();
        int idCount = 0;


        public int GetId(string key)
        {
            int id;

            if (table.TryGetValue(key, out id))
            {
            }
            else
            {
                lock (this)
                {
                    idCount++;
                    id = idCount;

                    if (table.TryAdd(key, id) == false)
                    {
                        return GetId(key);
                    }
                    table2.TryAdd(id, key);

                }
            }
            return id;
        }


        public string GetName(int id)
        {

            if (table2.ContainsKey(id))
            {
                return table2[id];
            }
            else
            {
                return null;
            }
        }





        public void Dispose()
        {
            table.Clear();
            table2.Clear();
        }
    }
}
