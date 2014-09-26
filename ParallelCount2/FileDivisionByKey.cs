using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public class FileDivisionByKey
    {
        public string Folder { get; set; }
        public string FileNameHeader { get; set; }

        public Func<string, string> GetKeyFunc { get; set; }
        public Func<string, string> GetSubKeyFunc { get; set; }
        public Func<string, string> GetHashFunc { get; set; }
        public Action<string> Report { get; set; }

        protected void OnReport(string message)
        {
            if (Report != null)
            {
                Report(message);
            }
        }

        public void Run(string sourceFile)
        {
            Run(new List<string> { sourceFile });
        }

        public void Run(IEnumerable<string> sourceFilesName)
        {
            Dictionary<string, System.IO.StreamWriter> dic = new Dictionary<string, System.IO.StreamWriter>();
            List<string> fileList = new List<string>();
            OnReport("ファイル分割開始");

            if (System.IO.Directory.Exists(Folder) == false)
            {
                System.IO.Directory.CreateDirectory(Folder);
            }

            foreach (var item in sourceFilesName)
            {
                OnReport(item + "解析中");
                foreach (var line in System.IO.File.ReadLines(item))
                {
                    string key = GetHashFunc(line);
                    System.IO.StreamWriter sw;
                    if (dic.TryGetValue(key, out sw) == false)
                    {
                        string file = Folder + "/" + FileNameHeader + "_" + key + ".txt";
                        sw = System.IO.File.CreateText(file);
                        dic.Add(key, sw);
                        fileList.Add(file);
                    }
                    sw.WriteLine(line);
                }
            }

            foreach (var item in dic)
            {
                item.Value.Close();
            }
            OnReport("ソート処理開始");
            if (GetKeyFunc != null)
            {
                foreach (var item in fileList)
                {
                    OnReport(item + ":開始");
                    using (var file = System.IO.File.CreateText(item.Replace(".txt", "_sorted.txt")))
                    {
                        if (GetSubKeyFunc != null)
                        {
                            foreach (var line in System.IO.File.ReadLines(item).OrderBy(GetKeyFunc).ThenBy(GetSubKeyFunc))
                            {
                                file.WriteLine(line);
                            }
                        }
                        else
                        {
                            foreach (var line in System.IO.File.ReadLines(item).OrderBy(GetKeyFunc))
                            {
                                file.WriteLine(line);
                            }

                        }
                    }
                    System.IO.File.Delete(item);
                    OnReport(item + ":終了");
                }
            }

        }

    }


    class SampleClass
    {
        void Sample()
        {
            FileDivisionByKey file = new FileDivisionByKey();
            file.GetHashFunc = (n) =>
            {
                var s = n.Split(',').First();
                long value;
                if (long.TryParse(s, out value))
                {
                    var a = long.Parse(s) / 120;
                    return a.ToString();
                }
                return "-1";
            };
            file.GetKeyFunc = (n) =>
                {
                    return n.Split(',').FirstOrDefault();
                };
            file.GetSubKeyFunc = (n) =>
                {
                    return n.Split(',').ElementAtOrDefault(1);
                };
            file.FileNameHeader = "Division";
            file.Folder = "Data";
            file.Run("DataSource.txt");
        }
    }
}
