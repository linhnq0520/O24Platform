namespace O24OpenAPI.Contracts.Constants;

public partial class ResourceCode
{
    public partial class Common
    {
        public const string Success = "success";
        public const string Fail = "fail";
        public const string Error = "error";
        public const string Warning = "warning";
        public const string Info = "info";
        public const string NotFound = "not.found";
        public const string NotExists = "not.exist";
        public const string DynamicInvocationFailed = "error.dynamic.invocation.failed";
        public const string Unauthorized = "unauthorized";
        public const string Forbidden = "forbidden";
        public const string BadRequest = "bad.request";
        public const string ServerError = "server.error";
        public const string Timeout = "timeout";
        public const string NoData = "no.data";
        public const string DataExists = "data.exists";
        public const string Required = "required";
        public const string InvalidFormat = "invalid.format";
        public const string Confirmation = "confirmation";
        public const string Cancel = "cancel";
        public const string Loading = "loading";
        public const string Processing = "processing";
        public const string NotImplemented = "not.implemented";
        public const string ServiceUnavailable = "service.unavailable";
        public const string PartialSuccess = "partial.success";
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string InProgress = "in.progress";
        public const string Deleted = "deleted";
        public const string Archived = "archived";
        public const string Restored = "restored";
        public const string SystemError = "system.error";
        public const string NetworkError = "network.error";
        public const string MaintenanceMode = "maintenance.mode";
        public const string SessionExpired = "session.expired";
        public const string InvalidToken = "invalid.token";
        public const string TokenExpired = "token.expired";
    }

    public partial class Validation
    {
        public const string InvalidInput = "invalid.input";
        public const string InvalidLanguage = "invalid.language";
        public const string RequiredField = "validation.required";
        public const string UniqueField = "validation.unique";
        public const string InvalidEmail = "validation.email";
        public const string InvalidPhone = "validation.phone";
        public const string InvalidDate = "validation.date";
        public const string TooShort = "validation.too.short";
        public const string TooLong = "validation.too.long";
        public const string PasswordMismatch = "validation.password.mismatch";
        public const string InvalidPassword = "validation.password.invalid";
        public const string NotNullOrEmpty = "validation.not.null.or.empty";
        public const string NullOrEmpty = "validation.null.or.empty";
        public const string InvalidRange = "validation.invalid.range";
        public const string InvalidLength = "validation.invalid.length";
        public const string InvalidCharacters = "validation.invalid.characters";
        public const string InvalidUrl = "validation.invalid.url";
        public const string InvalidIpAddress = "validation.invalid.ip";
        public const string InvalidCreditCard = "validation.invalid.credit.card";
        public const string InvalidPostalCode = "validation.invalid.postal.code";
        public const string InvalidCurrency = "validation.invalid.currency";
        public const string InvalidNumber = "validation.invalid.number";
        public const string InvalidInteger = "validation.invalid.integer";
        public const string InvalidDecimal = "validation.invalid.decimal";
        public const string InvalidBoolean = "validation.invalid.boolean";
        public const string InvalidGuid = "validation.invalid.guid";
        public const string InvalidJsonFormat = "validation.invalid.json";
        public const string InvalidXmlFormat = "validation.invalid.xml";
        public const string InvalidFileFormat = "validation.invalid.file.format";
        public const string FileTooLarge = "validation.file.too.large";
        public const string DuplicateEntry = "validation.duplicate.entry";
        public const string MustBeUnique = "validation.must.be.unique";
        public const string MustMatchPattern = "validation.must.match.pattern";
        public const string InvalidTimeFormat = "validation.invalid.time.format";
        public const string DateInPast = "validation.date.in.past";
        public const string DateInFuture = "validation.date.in.future";
        public const string GreaterThan = "validation.greater.than";
        public const string LessThan = "validation.less.than";
        public const string EqualTo = "validation.equal.to";
        public const string NotEqualTo = "validation.not.equal.to";
    }

    public partial class Operation
    {
        public const string Create = "operation.create";
        public const string Update = "operation.update";
        public const string Delete = "operation.delete";
        public const string Read = "operation.read";
        public const string Save = "operation.save";
        public const string Submit = "operation.submit";
        public const string Approve = "operation.approve";
        public const string Reject = "operation.reject";
        public const string Upload = "operation.upload";
        public const string Download = "operation.download";
        public const string Export = "operation.export";
        public const string Import = "operation.import";
        public const string Print = "operation.print";
        public const string Search = "operation.search";
        public const string Filter = "operation.filter";
        public const string Sort = "operation.sort";
        public const string Login = "operation.login";
        public const string Logout = "operation.logout";
        public const string Register = "operation.register";
        public const string ChangePassword = "operation.change.password";
        public const string ResetPassword = "operation.reset.password";
        public const string VerifyAccount = "operation.verify.account";
        public const string SendNotification = "operation.send.notification";
        public const string Schedule = "operation.schedule";
        public const string Cancel = "operation.cancel";
        public const string Retry = "operation.retry";
        public const string Lock = "operation.lock";
        public const string Unlock = "operation.unlock";
        public const string Enable = "operation.enable";
        public const string Disable = "operation.disable";
    }

    public partial class Status
    {
        public const string Active = "status.active";
        public const string Inactive = "status.inactive";
        public const string Pending = "status.pending";
        public const string Processing = "status.processing";
        public const string Completed = "status.completed";
        public const string Failed = "status.failed";
        public const string Canceled = "status.canceled";
        public const string OnHold = "status.on.hold";
        public const string Expired = "status.expired";
        public const string Draft = "status.draft";
        public const string Published = "status.published";
        public const string Archived = "status.archived";
        public const string Blocked = "status.blocked";
        public const string Flagged = "status.flagged";
        public const string Verified = "status.verified";
        public const string Unverified = "status.unverified";
        public const string Approved = "status.approved";
        public const string Rejected = "status.rejected";
        public const string New = "status.new";
        public const string Duplicate = "status.duplicate";
        public const string InReview = "status.in.review";
    }

    public partial class Notification
    {
        public const string Success = "notification.success";
        public const string Error = "notification.error";
        public const string Warning = "notification.warning";
        public const string Info = "notification.info";
        public const string SuccessCreate = "notification.success.create";
        public const string SuccessUpdate = "notification.success.update";
        public const string SuccessDelete = "notification.success.delete";
        public const string ConfirmDelete = "notification.confirm.delete";
        public const string ConfirmAction = "notification.confirm.action";
        public const string UnsavedChanges = "notification.unsaved.changes";
        public const string NewMessage = "notification.new.message";
        public const string SystemAlert = "notification.system.alert";
    }

    public partial class Permission
    {
        public const string AccessDenied = "permission.access.denied";
        public const string InsufficientRights = "permission.insufficient.rights";
        public const string RequireAuthentication = "permission.require.authentication";
        public const string RequireAuthorization = "permission.require.authorization";
        public const string ReadOnly = "permission.read.only";
        public const string EditRights = "permission.edit.rights";
        public const string AdminRequired = "permission.admin.required";
        public const string OwnerOnly = "permission.owner.only";
    }

    public partial class Security
    {
        public const string InvalidCredentials = "security.invalid.credentials";
        public const string AccountLocked = "security.account.locked";
        public const string PasswordExpired = "security.password.expired";
        public const string TwoFactorRequired = "security.two.factor.required";
        public const string InvalidTwoFactorCode = "security.invalid.two.factor.code";
        public const string SuspiciousActivity = "security.suspicious.activity";
        public const string CaptchaRequired = "security.captcha.required";
        public const string InvalidCaptcha = "security.invalid.captcha";
        public const string RateLimitExceeded = "security.rate.limit.exceeded";
    }

    public partial class Pagination
    {
        public const string FirstPage = "pagination.first.page";
        public const string LastPage = "pagination.last.page";
        public const string NextPage = "pagination.next.page";
        public const string PreviousPage = "pagination.previous.page";
        public const string PageSize = "pagination.page.size";
        public const string TotalItems = "pagination.total.items";
        public const string TotalPages = "pagination.total.pages";
        public const string CurrentPage = "pagination.current.page";
        public const string ShowingItemsOf = "pagination.showing.items.of";
    }

    public partial class DateTime
    {
        public const string InvalidFormat = "datetime.invalid.format";
        public const string Today = "datetime.today";
        public const string Yesterday = "datetime.yesterday";
        public const string Tomorrow = "datetime.tomorrow";
        public const string ThisWeek = "datetime.this.week";
        public const string LastWeek = "datetime.last.week";
        public const string NextWeek = "datetime.next.week";
        public const string ThisMonth = "datetime.this.month";
        public const string LastMonth = "datetime.last.month";
        public const string NextMonth = "datetime.next.month";
        public const string ThisYear = "datetime.this.year";
        public const string LastYear = "datetime.last.year";
        public const string NextYear = "datetime.next.year";
        public const string DateRange = "datetime.date.range";
    }

    public class OpenApi
    {
        public const string InvalidClient = "openapi.auth.invalid.client";
        public const string InvalidClientSecret = "openapi.auth.invalid.secret";
        public const string InvalidGrant = "openapi.auth.invalid.grant";
        public const string UnsupportedGrantType = "openapi.auth.unsupported.grant_type";
        public const string InvalidRequest = "openapi.auth.invalid.request";
        public const string ExpiredToken = "openapi.auth.token.expired";
        public const string AccessDenied = "openapi.auth.access.denied";
        public const string Unauthorized = "openapi.auth.unauthorized";
        public const string BadRequest = "openapi.request.bad";
        public const string SystemError = "openapi.system.error";
    }
}
