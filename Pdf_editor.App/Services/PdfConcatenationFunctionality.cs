using Pdf_editor.App.Configs;
using Pdf_editor.App.Interfaces;
using Pdf_editor.App.Results;

namespace Pdf_editor.App.Services;

/// <summary>
/// Functionality for concatenating multiple PDF files
/// </summary>
public class PdfConcatenationFunctionality : IServiceFunctionality
{
    private readonly IPdfService _pdfService;
    private readonly IInputService _inputService;

    public PdfConcatenationFunctionality()
    {
        _pdfService = new PdfService();
        _inputService = new InputService();
    }

    public PdfConcatenationFunctionality(IPdfService pdfService, IInputService inputService)
    {
        _pdfService = pdfService;
        _inputService = inputService;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n=== PDF Concatenation ===");

        try
        {
            // Get concatenation mode
            var mode = GetConcatenationMode();
            
            // Get input files based on mode
            string[] inputFiles = mode switch
            {
                ConcatenationMode.FromFolder => _inputService.GetPdfFilesFromFolder(),
                ConcatenationMode.IndividualFiles => _inputService.GetIndividualPdfFiles(),
                _ => Array.Empty<string>()
            };

            if (!inputFiles.Any())
            {
                Console.WriteLine("No files selected for concatenation.\n");
                return;
            }

            // Validate all files and show summary
            var validFiles = ValidateAndShowSummary(inputFiles);
            if (!validFiles.Any())
            {
                Console.WriteLine("No valid PDF files to concatenate.\n");
                return;
            }

            // Generate default output path
            var defaultOutputPath = GenerateDefaultOutputPath(validFiles, mode);
            
            // Get output path
            var outputPath = _inputService.GetOutputPath(defaultOutputPath);

            // Create concatenation configuration
            var config = new ConcatenationConfig
            {
                InputPaths = validFiles,
                OutputPath = outputPath,
                Mode = mode
            };

            // Execute concatenation
            var result = await ConcatenatePdfsAsync(config);

            // Display result
            DisplayResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}\n");
        }
    }

    private ConcatenationMode GetConcatenationMode()
    {
        Console.WriteLine("\nHow would you like to specify PDF files?");
        Console.WriteLine("1. From folder (all PDF files in alphabetical order)");
        Console.WriteLine("2. Individual files (specify each file path)");

        while (true)
        {
            Console.Write("\nSelect option (1-2): ");
            var choice = Console.ReadLine()?.Trim();

            return choice switch
            {
                "1" => ConcatenationMode.FromFolder,
                "2" => ConcatenationMode.IndividualFiles,
                _ => GetInvalidChoiceAndRetry()
            };
        }

        ConcatenationMode GetInvalidChoiceAndRetry()
        {
            Console.WriteLine("Invalid choice. Please select 1 or 2.");
            return GetConcatenationMode();
        }
    }

    private string[] ValidateAndShowSummary(string[] inputFiles)
    {
        Console.WriteLine("\n=== Concatenation Summary ===");
        var validFiles = new List<string>();
        var totalPages = 0;

        for (int i = 0; i < inputFiles.Length; i++)
        {
            var file = inputFiles[i];
            if (_pdfService.ValidateFile(file))
            {
                var pageCount = _pdfService.GetPageCountAsync(file).Result;
                validFiles.Add(file);
                totalPages += pageCount;
                
                Console.WriteLine($"{i + 1}. {Path.GetFileName(file)} ({pageCount} pages)");
            }
            else
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(file)} (INVALID - SKIPPED)");
            }
        }

        if (validFiles.Any())
        {
            Console.WriteLine($"\nTotal: {validFiles.Count} files, {totalPages} pages");
            Console.Write("\nProceed with concatenation? (y/n): ");
            
            if (Console.ReadLine()?.Trim().ToLower() != "y")
            {
                return Array.Empty<string>();
            }
        }

        return validFiles.ToArray();
    }

    private string GenerateDefaultOutputPath(string[] inputFiles, ConcatenationMode mode)
    {
        var firstFileDirectory = Path.GetDirectoryName(inputFiles.First()) ?? Environment.CurrentDirectory;
        
        var outputName = mode switch
        {
            ConcatenationMode.FromFolder => "concatenated_folder_pdfs.pdf",
            ConcatenationMode.IndividualFiles => "concatenated_files.pdf",
            _ => "concatenated.pdf"
        };

        return Path.Combine(firstFileDirectory, outputName);
    }

    private async Task<ExtractionResult> ConcatenatePdfsAsync(ConcatenationConfig config)
    {
        try
        {
            Console.WriteLine($"\nConcatenating {config.InputPaths.Length} PDF files...");

            var success = await _pdfService.ConcatenatePdfsAsync(config.InputPaths, config.OutputPath);

            if (!success)
                return new ExtractionResult
                {
                    Success = false,
                    Message = "Failed to concatenate PDF files. Please check the input files and try again."
                };
            
            // Calculate total pages in output
            var totalPages = 0;
            foreach (var file in config.InputPaths)
            {
                totalPages += await _pdfService.GetPageCountAsync(file);
            }

            return new ExtractionResult
            {
                Success = true,
                Message = "PDF files concatenated successfully!",
                OutputPath = config.OutputPath,
                ExtractedPages = totalPages
            };

        }
        catch (Exception ex)
        {
            return new ExtractionResult
            {
                Success = false,
                Message = $"Error during concatenation: {ex.Message}"
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
            Console.WriteLine($"  Total pages: {result.ExtractedPages}");
        }
        else
        {
            Console.WriteLine("✗ " + result.Message);
        }
        Console.WriteLine();
    }
}