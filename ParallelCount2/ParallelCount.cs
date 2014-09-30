using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class ParallelCount
    {
        static int threadNum = 6;
        public static int ThreadNum
        {
            get { return ParallelCount.threadNum; }
            set { ParallelCount.threadNum = value; }
        }
        public static CountDictionary Run<T>(IEnumerable<T> source, Func<T, IEnumerable<string>> func)
        {
            return Run<T>(source, func, 10000,2);
        }

        public static CountDictionary RunTextFile(string fileName, Func<string, IEnumerable<string>> func, int readRange, int minCount)
        {
            return Run<string>(SynchronizedReadLines(fileName), func, readRange, minCount);
        }
        public static CountDictionary RunTextFile(string fileName, Func<string, IEnumerable<string>> func)
        {
            return Run<string>(SynchronizedReadLines(fileName), func, 10000, 1);
        }




        public static IEnumerable<T> ForEach<T>(IEnumerable<T> source,Action<T> action )
        {
            ConcurrentStack<T> stack = new ConcurrentStack<T>(source);
            List<System.Threading.Tasks.Task<List<T>>> tasks = new List<System.Threading.Tasks.Task<List<T>>>();
            for (int i = 0; i < ThreadNum; i++)
            {
                var task = System.Threading.Tasks.Task.Factory.StartNew<List<T>>((n) =>
                {
                    List<T> list = new List<T>();
                    while (true)
                    {
                        T s;
                        if (stack.TryPop(out s) == false)
                        {
                            break;
                        }
                        action(s);
                        list.Add(s);
                    }
                    return list;
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
                if (task != null) tasks.Add(task);
            }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            return tasks.SelectMany(n => n.Result);
        }
 

        public static IEnumerable<T> ForEach<Source,T>(IEnumerable<Source> source,Func<Source,T> func )
        {
            ConcurrentStack<Source> stack = new ConcurrentStack<Source>(source);
                
            List<System.Threading.Tasks.Task<List<T>>> tasks = new List<System.Threading.Tasks.Task<List<T>>>();
            for (int i = 0; i < ThreadNum; i++)
            {
                 var task = System.Threading.Tasks.Task.Factory.StartNew<List<T>>((n) =>
                {
                    List<T> list = new List<T>();
                    while (true)
                    {
                        Source s;
                        if (stack.TryPop(out s) == false )
                        {
                            break;
                        }
                        list.Add(func(s));
                    }
                    return list;
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
                if (task != null) tasks.Add(task);
            }            
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            return tasks.SelectMany(n => n.Result);
        }



        public static CountDictionary Run<T>(IEnumerable<T> source, Func<T, IEnumerable<string>> func, int readRange, int minCount)
        {
            LoadStack<T> stack = new LoadStack<T>();
            stack.Source = source;
            stack.Take = readRange * 10;
            stack.LoadNoTask();
            List<System.Threading.Tasks.Task<CountDictionary>> tasks = new List<System.Threading.Tasks.Task<CountDictionary>>();
            for (int i = 0; i < ThreadNum; i++)
            {
                var task = System.Threading.Tasks.Task.Factory.StartNew<CountDictionary>((n) =>
                {
                    T[] range = new T[readRange];
                    CountDictionary cDic = new CountDictionary();
                    while (true)
                    {
                        if (stack.TryPopRange(range) == 0)
                        {
                            if (stack.Completed) break;
                            System.Threading.Thread.Sleep(100);
                        }
                        stack.Load();
                        foreach (var item in range.Where(m => m != null))
                        {
                            foreach (var item2 in func(item))
                            {
                                cDic.AddCount(item2);
                            }
                        }
                    }
                    return cDic;
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
                if (task != null) tasks.Add(task);
            }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            CountDictionary allDic = new CountDictionary();
            foreach (var item in tasks)
            {
                foreach (var d in item.Result.GetDictionary(minCount))
                {
                    allDic.AddCount(d.Key, d.Value);
                }
            }
            return allDic;
        }

        public static IEnumerable<string> SynchronizedReadLines(string file)
        {
            using (var stream = System.IO.File.OpenText(file))
            using (var stream2 = StreamReader.Synchronized(stream))
            {
                while (true)
                {
                    if (stream2.Peek() > 0)
                    {
                        yield return stream2.ReadLine();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
