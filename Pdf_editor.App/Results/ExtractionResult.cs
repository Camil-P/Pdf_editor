namespace Pdf_editor.App.Results;

/// <summary>
/// Represents the result of a page extraction operation
/// </summary>
public record ExtractionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? OutputPath { get; init; }
    public int ExtractedPages { get; init; }
}