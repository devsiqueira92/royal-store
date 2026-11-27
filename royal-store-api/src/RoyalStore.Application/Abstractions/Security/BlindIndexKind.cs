namespace RoyalStore.Application.Abstractions.Security;

/// <summary>How a value is normalized before its blind index is computed.</summary>
public enum BlindIndexKind
{
    /// <summary>Trimmed, Unicode NFC. Case-sensitive.</summary>
    Text = 0,

    /// <summary>Trimmed, Unicode NFC, lower-cased (invariant).</summary>
    Email = 1,

    /// <summary>ASCII digits only — CPF, CNPJ, phone ("123.456.789-09" ≡ "12345678909").</summary>
    Digits = 2,
}
