using Microsoft.Extensions.Logging.Abstractions;

namespace CheckYoSelfAI.Tests.Common;

internal static class TestLogger
{
    public static Microsoft.Extensions.Logging.ILogger<T> Create<T>() => NullLogger<T>.Instance;
}
