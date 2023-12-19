using System.Collections.Generic;
using Seq.Apps;
using Serilog;

namespace Seq.App.SendGrid.Tests.Support;

/// <summary>
/// Class TestAppHost.
/// Implements the <see cref="IAppHost" />
/// </summary>
/// <seealso cref="IAppHost" />
internal class TestAppHost : IAppHost
{
    /// <summary>
    /// The app being run.
    /// </summary>
    /// <value>The application.</value>
    public Apps.App App { get; } = new Apps.App("appinstance-0", "Test", new Dictionary<string, string>(), "./storage");

    /// <summary>
    /// The host running the app.
    /// </summary>
    /// <value>The host.</value>
    public Host Host { get; } = Error.Host();

    /// <summary>
    /// A logger through which the app can raise diagnostic events.
    /// </summary>
    /// <value>The logger.</value>
    public ILogger Logger { get; } = new LoggerConfiguration().CreateLogger();

    /// <summary>
    /// A folder in which the app may store data.
    /// </summary>
    /// <value>The storage path.</value>
    public string StoragePath { get; } = "./storage";
}