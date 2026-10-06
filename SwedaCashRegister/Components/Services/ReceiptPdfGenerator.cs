using QuestPDF.Fluent;
using QuestPDF.Helpers;

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
                    page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(9));
                    page.Content().Text(receiptText);
                });
            });
        }
    }
}

