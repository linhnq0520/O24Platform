namespace O24OpenAPI.Core.ComponentModel;

/// <summary>
/// /// The reader write lock disposable class
/// </summary>
/// <seealso cref="IDisposable"/>
public class ReaderWriteLockDisposable : IDisposable
{
    /// <summary>
    /// The disposed
    /// </summary>
    private bool _disposed = false;

    /// <summary>
    /// The rw lock
    /// </summary>
    private readonly ReaderWriterLockSlim _rwLock;

    /// <summary>
    /// The reader write lock type
    /// </summary>
    private readonly ReaderWriteLockType _readerWriteLockType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderWriteLockDisposable"/> class
    /// </summary>
    /// <param name="rwLock">The rw lock</param>
    /// <param name="readerWriteLockType">The reader write lock type</param>
    public ReaderWriteLockDisposable(
        ReaderWriterLockSlim rwLock,
        ReaderWriteLockType readerWriteLockType = ReaderWriteLockType.Write
    )
    {
        this._rwLock = rwLock;
        this._readerWriteLockType = readerWriteLockType;
        switch (this._readerWriteLockType)
        {
            case ReaderWriteLockType.Read:
                this._rwLock.EnterReadLock();
                break;
            case ReaderWriteLockType.Write:
                this._rwLock.EnterWriteLock();
                break;
            case ReaderWriteLockType.UpgradeableRead:
                this._rwLock.EnterUpgradeableReadLock();
                break;
        }
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the disposing
    /// </summary>
    /// <param name="disposing">The disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed)
        {
            return;
        }

        if (disposing)
        {
            switch (this._readerWriteLockType)
            {
                case ReaderWriteLockType.Read:
                    this._rwLock.ExitReadLock();
                    break;
                case ReaderWriteLockType.Write:
                    this._rwLock.ExitWriteLock();
                    break;
                case ReaderWriteLockType.UpgradeableRead:
                    this._rwLock.ExitUpgradeableReadLock();
                    break;
            }
        }
        this._disposed = true;
    }
}
