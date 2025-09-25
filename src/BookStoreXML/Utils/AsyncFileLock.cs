namespace BookStoreXML.Utils
{
    public sealed class AsyncFileLock
    {
        private readonly SemaphoreSlim _sem = new(1, 1);
        public async Task<IDisposable> AcquireWriteAsync(CancellationToken ct = default)
        {
            await _sem.WaitAsync(ct).ConfigureAwait(false);
            return new Releaser(_sem);
        }

        private sealed class Releaser : IDisposable
        {
            private SemaphoreSlim? _sem;
            public Releaser(SemaphoreSlim sem) => _sem = sem;
            public void Dispose() => Interlocked.Exchange(ref _sem, null)?.Release();
        }
    }
}
