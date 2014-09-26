using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class BaseReadData<DataType>where DataType:ICountDataStruct<DataType>
    {
        public BaseReadData()
        {
            ReadLines = new List<string>();
        }
        List<string> readLines = new List<string>();

        public List<string> ReadLines
        {
            get { return readLines; }
            set { readLines = value; }
        }
         
        public string Key
        {
            get;
            set;
        }

        public Action<string, string, DataType> AddCountAction { get; set; }

        protected void OnAddCount(string hash,string value,DataType count)
        {
            if (AddCountAction != null)
            {
                AddCountAction(hash, value, count);
            }
            else
            {
                throw new Exception("AddCountActionの登録がありません");
            }
        }

        public virtual string GetGroupByKey(string line)
        {
            return null;
        }
        

        public virtual void ReadLinesAction()
        {

        }
    }




    public class ReportEventArgs:EventArgs
    {
        public string Message { get; set; }
    }
}
