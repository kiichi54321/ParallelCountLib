using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCount
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

        public delegate void AddCountDelegate(string hash, string value, DataType count);
        public AddCountDelegate AddCount;
        public Func<string, string> GroupByKeyFunc { get; set; }
    }


    public class ReadDataEventArgs<ReadData1,DataType>:EventArgs 
        where DataType:ICountDataStruct<DataType>
        where ReadData1:BaseReadData<DataType>
    {
        public ReadData1 ReadData { get; set; }
    }

    public class ReportEventArgs:EventArgs
    {
        public string Message { get; set; }
    }
}
