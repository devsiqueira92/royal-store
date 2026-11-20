namespace RoyalStore.Domain.Labels;

/// <summary>The medium a label is printed on (065). Serialized as its number; the designer sends it so.</summary>
public enum LabelPaperType
{
    /// <summary>A page with rows and columns of labels.</summary>
    Sheet = 0,

    /// <summary>A continuous strip; the paper height is one label pitch.</summary>
    Roll = 1,
}
