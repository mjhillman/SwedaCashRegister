using Microsoft.AspNetCore.Components;
using SwedaCashRegister.Components.Custom;
using HITS.Blazor.PushButton;

namespace SwedaCashRegister.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private enum TransactionStateEnum { New, ItemEntry, Total }
        private TransactionStateEnum _transactionState = TransactionStateEnum.New;

        private const int ButtonHeight = 40;
        private const int ButtonWidth = 40;
        private const double RowGap = 0.5; // rem — MUST match .button-row's margin-bottom in CSS

        private const int BigButtonWidth = 90;
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
        private static readonly (string Text, string Color)[][] RowValues = new[]
        {
        new[] { ("900","white"), ("90","white"), ("9","white"), ("90","green"), ("9","green") },
        new[] { ("800","white"), ("80","white"), ("8","white"), ("80","green"), ("8","green") },
        new[] { ("700","white"), ("70","white"), ("7","white"), ("70","green"), ("7","green") },
        new[] { ("600","white"), ("60","white"), ("6","white"), ("60","green"), ("6","green") },
        new[] { ("500","white"), ("50","white"), ("5","white"), ("50","green"), ("5","green") },
        new[] { ("400","white"), ("40","white"), ("4","white"), ("40","green"), ("4","green") },
        new[] { ("300","white"), ("30","white"), ("3","white"), ("30","green"), ("3","green") },
        new[] { ("200","white"), ("20","white"), ("2","white"), ("20","green"), ("2","green") },
        new[] { ("100","white"), ("10","white"), ("1","white"), ("10","green"), ("1","green") },
    };

        // column index -> currently-down ROW (digit) in that column
        private readonly Dictionary<int, int> _downRowByColumn = new();

        private bool IsKeyDown(int row, int col) =>
            _downRowByColumn.TryGetValue(col, out var downRow) && downRow == row;

        private void SelectKey(int row, int col)
        {
            _downRowByColumn[col] = row;
        }

        private void SelectTax(TaxKey key)
        {
            _taxSelection = key;
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
                var (text, _) = RowValues[row][col];
                int value = int.Parse(text);

                if (col <= 2)
                    dollars += value;
                else
                    cents += value;
            }

            itemAmount = dollars + (cents / 100m);

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
                _currentTransaction.ItemList.Add(new Item("Taxable", true, 1, "Item", itemAmount));
            }
            else
            {
                _currentTransaction.ItemList.Add(new Item("Non-Tax", false, 1, "Item", itemAmount));
            }
            _currentTransaction.CalculateSubtotal();
            Suffix = "ST";
            Digits = DisplaySubtotalDigits;
            ResetAllKeys();
        }

        private void HandleItemVoid()
        {
            _transactionState = TransactionStateEnum.ItemEntry;
            decimal itemAmount = 0;
            bool isTaxable = GetItemEntry(ref itemAmount);
            if (isTaxable)
            {
                _currentTransaction.ItemList.Add(new Item("Taxable", true, 1, "Void", itemAmount * -1));
            }
            else
            {
                _currentTransaction.ItemList.Add(new Item("Non-Tax", false, 1, "Void", itemAmount * -1));
            }
            _currentTransaction.CalculateSubtotal();
            Suffix = "ST";
            Digits = DisplaySubtotalDigits;
            ResetAllKeys();

        }

        private void HandleTotal()
        {
            _transactionState = TransactionStateEnum.Total;
            _currentTransaction.CalculateTotal();
            Suffix = "TL";
            Digits = DisplayTotalDigits;
            ResetAllKeys();
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


    }
}
