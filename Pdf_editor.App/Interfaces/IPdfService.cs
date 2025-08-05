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
}
