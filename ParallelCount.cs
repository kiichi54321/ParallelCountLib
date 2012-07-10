using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class ParallelCount<CountData, ReadData>
        where CountData : ICountDataStruct<CountData>, new()
        where ReadData : BaseReadData<CountData>, new()
    {

        private int threadNum = 6;

        public int ThreadNum
        {
            get { return threadNum; }
            set { threadNum = value; }
        }

        public Action<ReadData> ReadLineAction { get; set; }

        public Action<string> ReportAction { get; set; }

        protected void OnReport(string message)
        {
            if (ReportAction != null)
            {
                ReportAction(message);
            }
        }

        public string BaseFolder { get; set; }


        public void Run(string resultFile, IEnumerable<string> sourceFiles)
        {
            System.Collections.Concurrent.ConcurrentStack<string> stack = new System.Collections.Concurrent.ConcurrentStack<string>();
            stack.PushRange(sourceFiles.ToArray());
            HashNameManage hnm = new HashNameManage();
            List<System.Threading.Tasks.Task> taskList = new List<System.Threading.Tasks.Task>();
            List<DataStockManage<CountData, ReadData>> dataStockMagageList = new List<DataStockManage<CountData, ReadData>>();
            for (int i = 0; i < ThreadNum; i++)
            {
                var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    string n;

                    DataStockManage<CountData, ReadData> dsManage = new DataStockManage<CountData, ReadData>() { BaseFolder = BaseFolder, StockType = StockType.Memory, BaseFileName = "Thread" + i.ToString() , HashNameManage = hnm};
                    dataStockMagageList.Add(dsManage);
                    if (ReadLineAction != null)
                    {
                        dsManage.ReadLineAction = ReadLineAction;
                    }

                    while (true)
                    {
                        if (stack.TryPop(out n) == false)
                        {
                            break;
                        }

                        OnReport(n + "\tstart");

                        DateTime start = DateTime.Now;
                        dsManage.FileRead(n);

                        string str = n + "\t" + (DateTime.Now - start).TotalMinutes.ToString();// +"\t" + System.GC.GetTotalMemory(false).ToString();

                        OnReport(str);
                    }
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

                taskList.Add(task);
            }
            System.Threading.Tasks.Task.WaitAll(taskList.ToArray());
            OnReport("Start Reduce");

            DataStockManage<CountData, ReadData>.Reduce(resultFile, dataStockMagageList);
        }




    }
}
