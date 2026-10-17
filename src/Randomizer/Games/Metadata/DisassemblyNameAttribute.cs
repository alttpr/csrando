namespace Randomizer.Games.Metadata;

using System;

/// <summary>
/// This attribute is used to annotate where in a public disassembly project information, etc. comes from.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public sealed class DisassemblyNameAttribute : Attribute
{
    /// <summary>
    /// The name of the file this information comes from.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The name of the symbol, if applicable.
    /// </summary>
    public string? SymbolName { get; init; }

    /// <summary>
    /// The address of the data.
    /// This should be used to supplement <see cref="SymbolName"/> in case the name changes.
    /// </summary>
    public int Address { get; init; } = -1;
}
