using Pdf_editor.App.Configs;
using Pdf_editor.App.Interfaces;
using Pdf_editor.App.Results;

namespace Pdf_editor.App.Services;

/// <summary>
/// Functionality for extracting multiple pages into separate PDF files
/// </summary>
public class MultiPageExtractionService : IServiceFunctionality
{
    private readonly IPdfService _pdfService;
    private readonly IInputService _inputService;

    public MultiPageExtractionService()
    {
        _pdfService = new PdfService();
        _inputService = new InputService();
    }

    public MultiPageExtractionService(IPdfService pdfService, IInputService inputService)
    {
        _pdfService = pdfService;
        _inputService = inputService;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n=== Split PDF Pages into Separate Files ===");

        try
        {
            // Get input file path
            var inputPath = _inputService.GetFilePath("Enter PDF file path");

            // Validate the PDF file
            if (!_pdfService.ValidateFile(inputPath))
            {
                Console.WriteLine("Error: Invalid or corrupted PDF file.\n");
                return;
            }

            // Get total page count
            var totalPages = await _pdfService.GetPageCountAsync(inputPath);
            if (totalPages == 0)
            {
                Console.WriteLine("Error: Could not read PDF or PDF has no pages.\n");
                return;
            }

            // Ask user which pages to split
            var pageNumbers = _inputService.GetPageNumbers(totalPages);

            // Default output folder is the input file's directory
            var inputDirectory = Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory;
            var inputFileName = Path.GetFileNameWithoutExtension(inputPath);
            var defaultFolder = inputDirectory;

            // Ask for output folder
            var outputFolder = _inputService.GetOutputFolderPath(defaultFolder);

            // Config
            var config = new MultiPageExtractionConfig
            {
                InputPath = inputPath,
                OutputFolder = outputFolder,
                PageNumbers = pageNumbers,
                BaseFileName = inputFileName
            };

            // Execute
            var result = SplitPages(config);

            // Display result
            DisplayResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}\n");
        }
    }

    private BatchExtractionResult SplitPages(MultiPageExtractionConfig config)
    {
        try
        {
            Console.WriteLine($"\nExtracting {config.PageNumbers.Length} page(s) into separate files...");
            var result = _pdfService.ExtractPagesToSeparateFiles(
                config.InputPath,
                config.OutputFolder,
                config.PageNumbers,
                config.BaseFileName);
            return result;
        }
        catch (Exception ex)
        {
            return new BatchExtractionResult
            {
                Success = false,
                Message = $"Error during split extraction: {ex.Message}",
                OutputFolder = config.OutputFolder,
                FilesCreated = 0,
                CreatedFiles = Array.Empty<string>()
            };
        }
    }

    private static void DisplayResult(BatchExtractionResult result)
    {
        Console.WriteLine();
        if (result.Success)
        {
            Console.WriteLine("? " + result.Message);
            Console.WriteLine($"  Output folder: {result.OutputFolder}");
            Console.WriteLine($"  Files created: {result.FilesCreated}");
            foreach (var file in result.CreatedFiles)
            {
                Console.WriteLine($"    - {Path.GetFileName(file)}");
            }
        }
        else
        {
            Console.WriteLine("? " + result.Message);
        }
        Console.WriteLine();
    }
}
