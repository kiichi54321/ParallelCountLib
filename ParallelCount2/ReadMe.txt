ParallelCountLib

C#で作成した並列化したカウントクラスです。
MapReduceアルゴリズムを元ネタにしています。
大体、シングルスレッド時と比べて、3分の1から4分の１のスピードが出るようです。
とりあえず、でかいファイルだし、並列して集計したいわ、というのが目的です。
わざわざ分散させて集計するなんていうのは、面倒という人がターゲットですね。

小規模ファイル用のやり方（簡単）

簡単に一つのファイルを並列処理して集計したい用です。
一応、逐次読み込みで、一度にすべてのファイルをメモリに読み込む、なんていうことはしません。



大規模ファイル用のやり方（ややこしい）

Core i7 で、4GBのデータの組み合わせカウントをシングルスレッドでは65分のところを、20分程度で終わらせることができました。
3分の1程度の速度UPでしょうか。ファイルIOに関しての最適化が全くしていないからでしょう。
データをRamドライブにおくと、ファイルIOがシビアじゃなくなるので、もっと早くなると思うのですが。

使い方

分割ファイルの作成
	FileDivisionByKeyクラスで巨大ファイルを分割。通常のトランザクションデータなら、ユーザIDをKeyとしてソートする。

数え方を定義（通常はIntCountを使えばいいのでスルーしていい）
	ICountDataStruct<T>を継承したクラスを作る。
	通常のただ数えるだけならIntCountを使えばいいのでスルーしていい
	同じKeyのものがあった時の処理を定義している。MapReduceにおいてはReduce処理に相当。
	定義次第では、ベクトルとかも扱える。まだ作っていないけど。

ReadDataクラスの作成
	BaseReadDataクラスを継承した、データを読み込むためのクラスを作る。
	GroupByKeyFuncを設定して、まとめるKeyを設定する。
	その中で、ReadLinesAction()をオーバーライドして、蓄積されたReadLinesからOnAddCountを叩いて数え上げるデータを生成する。

ParallelCountForFileの起動
	今まで作ったReadData、CountDataを設定して　ParallelCountForFile<CountData, ReadData>をNewする。
	作成するスレッド数は初期設定で６です。CPUやメモリーなどをかんがみて設定してください。
	CPUのコア数以上にスレッドを設定しても遅くなるし、スレッドを増やすとメモリー使用量も増えます。
	Runで、分割ファイルを最終出力ファイルを設定して、実行開始。待つ。
	結果はタブ区切りのKeyValueデータで返ってきます。


ファイル分割のサンプル
void Sample()
{
　	//カンマ区切りのデータであること前提
	FileDivisionByKey file = new FileDivisionByKey();
	//分割先の設定
	file.GetHashFunc = (n) =>
	{
		//初めの列を使い、それがLong型なので変換して、それを120の余りをハッシュに設定。
		var s = n.Split(',').First();
		long value;
		if (long.TryParse(s, out value))
		{
			var a = long.Parse(s) % 120;
			return a.ToString();
		}
		return "-1";
	};
	//初めの列を一番目のソートキーにする
	file.GetKeyFunc = (n) =>
		{
			return n.Split(',').FirstOrDefault();
		};
	//二番目の列を２番目のソートキーにする
	file.GetSubKeyFunc = (n) =>
		{
			return n.Split(',').ElementAtOrDefault(1);
		};
	file.FileNameHeader = "Division";
	file.Folder = "Data";
	file.Run("DataSource.txt");
	//これで、データを120個に分割し、それぞれがソートされたデータが作られる。
	//重たい処理なので、実行時はスレッドにするといいと思う。
}

ReadDataクラスのサンプル
public class SampleReadData : BaseReadData<IntCount>
{
	public override string GetGroupByKey(string line)
	{
		//カンマ区切りの初めの列をキーとして設定。
		return line.Split(',').FirstOrDefault();
	}

	public override void ReadLinesAction()
	{
		//カンマ区切りの3番目の列を集計に使用。
		List<string> list = new List<string>();
		foreach (var item in this.ReadLines)
		{
			var data = item.Split(',').ElementAtOrDefault(2);
			if (data != null) list.Add(data);
		}

		list = list.Distinct().OrderBy(n=>n).ToList();

		//組み合わせ生成
		for (int i = 0; i < list.Count - 1; i++)
		{
			for (int l = i + 1; l < list.Count; l++)
			{
				//カウントするのをOnAddCountで設定。
				this.OnAddCount(list[i], list[i] + "_" + list[l], IntCount.Default);
			}
		}

		foreach (var item in list)
		{
			this.OnAddCount(item, item, IntCount.Default);
		}
	}
}

実行例

void Run()
{
	var files = System.IO.Directory.GetFiles("Data").Where(n => n.Contains("sorted"));
	using (ParallelCountForFile<IntCount,SampleReadData> paraCount = new ParallelCountForFile<IntCount, SampleReadData>())
	{
		paraCount.ThreadNum = 4;
		paraCount.ReportAction = (n) => { System.Console.WriteLine(n); };
		paraCount.Run("result.txt", files);
	}
}

