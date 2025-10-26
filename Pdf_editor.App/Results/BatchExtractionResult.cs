namespace Pdf_editor.App.Results;

/// <summary>
/// Represents the result of a batch page extraction where pages are split into separate files
/// </summary>
public record BatchExtractionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string OutputFolder { get; init; } = string.Empty;
    public int FilesCreated { get; init; }
    public IReadOnlyList<string> CreatedFiles { get; init; } = Array.Empty<string>();
}
