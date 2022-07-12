namespace SampleLib
{
    /// <summary>
    /// パブリックなクラスです。
    /// さらに改行があるサマリーです。
    /// 
    /// 複数の改行を入れます。
    /// </summary>
    /// <remarks>
    /// パブリックなクラスのremarksです。
    /// </remarks>
    public class PublicClass
    {

        /// <summary>
        /// デフォルトコンストラクタを定義しています。
        /// </summary>
        public PublicClass()
        {
        }

        /// <summary>
        /// 引数付きのコンストラクタです。
        /// </summary>
        /// <param name="id">IDです。</param>
        public PublicClass(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// パブリックフィールドです。
        /// </summary>
        public string Id;

        /// <summary>
        /// Getだけのプロパティです。
        /// </summary>
        public string GetOnly { get; }

        /// <summary>
        /// Setできるプロパティです。
        /// </summary>
        public int GetSet { get; set; }

        /// <summary>
        /// Initなプロパティです。
        /// </summary>
        public string GetInit { get; init; }

        /// <summary>
        /// Setだけのプロパティです。
        /// </summary>
        public string SetOnly { 
            set { }
        }

        /// <summary>
        /// Initだけのプロパティです。
        /// </summary>
        public string InitOnly
        {
            init { }
        }


    }
}