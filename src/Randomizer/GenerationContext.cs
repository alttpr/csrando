namespace Randomizer;

using System.Threading;

/// <summary>
/// Carries the lifetime of the current generation through the synchronous generator.
/// Generator code is deliberately CPU-bound, so cancellation must be observed at
/// cooperative checkpoints rather than only at the HTTP request boundary.
/// </summary>
internal static class GenerationContext
{
    private sealed record State(CancellationToken CancellationToken);

    private static readonly AsyncLocal<State?> Current = new();

    public static CancellationToken CancellationToken =>
        Current.Value?.CancellationToken ?? System.Threading.CancellationToken.None;

    public static IDisposable Begin(CancellationToken cancellationToken)
    {
        var previous = Current.Value;
        Current.Value = new State(cancellationToken);
        return new Scope(previous);
    }

    public static void ThrowIfCancellationRequested() =>
        CancellationToken.ThrowIfCancellationRequested();

    private sealed class Scope(State? previous) : IDisposable
    {
        private State? _previous = previous;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            Current.Value = _previous;
            _previous = null;
            _disposed = true;
        }
    }
}
