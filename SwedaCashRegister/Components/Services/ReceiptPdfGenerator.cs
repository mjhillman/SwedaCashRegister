using Microsoft.AspNetCore.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using SwedaCashRegister.Components.Custom;

namespace SwedaCashRegister.Components.Services
{
    public static class ReceiptPdfGenerator
    {
        public static Document GeneratePdf(string receiptText)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(9));
                    page.Content().Text(receiptText);
                });
            });
        }

        public static string SavePdfToDisk(string fileName, string text)
        {
            string folder = Path.Combine(Program.EXECUTING_DIRECTORY, "wwwroot", "receipts");
            Directory.CreateDirectory(folder); // Ensure the directory exists
            string path = Path.Combine(folder, fileName);
            File.WriteAllText(path, text);
            return path;
        }
    }
}

