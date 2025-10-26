using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Pdf_editor.App.Interfaces;
using Pdf_editor.App.Results;

namespace Pdf_editor.App.Services;

/// <summary>
/// Service for PDF operations using iText 9.2.0 with cross-platform compatibility
/// </summary>
public class PdfService : IPdfService
{
    public async Task<bool> ExtractPagesAsync(string inputPath, string outputPath, int[] pageNumbers)
    {
        Console.WriteLine($"DEBUG: Starting extraction from '{inputPath}' to '{outputPath}'");
        Console.WriteLine($"DEBUG: Pages to extract: [{string.Join(", ", pageNumbers)}]");

        try
        {
            // Validate input file exists and is accessible
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"ERROR: Input file does not exist: {inputPath}");
                return false;
            }

            var inputFileInfo = new FileInfo(inputPath);
            Console.WriteLine($"DEBUG: Input file size: {inputFileInfo.Length} bytes");

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Console.WriteLine($"DEBUG: Creating output directory: {outputDir}");
                Directory.CreateDirectory(outputDir);
            }

            // Delete existing output file if it exists
            if (File.Exists(outputPath))
            {
                Console.WriteLine($"DEBUG: Deleting existing output file: {outputPath}");
                File.Delete(outputPath);
            }

            using var reader = new PdfReader(inputPath);
            Console.WriteLine($"DEBUG: PDF Reader created successfully");

            // Configure writer properties to avoid SmartMode issues
            var writerProperties = new WriterProperties();
            writerProperties.SetCompressionLevel(CompressionConstants.DEFAULT_COMPRESSION);
            
            using var writer = new PdfWriter(outputPath, writerProperties);
            Console.WriteLine($"DEBUG: PDF Writer created successfully");

            using var sourceDoc = new PdfDocument(reader);
            Console.WriteLine($"DEBUG: Source document opened. Total pages: {sourceDoc.GetNumberOfPages()}");

            using var targetDoc = new PdfDocument(writer);
            Console.WriteLine($"DEBUG: Target document created");

            var pagesToCopy = pageNumbers.Where(p => p > 0 && p <= sourceDoc.GetNumberOfPages())
                                        .OrderBy(p => p)
                                        .ToArray();

            Console.WriteLine($"DEBUG: Valid pages to copy: [{string.Join(", ", pagesToCopy)}]");

            if (!pagesToCopy.Any())
            {
                Console.WriteLine("ERROR: No valid page numbers provided after filtering.");
                return false;
            }

            // Copy specified pages to the target document
            Console.WriteLine($"DEBUG: Starting page copy operation...");
            sourceDoc.CopyPagesTo(pagesToCopy.ToList(), targetDoc);
            Console.WriteLine($"DEBUG: Page copy completed");

            // Add some content to ensure document is not empty
            targetDoc.GetDocumentInfo().SetTitle("Extracted Pages");
            targetDoc.GetDocumentInfo().SetCreator("PDF Editor App");
            Console.WriteLine($"DEBUG: Document metadata added");

            Console.WriteLine($"DEBUG: Extraction completed successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: PDF extraction failed: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"ERROR: Stack trace: {ex.StackTrace}");
            
            // Check if output file was created but is empty
            if (File.Exists(outputPath))
            {
                var outputFileInfo = new FileInfo(outputPath);
                Console.WriteLine($"ERROR: Output file exists but size is: {outputFileInfo.Length} bytes");
                
                if (outputFileInfo.Length == 0)
                {
                    Console.WriteLine("ERROR: Output file is empty, deleting it");
                    File.Delete(outputPath);
                }
            }
            
            return false;
        }
    }

    public BatchExtractionResult ExtractPagesToSeparateFiles(string inputPath, string outputFolder, int[] pageNumbers, string? baseFileName = null)
    {
        Console.WriteLine($"DEBUG: Starting split extraction from '{inputPath}' into folder '{outputFolder}'");
        Console.WriteLine($"DEBUG: Pages to extract: [{string.Join(", ", pageNumbers)}]");

        var createdFiles = new List<string>();

        try
        {
            if (!File.Exists(inputPath))
            {
                return new BatchExtractionResult
                {
                    Success = false,
                    Message = $"Input file does not exist: {inputPath}",
                    OutputFolder = outputFolder,
                    FilesCreated = 0,
                    CreatedFiles = createdFiles
                };
            }

            // Ensure output folder exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using var reader = new PdfReader(inputPath);
            using var sourceDoc = new PdfDocument(reader);
            var totalPages = sourceDoc.GetNumberOfPages();

            var validPages = pageNumbers
                .Where(p => p > 0 && p <= totalPages)
                .Distinct()
                .OrderBy(p => p)
                .ToArray();

            if (validPages.Length == 0)
            {
                return new BatchExtractionResult
                {
                    Success = false,
                    Message = "No valid pages to extract.",
                    OutputFolder = outputFolder,
                    FilesCreated = 0,
                    CreatedFiles = createdFiles
                };
            }

            var inputName = Path.GetFileNameWithoutExtension(inputPath);
            var safeBase = string.IsNullOrWhiteSpace(baseFileName) ? inputName : baseFileName.Trim();

            foreach (var page in validPages)
            {
                var outputPath = Path.Combine(outputFolder, $"{safeBase}_page_{page}.pdf");

                // Remove pre-existing file
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                var writerProps = new WriterProperties();
                writerProps.SetCompressionLevel(CompressionConstants.DEFAULT_COMPRESSION);

                using var writer = new PdfWriter(outputPath, writerProps);
                using var targetDoc = new PdfDocument(writer);

                sourceDoc.CopyPagesTo(page, page, targetDoc);

                // Add metadata
                targetDoc.GetDocumentInfo().SetTitle($"{safeBase} - Page {page}");
                targetDoc.GetDocumentInfo().SetCreator("PDF Editor App");

                createdFiles.Add(outputPath);
                Console.WriteLine($"DEBUG: Created file: {outputPath}");
            }

            return new BatchExtractionResult
            {
                Success = true,
                Message = $"Successfully extracted {createdFiles.Count} files.",
                OutputFolder = outputFolder,
                FilesCreated = createdFiles.Count,
                CreatedFiles = createdFiles
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Split extraction failed: {ex.Message}");
            return new BatchExtractionResult
            {
                Success = false,
                Message = $"Split extraction failed: {ex.Message}",
                OutputFolder = outputFolder,
                FilesCreated = createdFiles.Count,
                CreatedFiles = createdFiles
            };
        }
    }

    public async Task<int> GetPageCountAsync(string filePath)
    {
        Console.WriteLine($"DEBUG: Getting page count for: {filePath}");
        
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"ERROR: File does not exist: {filePath}");
                return 0;
            }

            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);
            var pageCount = pdfDoc.GetNumberOfPages();
            
            Console.WriteLine($"DEBUG: Page count for {Path.GetFileName(filePath)}: {pageCount}");
            return await Task.FromResult(pageCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to get page count for {filePath}: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> ConcatenatePdfsAsync(string[] inputPaths, string outputPath)
    {
        Console.WriteLine($"DEBUG: Starting concatenation to '{outputPath}'");
        Console.WriteLine($"DEBUG: Input files: [{string.Join(", ", inputPaths.Select(Path.GetFileName))}]");

        if (inputPaths == null || !inputPaths.Any())
        {
            Console.WriteLine("ERROR: No input paths provided");
            return false;
        }

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Console.WriteLine($"DEBUG: Creating output directory: {outputDir}");
                Directory.CreateDirectory(outputDir);
            }

            // Delete existing output file if it exists
            if (File.Exists(outputPath))
            {
                Console.WriteLine($"DEBUG: Deleting existing output file: {outputPath}");
                File.Delete(outputPath);
            }

            // Configure writer properties to avoid SmartMode issues
            var writerProperties = new WriterProperties();
            writerProperties.SetCompressionLevel(CompressionConstants.DEFAULT_COMPRESSION);
            
            using var writer = new PdfWriter(outputPath, writerProperties);
            Console.WriteLine($"DEBUG: PDF Writer created");

            using var mergedDoc = new PdfDocument(writer);
            Console.WriteLine($"DEBUG: Merged document created");
            
            var merger = new PdfMerger(mergedDoc);
            Console.WriteLine($"DEBUG: PDF Merger created");
            
            bool hasContent = false;
            int totalPagesCopied = 0;

            foreach (var inputPath in inputPaths)
            {
                Console.WriteLine($"DEBUG: Processing file: {Path.GetFileName(inputPath)}");
                
                if (!ValidateFile(inputPath))
                {
                    Console.WriteLine($"WARNING: Skipping invalid file: {Path.GetFileName(inputPath)}");
                    continue;
                }

                try
                {
                    using var reader = new PdfReader(inputPath);
                    using var sourceDoc = new PdfDocument(reader);
                    
                    var pageCount = sourceDoc.GetNumberOfPages();
                    Console.WriteLine($"DEBUG: Source document has {pageCount} pages");
                    
                    if (pageCount > 0)
                    {
                        Console.WriteLine($"DEBUG: Merging {pageCount} pages from {Path.GetFileName(inputPath)}");
                        merger.Merge(sourceDoc, 1, pageCount);
                        hasContent = true;
                        totalPagesCopied += pageCount;
                        Console.WriteLine($"DEBUG: Successfully merged {pageCount} pages");
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Source document has no pages: {Path.GetFileName(inputPath)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to process file {Path.GetFileName(inputPath)}: {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"DEBUG: Total pages copied: {totalPagesCopied}");

            if (hasContent)
            {
                // Add document info to ensure it's not empty
                mergedDoc.GetDocumentInfo().SetTitle("Concatenated PDF");
                mergedDoc.GetDocumentInfo().SetCreator("PDF Editor App");
                Console.WriteLine($"DEBUG: Document metadata added");
            }
            else
            {
                Console.WriteLine("ERROR: No content was added to the merged document");
            }

            Console.WriteLine($"DEBUG: Concatenation completed. Success: {hasContent}");
            return await Task.FromResult(hasContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: PDF concatenation failed: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"ERROR: Stack trace: {ex.StackTrace}");
            
            // Check if output file was created but is empty
            if (File.Exists(outputPath))
            {
                var outputFileInfo = new FileInfo(outputPath);
                Console.WriteLine($"ERROR: Output file exists but size is: {outputFileInfo.Length} bytes");
                
                if (outputFileInfo.Length == 0)
                {
                    Console.WriteLine("ERROR: Output file is empty, deleting it");
                    File.Delete(outputPath);
                }
            }
            
            return false;
        }
    }

    public bool ValidateFile(string filePath)
    {
        Console.WriteLine($"DEBUG: Validating file: {Path.GetFileName(filePath)}");
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ERROR: File does not exist: {filePath}");
            return false;
        }

        if (!Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"ERROR: File is not a PDF: {filePath}");
            return false;
        }

        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"DEBUG: File size: {fileInfo.Length} bytes");
        
        if (fileInfo.Length == 0)
        {
            Console.WriteLine($"ERROR: File is empty: {filePath}");
            return false;
        }

        try
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);
            var pageCount = pdfDoc.GetNumberOfPages();
            var isValid = pageCount > 0;
            
            Console.WriteLine($"DEBUG: PDF validation result for {Path.GetFileName(filePath)}: {(isValid ? "VALID" : "INVALID")} ({pageCount} pages)");
            return isValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: PDF validation failed for {Path.GetFileName(filePath)}: {ex.Message}");
            return false;
        }
    }
}