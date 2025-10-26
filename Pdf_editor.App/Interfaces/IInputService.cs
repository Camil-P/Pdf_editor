namespace Pdf_editor.App.Interfaces;

/// <summary>
/// Service interface for user input operations
/// </summary>
public interface IInputService
{
    string GetFilePath(string prompt);
    int[] GetPageNumbers(int totalPages);
    string GetOutputPath(string defaultPath);
    string GetOutputFolderPath(string defaultFolder);
    string[] GetPdfFilesFromFolder();
    string[] GetIndividualPdfFiles();
}