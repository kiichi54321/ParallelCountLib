using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ParallelCountLib
{
    public static class HashNameManage
    {
        static ConcurrentDictionary<string, HashNameData> dic = new ConcurrentDictionary<string, HashNameData>();

        public static HashNameData GetHashNameData(string hash)
        {
            HashNameData hashData;
            if (dic.TryGetValue(hash, out hashData) == false)
            {
                lock (dic)
                {
                    hashData = new HashNameData();
                    dic.TryAdd(hash, hashData);
                }
            }
            return hashData;
        }

        public static void Clear()
        {
            dic.Clear();
        }
    }

    public class HashNameData
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




    }
}
