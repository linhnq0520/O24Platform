namespace O24OpenAPI.Framework.Extensions;

/// <summary>
/// The document filter attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DocumentFilterAttribute : Attribute { }

/// <summary>
/// The schema filter attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SchemaFilterAttribute : Attribute { }

/// <summary>
/// The operation filter attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class OperationFilterAttribute : Attribute { }
