using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class ParallelCountForFile<CountData, ReadData>:IDisposable
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

        List<System.Threading.Tasks.Task> taskList = new List<System.Threading.Tasks.Task>();
        HashNameManage hnm = new HashNameManage();
        List<DataStockManage<CountData, ReadData>> dataStockMagageList = new List<DataStockManage<CountData, ReadData>>();

        public void Run(string resultFile, IEnumerable<string> sourceFiles)
        {
            OnReport("----------------- "+resultFile+"開始 ----------------------");
            System.Collections.Concurrent.ConcurrentStack<string> stack = new System.Collections.Concurrent.ConcurrentStack<string>();
            stack.PushRange(sourceFiles.ToArray());
            for (int i = 0; i < ThreadNum; i++)
            {
                var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    string n;

                    DataStockManage<CountData, ReadData> dsManage = new DataStockManage<CountData, ReadData>() { BaseFolder = BaseFolder, StockType = StockType.Memory, BaseFileName = "Thread" + i.ToString() , HashNameManage = hnm};
                    if (dsManage != null)
                    {
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
                    }
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

                if(task !=null) taskList.Add(task);
            }
            System.Threading.Tasks.Task.WaitAll(taskList.ToArray());
            OnReport("Start Reduce");

            DataStockManage<CountData, ReadData>.Reduce(resultFile, dataStockMagageList);
        }





        public void Dispose()
        {
            hnm.Dispose();
            foreach (var item in dataStockMagageList)
            {
                item.Dispose();
            }
            dataStockMagageList.Clear();
            foreach (var item in taskList)
            {
                item.Dispose();
            }
            taskList.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
