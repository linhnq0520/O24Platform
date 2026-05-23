using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using O24OpenAPI.Contracts.Abstractions;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Models.O24OpenAPI;
using O24OpenAPI.Framework.Models.UtilityModels;
using O24OpenAPI.Framework.Services;
using O24OpenAPI.Framework.Services.Queue;
using O24OpenAPI.Framework.Services.Security;
using O24OpenAPI.Framework.Utils;
using System.Data;
using System.IO;

namespace O24OpenAPI.Framework.Controllers;

/// <summary>
/// ToolController
/// </summary>
/// <param name="encryptionService"></param>
public class ToolController(
    IEncryptionService encryptionService,
    IStoreFunctionService storeFunctionService
) : BaseController
{
    /// <summary>
    /// The encryption service
    /// </summary>
    private readonly IEncryptionService _encryptionService = encryptionService;

    /// <summary>
    /// The database function service
    /// </summary>
    private readonly IStoreFunctionService _storeFunctionService = storeFunctionService;

    /// <summary>
    /// Encrypt text
    /// </summary>
    /// <param name="text"></param>
    /// <param name="secretKey"></param>
    /// <param name="encryptType"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public virtual IActionResult Encrypt(
        string text,
        string secretKey = "",
        string encryptType = "3DES"
    )
    {
        return Ok(_encryptionService.EncryptText(text, secretKey, encryptType));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="text"></param>
    /// <param name="secretKey"></param>
    /// <param name="encryptType"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public virtual IActionResult Decrypt(
        string text,
        string secretKey = "",
        string encryptType = "3DES"
    )
    {
        return Ok(_encryptionService.DecryptText(text, secretKey, encryptType));
    }

    /// <summary>
    /// Copies the table using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="sourceTable">The source table</param>
    /// <param name="targetTable">The target table</param>
    /// <returns>The action result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public virtual IActionResult CopyTable(
        string connectionString,
        string sourceTable,
        string targetTable
    )
    {
        if (string.IsNullOrWhiteSpace(sourceTable) || string.IsNullOrWhiteSpace(targetTable))
        {
            return BadRequest(new { Message = "Tên bảng nguồn và bảng đích không được để trống!" });
        }

        try
        {
            using (SqlConnection conn = new(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra bảng nguồn có dữ liệu không
                        using (
                            SqlCommand cmd = new(
                                $"SELECT COUNT(*) FROM {sourceTable}",
                                conn,
                                transaction
                            )
                        )
                        {
                            int count = (int)cmd.ExecuteScalar();
                            if (count == 0)
                            {
                                return Ok(new { Message = "❌ Bảng nguồn không có dữ liệu!" });
                            }
                        }

                        // Lấy dữ liệu từ bảng nguồn
                        DataTable dataTable = new();
                        using (
                            SqlCommand cmd = new($"SELECT * FROM {sourceTable}", conn, transaction)
                        )
                        using (SqlDataAdapter adapter = new(cmd))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Xóa dữ liệu bảng đích
                        using (
                            SqlCommand deleteCmd = new(
                                $"DELETE FROM {targetTable}",
                                conn,
                                transaction
                            )
                        )
                        {
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Kiểm tra bảng đích có cột IDENTITY không
                        bool hasIdentityColumn = false;
                        using (
                            SqlCommand identityCmd = new(
                                $@"
                        SELECT 1 FROM sys.columns c
                        INNER JOIN sys.objects o ON c.object_id = o.object_id
                        WHERE o.name = @tableName AND c.is_identity = 1",
                                conn,
                                transaction
                            )
                        )
                        {
                            identityCmd.Parameters.AddWithValue("@tableName", targetTable);
                            using (SqlDataReader reader = identityCmd.ExecuteReader())
                            {
                                hasIdentityColumn = reader.HasRows;
                            }
                        }

                        // Lấy danh sách cột của bảng đích (bỏ qua phân biệt hoa/thường)
                        Dictionary<string, string> targetColumns = new(
                            StringComparer.OrdinalIgnoreCase
                        );
                        using (
                            SqlCommand cmd = new(
                                $"SELECT TOP 0 * FROM {targetTable}",
                                conn,
                                transaction
                            )
                        )
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                targetColumns[reader.GetName(i).ToLower()] = reader.GetName(i);
                            }
                        }

                        // Bật IDENTITY_INSERT nếu có cột IDENTITY
                        if (hasIdentityColumn)
                        {
                            using (
                                SqlCommand setIdentityOn = new(
                                    $"SET IDENTITY_INSERT {targetTable} ON",
                                    conn,
                                    transaction
                                )
                            )
                            {
                                setIdentityOn.ExecuteNonQuery();
                            }
                        }

                        // Copy dữ liệu
                        using (
                            SqlBulkCopy bulkCopy = new(
                                conn,
                                SqlBulkCopyOptions.KeepIdentity,
                                transaction
                            )
                        )
                        {
                            bulkCopy.DestinationTableName = targetTable;

                            // Mapping cột không phân biệt hoa/thường
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                string colNameLower = col.ColumnName.ToLower();
                                if (targetColumns.ContainsKey(colNameLower))
                                {
                                    bulkCopy.ColumnMappings.Add(
                                        col.ColumnName,
                                        targetColumns[colNameLower]
                                    );
                                }
                            }

                            bulkCopy.WriteToServer(dataTable);
                        }

                        // Tắt IDENTITY_INSERT nếu đã bật
                        if (hasIdentityColumn)
                        {
                            using (
                                SqlCommand setIdentityOff = new(
                                    $"SET IDENTITY_INSERT {targetTable} OFF",
                                    conn,
                                    transaction
                                )
                            )
                            {
                                setIdentityOff.ExecuteNonQuery();
                            }
                        }

                        // Commit transaction nếu mọi thứ đều thành công
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, rollback transaction
                        transaction.Rollback();
                        return StatusCode(
                            500,
                            new { Message = "Lỗi khi copy dữ liệu!", Error = ex.Message }
                        );
                    }
                }
            }

            return Ok(
                new
                {
                    Message = $"✅ Dữ liệu đã copy từ {sourceTable} sang {targetTable} thành công!",
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "❌ Lỗi xảy ra!", Error = ex.Message });
        }
    }

    /// <summary>
    /// Copies the table from different server using the specified source connection string
    /// </summary>
    /// <param name="sourceConnectionString">The source connection string</param>
    /// <param name="sourceTable">The source table</param>
    /// <param name="targetTable">The target table</param>
    /// <param name="targetConnectionString">The target connection string</param>
    /// <returns>The action result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public virtual IActionResult CopyTableFromDifferentServer(
        string sourceConnectionString,
        string sourceTable,
        string targetTable,
        string targetConnectionString = null
    ) // Thêm tham số targetConnectionString, mặc định là null
    {
        if (string.IsNullOrWhiteSpace(sourceTable) || string.IsNullOrWhiteSpace(targetTable))
        {
            return BadRequest(new { Message = "Tên bảng nguồn và bảng đích không được để trống!" });
        }

        // Nếu targetConnectionString không được cung cấp, sử dụng sourceConnectionString
        targetConnectionString = string.IsNullOrWhiteSpace(targetConnectionString)
            ? sourceConnectionString
            : targetConnectionString;

        try
        {
            using (SqlConnection sourceConn = new(sourceConnectionString))
            using (SqlConnection targetConn = new(targetConnectionString))
            {
                sourceConn.Open();
                targetConn.Open();

                using (SqlTransaction sourceTransaction = sourceConn.BeginTransaction())
                using (SqlTransaction targetTransaction = targetConn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra bảng nguồn có dữ liệu không
                        using (
                            SqlCommand cmd = new(
                                $"SELECT COUNT(*) FROM {sourceTable}",
                                sourceConn,
                                sourceTransaction
                            )
                        )
                        {
                            int count = (int)cmd.ExecuteScalar();
                            if (count == 0)
                            {
                                return Ok(new { Message = "❌ Bảng nguồn không có dữ liệu!" });
                            }
                        }

                        // Lấy dữ liệu từ bảng nguồn
                        DataTable dataTable = new();
                        using (
                            SqlCommand cmd = new(
                                $"SELECT * FROM {sourceTable}",
                                sourceConn,
                                sourceTransaction
                            )
                        )
                        using (SqlDataAdapter adapter = new(cmd))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Xóa dữ liệu bảng đích
                        using (
                            SqlCommand deleteCmd = new(
                                $"DELETE FROM {targetTable}",
                                targetConn,
                                targetTransaction
                            )
                        )
                        {
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Kiểm tra bảng đích có cột IDENTITY không
                        bool hasIdentityColumn = false;
                        using (
                            SqlCommand identityCmd = new(
                                $@"
                        SELECT 1 FROM sys.columns c
                        INNER JOIN sys.objects o ON c.object_id = o.object_id
                        WHERE o.name = @tableName AND c.is_identity = 1",
                                targetConn,
                                targetTransaction
                            )
                        )
                        {
                            identityCmd.Parameters.AddWithValue("@tableName", targetTable);
                            using (SqlDataReader reader = identityCmd.ExecuteReader())
                            {
                                hasIdentityColumn = reader.HasRows;
                            }
                        }

                        // Lấy danh sách cột của bảng đích (bỏ qua phân biệt hoa/thường)
                        Dictionary<string, string> targetColumns = new(
                            StringComparer.OrdinalIgnoreCase
                        );
                        using (
                            SqlCommand cmd = new(
                                $"SELECT TOP 0 * FROM {targetTable}",
                                targetConn,
                                targetTransaction
                            )
                        )
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                targetColumns[reader.GetName(i).ToLower()] = reader.GetName(i);
                            }
                        }

                        // Bật IDENTITY_INSERT nếu có cột IDENTITY
                        if (hasIdentityColumn)
                        {
                            using (
                                SqlCommand setIdentityOn = new(
                                    $"SET IDENTITY_INSERT {targetTable} ON",
                                    targetConn,
                                    targetTransaction
                                )
                            )
                            {
                                setIdentityOn.ExecuteNonQuery();
                            }
                        }

                        // Copy dữ liệu
                        using (
                            SqlBulkCopy bulkCopy = new(
                                targetConn,
                                SqlBulkCopyOptions.KeepIdentity,
                                targetTransaction
                            )
                        )
                        {
                            bulkCopy.DestinationTableName = targetTable;

                            // Mapping cột không phân biệt hoa/thường
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                string colNameLower = col.ColumnName.ToLower();
                                if (targetColumns.TryGetValue(colNameLower, out string value))
                                {
                                    bulkCopy.ColumnMappings.Add(col.ColumnName, value);
                                }
                            }

                            bulkCopy.WriteToServer(dataTable);
                        }

                        // Tắt IDENTITY_INSERT nếu đã bật
                        if (hasIdentityColumn)
                        {
                            using SqlCommand setIdentityOff = new(
                                $"SET IDENTITY_INSERT {targetTable} OFF",
                                targetConn,
                                targetTransaction
                            );
                            setIdentityOff.ExecuteNonQuery();
                        }

                        // Commit cả hai transaction nếu mọi thứ thành công
                        sourceTransaction.Commit();
                        targetTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, rollback cả hai transaction
                        sourceTransaction.Rollback();
                        targetTransaction.Rollback();
                        return StatusCode(
                            500,
                            new { Message = "Lỗi khi copy dữ liệu!", Error = ex.Message }
                        );
                    }
                }
            }

            return Ok(
                new
                {
                    Message = $"✅ Dữ liệu đã copy từ {sourceTable} sang {targetTable} thành công!",
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "❌ Lỗi xảy ra!", Error = ex.Message });
        }
    }

    /// <summary>
    /// Exports the multi file using the specified list fields
    /// </summary>
    /// <param name="listFields">The list fields</param>
    /// <param name="entityName">The entity name</param>
    /// <returns>The action result</returns>
    [HttpPost]
    public ActionResult ExportMultiFile(
        List<Dictionary<string, object>> listFields,
        string entityName
    )
    {
        try
        {
            List<FileModel> files = DataUtils.ExportMultiFiles(
                HttpContext.Request.Host.ToString(),
                entityName,
                listFields
            );

            SaveToFile(files, entityName);

            return Ok($"Converted {files.Count} files");
        }
        catch (Exception ex)
        {
            return Ok(ex);
        }
    }

    /// <summary>
    /// Exports the file using the specified list fields
    /// </summary>
    /// <param name="listFields">The list fields</param>
    /// <param name="entityName">The entity name</param>
    /// <returns>The action result</returns>
    [HttpPost]
    public ActionResult ExportFile(
        List<Dictionary<string, object>> listFields,
        string entityName,
        string fileName = null
    )
    {
        try
        {
            FileModel file = DataUtils.ExportFile(
                HttpContext.Request.Host.ToString(),
                entityName,
                listFields
            );

            fileName = string.IsNullOrWhiteSpace(fileName) ? entityName : fileName;
            string path = $"Migrations/DataJson/{fileName}";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = Path.Combine(path, file.FileName);
            System.IO.File.WriteAllText(fullPath, file.FileContent);

            return Ok($"Successfully");
        }
        catch (Exception ex)
        {
            return Ok(ex);
        }
    }

    /// <summary>
    /// Saves the to file using the specified files
    /// </summary>
    /// <param name="files">The files</param>
    /// <param name="tableName">The table name</param>
    /// <returns>The bool</returns>
    private static bool SaveToFile(List<FileModel> files, string tableName)
    {
        try
        {
            foreach (FileModel file in files)
            {
                string path = $"Migrations/DataJson/{tableName}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fullPath = Path.Combine(path, file.FileName);
                System.IO.File.WriteAllText(fullPath, file.FileContent);
            }
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Upload data
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadData(
        IFormFile file,
        string filedConstraintsJson,
        bool preserveIds = false
    )
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        List<string> filedConstraints;
        try
        {
            filedConstraints =
            [
                .. (filedConstraintsJson ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0),
            ];
        }
        catch (Exception)
        {
            return BadRequest("Invalid filedConstraints format.");
        }

        string utfString;
        using (MemoryStream ms = new())
        {
            await file.CopyToAsync(ms);
            utfString = Encoding.UTF8.GetString(ms.ToArray());
        }

        MigrationJsonImportResult result = await DataUtils.ImportFile(
            utfString,
            filedConstraints,
            preserveIds
        );

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MigrationJsonImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportJsonFolder(
        string folderPath,
        string filedConstraintsJson,
        bool preserveIds = false
    )
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return BadRequest("folderPath is required.");
        }

        List<string> filedConstraints;
        try
        {
            filedConstraints =
            [
                .. (filedConstraintsJson ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0),
            ];
        }
        catch (Exception)
        {
            return BadRequest("Invalid filedConstraints format.");
        }

        MigrationJsonImportResult result = await DataUtils.ImportFolder(
            folderPath,
            filedConstraints,
            preserveIds
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetFullClassName(string assemblyName)
    {
        await Task.CompletedTask;
        IEnumerable<Type> listType = EngineContext
            .Current.Resolve<ITypeFinder>()
            .FindClassesOfType<BaseQueue>();

        if (listType is null || !listType.Any())
        {
            return Ok(new List<string>());
        }

        List<string> result = [];
        foreach (Type type in listType)
        {
            string fullClassName = type.FullName;
            if (fullClassName != null && fullClassName.Contains(assemblyName))
            {
                result.Add(fullClassName);
            }
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetListMethod(string fullClassName, string assemblyName)
    {
        await Task.CompletedTask;
        string typeName = $"{fullClassName}, {assemblyName}";
        Type type = Type.GetType(typeName);
        List<MethodResponse> listModel = [];

        if (type != null)
        {
            List<MethodResponse> methods = type.GetMethods()
                .Where(m => m.IsPublic && !m.IsStatic)
                .Select(m => new MethodResponse { MethodName = m.Name })
                .ToList();
            listModel.AddRange(methods);
        }
        IPagedList<MethodResponse> pageList = await listModel
            .AsQueryable()
            .ToPagedList(0, int.MaxValue);
        PagedListModel<MethodResponse, MethodResponse> response = pageList.ToPagedListModel<
            MethodResponse,
            MethodResponse
        >();

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> ExportFunction()
    {
        int count = await _storeFunctionService.ExportToFile();

        return Ok($"Exported: {count}");
    }

    /// <summary>
    /// Register Grpc
    /// </summary>
    /// <param name="yourServiceID"></param>
    /// <param name="yourGrpcURL"></param>
    /// <param name="assemblyMigration"></param>
    /// <returns></returns>
    [HttpPost]
    [HttpPost("register-grpc")]
    public virtual async Task<IActionResult> RegisterGrpc(
        string yourServiceID,
        string yourGrpcURL,
        string assemblyMigration
    )
    {
        try
        {
            IWFOGrpcClientBaseService? wfoGrpcClient = EngineContext.Current.Resolve<IWFOGrpcClientBaseService>();

            string grpcService = $"{yourServiceID}GrpcService";

            string result = await wfoGrpcClient.RegisterServiceGrpcEndpointAsync(
                serviceCode: yourServiceID,
                serviceHandleName: grpcService,
                grpcEndpointURL: yourGrpcURL,
                instanceID: Guid.NewGuid().ToString("N"),
                serviceAssemblyName: assemblyMigration
            );

            return Ok(
                new
                {
                    success = true,
                    message = "Register gRPC service successfully",
                    service = yourServiceID,
                    endpoint = yourGrpcURL,
                    serviceresult = result,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}
