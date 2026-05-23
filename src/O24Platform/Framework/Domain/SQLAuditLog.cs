using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

/// <summary>
/// The sql audit log class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class SQLAuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the command name
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the value of the command type
    /// </summary>
    public string CommandType { get; set; }

    /// <summary>
    /// Gets or sets the value of the params
    /// </summary>
    public string Params { get; set; }

    /// <summary>
    /// Gets or sets the value of the query
    /// </summary>
    public string Query { get; set; } // Câu SQL được thực thi

    /// <summary>
    /// Gets or sets the value of the execution time
    /// /// </summary>
    public DateTime ExecutionTime { get; set; } // Thời gian thực thi

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string Status { get; set; } // Trạng thái (Success/Failure)

    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string ErrorMessage { get; set; } // Thông báo lỗi (nếu có)

    /// <summary>
    /// Gets or sets the value of the result
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// Gets or sets the value of the executed by
    /// </summary>
    public string ExecutedBy { get; set; } // Người hoặc dịch vụ thực thi

    /// <summary>
    /// Gets or sets the value of the source service
    /// </summary>
    public string SourceService { get; set; } // Dịch vụ hoặc hệ thống gọi
}
