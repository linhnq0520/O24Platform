namespace O24OpenAPI.Framework.Abstractions;

public interface IValidator<T>
{
    Task ValidateAsync(T model);
}
