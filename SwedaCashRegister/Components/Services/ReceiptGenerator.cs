using SwedaCashRegister.Components.Custom;
using System.Text;

namespace SwedaCashRegister.Components.Services
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
                .Where(i => !i.IsTaxable && i.DeptName == Item.NON_TAX)
                .ToList();

            if (nonTaxItems.Count > 0)
            {
                foreach (var item in nonTaxItems)
                {
                    sb.AppendLine(FormatItemLine(item.DeptName, item.ItemAmount));
                }

                if (nonTaxItems.Count > 1)
                {
                    sb.AppendLine(FormatTotalLine("Non-Tax Subtotal", transaction.NonTaxTotal));
                    sb.AppendLine();
                }
            }

            // ---- Taxable items ----
            var taxItems = transaction.ItemList
                .Where(i => i.IsTaxable && i.DeptName == Item.TAXABLE)
                .ToList();

            if (taxItems.Count > 0)
            {
                foreach (var item in taxItems)
                {
                    sb.AppendLine(FormatItemLine(item.DeptName, item.ItemAmount));
                }

                if (taxItems.Count > 1)
                {
                    sb.AppendLine(FormatTotalLine("Taxable Subtotal", transaction.TaxableTotal));
                    sb.AppendLine();
                }
            }

            // ---- Tax / Rounding / Total ----
            if (transaction.TaxAmount > 0)
            {
                sb.AppendLine(FormatItemLine("Sales Tax", transaction.TaxAmount));
            }

            var roundingItem = transaction.ItemList.FirstOrDefault(i => i.ItemName == "RND");
            decimal roundingAmount = roundingItem?.ItemAmount ?? 0m;
            if (roundingAmount != 0)
            {
                sb.AppendLine(FormatItemLine("Rounding", roundingAmount));
            }

            sb.AppendLine(new string('-', ReceiptWidth));

            decimal total = transaction.TaxableTotal + transaction.NonTaxTotal + transaction.TaxAmount + roundingAmount;
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