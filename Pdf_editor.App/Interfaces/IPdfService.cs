using Pdf_editor.App.Results;

namespace Pdf_editor.App.Interfaces;

/// <summary>
/// Service interface for PDF operations
/// </summary>
public interface IPdfService
{
    Task<bool> ExtractPagesAsync(string inputPath, string outputPath, int[] pageNumbers);
    Task<bool> ConcatenatePdfsAsync(string[] inputPaths, string outputPath);
    Task<int> GetPageCountAsync(string filePath);
    bool ValidateFile(string filePath);
    
    // New: Extract specific pages into separate PDF files inside a target folder
    BatchExtractionResult ExtractPagesToSeparateFiles(
        string inputPath,
        string outputFolder,
        int[] pageNumbers,
        string? baseFileName = null);
}
