namespace Pdf_editor.App.Configs;

/// <summary>
/// Configuration for PDF concatenation
/// </summary>
public record ConcatenationConfig
{
    public string[] InputPaths { get; init; } = [];
    public string OutputPath { get; init; } = string.Empty;
    public ConcatenationMode Mode { get; init; }
}

/// <summary>
/// Modes for PDF concatenation
/// </summary>
public enum ConcatenationMode
{
    FromFolder,
    IndividualFiles
}