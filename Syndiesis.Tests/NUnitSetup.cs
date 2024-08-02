using System.Diagnostics;

namespace Syndiesis.Tests;

[SetUpFixture]
public class NUnitSetup
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        Trace.Flush();
    }
}
