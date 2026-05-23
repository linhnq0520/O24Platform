using Newtonsoft.Json;

namespace O24OpenAPI.Framework.Extensions;

public static class JsonExtension
{
    public static T JsonConvertToModels<T>(this object objectValue)
    {
        if (objectValue == null)
        {
            return default;
        }

        try
        {
            if (objectValue is string v)
            {
                return JsonConvert.DeserializeObject<T>(v);
            }
            else
            {
                string strObject = JsonConvert.SerializeObject(objectValue);
                return JsonConvert.DeserializeObject<T>(strObject);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return default;
    }
}
