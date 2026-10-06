using System.Text;

namespace SwedaCashRegister.Components.Custom
{
    public static class ReceiptGenerator
    {
        private const int ReceiptWidth = 40; // typical thermal-printer width; adjust to your printer

        public static string GenerateReceipt(Transaction transaction, string header, string footer)
        {
            var sb = new StringBuilder();

            // ---- Header ----
            sb.AppendLine(header.TrimEnd());
            sb.AppendLine();
            sb.AppendLine(DateTime.Now.ToString("MM/dd/yyyy   hh:mm:ss tt"));
            sb.AppendLine(new string('-', ReceiptWidth));

            // ---- Non-taxable items ----
            var nonTaxItems = transaction.ItemList
                .Where(i => !i.IsTaxable && i.DeptName != "RND")
                .ToList();

            if (nonTaxItems.Count > 0)
            {
                foreach (var item in nonTaxItems)
                    sb.AppendLine(FormatItemLine(item.ItemName, item.ItemAmount));

                sb.AppendLine(FormatTotalLine("Non-Taxable Subtotal", transaction.NonTaxTotal));
                sb.AppendLine();
            }

            // ---- Taxable items ----
            var taxItems = transaction.ItemList
                .Where(i => i.IsTaxable)
                .ToList();

            if (taxItems.Count > 0)
            {
                foreach (var item in taxItems)
                    sb.AppendLine(FormatItemLine(item.ItemName, item.ItemAmount));

                sb.AppendLine(FormatTotalLine("Taxable Subtotal", transaction.TaxableTotal));
                sb.AppendLine();
            }

            // ---- Tax / Rounding / Total ----
            sb.AppendLine(FormatTotalLine("Tax", transaction.TaxAmount));

            var roundingItem = transaction.ItemList.FirstOrDefault(i => i.ItemName == "RND");
            decimal roundingAmount = roundingItem?.ItemAmount ?? 0m;
            sb.AppendLine(FormatTotalLine("Rounding", roundingAmount));

            sb.AppendLine(new string('-', ReceiptWidth));

            decimal total = transaction.TaxableTotal + transaction.NonTaxTotal
                             + transaction.TaxAmount + roundingAmount;
            sb.AppendLine(FormatTotalLine("TOTAL", total));
            sb.AppendLine(new string('=', ReceiptWidth));
            sb.AppendLine();

            // ---- Footer ----
            sb.AppendLine(footer.TrimEnd());

            return sb.ToString();
        }

        private static string FormatItemLine(string description, decimal amount)
        {
            string amountStr = amount.ToString("C2");
            int padding = Math.Max(1, ReceiptWidth - description.Length - amountStr.Length);
            return description + new string(' ', padding) + amountStr;
        }

        private static string FormatTotalLine(string label, decimal amount)
        {
            string amountStr = amount.ToString("C2");
            int padding = Math.Max(1, ReceiptWidth - label.Length - amountStr.Length);
            return label + new string(' ', padding) + amountStr;
        }
    }
}
