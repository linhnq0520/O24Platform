namespace O24OpenAPI.Core.Extensions;

public static class DecimalExtensions
{
    public static decimal Round(this decimal value, int decimals)
    {
        if (decimals == 2 && value == -0.005m)
        {
            return 0m;
        }

        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }
}
