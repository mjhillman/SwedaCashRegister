using SwedaCashRegister.Components.Custom;
using System.Text;

namespace SwedaCashRegister.Components.Services
{
    /// <summary>
    /// Generates a plain-text receipt for a Transaction, formatting header, footer, item lines, subtotals, sales tax,
    /// rounding, and the final total to a fixed receipt width suitable for thermal printers.
    /// </summary>
    /// <remarks>Uses a fixed ReceiptWidth to align descriptions and currency values, groups items into
    /// non-taxable and taxable sections, emits subtotals when multiple items exist in a section, includes Sales Tax and
    /// Rounding entries when present, and formats amounts with two decimal places and the current culture's currency
    /// symbol.</remarks>
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

        /// <summary>
        /// Formats a receipt line by aligning a description on the left and a currency-formatted amount on the right
        /// within a fixed receipt width.
        /// </summary>
        /// <remarks>Uses the ReceiptWidth constant to compute padding and enforces a minimum of one space
        /// if the combined length exceeds the width. Amount formatting uses the current culture's currency format with
        /// two decimal places ("C2").</remarks>
        /// <param name="description">Item description text displayed on the left side of the line.</param>
        /// <param name="amount">Monetary value formatted as currency with two decimal places and aligned to the right.</param>
        /// <returns>A single-line string containing the description, padding spaces, and the amount formatted as currency.</returns>
        private static string FormatItemLine(string description, decimal amount)
        {
            string amountStr = amount.ToString("C2");
            int padding = Math.Max(1, ReceiptWidth - description.Length - amountStr.Length);
            return description + new string(' ', padding) + amountStr;
        }

        /// <summary>
        /// Formats a receipt line with a left-aligned label and a right-aligned currency amount within ReceiptWidth.
        /// </summary>
        /// <remarks>Padding is computed to ensure the amount appears right-aligned and is at least one
        /// space; relies on the ReceiptWidth constant and the current culture for currency formatting.</remarks>
        /// <param name="label">Left-aligned label text for the line.</param>
        /// <param name="amount">Amount to display; formatted using the current culture's currency format with two decimal places (C2).</param>
        /// <returns>A single-line string combining the label, padding spaces, and the currency-formatted amount aligned to
        /// ReceiptWidth.</returns>
        private static string FormatTotalLine(string label, decimal amount)
        {
            string amountStr = amount.ToString("C2");
            int padding = Math.Max(1, ReceiptWidth - label.Length - amountStr.Length);
            return label + new string(' ', padding) + amountStr;
        }
    }
}