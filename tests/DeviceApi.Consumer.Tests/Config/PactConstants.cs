namespace DeviceApi.Consumer.Tests.Config;

/// <summary>
/// Single source of truth for all Pact-related constants shared across the
/// consumer project. Keeping names and paths here means a provider rename or
/// pact-broker URL change requires editing exactly one file.
/// </summary>
public static class PactConstants
{
    // ── Participant names ──────────────────────────────────────────────────

    /// <summary>Name of this consumer as it will appear in the pact file.</summary>
    public const string ConsumerName = "DeviceApi-Consumer";

    /// <summary>Name of the provider as it will appear in the pact file.</summary>
    public const string ProviderName = "DeviceApi";

    // ── File system paths ────────────────────────────────────────────────────

    /// <summary>
    /// Absolute path to the shared /contracts directory at the workspace root.
    /// Resolved at runtime so it works regardless of Debug/Release/framework
    /// output sub-folder depth.
    ///
    /// Depth from AppContext.BaseDirectory (bin/Debug/net10.0):
    ///   net10.0 → Debug → bin → DeviceApi.Consumer.Tests → tests → workspace root
    ///   = 5 levels up → then "contracts"
    /// </summary>
    public static readonly string PactDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "contracts"));

    /// <summary>Full path to the generated pact JSON file.</summary>
    public static string PactFilePath =>
        Path.Combine(PactDir, $"{ConsumerName}-{ProviderName}.json");
}
