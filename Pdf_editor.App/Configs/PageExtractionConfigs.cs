namespace Pdf_editor.App.Configs;

/// <summary>
/// Configuration for page extraction
/// </summary>
public record PageExtractionConfig
{
    public string InputPath { get; init; } = string.Empty;
    public string OutputPath { get; init; } = string.Empty;
    public int[] PageNumbers { get; init; } = [];
}