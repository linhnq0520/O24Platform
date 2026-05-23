//using System.Text.Json;
//using Newtonsoft.Json.Linq;

//namespace O24OpenAPI.Client.Lib.mapping;

///// <summary>
///// The 24 open api expression class
///// </summary>
//public class O24OpenAPIExpression<TExpressionType>
//{
//    /// <summary>
//    /// The contextobject
//    /// </summary>
//    [NonSerialized]
//    public JObject __ContextObject;

//    /// <summary>
//    /// Gets or sets the value of the expression
//    /// </summary>
//    public TExpressionType expression { get; set; }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="O24OpenAPIExpression{TExpressionType}"/> class
//    /// </summary>
//    public O24OpenAPIExpression() { }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="O24OpenAPIExpression{TExpressionType}"/> class
//    /// </summary>
//    /// <param name="pContextObject">The context object</param>
//    public O24OpenAPIExpression(JObject pContextObject)
//    {
//        __ContextObject = pContextObject;
//    }

//    /// <summary>
//    /// Evaluates the p boolean expression
//    /// </summary>
//    /// <param name="pBooleanExpression">The boolean expression</param>
//    /// <param name="pDictionary">The dictionary</param>
//    /// <returns>The expression type</returns>
//    public TExpressionType Evaluate(
//        string pBooleanExpression,
//        IDictionary<string, string> pDictionary
//    )
//    {
//        return JsonSerializer
//            .Deserialize<O24OpenAPIExpression<TExpressionType>>(
//                JsonSerializer.Serialize(
//                    O24OpenAPIJsonMapper.convertTemplateToMappedObject(
//                        pBooleanExpression,
//                        __ContextObject,
//                        pDictionary
//                    )
//                )
//            )
//            .expression;
//    }
//}
