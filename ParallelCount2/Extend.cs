using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelCount.Extend
{
    public static class Parallel
    {
        private static int threadNum = 6;

        public static int ThreadNum
        {
            get { return threadNum; }
            set { threadNum = value; }
        }
             
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
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


        public static IEnumerable<T> ForEach<Source, T>(this IEnumerable<Source> source, Func<Source, T> func)
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
                        if (stack.TryPop(out s) == false)
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

    }
}
