namespace RandomizerTests.Unit;

using Randomizer;
using Randomizer.ApiControllers;

[TestClass]
public sealed class GenerationLimitsTest
{
    [TestMethod]
    public async Task Limiter_BoundsConcurrentGenerations()
    {
        var limiter = new GenerationLimiter(new GenerationLimitsOptions
        {
            MaxConcurrency = 1,
            TimeoutSeconds = 30,
        });

        var first = await limiter.EnterAsync(CancellationToken.None);
        var waiting = limiter.EnterAsync(CancellationToken.None).AsTask();

        Assert.IsFalse(waiting.IsCompleted);

        first.Dispose();
        using var second = await waiting.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void GenerationContext_ExposesCancellationToLongRunningWork()
    {
        using var cancellationSource = new CancellationTokenSource();
        using var scope = GenerationContext.Begin(cancellationSource.Token);

        cancellationSource.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(
            GenerationContext.ThrowIfCancellationRequested);
    }
}
