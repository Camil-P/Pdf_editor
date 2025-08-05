using Pdf_editor.App.Configs;
using Pdf_editor.App.Interfaces;
using Pdf_editor.App.Results;

namespace Pdf_editor.App.Services;

/// <summary>
/// Functionality for extracting pages from PDF files
/// </summary>
public class PageExtractionService(IPdfService pdfService, IInputService inputService) : IServiceFunctionality
{
    public PageExtractionService() : this(new PdfService(), new InputService())
    {
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n=== PDF Page Extraction ===");

        try
        {
            // Get input file path
            var inputPath = inputService.GetFilePath("Enter PDF file path");

            // Validate the PDF file
            if (!pdfService.ValidateFile(inputPath))
            {
                Console.WriteLine("Error: Invalid or corrupted PDF file.\n");
                return;
            }

            // Get total page count
            var totalPages = await pdfService.GetPageCountAsync(inputPath);
            if (totalPages == 0)
            {
                Console.WriteLine("Error: Could not read PDF or PDF has no pages.\n");
                return;
            }

            // Get pages to extract
            var pageNumbers = inputService.GetPageNumbers(totalPages);

            // Generate default output path
            var inputFileName = Path.GetFileNameWithoutExtension(inputPath);
            var inputDirectory = Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory;
            var defaultOutputPath = Path.Combine(inputDirectory, $"{inputFileName}_extracted.pdf");

            // Get output path
            var outputPath = inputService.GetOutputPath(defaultOutputPath);

            // Create extraction configuration
            var config = new PageExtractionConfig
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                PageNumbers = pageNumbers
            };

            // Execute extraction
            var result = await ExtractPagesAsync(config);

            // Display result
            DisplayResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}\n");
        }
    }

    private async Task<ExtractionResult> ExtractPagesAsync(PageExtractionConfig config)
    {
        try
        {
            Console.WriteLine($"\nExtracting pages {string.Join(", ", config.PageNumbers)}...");

            var success = await pdfService.ExtractPagesAsync(
                config.InputPath, 
                config.OutputPath, 
                config.PageNumbers);

            if (success)
            {
                return new ExtractionResult
                {
                    Success = true,
                    Message = "Pages extracted successfully!",
                    OutputPath = config.OutputPath,
                    ExtractedPages = config.PageNumbers.Length
                };
            }
            else
            {
                return new ExtractionResult
                {
                    Success = false,
                    Message = "Failed to extract pages. Please check the input file and try again."
                };
            }
        }
        catch (Exception ex)
        {
            return new ExtractionResult
            {
                Success = false,
                Message = $"Error during extraction: {ex.Message}"
            };
        }
    }

    private static void DisplayResult(ExtractionResult result)
    {
        Console.WriteLine();
        if (result.Success)
        {
            Console.WriteLine("✓ " + result.Message);
            Console.WriteLine($"  Output file: {result.OutputPath}");
            Console.WriteLine($"  Pages extracted: {result.ExtractedPages}");
        }
        else
        {
            Console.WriteLine("✗ " + result.Message);
        }
        Console.WriteLine();
    }
}