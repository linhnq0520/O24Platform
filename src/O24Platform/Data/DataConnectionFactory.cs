using System.Data;
using LinqToDB.Data;

namespace O24OpenAPI.Data;

public interface IDataConnectionFactory : IDisposable, IAsyncDisposable
{
    DataConnection GetOrCreateConnection();

    Task<IDbConnection> GetDbConnectionAsync(CancellationToken cancellationToken);

    IDbConnection GetConnection();
    IDbTransaction? GetTransaction();

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync();
    Task RollbackAsync();

    bool HasActiveTransaction { get; }
}

public class DataConnectionFactory(IO24OpenAPIDataProvider dataProvider) : IDataConnectionFactory
{
    private readonly IO24OpenAPIDataProvider _dataProvider = dataProvider;

    private DataConnection? _connection;
    private DataConnectionTransaction? _transaction;
    private bool _disposed;

    public bool HasActiveTransaction => _transaction != null;

    #region Connection

    public DataConnection GetOrCreateConnection()
    {
        return _disposed
            ? throw new ObjectDisposedException(nameof(DataConnectionFactory))
            : (_connection ??= _dataProvider.CreateDataConnection());
    }

    public IDbConnection GetConnection()
    {
        return GetOrCreateConnection().TryGetDbConnection()!;
    }

    public IDbTransaction? GetTransaction()
    {
        return _connection?.Transaction;
    }

    public async Task<IDbConnection> GetDbConnectionAsync(CancellationToken cancellationToken)
    {
        DataConnection conn = GetOrCreateConnection();
        return await conn.OpenDbConnectionAsync(cancellationToken);
    }

    #endregion

    #region Transaction

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            return;

        DataConnection conn = GetOrCreateConnection();
        _transaction = await conn.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // auto rollback nếu quên commit
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }

            _connection?.Dispose();
            _connection = null;
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
    #endregion
}
