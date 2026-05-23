using O24OpenAPI.Core.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The transaction service interface
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Gets the by id using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>A task containing the transaction</returns>
    Task<Transaction> GetById(int id);

    /// <summary>
    /// Gets the by ref id using the specified ref id
    /// </summary>
    /// <param name="refId">The ref id</param>
    /// <returns>A task containing the transaction</returns>
    Task<Transaction> GetByRefId(string refId);

    /// <summary>
    /// Gets the by transaction number using the specified transaction number
    /// </summary>
    /// <param name="transactionNumber">The transaction number</param>
    /// <returns>A task containing the transaction</returns>
    Task<Transaction> GetByTransactionNumber(string transactionNumber);

    /// <summary>
    /// Removes the missing transaction number
    /// </summary>
    /// <returns>A task containing the int</returns>
    Task<int> RemoveMissingTransactionNumber();

    /// <summary>
    /// Lists the missing transaction number
    /// </summary>
    /// <returns>A task containing a list of transaction</returns>
    Task<List<Transaction>> ListMissingTransactionNumber();

    /// <summary>
    /// Inserts the transaction
    /// </summary>
    /// <param name="transaction">The transaction</param>
    Task Insert(Transaction transaction);

    /// <summary>
    /// Reverses the ref id
    /// </summary>
    /// <param name="refId">The ref id</param>
    /// <param name="removeInsert">The remove insert</param>
    Task Reverse(string refId, bool removeInsert = false);

    /// <summary>
    /// Reverses this instance
    /// </summary>
    Task Reverse();

    /// <summary>
    /// Lists the audit transactions using the specified entity
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="pageIndex">The page index</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="showCompleteOnly">The show complete only</param>
    /// <returns>A task containing a paged list model of transaction and transaction audit model</returns>
    Task<PagedListModel<Transaction, TransactionAuditModel>> ListAuditTransactions<T>(
        T entity,
        int pageIndex = 0,
        int pageSize = 2147483647,
        bool showCompleteOnly = true
    )
        where T : BaseEntity;
}
