using DotDoc.Core;
using Xunit.Abstractions;

namespace DotDoc.Tests.Helper;

public class XUnitLogger: ILogger
{
    private readonly ITestOutputHelper _output;

    public XUnitLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Trace(string message)
    {
        _output?.WriteLine($"Trace: {message}");
    }

    public void Info(string message)
    {
        _output?.WriteLine($"Info: {message}");
    }

    public void Warn(string message)
    {
        _output?.WriteLine($"Warn: {message}");
    }

    public void Error(string message)
    {
        _output?.WriteLine($"Error: {message}");
    }
}