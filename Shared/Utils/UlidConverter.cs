using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Utils;

public class UlidConverter<TProvider>() : ValueConverter<Ulid, TProvider>(
    ConvertToProviderExpression,
    ConvertFromProviderExpression,
    DefaultHints
)
{
    private static readonly ConverterMappingHints DefaultHints = null!;
    private new static readonly Expression<Func<Ulid, TProvider>> ConvertToProviderExpression = null!;
    private new static readonly Expression<Func<TProvider, Ulid>> ConvertFromProviderExpression = null!;

    static UlidConverter()
    {
        Type providerType = typeof(TProvider);

        if (providerType == typeof(byte[]))
        {
            DefaultHints = new ConverterMappingHints(16);
            ConvertToProviderExpression = ulid => (TProvider)(object)ulid.ToByteArray();
            ConvertFromProviderExpression = value => new Ulid((byte[])(object)value!);
        }
        else if (providerType == typeof(string))
        {
            DefaultHints = new ConverterMappingHints(26);
            ConvertToProviderExpression = ulid => (TProvider)(object)ulid.ToString();
            ConvertFromProviderExpression = value => Ulid.Parse((string)(object)value!);
        }
    }
}
