namespace Pdf_editor.App.Configs;

/// <summary>
/// Configuration for extracting multiple pages into separate files
/// </summary>
public record MultiPageExtractionConfig
{
    public string InputPath { get; init; } = string.Empty;
    public string OutputFolder { get; init; } = string.Empty;
    public int[] PageNumbers { get; init; } = [];
    public string BaseFileName { get; init; } = string.Empty; // optional base name for output files
}
