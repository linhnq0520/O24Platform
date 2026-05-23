//using Humanizer;
//using Microsoft.OpenApi.Models;
//using O24OpenAPI.Framework.Extensions;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using System.Reflection;

//namespace O24OpenAPI.Framework.Helpers
//{
//    /// <summary>
//    /// The swagger exclude schema filter class
//    /// </summary>
//    /// <seealso cref="ISchemaFilter"/>
//    /// <seealso cref="ISwaggerFilter"/>
//    [SchemaFilter]
//    public class SwaggerExcludeSchemaFilter : ISchemaFilter, ISwaggerFilter
//    {
//        /// <summary>
//        /// Applies the schema
//        /// </summary>
//        /// <param name="schema">The schema</param>
//        /// <param name="schemaFilterContext">The schema filter context</param>
//        public void Apply(OpenApiSchema schema, SchemaFilterContext schemaFilterContext)
//        {
//            if (schema?.Properties == null)
//                return;
//            foreach (
//                PropertyInfo propertyInfo in schemaFilterContext
//                    .Type.GetProperties()
//                    .Where(t =>
//                        t.GetCustomAttribute<SwaggerIgnoreAttribute>() != null
//                    )
//            )
//            {
//                PropertyInfo excludedProperty = propertyInfo;
//                string key = schema.Properties.Keys.SingleOrDefault(x =>
//                    InflectorExtensions.Underscore(x)
//                    == InflectorExtensions.Underscore(excludedProperty.Name)
//                );
//                if (key != null)
//                    schema.Properties.Remove(key);
//            }

//        }
//    }
//}
