using System.Text.RegularExpressions;
using Pdf_editor.App.Interfaces;

namespace Pdf_editor.App.Services;

/// <summary>
/// Service for handling user input operations
/// </summary>
public class InputService(IPdfService pdfService) : IInputService
{
    public InputService() : this(new PdfService())
    {
    }

    public string GetFilePath(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var input = Console.ReadLine()?.Trim().Trim('"');
            
            Console.WriteLine($"DEBUG: User input received: '{input}'");
            
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please provide a valid file path.");
                continue;
            }

            Console.WriteLine($"DEBUG: Checking if file exists: {input}");
            
            if (!File.Exists(input))
            {
                Console.WriteLine($"DEBUG: File.Exists returned false for: {input}");
                Console.WriteLine("File not found. Please check the path and try again.");
                continue;
            }

            Console.WriteLine($"DEBUG: File exists, checking extension");
            
            var extension = Path.GetExtension(input);
            Console.WriteLine($"DEBUG: File extension: '{extension}'");
            
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Please provide a PDF file.");
                continue;
            }

            Console.WriteLine($"DEBUG: File path validation successful: {input}");
            return input;
        }
    }

    public int[] GetPageNumbers(int totalPages)
    {
        Console.WriteLine($"DEBUG: GetPageNumbers called with totalPages: {totalPages}");
        Console.WriteLine($"\nPDF has {totalPages} pages.");
        Console.WriteLine("How would you like to specify pages?");
        Console.WriteLine("1. Sequential range (e.g., 1-5)");
        Console.WriteLine("2. Individual page numbers (e.g., 1,3,5,7)");
        Console.WriteLine("3. Mixed (e.g., 1-3,7,10-12)");
        
        while (true)
        {
            Console.Write("\nSelect option (1-3): ");
            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine($"DEBUG: User selected option: '{choice}'");

            switch (choice)
            {
                case "1":
                    return GetSequentialPages(totalPages);
                case "2":
                    return GetIndividualPages(totalPages);
                case "3":
                    return GetMixedPages(totalPages);
                default:
                    Console.WriteLine("Invalid choice. Please select 1, 2, or 3.");
                    break;
            }
        }
    }

    public string GetOutputPath(string defaultPath)
    {
        Console.WriteLine($"DEBUG: GetOutputPath called with defaultPath: '{defaultPath}'");
        Console.Write($"\nOutput file path (press Enter for '{defaultPath}'): ");
        var input = Console.ReadLine()?.Trim().Trim('"');
        
        Console.WriteLine($"DEBUG: User output path input: '{input}'");
        
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine($"DEBUG: Using default path: {defaultPath}");
            return defaultPath;
        }

        var directory = Path.GetDirectoryName(input);
        Console.WriteLine($"DEBUG: Output directory: '{directory}'");
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Console.WriteLine($"DEBUG: Output directory does not exist, attempting to create: {directory}");
            try
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"DEBUG: Successfully created directory: {directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Failed to create directory: {ex.Message}");
                Console.WriteLine("Could not create output directory. Using default path.");
                return defaultPath;
            }
        }

        Console.WriteLine($"DEBUG: Final output path: {input}");
        return input;
    }

    private int[] GetSequentialPages(int totalPages)
    {
        while (true)
        {
            Console.Write($"Enter page range (e.g., 1-5) [1-{totalPages}]: ");
            var input = Console.ReadLine()?.Trim();
            Console.WriteLine($"DEBUG: Sequential pages input: '{input}'");

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please provide a valid range.");
                continue;
            }

            var match = Regex.Match(input, @"^(\d+)-(\d+)$");
            if (!match.Success)
            {
                Console.WriteLine("Invalid format. Use format: 1-5");
                continue;
            }

            if (!int.TryParse(match.Groups[1].Value, out int start) || 
                !int.TryParse(match.Groups[2].Value, out int end))
            {
                Console.WriteLine("Invalid page numbers.");
                continue;
            }

            Console.WriteLine($"DEBUG: Parsed range: {start}-{end}");

            if (start < 1 || end > totalPages || start > end)
            {
                Console.WriteLine($"Page numbers must be between 1 and {totalPages}, and start <= end.");
                continue;
            }

            var result = Enumerable.Range(start, end - start + 1).ToArray();
            Console.WriteLine($"DEBUG: Sequential pages result: [{string.Join(", ", result)}]");
            return result;
        }
    }

    private int[] GetIndividualPages(int totalPages)
    {
        while (true)
        {
            Console.Write($"Enter page numbers separated by commas (e.g., 1,3,5) [1-{totalPages}]: ");
            var input = Console.ReadLine()?.Trim();
            Console.WriteLine($"DEBUG: Individual pages input: '{input}'");

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please provide valid page numbers.");
                continue;
            }

            var pageStrings = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var pages = new List<int>();

            bool isValid = true;
            foreach (var pageStr in pageStrings)
            {
                if (!int.TryParse(pageStr.Trim(), out int page) || page < 1 || page > totalPages)
                {
                    Console.WriteLine($"Invalid page number: {pageStr.Trim()}. Must be between 1 and {totalPages}.");
                    isValid = false;
                    break;
                }
                pages.Add(page);
            }

            if (isValid && pages.Any())
            {
                var result = pages.Distinct().OrderBy(p => p).ToArray();
                Console.WriteLine($"DEBUG: Individual pages result: [{string.Join(", ", result)}]");
                return result;
            }
        }
    }

    private int[] GetMixedPages(int totalPages)
    {
        while (true)
        {
            Console.Write($"Enter mixed format (e.g., 1-3,7,10-12) [1-{totalPages}]: ");
            var input = Console.ReadLine()?.Trim();
            Console.WriteLine($"DEBUG: Mixed pages input: '{input}'");

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please provide valid page specification.");
                continue;
            }

            var pages = new HashSet<int>();
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            bool isValid = true;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                
                if (trimmed.Contains('-'))
                {
                    var match = Regex.Match(trimmed, @"^(\d+)-(\d+)$");
                    if (!match.Success)
                    {
                        Console.WriteLine($"Invalid range format: {trimmed}");
                        isValid = false;
                        break;
                    }

                    if (!int.TryParse(match.Groups[1].Value, out int start) || 
                        !int.TryParse(match.Groups[2].Value, out int end))
                    {
                        Console.WriteLine($"Invalid range: {trimmed}");
                        isValid = false;
                        break;
                    }

                    if (start < 1 || end > totalPages || start > end)
                    {
                        Console.WriteLine($"Invalid range {trimmed}. Pages must be between 1 and {totalPages}.");
                        isValid = false;
                        break;
                    }

                    for (int i = start; i <= end; i++)
                    {
                        pages.Add(i);
                    }
                }
                else
                {
                    if (!int.TryParse(trimmed, out int page) || page < 1 || page > totalPages)
                    {
                        Console.WriteLine($"Invalid page number: {trimmed}. Must be between 1 and {totalPages}.");
                        isValid = false;
                        break;
                    }
                    pages.Add(page);
                }
            }

            if (isValid && pages.Any())
            {
                var result = pages.OrderBy(p => p).ToArray();
                Console.WriteLine($"DEBUG: Mixed pages result: [{string.Join(", ", result)}]");
                return result;
            }
        }
    }

    public string[] GetPdfFilesFromFolder()
    {
        while (true)
        {
            Console.Write("Enter folder path containing PDF files: ");
            var folderPath = Console.ReadLine()?.Trim().Trim('"');
            Console.WriteLine($"DEBUG: Folder path input: '{folderPath}'");

            if (string.IsNullOrEmpty(folderPath))
            {
                Console.WriteLine("Please provide a valid folder path.");
                continue;
            }

            Console.WriteLine($"DEBUG: Checking if directory exists: {folderPath}");
            
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder not found. Please check the path and try again.");
                continue;
            }

            // Get all PDF files in the folder
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
                                  .OrderBy(f => f)
                                  .ToArray();

            Console.WriteLine($"DEBUG: Found {pdfFiles.Length} PDF files");
            foreach (var file in pdfFiles)
            {
                Console.WriteLine($"DEBUG: Found file: {file}");
            }

            if (!pdfFiles.Any())
            {
                Console.WriteLine("No PDF files found in the specified folder.");
                continue;
            }

            Console.WriteLine($"Found {pdfFiles.Length} PDF files in the folder.");
            return pdfFiles;
        }
    }

    public string[] GetIndividualPdfFiles()
    {
        var pdfFiles = new List<string>();
        bool continueAdding = true;

        Console.WriteLine("Enter paths of PDF files to concatenate (one per line).");
        Console.WriteLine("Press Enter on an empty line when done.");

        while (continueAdding)
        {
            Console.Write($"PDF file path #{pdfFiles.Count + 1} (or Enter to finish): ");
            var input = Console.ReadLine()?.Trim().Trim('"');
            Console.WriteLine($"DEBUG: Individual file input: '{input}'");

            if (string.IsNullOrEmpty(input))
            {
                if (pdfFiles.Count > 0)
                {
                    Console.WriteLine($"DEBUG: Finished adding files. Total: {pdfFiles.Count}");
                    continueAdding = false;
                    continue;
                }
                else
                {
                    Console.WriteLine("Please provide at least one PDF file.");
                    continue;
                }
            }

            Console.WriteLine($"DEBUG: Checking if file exists: {input}");
            
            if (!File.Exists(input))
            {
                Console.WriteLine("File not found. Please check the path and try again.");
                continue;
            }

            if (!Path.GetExtension(input).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Please provide a PDF file.");
                continue;
            }

            Console.WriteLine($"DEBUG: Validating PDF file: {input}");
            
            if (pdfService.ValidateFile(input))
            {
                pdfFiles.Add(input);
                Console.WriteLine($"Added: {Path.GetFileName(input)}");
                Console.WriteLine($"DEBUG: Successfully added file. Total files: {pdfFiles.Count}");
            }
            else
            {
                Console.WriteLine("The file appears to be an invalid PDF. Please try another file.");
            }
        }

        Console.WriteLine($"DEBUG: Returning {pdfFiles.Count} files for concatenation");
        return pdfFiles.ToArray();
    }
}