using Newtonsoft.Json;
using O24OpenAPI.Core.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The string extensions class
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns the model using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <param name="crossservice">The crossservice</param>
    /// <exception cref="Exception">Data JSON invalid</exception>
    /// <returns>The</returns>
    public static T ToModel<T>(this string value, bool crossservice = false)
        where T : BaseTransactionModel
    {
        try
        {
            if (crossservice)
            {
                // Using Newtonsoft.Json
                // Do Request.RequestBody.Data truyen tu crossservice bị parse thành JSON String. Nên cần đưa về object trước đã.

                var workflow = JsonConvert.DeserializeObject<Workflow<object>>(value);
                if (workflow?.Request?.RequestBody?.Data is string jsonData)
                {
                    // Nếu Data là chuỗi JSON, deserialize lần hai thành kiểu T
                    return JsonConvert.DeserializeObject<T>(jsonData);
                }
                else
                {
                    throw new Exception("Data JSON invalid");
                }
            }
            else
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.Converters.Add(new NumberConverter());
                return JsonSerializer
                    .Deserialize<Workflow<T>>(value, options)
                    .Request.RequestBody.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Returns the workflow input using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <param name="crossservice">The crossservice</param>
    /// <returns>The workflow input</returns>
    public static WorkflowInput ToWorkflowInput<T>(this string value, bool crossservice = false)
        where T : BaseTransactionModel
    {
        if (crossservice)
        {
            // Using Newtonsoft.Json
            return JsonConvert.DeserializeObject<WorkflowInput>(value);
        }
        else
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new NumberConverter());
            return JsonSerializer
                .Deserialize<Workflow<T>>(value, options)
                .Request.RequestBody.WorkflowInput;
        }
    }

    /// <summary>
    /// Try to get label from JSON string based on the specified language
    /// </summary>
    /// <param name="json"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    public static string TryGetLabelFromJson(string json, string lang)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "";
        }

        try
        {
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<
                Dictionary<string, string>
            >(json);
            return dict.TryGetValue(lang, out string value) ? value : "";
        }
        catch
        {
            return "";
        }
    }
}
