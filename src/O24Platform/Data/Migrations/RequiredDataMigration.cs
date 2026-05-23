using FluentMigrator;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Extensions;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The required data migration class
/// </summary>
/// <seealso cref="AutoReversingMigration"/>
[O24OpenAPIMigration(
    "2024/01/01 07:00:00:0000000",
    "7. Init Core data required data",
    MigrationProcessType.Update
)]
[Environment(EnvironmentType.All)]
public class RequiredDataMigration(IO24OpenAPIDataProvider dataProvider) : BaseMigration
{
    /// <summary>
    /// The data provider
    /// </summary>
    private readonly IO24OpenAPIDataProvider _dataProvider = dataProvider;

    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        this.InstallLanguages();
        this.InstallInitialData();
        this.InstallSettings();
        this.InstallSpecialStoreCommand();
        // this.InstallScheduleTasks();
    }

    /// <summary>
    /// Installs the initial data
    /// </summary>
    protected void InstallInitialData()
    {
        SeedListData(
                [
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = ResourceCode.Validation.InvalidLanguage,
                        ResourceValue = "Invalid language",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "Common.Value.Required",
                        ResourceValue = "{0} is required",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "Common.Value.Unique",
                        ResourceValue = "{0} is unique",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = ResourceCode.Validation.InvalidLanguage,
                        ResourceValue = "{0} should not exceed the max length {1}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = ResourceCode.Common.NotExists,
                        ResourceValue = "{0} [{1}] does not exist",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "Common.Value.NotExist",
                        ResourceValue = "{0} [{1}] does not exist",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "success",
                        ResourceValue = "Operation completed successfully",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "fail",
                        ResourceValue = "Operation failed",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "error",
                        ResourceValue = "An error occurred",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "not.found",
                        ResourceValue = "{0} not found",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "invalid.input",
                        ResourceValue = "Invalid input provided",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "unauthorized",
                        ResourceValue = "Unauthorized access",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "forbidden",
                        ResourceValue = "Access forbidden",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "bad.request",
                        ResourceValue = "Bad request",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "server.error",
                        ResourceValue = "An error occurred on the server",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "timeout",
                        ResourceValue = "Operation timed out",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "no.data",
                        ResourceValue = "No data available",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "data.exists",
                        ResourceValue = "{0} already exists",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "required",
                        ResourceValue = "{0} is required",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "invalid.format",
                        ResourceValue = "{0} has an invalid format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "confirmation",
                        ResourceValue = "Please confirm this action",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "cancel",
                        ResourceValue = "Operation cancelled",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "loading",
                        ResourceValue = "Loading, please wait...",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "processing",
                        ResourceValue = "Processing your request...",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "not.implemented",
                        ResourceValue = "Feature not implemented",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "service.unavailable",
                        ResourceValue = "Service currently unavailable",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "partial.success",
                        ResourceValue = "Operation partially completed",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "pending",
                        ResourceValue = "Operation pending",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "completed",
                        ResourceValue = "Operation completed",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "in.progress",
                        ResourceValue = "Operation in progress",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "deleted",
                        ResourceValue = "{0} has been deleted",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "archived",
                        ResourceValue = "{0} has been archived",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "restored",
                        ResourceValue = "{0} has been restored",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "system.error",
                        ResourceValue = "System error occurred {0}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "network.error",
                        ResourceValue = "Network connection error",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "maintenance.mode",
                        ResourceValue = "System is in maintenance mode",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "session.expired",
                        ResourceValue = "Your session has expired",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "invalid.token",
                        ResourceValue = "Invalid authentication token",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "token.expired",
                        ResourceValue = "Authentication token has expired",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.required",
                        ResourceValue = "{0} is required",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.email",
                        ResourceValue = "Invalid email address format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.phone",
                        ResourceValue = "Invalid phone number format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.date",
                        ResourceValue = "Invalid date format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.too.short",
                        ResourceValue = "{0} is too short (minimum length: {1})",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.too.long",
                        ResourceValue = "{0} is too long (maximum length: {1})",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.password.mismatch",
                        ResourceValue = "Passwords do not match",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.password.invalid",
                        ResourceValue = "Password does not meet requirements",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.not.null.or.empty",
                        ResourceValue = "{0} cannot be null or empty",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.null.or.empty",
                        ResourceValue = "{0} must be null or empty",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.range",
                        ResourceValue = "{0} must be between {1} and {2}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.length",
                        ResourceValue = "{0} length must be between {1} and {2} characters",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.characters",
                        ResourceValue = "{0} contains invalid characters",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.url",
                        ResourceValue = "Invalid URL format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.ip",
                        ResourceValue = "Invalid IP address format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.credit.card",
                        ResourceValue = "Invalid credit card number",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "validation.invalid.postal.code",
                        ResourceValue = "Invalid postal code format",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "error.dynamic.invocation.failed",
                        ResourceValue = "Failed to dynamically invoke method",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "operation.create",
                        ResourceValue = "Create operation",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "operation.update",
                        ResourceValue = "Update operation",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "operation.delete",
                        ResourceValue = "Delete operation",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "operation.read",
                        ResourceValue = "Read operation",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "operation.save",
                        ResourceValue = "Save operation",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "status.active",
                        ResourceValue = "Active",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "status.inactive",
                        ResourceValue = "Inactive",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "status.pending",
                        ResourceValue = "Pending",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "notification.success",
                        ResourceValue = "Success: {0}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "notification.error",
                        ResourceValue = "Error: {0}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "notification.warning",
                        ResourceValue = "Warning: {0}",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "permission.access.denied",
                        ResourceValue = "Access denied",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "permission.insufficient.rights",
                        ResourceValue = "Insufficient rights to perform this action",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "security.invalid.credentials",
                        ResourceValue = "Invalid username or password",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "security.account.locked",
                        ResourceValue = "Account has been locked",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "pagination.first.page",
                        ResourceValue = "First page",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "pagination.last.page",
                        ResourceValue = "Last page",
                    },
                    new LocaleStringResource()
                    {
                        Language = "en",
                        ResourceName = "datetime.invalid.format",
                        ResourceValue = "Invalid date/time format",
                    },
                ],
                [nameof(LocaleStringResource.Language), nameof(LocaleStringResource.ResourceName)]
            )
            .Wait();
    }

    /// <summary>
    /// Installs the settings
    /// </summary>
    protected void InstallSettings()
    {
        this._dataProvider.BulkInsertEntities(
                [
                    new Setting() { Name = "WebApiSettings.DeveloperMode", Value = "true" },
                    new Setting() { Name = "WebApiSettings.TokenLifetimeDays", Value = "7" },
                    new Setting() { Name = "WebApiSettings.StaticTokenLifetimeDays", Value = "7" },
                    new Setting()
                    {
                        Name = "WebApiSettings.SecretKey",
                        Value = "X5hGbJ9X8JH6S1TJpJ0t4zP+xJb+7pZoN8L9xJm4Qj8=",
                    },
                    new Setting() { Name = "WebApiSettings.CurrentVersion", Value = "1.0" },
                ]
            )
            .GetAsyncResult();
    }

    /// <summary>
    /// Installs the languages
    /// </summary>
    protected void InstallLanguages()
    {
        this._dataProvider.BulkInsertEntities(
                [
                    new Language()
                    {
                        Name = "Cambodia",
                        LanguageCulture = "kh-KH",
                        UniqueSeoCode = "kh",
                        DisplayOrder = 2,
                    },
                    new Language()
                    {
                        Name = "English",
                        LanguageCulture = "en-US",
                        UniqueSeoCode = "en",
                        DisplayOrder = 1,
                    },
                    new Language()
                    {
                        Name = "Lao",
                        LanguageCulture = "la-LA",
                        UniqueSeoCode = "la",
                        DisplayOrder = 3,
                    },
                    new Language()
                    {
                        Name = "Thailand",
                        LanguageCulture = "th-TH",
                        UniqueSeoCode = "th",
                        DisplayOrder = 4,
                    },
                    new Language()
                    {
                        Name = "Vietnam",
                        LanguageCulture = "vi-VN",
                        UniqueSeoCode = "vn",
                        DisplayOrder = 5,
                    },
                    new Language()
                    {
                        Name = "Chinese",
                        LanguageCulture = "zh-cn",
                        UniqueSeoCode = "zh",
                        DisplayOrder = 6,
                    },
                ]
            )
            .GetAsyncResult();
    }

    /// <summary>
    /// Installs the schedule tasks
    /// </summary>
    protected void InstallScheduleTasks()
    {
        DateTime utcNow = DateTime.UtcNow;
        this._dataProvider.BulkInsertEntities<ScheduleTask>(

                    [
                        new ScheduleTask()
                        {
                            Name = "Check queue connection",
                            Seconds = 60,
                            Type =
                                "Jits.Neptune.Web.Framework.Services.Queue.KeepConnectionTask, Jits.Neptune.Web.Framework",
                            Enabled = true,
                            StopOnError = false,
                        },
                        new ScheduleTask()
                        {
                            Name = "Clear cache",
                            Seconds = 3600,
                            Type =
                                "Jits.Neptune.Web.Framework.Services.Caching.ClearCacheTask, Jits.Neptune.Web.Framework",
                            Enabled = true,
                            StopOnError = false,
                        },
                        new ScheduleTask()
                        {
                            Name = "Clear log",
                            Seconds = 3600,
                            Type =
                                "Jits.Neptune.Web.Framework.Services.Logging.ClearLogTask, Jits.Neptune.Web.Framework",
                            Enabled = true,
                            StopOnError = false,
                        },
                    ]
            )
            .GetAsyncResult();
    }

    /// <summary>
    /// Installs the settings
    /// </summary>
    protected void InstallSpecialStoreCommand()
    {
        bool exists = _dataProvider.GetTable<StoredCommand>().Any(x => x.Name == "GetDbFunctions");

        if (!exists)
        {
            StoredCommand cmd = new()
            {
                Name = "GetDbFunctions",
                Query =
                    "SELECT\r\n    SCHEMA_NAME(o.schema_id) COLLATE DATABASE_DEFAULT + '.' + o.[name] COLLATE DATABASE_DEFAULT AS [ObjectName],\r\n    CASE o.[type]\r\n        WHEN 'P' THEN 'Stored Procedure'\r\n        WHEN 'FN' THEN 'Scalar Function'\r\n        WHEN 'TF' THEN 'Table Function'\r\n        WHEN 'IF' THEN 'Inline Table Function'\r\n    END AS [ObjectType],\r\n    'USE O24CMS' + CHAR(13) + CHAR(10) +\r\n    'GO' + CHAR(13) + CHAR(10) +\r\n    'IF OBJECT_ID(''' + SCHEMA_NAME(o.schema_id) COLLATE DATABASE_DEFAULT + '.' +\r\n    o.[name] COLLATE DATABASE_DEFAULT + ''', ''' + o.[type] COLLATE DATABASE_DEFAULT + ''') IS NOT NULL ' +\r\n    'DROP ' +\r\n    CASE o.[type]\r\n        WHEN 'P' THEN 'PROCEDURE '\r\n        WHEN 'FN' THEN 'FUNCTION '\r\n        WHEN 'TF' THEN 'FUNCTION '\r\n        WHEN 'IF' THEN 'FUNCTION '\r\n    END +\r\n    SCHEMA_NAME(o.schema_id) COLLATE DATABASE_DEFAULT + '.' +\r\n    o.[name] COLLATE DATABASE_DEFAULT + ';' + CHAR(13) + CHAR(10) +\r\n    'GO' + CHAR(13) + CHAR(10) +\r\n    m.[definition] COLLATE DATABASE_DEFAULT + CHAR(13) + CHAR(10) +\r\n    'GO' AS [Script]\r\nFROM\r\n    sys.sql_modules m\r\nINNER JOIN\r\n    sys.objects o ON m.object_id = o.object_id\r\nWHERE\r\n    o.[type] IN ('P', 'FN', 'TF', 'IF')\r\n    AND SCHEMA_NAME(o.schema_id) = 'dbo'\r\nORDER BY\r\n    o.[type], o.[name];\r\n",
                Type = "Query",
                Description = "GetDbFunctions",
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            _dataProvider.InsertEntity(cmd).GetAsyncResult();
        }
    }

    /// <summary>
    /// The required data migration class
    /// </summary>
    /// <seealso cref="AutoReversingMigration"/>
    [O24OpenAPIMigration(
        "2024/01/01 08:00:00:0000000",
        "8. Add static token setting",
        MigrationProcessType.Update
    )]
    [Environment(EnvironmentType.All)]
    public class RequiredDataMigration1(IO24OpenAPIDataProvider dataProvider)
        : AutoReversingMigration
    {
        /// <summary>
        /// The data provider
        /// </summary>
        private readonly IO24OpenAPIDataProvider _dataProvider = dataProvider;

        /// <summary>
        /// Ups this instance
        /// </summary>
        public override void Up()
        {
            List<Setting> list = new()
            {
                new() { Name = "WebApiSettings.DeveloperMode", Value = "true" },
                new()
                {
                    Name = "WebApiSettings.SecretKey",
                    Value = "X5hGbJ9X8JH6S1TJpJ0t4zP+xJb+7pZoN8L9xJm4Qj8=",
                },
                new() { Name = "WebApiSettings.CurrentVersion", Value = "1.0" },
                new() { Name = "WebApiSettings.TokenLifetimeDays", Value = "7" },
                new() { Name = "WebApiSettings.StaticTokenLifetimeDays", Value = "7" },
                new() { Name = "WebApiSettings.TokenLifetimeMinutes", Value = "10" },
                new() { Name = "WFOSetting.MessageBrokerHostName", Value = "localhost" },
                new() { Name = "WFOSetting.MessageBrokerPort", Value = "5672" },
                new() { Name = "WFOSetting.MessageBrokerUserName", Value = "admin" },
                new() { Name = "WFOSetting.MessageBrokerPassword", Value = "admin" },
                new()
                {
                    Name = "WFOSetting.MessageBrokerPasswordEncryptionMethod",
                    Value = "SHA256",
                },
                new() { Name = "WFOSetting.MessageBrokerVirtualHost", Value = "/" },
                new() { Name = "WFOSetting.ServerPingIntervalInSecond", Value = "60" },
                new() { Name = "WFOSetting.MessageBrokerSSLActive", Value = "false" },
                new() { Name = "WFOSetting.MessageBrokerReconnectIntervalInSecond", Value = "60" },
                new() { Name = "WFOSetting.RedisServerName", Value = "localhost" },
                new() { Name = "WFOSetting.RedisServerPort", Value = "6379" },
            };

            foreach (var item in list)
            {
                var exists = _dataProvider.GetTable<Setting>().Any(s => s.Name == item.Name);
                if (!exists)
                {
                    _dataProvider.InsertEntity(item).GetAsyncResult();
                }
            }
        }
    }
}
