using System.ComponentModel;
using System.Globalization;
using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

public class TransactionActionService(
    IMasterMappingService masterMappingService,
    IRepository<GLEntries> glEntriesRepository,
    IWorkContext workContext,
    IRepository<TransactionAction> transactionActionRepository
) : ITransactionActionService
{
    private readonly IMasterMappingService _masterMappingService = masterMappingService;
    private readonly IRepository<GLEntries> _glEntriesRepository = glEntriesRepository;
    private readonly IWorkContext _workContext = workContext;
    private readonly IRepository<TransactionAction> _transactionActionRepository =
        transactionActionRepository;

    /// <summary>
    /// Generate a single GL entry for the given transaction and master record.
    /// Returns the GL account that was used for posting, or an empty string if nothing was posted.
    /// </summary>
    /// <typeparam name="T">Master entity type.</typeparam>
    /// <param name="transId">Transaction ID in the GL entries table.</param>
    /// <param name="transaction">Transaction header/model (must contain TransactionNumber, ValueDate, IsReverse).</param>
    /// <param name="master">The master entity used to resolve mapping fields.</param>
    /// <param name="amount">Posting amount (base amount in the transaction currency).</param>
    /// <param name="sysAccountName">System account name (mapping key).</param>
    /// <param name="dorc">"D" (debit) or "C" (credit).</param>
    /// <param name="crossBranchCode">Cross-branch code if cross-branch posting.</param>
    /// <param name="crossCurrencyCode">Cross-currency code if cross-currency posting.</param>
    /// <param name="baseCurrencyAmount">Amount converted to base currency (for reporting/balance purposes).</param>
    /// <param name="accountingGroup">Accounting group (default: 1).</param>
    /// <returns>GL account used for the entry, or empty string if no entry created.</returns>
    public async Task<string> CreateGLEntry<T>(
        string transId,
        BaseTransactionModel transaction,
        T master,
        decimal amount,
        string sysAccountName,
        string dorc,
        string crossBranchCode,
        string crossCurrencyCode,
        decimal baseCurrencyAmount,
        int accountingGroup = 1
    )
        where T : BaseEntity
    {
        // ---- Guard clauses ------------------------------------------------------
        if (string.IsNullOrWhiteSpace(sysAccountName))
        {
            return string.Empty;
        }

        if (amount == 0m)
        {
            return string.Empty;
        }

        if (transaction is null)
        {
            return string.Empty;
        }

        if (master is null)
        {
            return string.Empty;
        }

        string masterTypeName = typeof(T).FullName;
        if (string.IsNullOrEmpty(masterTypeName))
        {
            return string.Empty;
        }

        MasterMapping mapping = await _masterMappingService.GetByMasterClass(masterTypeName);
        if (mapping is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(mapping.GLEntriesClass))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(mapping.MasterGLClass))
        {
            return string.Empty;
        }

        // ---- Build master dictionary for GL definition lookup -------------------
        Dictionary<string, string> masterFieldMap = new(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(mapping.MasterFields))
        {
            string[] pairs = mapping.MasterFields.Split(
                '#',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split(
                    ':',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );
                if (kv.Length != 2)
                {
                    continue;
                }

                string codeField = kv[0];
                string valueField = kv[1];
                string resolvedValue = master.GetMasterStringField(valueField);
                if (!string.IsNullOrWhiteSpace(codeField))
                {
                    masterFieldMap[codeField] = resolvedValue ?? string.Empty;
                }
            }
        }

        // ---- Resolve GL definition (account) ------------------------------------
        BaseMasterGL glDef = await GetGL(mapping.MasterGLClass, sysAccountName, masterFieldMap);
        if (glDef is null || string.IsNullOrWhiteSpace(glDef.GLAccount))
        {
            return string.Empty;
        }

        // ---- Prepare posting data -----------------------------------------------
        string transTableName = ResolveClassShortName(mapping.MasterTransClass);

        // await the async getters
        Task<string> branchTask = master.GetMasterRelatedStringFieldAsync(
            mapping.MasterBranchCodeField
        );
        Task<string> currencyTask = master.GetMasterRelatedStringFieldAsync(
            mapping.MasterCurrencyCodeField
        );
        await Task.WhenAll(branchTask, currencyTask);

        string branchCode = await branchTask ?? string.Empty;
        string currencyCode = await currencyTask ?? string.Empty;
        // ---- Post GL -------------------------------------------------------------
        await PostGL(
            transId: transId,
            transTableName: transTableName,
            transactionNumber: transaction.TransactionNumber,
            valueDate: transaction.ValueDate,
            sysAccountName: sysAccountName,
            glAccount: glDef.GLAccount,
            amount: amount,
            dorc: dorc,
            branchCode: branchCode,
            currencyCode: currencyCode,
            crossBranch: crossBranchCode,
            crossCurrency: crossCurrencyCode,
            baseAmount: baseCurrencyAmount,
            isReverse: transaction.IsReverse,
            accountingGroup: accountingGroup
        );

        return glDef.GLAccount;

        static string ResolveClassShortName(string fullClassName)
        {
            if (string.IsNullOrWhiteSpace(fullClassName))
            {
                return string.Empty;
            }

            string[] parts = fullClassName.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            return parts.Length == 0 ? fullClassName : parts[^1];
        }
    }

    /// <summary>
    /// Get GL definition by master GL class, system account name, and master field filters.
    /// </summary>
    /// <param name="masterGLClass"></param>
    /// <param name="sysAccountName"></param>
    /// <param name="masterFields"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private static async Task<BaseMasterGL> GetGL(
        string masterGLClass,
        string sysAccountName,
        Dictionary<string, string> masterFields
    )
    {
        // ---- Guard clauses ------------------------------------------------------
        if (string.IsNullOrWhiteSpace(masterGLClass))
        {
            throw new ArgumentException("masterGLClass is required.", nameof(masterGLClass));
        }

        if (string.IsNullOrWhiteSpace(sysAccountName))
        {
            throw new ArgumentException("sysAccountName is required.", nameof(sysAccountName));
        }

        // Avoid null refs; use case-insensitive keys if your data is config-ish
        Dictionary<string, string> searchGL = new(
            capacity: (masterFields?.Count ?? 0) + 1,
            comparer: StringComparer.OrdinalIgnoreCase
        );

        if (masterFields != null)
        {
            foreach (KeyValuePair<string, string> kv in masterFields)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key))
                {
                    searchGL[kv.Key.Trim()] = kv.Value ?? string.Empty;
                }
            }
        }

        // Always include SysAccountName
        searchGL["SysAccountName"] = sysAccountName;

        // For diagnostics: "Key - Value#Key - Value#..."
        string textSearch = string.Join('#', searchGL.Select(kv => $"{kv.Key} - {kv.Value}"));

        BaseMasterGL glDef = await DynamicRepositoryHelper.DynamicGetByFields<BaseMasterGL>(
            masterGLClass,
            searchGL
        );

        if (glDef == null)
        {
            // Keep the original message semantics
            throw new InvalidOperationException(
                $"{sysAccountName} GL is not defined for {textSearch}. Please check in {masterGLClass}"
            );
        }

        return glDef;
    }

    public async Task PostGL(
        string transId,
        string transTableName,
        string transactionNumber,
        DateTime valueDate,
        string sysAccountName,
        string glAccount,
        decimal amount,
        string dorc,
        string branchCode,
        string currencyCode,
        string crossBranch,
        string crossCurrency,
        decimal baseAmount,
        bool isReverse = false,
        int accountingGroup = 1
    )
    {
        // ---- Guard clauses (cheap sanity checks) --------------------------------
        if (string.IsNullOrWhiteSpace(transactionNumber))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sysAccountName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(glAccount))
        {
            return;
        }

        // Normalize minimal fields
        dorc = string.IsNullOrWhiteSpace(dorc) ? dorc : dorc.Trim().ToUpperInvariant();

        if (!isReverse)
        {
            try
            {
                GLEntries entry = new()
                {
                    TransactionNumber = transactionNumber.Trim(),
                    TransTableName = transTableName?.Trim(),
                    TransId = transId?.Trim(),
                    SysAccountName = sysAccountName.Trim(),
                    GLAccount = glAccount.Trim(),
                    DorC = dorc,
                    TransactionStatus = "N",
                    Amount = amount,
                    BranchCode = branchCode?.Trim(),
                    CurrencyCode = currencyCode?.Trim(),
                    ValueDate = valueDate,
                    Posted = true,
                    CrossBranchCode = crossBranch?.Trim(),
                    CrossCurrencyCode = crossCurrency?.Trim(),
                    BaseCurrencyAmount = baseAmount,
                    AccountingGroup = accountingGroup,
                };

                await _glEntriesRepository.InsertAsync(entry);
            }
            catch (Exception ex)
            {
                await ex.LogErrorAsync();
            }

            return;
        }
        else
        {
            try
            {
                IQueryable<GLEntries> query = _glEntriesRepository.Table.Where(g =>
                    g.TransactionNumber == transactionNumber
                );

                await _glEntriesRepository.UpdateNoAudit(
                    query,
                    propertyName: "TransactionStatus",
                    value: "R"
                );
            }
            catch (Exception ex)
            {
                await ex.LogErrorAsync();
            }
        }
    }

    /// <summary>
    /// ListByCode
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public virtual async Task<List<TransactionAction>> ListByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Transaction code is required.", nameof(code));
        }

        return await _transactionActionRepository
            .Table.Where(a => a.TransCode == code)
            .ToListAsync();
    }

    private static async Task<BaseGLConfig> GetGLConfig<T>(
        T master,
        string glConfigClass,
        string mappingFields,
        string code
    )
        where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(glConfigClass))
        {
            return null;
        }

        if (master is null)
        {
            throw new ArgumentNullException(nameof(master));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Transaction code is required.", nameof(code));
        }

        Dictionary<string, string> searchConfig = new(StringComparer.OrdinalIgnoreCase)
        {
            ["TransCode"] = code,
        };

        if (!string.IsNullOrWhiteSpace(mappingFields))
        {
            string[] pairs = mappingFields.Split(
                '#',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            List<Task> asyncSetters = [];

            foreach (string pair in pairs)
            {
                string[] items = pair.Split(
                    ':',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );
                if (items.Length == 0)
                {
                    continue;
                }

                string key = items[0];
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (items.Length == 1)
                {
                    string val = master.GetMasterStringField(key) ?? string.Empty;
                    searchConfig[key] = val;
                }
                else
                {
                    string valueField = items[1];

                    if (
                        valueField
                            .Split(
                                '.',
                                StringSplitOptions.RemoveEmptyEntries
                                    | StringSplitOptions.TrimEntries
                            )
                            .Length < 2
                    )
                    {
                        string val = master.GetMasterStringField(valueField) ?? string.Empty;
                        searchConfig[key] = val;
                    }
                    else
                    {
                        asyncSetters.Add(
                            Task.Run(async () =>
                            {
                                string related = await master
                                    .GetMasterRelatedStringFieldAsync(valueField)
                                    .ConfigureAwait(false);
                                searchConfig[key] = related ?? string.Empty;
                            })
                        );
                    }
                }
            }

            if (asyncSetters.Count > 0)
            {
                await Task.WhenAll(asyncSetters).ConfigureAwait(false);
            }
        }

        BaseGLConfig ruleConfig = await DynamicRepositoryHelper
            .DynamicGetByFields<BaseGLConfig>(glConfigClass, searchConfig)
            .ConfigureAwait(false);

        return ruleConfig;
    }

    /// <summary>
    /// Convert string -> targetType using InvariantCulture, supports Nullable<T> and enums.
    /// Returns false if conversion is not possible.
    /// </summary>
    private static bool TryConvertFromInvariantString(
        string input,
        Type targetType,
        out object? result
    )
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (string.IsNullOrWhiteSpace(input))
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                result = null;
                return true;
            }
            if (underlying.IsValueType && underlying != typeof(string))
            {
                result = null!;
                return false;
            }
        }

        try
        {
            if (underlying.IsEnum)
            {
                if (
                    int.TryParse(
                        input,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int enumInt
                    )
                )
                {
                    result = Enum.ToObject(underlying, enumInt);
                }
                else
                {
                    result = Enum.Parse(underlying, input, ignoreCase: true);
                }
            }
            else if (underlying == typeof(Guid))
            {
                result = string.IsNullOrWhiteSpace(input) ? Guid.Empty : Guid.Parse(input);
            }
            else
            {
                TypeConverter converter = TypeDescriptor.GetConverter(underlying);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);
                }
                else
                {
                    result = Convert.ChangeType(input, underlying, CultureInfo.InvariantCulture);
                }
            }

            return true;
        }
        catch
        {
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// Create Statement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="master"></param>
    /// <param name="transCode"></param>
    /// <param name="amount"></param>
    /// <param name="statementCode"></param>
    /// <param name="refNumber"></param>
    /// <param name="baseCurrencyAmount"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task CreateStatement<T>(
        BaseTransactionModel transaction,
        T master,
        string transCode,
        decimal amount,
        string statementCode,
        string refNumber,
        decimal baseCurrencyAmount
    )
        where T : BaseEntity
    {
        // 1) Skip when no money to post
        if (amount == 0m)
        {
            return;
        }

        // 2) Resolve mapping
        MasterMapping mapping = await _masterMappingService.GetByMasterClass(
            master.GetType().FullName
        );
        if (mapping == null || string.IsNullOrWhiteSpace(mapping.StatementClass))
        {
            return; // nothing to do without a statement class
        }

        // 3) Resolve statement type (SỬA: dùng StatementClass, không phải MasterTransClass)
        Type statementType =
            TypeResolver.RequireType(mapping.StatementClass)
            ?? throw new InvalidOperationException($"Cannot find type '{mapping.StatementClass}'.");

        // 4) Reverse flow
        if (transaction.IsReverse)
        {
            try
            {
                Dictionary<string, string> search = new()
                {
                    { "TransactionNumber", transaction.TransactionNumber },
                };

                await DynamicRepositoryHelper.DynamicInvokeFilterAndUpdateWithAudit(
                    mapping.StatementClass,
                    search,
                    "StatementStatus",
                    "R"
                );
            }
            catch (Exception ex)
            {
                await ex.LogErrorAsync(
                    $"[ERROR] Reverse update failed for {transaction.TransactionNumber}: {ex.Message}"
                );
            }

            return;
        }

        // 5) Normal flow: create and fill BaseStatement
        (string _, string masterValueField) = ParseMasterFields(mapping.MasterFields);
        if (string.IsNullOrWhiteSpace(masterValueField))
        {
            throw new InvalidOperationException(
                $"Invalid MasterFields='{mapping.MasterFields}'. Expect format 'CodeField:ValueField[#...]'."
            );
        }

        if (Activator.CreateInstance(statementType) is not BaseStatement stm)
        {
            throw new InvalidOperationException(
                $"Cannot create instance of '{mapping.StatementClass}'."
            );
        }

        string accountNumber = master.GetMasterStringField(masterValueField);
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new InvalidOperationException(
                $"Cannot get master value for field '{masterValueField}'."
            );
        }

        string currencyCode = master.GetMasterStringField(mapping.MasterCurrencyCodeField);
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new InvalidOperationException(
                $"Cannot get currency code from field '{master + ";" + mapping.MasterCurrencyCodeField}'."
            );
        }

        // 6) Set fields
        stm.AccountNumber = accountNumber;
        stm.StatementDate = transaction.TransactionDate;
        stm.ReferenceId = transaction.RefId;
        stm.TransactionNumber = transaction.TransactionNumber;
        stm.ValueDate = transaction.ValueDate.Date;
        stm.Amount = amount;
        stm.CurrencyCode = currencyCode;
        stm.ConvertAmount = baseCurrencyAmount;
        stm.StatementCode = statementCode;
        stm.StatementStatus = "N";
        stm.RefNumber = refNumber;
        stm.TransCode = transCode;
        stm.Description = transaction.Description;

        await stm.Insert(transaction.RefId);
    }

    /// <summary>
    /// Parse mapping.MasterFields with expected format "CodeField:ValueField[#optional...]".
    /// Returns (codeField, valueField).
    /// </summary>
    private static (string codeField, string valueField) ParseMasterFields(string masterFields)
    {
        if (string.IsNullOrWhiteSpace(masterFields))
        {
            return (string.Empty, string.Empty);
        }

        // Take first segment before '#'
        string first = masterFields.Split('#')[0];
        string[] parts = first.Split(':');

        if (parts.Length >= 2)
        {
            return (parts[0].Trim(), parts[1].Trim());
        }

        return (string.Empty, string.Empty);
    }

    public virtual async Task<string> Execute<T>(
        BaseTransactionModel transaction,
        T master,
        string code,
        decimal amount,
        string crossBranchCode = "",
        string crossCurrencyCode = "",
        decimal baseCurrencyAmount = 0m,
        string statementCode = "",
        string refNumber = "",
        int accountingGroup = 1,
        bool IsUpdateMaster = true
    )
        where T : BaseEntity
    {
        Console.WriteLine($"TransactionActionService - Execute {master}");
        // ---- Guard clauses ------------------------------------------------------
        ArgumentNullException.ThrowIfNull(master);
        ArgumentNullException.ThrowIfNull(transaction);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Transaction code is required.", nameof(code));
        }

        string transId = Guid.NewGuid().ToString().Replace("-", "");

        try
        {
            // Lấy danh sách action theo code
            List<TransactionAction> actions = await ListByCode(code);
            if (actions == null || actions.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Not found config for action '{code}' of {typeof(T).FullName}"
                );
            }

            // Mapping cho master type (không cần gọi lặp lại trong vòng lặp)
            MasterMapping mapping =
                await _masterMappingService.GetByMasterClass(typeof(T).FullName!)
                ?? throw new InvalidOperationException(
                    $"Mapping not found for {typeof(T).FullName}"
                );
            string currentAction = string.Empty;

            foreach (TransactionAction action in actions)
            {
                // Lấy GL rule theo code
                BaseGLConfig ruleConfig = await GetGLConfig(
                    master,
                    mapping.GLConfigClass,
                    mapping.MasterGLFields,
                    code
                );

                string creditAccountName = ruleConfig?.CreditSysAccountName ?? string.Empty;
                string debitAccountName = ruleConfig?.DebitSysAccountName ?? string.Empty;

                if (action.HasStatement)
                {
                    await CreateStatement<T>(
                        transaction,
                        master,
                        code,
                        amount,
                        statementCode,
                        refNumber,
                        baseCurrencyAmount
                    );
                }

                if (ruleConfig != null)
                {
                    if (!string.Equals(currentAction, action.TransCode, StringComparison.Ordinal))
                    {
                        currentAction = action.TransCode;

                        // Credit
                        if (!string.IsNullOrWhiteSpace(creditAccountName))
                        {
                            _ = await CreateGLEntry<T>(
                                transId,
                                transaction,
                                master,
                                amount,
                                creditAccountName,
                                "C",
                                crossBranchCode,
                                crossCurrencyCode,
                                baseCurrencyAmount,
                                accountingGroup
                            );
                        }

                        // Debit
                        if (!string.IsNullOrWhiteSpace(debitAccountName))
                        {
                            _ = await CreateGLEntry<T>(
                                transId,
                                transaction,
                                master,
                                amount,
                                debitAccountName,
                                "D",
                                crossBranchCode,
                                crossCurrencyCode,
                                baseCurrencyAmount,
                                accountingGroup
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await ex.LogErrorAsync();
            throw;
        }

        return transId;
    }
}
