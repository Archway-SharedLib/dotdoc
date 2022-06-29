namespace SampleLib
{
    /// <summary>
    /// パブリックなクラスです。
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
        public string Name { get; }

        /// <summary>
        /// Setできるプロパティです。
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Initなプロパティです。
        /// </summary>
        public string Address { get; init; }

    }
}