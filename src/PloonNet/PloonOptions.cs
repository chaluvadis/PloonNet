namespace PloonNet;

/// <summary>
/// Options for PLOON stringify operation
/// </summary>
public class StringifyOptions
{
    /// <summary>
    /// Output format (standard or compact)
    /// </summary>
    public PloonFormat Format { get; set; } = PloonFormat.Standard;

    /// <summary>
    /// Custom configuration (overrides defaults)
    /// </summary>
    public PloonConfig? Config { get; set; }
}

/// <summary>
/// Options for PLOON parse operation
/// </summary>
public class ParseOptions
{
    /// <summary>
    /// Strict mode - validate schema consistency
    /// </summary>
    public bool Strict { get; set; } = true;

    /// <summary>
    /// Custom configuration (overrides defaults)
    /// </summary>
    public PloonConfig? Config { get; set; }
}

/// <summary>
/// PLOON output format
/// </summary>
public enum PloonFormat
{
    /// <summary>
    /// Human-readable format with newlines
    /// </summary>
    Standard,

    /// <summary>
    /// Machine-optimized compact format with semicolons
    /// </summary>
    Compact
}
