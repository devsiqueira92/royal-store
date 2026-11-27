using Microsoft.EntityFrameworkCore.Metadata;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.Infrastructure.Persistence.Encryption;

/// <summary>A blind-indexed property: the shadow <see cref="HashProperty"/> mirrors <see cref="SourceProperty"/>.</summary>
internal sealed record BlindIndexDescriptor(string HashProperty, string SourceProperty, BlindIndexKind Kind);

public static class BlindIndex
{
    /// <summary>Shadow property (and column) name holding the blind index of <paramref name="sourceProperty"/>.</summary>
    public static string PropertyName(string sourceProperty) => sourceProperty + "Hash";

    internal static IReadOnlyList<BlindIndexDescriptor> Describe(IReadOnlyEntityType entityType) =>
        entityType.GetProperties()
            .Select(p => (Property: p,
                Source: p.FindAnnotation(EncryptionModelBuilderExtensions.BlindIndexSourceAnnotation)?.Value as string,
                Kind: p.FindAnnotation(EncryptionModelBuilderExtensions.BlindIndexKindAnnotation)?.Value))
            .Where(x => x.Source is not null && x.Kind is BlindIndexKind)
            .Select(x => new BlindIndexDescriptor(x.Property.Name, x.Source!, (BlindIndexKind)x.Kind!))
            .ToArray();
}
