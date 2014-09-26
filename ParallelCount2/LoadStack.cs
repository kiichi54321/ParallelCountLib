using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class LoadStack<T> : System.Collections.Concurrent.ConcurrentStack<T>
    {
        public IEnumerable<T> Source { get; set; }

        int take = 10000;

        public int Take
        {
            get { return take; }
            set { take = value; }
        }
        bool completed = false;

        public bool Completed
        {
            get { return completed; }
          private  set { completed = value; }
        }

        System.Threading.Tasks.Task readTask = null;
        IEnumerator<T> read;

        /// <summary>
        /// Load同期版
        /// </summary>
        public void LoadNoTask()
        {
            if (read == null) read = Source.GetEnumerator();
            int c = 0;
            while (c < take)
            {
                if (read.MoveNext())
                {
                    this.Push(read.Current);
                }
                else
                {
                    Completed = true;
                    break;
                }
                c++;
            }

        }
        
        /// <summary>
        /// Load()タスク版
        /// </summary>
        public void Load()
        {
            if (Completed == false)
            {
                if (readTask != null && readTask.Status == System.Threading.Tasks.TaskStatus.Running)
                {
                    return;
                }
                if (read == null) read = Source.GetEnumerator();

                if (this.Count < Take*0.8)
                {
                    readTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        int c = 0;
                        while (c < take)
                        {
                            if (read.MoveNext())
                            {
                                this.Push(read.Current);
                            }
                            else
                            {
                                Completed = true;
                                break;
                            }
                            c++;
                        }
                    });
                }
            }
        }
    }
}
