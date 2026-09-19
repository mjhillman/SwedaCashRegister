using Microsoft.AspNetCore.Components;
using MudBlazor;
using QuestPDF.Fluent;
using SwedaCashRegister.Components.Custom;
using SwedaCashRegister.Components.Services;

namespace SwedaCashRegister.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public IReceiptTemplateProvider ReceiptTemplate { get; set; }
        [Inject] public IDialogService DialogService { get; set; }

        private enum TransactionStateEnum { New, ItemEntry, Total }
        private TransactionStateEnum _transactionState = TransactionStateEnum.New;

        private const int ButtonHeight = 40;
        private const int ButtonWidth = 40;
        private const int ButtonMargin = 15;
        private const double RowGap = 0.5; // rem — MUST match .button-row's margin-bottom in CSS

        private const int BigButtonWidth = 110;
        private const int BigButtonHeight = 120;

        private const int MedButtonWidth = 80;
        private const int MedButtonHeight = 60;

        private const int DeptButtonWidth = 40;
        private const int DeptButtonHeight = 200;

        private enum TaxKey { None, Taxable, NonTaxable }
        private TaxKey _taxSelection = TaxKey.None;
        private Transaction _currentTransaction = new Transaction();

        public string Digits { get; set; } = "000000";
        public string Suffix { get; set; } = "ST";

        // Each row's buttons, in display order: (Text, Color)
        private static readonly (string DisplayText, int Value, string Color)[][] RowValues = new[]
        {
        new[] { ("900", 900, "orange"), ("90", 90, "orange"), ("$9", 9, "white"), ("90", 90, "white"), ("9", 9, "green") },
        new[] { ("800", 800, "orange"), ("80", 80, "orange"), ("$8", 8, "white"), ("80", 80, "white"), ("8", 8, "green") },
        new[] { ("700", 700, "orange"), ("70", 70, "orange"), ("$7", 7, "white"), ("70", 70, "white"), ("7", 7, "green") },
        new[] { ("600", 600, "orange"), ("60", 60, "orange"), ("$6", 6, "white"), ("60", 60, "white"), ("6", 6, "green") },
        new[] { ("500", 500, "orange"), ("50", 50, "orange"), ("$5", 5, "white"), ("50", 50, "white"), ("5", 5, "green") },
        new[] { ("400", 400, "orange"), ("40", 40, "orange"), ("$4", 4, "white"), ("40", 40, "white"), ("4", 4, "green") },
        new[] { ("300", 300, "orange"), ("30", 30, "orange"), ("$3", 3, "white"), ("30", 30, "white"), ("3", 3, "green") },
        new[] { ("200", 200, "orange"), ("20", 20, "orange"), ("$2", 2, "white"), ("20", 20, "white"), ("2", 2, "green") },
        new[] { ("100", 100, "orange"), ("10", 10, "orange"), ("$1", 1, "white"), ("10", 10, "white"), ("1", 1, "green") },
    };

        // column index -> currently-down ROW (digit) in that column
        private readonly Dictionary<int, int> _downRowByColumn = new();

        private bool IsKeyDown(int row, int col) => _downRowByColumn.TryGetValue(col, out var downRow) && downRow == row;

        private void SelectKey(int row, int col)
        {
            try
            {
                if (IsKeyDown(row, col))
                {
                    _downRowByColumn.Remove(col);
                }
                else
                {
                    _downRowByColumn[col] = row;
                }

                decimal amt = 0;
                GetItemEntry(ref amt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SelectKey threw: {ex}");
                throw; // rethrow so you still see behavior, but now you'll have the real message first
            }
        }

        private void SelectTax(TaxKey key)
        {
            _taxSelection = key;
            HandleItem();
        }

        private void ResetAllKeys()
        {
            _taxSelection = TaxKey.None;
            _downRowByColumn.Clear();
        }

        private bool GetItemEntry(ref decimal itemAmount)
        {
            _transactionState = TransactionStateEnum.ItemEntry;
            int dollars = 0;   // from columns 0,1,2 (hundreds/tens/ones of dollars)
            int cents = 0;     // from columns 3,4 (tens/ones of cents)

            foreach (var (col, row) in _downRowByColumn)
            {
                var (_, value, _) = RowValues[row][col];

                if (col <= 2)
                    dollars += value;
                else
                    cents += value;
            }

            itemAmount = dollars + (cents / 100m);
            long totalCents = (long)Math.Round(itemAmount * 100m, MidpointRounding.AwayFromZero);
            totalCents = Math.Clamp(totalCents, 0, 999999);
            Digits = totalCents.ToString("D6");
            Suffix = "";


            if (_taxSelection == TaxKey.Taxable) return true;
            return false;
        }

        private void HandleReset()
        {
            _transactionState = TransactionStateEnum.New;
            _currentTransaction = new Transaction();
            ResetAllKeys();
            Digits = DisplayTotalDigits;
        }

        private void HandleItem()
        {
            _transactionState = TransactionStateEnum.ItemEntry;
            decimal itemAmount = 0;
            bool isTaxable = GetItemEntry(ref itemAmount);
            if (isTaxable)
            {
                _currentTransaction.ItemList.Add(new Item(Item.TAXABLE, true, 1, "TX Item", itemAmount));
            }
            else
            {
                _currentTransaction.ItemList.Add(new Item(Item.NON_TAX, false, 1, "NT Item", itemAmount));
            }

            Digits = DisplaySubtotalDigits;
            Suffix = "ST";
            ResetAllKeys();
        }

        private void HandleItemVoid(TaxKey taxKey)
        {
            if (_transactionState == TransactionStateEnum.New) return;
            _taxSelection = taxKey;
            _transactionState = TransactionStateEnum.ItemEntry;
            decimal itemAmount = 0;
            bool isTaxable = GetItemEntry(ref itemAmount);
            if (isTaxable)
            {
                _currentTransaction.ItemList.Add(new Item(Item.TAXABLE, true, 1, "Void TX", itemAmount * -1));
            }
            else
            {
                _currentTransaction.ItemList.Add(new Item(Item.NON_TAX, false, 1, "Void NT", itemAmount * -1));
            }
            Suffix = "ST";
            Digits = DisplaySubtotalDigits;
            ResetAllKeys();
        }

        private void HandleSubtotal()
        {
            _transactionState = TransactionStateEnum.Total;
            Suffix = "ST";
            Digits = DisplayTotalDigits;
        }

        private void HandleTotal()
        {
            _transactionState = TransactionStateEnum.Total;
            Suffix = "TL";
            Digits = DisplayTotalDigits;
            ResetAllKeys();
            _ = ShowReceipt();
            HandleReset();
        }

        private string DisplayTotalDigits
        {
            get
            {
                long totalCents = (long)Math.Round(_currentTransaction.CalculateTotal() * 100m, MidpointRounding.AwayFromZero);
                totalCents = Math.Clamp(totalCents, 0, 999999);
                return totalCents.ToString("D6");
            }
        }

        private string DisplaySubtotalDigits
        {
            get
            {
                long totalCents = (long)Math.Round(_currentTransaction.CalculateSubtotal() * 100m, MidpointRounding.AwayFromZero);
                totalCents = Math.Clamp(totalCents, 0, 999999);
                return totalCents.ToString("D6");
            }
        }

        private char GetDigit(int index) => index < Digits.Length ? Digits[index] : '0';

        private async Task ShowReceipt()
        {
            try
            {
                string receiptText = ReceiptGenerator.GenerateReceipt(_currentTransaction, ReceiptTemplate.Header, ReceiptTemplate.Footer);
                var pdfDocument = ReceiptPdfGenerator.GeneratePdf(receiptText);
                pdfDocument.GeneratePdfAndShow();
                string path = ReceiptPdfGenerator.SavePdfToDisk($"{DateTime.UtcNow:yyMMdd_HHmmss}.txt", receiptText);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBoxAsync("Error", $"An error occurred while generating the receipt: {ex.Message}");
            }
        }
    }
}
