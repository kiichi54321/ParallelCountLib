ParallelCountLib

C#で作成した並列化したカウントクラスです。
MapReduceアルゴリズムを元ネタにしています。
Core i7 で、4GBのデータの組み合わせカウントを20分程度で終わらせることができました。
Ramドライブをつかったら、もっと早くなるのでしょうね。

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

ParallelCountの起動
	今まで作ったReadData、CountDataを設定して　ParallelCount<CountData, ReadData>をNewする。
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




