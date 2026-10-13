namespace Randomizer.ApiControllers;

/// <summary>Resource limits for CPU-bound API generation requests.</summary>
public sealed class GenerationLimitsOptions
{
    public const string SectionName = "GenerationLimits";

    /// <summary>A request-wide limit, including time spent waiting and all retries.</summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// A deliberately small default prevents concurrent generators from competing for
    /// every core and causing memory/CPU thrashing. Override with
    /// GenerationLimits__MaxConcurrency when the host has measured spare capacity.
    /// </summary>
    public int MaxConcurrency { get; set; } = 2;
}

/// <summary>Limits how many CPU-bound generations may execute at the same time.</summary>
public sealed class GenerationLimiter
{
    private readonly SemaphoreSlim _slots;

    public TimeSpan Timeout { get; }

    public GenerationLimiter(GenerationLimitsOptions options)
    {
        if (options.TimeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.TimeoutSeconds), "Generation timeout must be positive.");
        }

        if (options.MaxConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.MaxConcurrency), "Generation concurrency must be positive.");
        }

        Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        _slots = new SemaphoreSlim(options.MaxConcurrency, options.MaxConcurrency);
    }

    public async ValueTask<IDisposable> EnterAsync(CancellationToken cancellationToken)
    {
        await _slots.WaitAsync(cancellationToken);
        return new Lease(_slots);
    }

    private sealed class Lease(SemaphoreSlim slots) : IDisposable
    {
        private SemaphoreSlim? _slots = slots;

        public void Dispose() => Interlocked.Exchange(ref _slots, null)?.Release();
    }
}
