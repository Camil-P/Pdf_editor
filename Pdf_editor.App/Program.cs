using Pdf_editor.App.Interfaces;
using Pdf_editor.App.Services;

var functionalities = new Dictionary<string, IServiceFunctionality>
{
    { "1", new PageExtractionService() },
    { "2", new PdfConcatenationFunctionality() }
};

Console.WriteLine("=== PDF Processing Application ===\n");

while (true)
{
    try
    {
        DisplayMenu();
        var choice = Console.ReadLine()?.Trim();

        if (choice?.ToLower() == "q")
        {
            Console.WriteLine("Goodbye!");
            break;
        }

        if (functionalities.TryGetValue(choice ?? "", out var functionality))
        {
            await functionality.ExecuteAsync();
        }
        else
        {
            Console.WriteLine("Invalid choice. Please try again.\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
    }
}

return;

void DisplayMenu()
{
    Console.WriteLine("Available functionalities:");
    Console.WriteLine("1. Extract Pages from PDF");
    Console.WriteLine("2. Concatenate PDF Files");
    Console.WriteLine("Q. Quit");
    Console.Write("\nSelect an option: ");
}