namespace SampleLib;

/// <summary>
/// レコードのXMLコメントでパラメーター付きです。
/// </summary>
/// <param name="A">レコードのAパラメーターです。</param>
/// <param name="B">レコードのBパラメーターです。</param>
public record PublicRecordWithArg(string A, int B)
{
    
}