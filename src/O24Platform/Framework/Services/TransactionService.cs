namespace O24OpenAPI.Framework.Services;

// public class TransactionService : ITransactionService
// {
//     private readonly
//
//         IRepository<Transaction> _transactionRepository;
//
//     private readonly IRepository<TransactionDone> _transactionDoneRepository;
//     private readonly IRepository<TransactionDetails> _detailRepository;
//     private readonly IRepository<TransactionDetailsDone> _detailDoneRepository;
//     private readonly ITransactionDetailsService _detailService;
//     private readonly IWorkContext _workContext;
//     private readonly IEntityFieldService _fieldService;
//
//     /// <summary>
//     ///
//     /// </summary>
//     /// <param name="transactionRepository"></param>
//     /// <param name="detailRepository"></param>
//     /// <param name="detailService"></param>
//     /// <param name="workContext"></param>
//     /// <param name="transactionDoneRepository"></param>
//     /// <param name="detailDoneRepository"></param>
//     /// <param name="fieldService"></param>
//     public TransactionService(
//         IRepository<Transaction> transactionRepository,
//         IRepository<TransactionDetails> detailRepository,
//         ITransactionDetailsService detailService,
//         IWorkContext workContext,
//         IRepository<TransactionDone> transactionDoneRepository,
//         IRepository<TransactionDetailsDone> detailDoneRepository,
//         IEntityFieldService fieldService)
//     {
//         this._transactionRepository = transactionRepository;
//         this._detailRepository = detailRepository;
//         this._detailService = detailService;
//         this._workContext = workContext;
//         this._fieldService = fieldService;
//         this._transactionDoneRepository = transactionDoneRepository;
//         this._detailDoneRepository = detailDoneRepository;
//     }
//
//     /// <summary>Gets a transaction by identifier</summary>
//     /// <param name="id"></param>
//     /// <returns></returns>
//     public virtual async Task<Transaction> GetById(int id)
//     {
//         Transaction byId = await this._transactionRepository.GetById(new int?(id),
//             (Func<IStaticCacheManager, CacheKey>)(cache => (CacheKey)null));
//         return byId;
//     }
//
//     /// <summary>Gets a transaction by reference identifier</summary>
//     /// <param name="refId"></param>
//     /// <returns></returns>
//     public virtual async Task<Transaction> GetByRefId(string refId)
//     {
//         IQueryable<Transaction> query =
//             this._transactionRepository.Table.Where<Transaction>(
//                 (Expression<Func<Transaction, bool>>)(p => p.RefId == refId));
//         Transaction transaction = await query.FirstOrDefaultAsync<Transaction>();
//         if (transaction == null)
//         {
//             query = (IQueryable<Transaction>)this._transactionDoneRepository.Table.Where<TransactionDone>(
//                 (Expression<Func<TransactionDone, bool>>)(p => p.RefId == refId));
//             transaction = await query.FirstOrDefaultAsync<Transaction>();
//         }
//
//         Transaction byRefId = transaction;
//         query = (IQueryable<Transaction>)null;
//         transaction = (Transaction)null;
//         return byRefId;
//     }
//
//     /// <summary>Gets a transaction by transaction number</summary>
//     /// <param name="transactionNumber"></param>
//     /// <returns></returns>
//     public virtual async Task<Transaction> GetByTransactionNumber(string transactionNumber)
//     {
//         IQueryable<Transaction> combinedQuery = this._transactionRepository.Table
//             .Where<Transaction>((Expression<Func<Transaction, bool>>)(p => p.TransactionNumber == transactionNumber))
//             .Union<Transaction>((IEnumerable<Transaction>)this._transactionDoneRepository.Table.Where<TransactionDone>(
//                 (Expression<Func<TransactionDone, bool>>)(q => q.TransactionNumber == transactionNumber)));
//         Transaction transaction = await combinedQuery.FirstOrDefaultAsync<Transaction>();
//         Transaction transactionNumber1 = transaction;
//         combinedQuery = (IQueryable<Transaction>)null;
//         transaction = (Transaction)null;
//         return transactionNumber1;
//     }
//
//     /// <summary>
//     ///
//     /// </summary>
//     /// <returns></returns>
//     public virtual async Task<int> RemoveMissingTransactionNumber()
//     {
//         DateTime dt = DateTime.UtcNow.AddHours(-1.0);
//         List<string> refIds = await this._transactionRepository.Table
//             .Where<Transaction>((Expression<Func<Transaction, bool>>)(t =>
//                 (t.TransactionNumber == default(string) || t.TransactionNumber == "") && t.ServiceSysDate < dt))
//             .Select<Transaction, string>((Expression<Func<Transaction, string>>)(t => t.RefId)).ToListAsync<string>();
//         foreach (string str in refIds)
//         {
//             string refId = str;
//             int num1 = await this._detailRepository.DeleteWhere(
//                 (Expression<Func<TransactionDetails, bool>>)(d => d.RefId == refId));
//             int num2 = await this._transactionRepository.DeleteWhere(
//                 (Expression<Func<Transaction, bool>>)(t => t.RefId == refId));
//         }
//
//         int num = 1;
//         refIds = (List<string>)null;
//         return num;
//     }
//
//     /// <summary>List transation that not have transaction number</summary>
//     /// <returns></returns>
//     public virtual async Task<List<Transaction>> ListMissingTransactionNumber()
//     {
//         IQueryable<Transaction> query = this._transactionRepository.Table
//             .Where<Transaction>((Expression<Func<Transaction, bool>>)(p =>
//                 p.TransactionNumber == default(string) || p.TransactionNumber == "")).Union<Transaction>(
//                 (IEnumerable<Transaction>)this._transactionDoneRepository.Table.Where<TransactionDone>(
//                     (Expression<Func<TransactionDone, bool>>)(q =>
//                         q.TransactionNumber == default(string) || q.TransactionNumber == "")));
//         List<Transaction> transactions = await query.ToListAsync<Transaction>();
//         List<Transaction> transactionList = transactions;
//         query = (IQueryable<Transaction>)null;
//         transactions = (List<Transaction>)null;
//         return transactionList;
//     }
//
//     /// <summary>Insert a transaction</summary>
//     /// <param name="transaction"></param>
//     /// <returns></returns>
//     public virtual async Task Insert(Transaction transaction)
//     {
//         transaction.ServiceSysDate = DateTime.UtcNow;
//         transaction.StartTime = CommonHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
//         transaction.Duration = long.MaxValue;
//         await transaction.Insert();
//     }
//
//     /// <summary>Reverse a transaction by referenceId</summary>
//     /// <param name="refId"></param>
//     /// <param name="removeInsert"></param>
//     /// <returns></returns>
//     public virtual async Task Reverse(string refId, bool removeInsert = false)
//     {
//         IList<TransactionDetails> details = await this._detailService.ListByRefId(refId);
//         bool forceReverse = await this._workContext.GetStatusReverse();
//         await this._detailService.Reverse(details, forceReverse, removeInsert);
//         IQueryable<Transaction> query =
//             this._transactionRepository.Table.Where<Transaction>(
//                 (Expression<Func<Transaction, bool>>)(p => p.RefId == refId));
//         Transaction transaction = await query.FirstOrDefaultAsync<Transaction>();
//         if (transaction == null)
//         {
//             details = (IList<TransactionDetails>)null;
//             query = (IQueryable<Transaction>)null;
//             transaction = (Transaction)null;
//         }
//         else
//         {
//             transaction.Status = forceReverse ? "N" : "R";
//             await transaction.Update();
//             details = (IList<TransactionDetails>)null;
//             query = (IQueryable<Transaction>)null;
//             transaction = (Transaction)null;
//         }
//     }
//
//     /// <summary>Reverse a transaction by referecne id</summary>
//     /// <returns></returns>
//     public virtual async Task Reverse()
//     {
//         string referenceId = await this._workContext.GetCurrentRefId();
//         if (string.IsNullOrEmpty(referenceId))
//             throw new NeptuneException("Cannot get the ReferenceId for reversal");
//         await this.Reverse(referenceId, false);
//         referenceId = (string)null;
//     }
//
//     public virtual async Task<PagedListModel<Transaction, TransactionAuditModel>> ListAuditTransactions<T>(
//         T entity,
//         int pageIndex = 0,
//         int pageSize = 2147483647,
//         bool showCompleteOnly = true)
//         where T : BaseEntity
//     {
//         IQueryable<Transaction> queryTransaction = this._transactionRepository.Table
//             .Where<Transaction>((Expression<Func<Transaction, bool>>)(t => t.Status == "C")).Union<Transaction>(
//                 (IEnumerable<Transaction>)this._transactionDoneRepository.Table.Where<TransactionDone>(
//                     (Expression<Func<TransactionDone, bool>>)(t => t.Status == "C")));
//         if (showCompleteOnly)
//             queryTransaction =
//                 queryTransaction.Where<Transaction>((Expression<Func<Transaction, bool>>)(t => t.Status == "C"));
//         IQueryable<TransactionDetails> queryDetail = this._detailRepository.Table
//             .Where<TransactionDetails>((Expression<Func<TransactionDetails, bool>>)(d =>
//                 d.Entity == typeof(T).Name && d.EntityId == entity.Id)).Union<TransactionDetails>(
//                 (IEnumerable<TransactionDetails>)this._detailDoneRepository.Table.Where<TransactionDetailsDone>(
//                     (Expression<Func<TransactionDetailsDone, bool>>)(d =>
//                         d.Entity == typeof(T).Name && d.EntityId == entity.Id)));
//         ParameterExpression parameterExpression1;
//         ParameterExpression parameterExpression2;
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         // ISSUE: method reference
//         IQueryable<Transaction> query = queryTransaction.Join<Transaction, TransactionDetails, string, Transaction>(
//             (IEnumerable<TransactionDetails>)queryDetail, (Expression<Func<Transaction, string>>)(t => t.RefId),
//             (Expression<Func<TransactionDetails, string>>)(d => d.RefId),
//             Expression.Lambda<Func<Transaction, TransactionDetails, Transaction>>(
//                 (Expression)Expression.MemberInit(Expression.New(typeof(Transaction)),
//                     (MemberBinding)Expression.Bind(
//                         (MethodInfo)MethodBase.GetMethodFromHandle(
//                             __methodref(Transaction.set_TransactionCode)),)))); // Unable to render the statement
//         query = (IQueryable<Transaction>)query.OrderByDescending<Transaction, DateTime>(
//             (Expression<Func<Transaction, DateTime>>)(t => t.TransactionDate));
//         query = query.Distinct<Transaction>();
//         Language lang = await this._workContext.GetWorkingLanguage();
//         IPagedList<Transaction> transactions = await query.ToPagedList<Transaction>(pageIndex, pageSize);
//         PagedListModel<Transaction, TransactionAuditModel> audits =
//             transactions.ToPagedListModel<Transaction, TransactionAuditModel>();
//         foreach (TransactionAuditModel audit in audits.Items)
//         {
//             audit.TransactionDate = audit.TransactionDateTime.ToString("o") + "Z";
//             if (audit.IsReverse)
//                 audit.Status = "R";
//             string str = audit.Status;
//             switch (str)
//             {
//                 case "N":
//                     audit.Status = "New";
//                     break;
//                 case "C":
//                     audit.Status = "Completed";
//                     break;
//                 case "R":
//                     audit.Status = "Reversed";
//                     break;
//                 case "E":
//                     audit.Status = "Error";
//                     break;
//             }
//
//             str = (string)null;
//             IList<TransactionDetails> details =
//                 await this._detailService.ListByRefIdAndEntity(audit.RefId, typeof(T).Name);
//             audit.Details = details.Select<TransactionDetails, TransactionDetailAuditModel>(
//                 (Func<TransactionDetails, TransactionDetailAuditModel>)(d =>
//                 {
//                     TransactionDetailAuditModel model = d.ToModel<TransactionDetailAuditModel>();
//                     switch (model.UpdateType)
//                     {
//                         case "I":
//                             model.UpdateType = "New";
//                             break;
//                         case "A":
//                             model.UpdateType = "Adjust";
//                             break;
//                         case "C":
//                             model.UpdateType = "Credit";
//                             break;
//                         case "D":
//                             model.UpdateType = "Debit";
//                             break;
//                         case "R":
//                             model.UpdateType = "Reversed";
//                             break;
//                     }
//
//                     model.FieldName = this._fieldService
//                         .GetByEntityField(model.Entity, model.FieldName, lang.UniqueSeoCode).Result;
//                     return model;
//                 })).ToList<TransactionDetailAuditModel>();
//             details = (IList<TransactionDetails>)null;
//         }
//
//         PagedListModel<Transaction, TransactionAuditModel> pagedListModel = audits;
//         queryTransaction = (IQueryable<Transaction>)null;
//         queryDetail = (IQueryable<TransactionDetails>)null;
//         query = (IQueryable<Transaction>)null;
//         transactions = (IPagedList<Transaction>)null;
//         audits = (PagedListModel<Transaction, TransactionAuditModel>)null;
//         return pagedListModel;
//     }
// }
