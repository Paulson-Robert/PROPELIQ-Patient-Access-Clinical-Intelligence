namespace Infrastructure.AI;

/// <summary>
/// Configuration for the AI intake provider (OpenAI GPT-4o or compatible).
/// Bind from the "AiIntake" section in appsettings.
/// The ApiKey MUST be supplied via environment variable or secrets manager — never hardcoded.
/// </summary>
public sealed class AiIntakeOptions
{
    public const string SectionName = "AiIntake";

    /// <summary>API key for the LLM provider. Set via AIINTAKE__APIKEY environment variable.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model identifier. Default: "gpt-4o".</summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>OpenAI-compatible chat completions endpoint URL.</summary>
    public string CompletionsUrl { get; set; } = "https://api.openai.com/v1/chat/completions";

    /// <summary>Per-request HTTP timeout in seconds. Default: 30.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Maximum tokens to generate per AI response. Default: 500.</summary>
    public int MaxResponseTokens { get; set; } = 500;
}
