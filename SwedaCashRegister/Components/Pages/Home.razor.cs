using Microsoft.AspNetCore.Components;
using MudBlazor;
using SwedaCashRegister.Components.Custom;
using SwedaCashRegister.Components.Services;

namespace SwedaCashRegister.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public IReceiptTemplateProvider ReceiptTemplate { get; set; }
        [Inject] public IDialogService DialogService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private decimal _lastAmount = 0m; //enables pressing the department key again for multiple items of the same price

        private const int NameplateButtonWidth = 100;
        private const int NameplateButtonHeight = 40;

        private const int ButtonHeight = 40;
        private const int ButtonWidth = 40;
        private const int ButtonMargin = 15;
        private const double RowGap = 0.5; // rem — MUST match .button-row's margin-bottom in CSS

        private const int BigButtonWidth = 110;
        private const int BigButtonHeight = 120;

        private enum TaxKey { None, Taxable, NonTaxable }
        private TaxKey _taxSelection = TaxKey.None;

        private Transaction _currentTransaction;
        private bool TotalButtonDisabled = true;

        public string Digits { get; set; } = "000000";
        public string Suffix { get; set; } = "ST";

        /// <summary>
        /// Button definitions organized by row in display order; each tuple contains display text, numeric value, and color.
        /// </summary>
        /// <remarks>Each inner array contains five button tuples. Rows are ordered from highest to lowest denomination (900 through 100).</remarks>
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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _currentTransaction = new Transaction(ReceiptTemplate.TaxRate);
        }

        // column index -> currently-down ROW (digit) in that column
        private readonly Dictionary<int, int> _downRowByColumn = new();

        /// <summary>
        /// Determines whether the key at the specified row and column is currently down.
        /// </summary>
        /// <param name="row">Zero-based row index of the key.</param>
        /// <param name="col">Zero-based column index of the key.</param>
        /// <returns>True if the recorded down row for the column equals the specified row; otherwise, false.</returns>
        private bool IsKeyDown(int row, int col) => _downRowByColumn.TryGetValue(col, out var downRow) && downRow == row;

        /// <summary>
        /// Toggle the selection state of the key at the specified row and column, update the internal _downRowByColumn
        /// mapping, and invoke GetItemEntry to refresh item entry state.
        /// </summary>
        /// <remarks>Writes any caught exception to the console and rethrows it. Modifies the
        /// _downRowByColumn mapping and calls GetItemEntry(ref decimal) as a side effect.</remarks>
        /// <param name="row">The zero-based row index of the key.</param>
        /// <param name="col">The zero-based column index of the key.</param>
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

        /// <summary>
        /// Parses digit inputs from _downRowByColumn to compute and set the item monetary amount, updates Digits,
        /// Suffix, and the stored _lastAmount, and determines taxability.
        /// </summary>
        /// <remarks>Columns 0–2 contribute dollars (hundreds/tens/ones) and columns 3–4 contribute cents
        /// (tens/ones). The amount is rounded to the nearest cent using MidpointRounding.AwayFromZero, clamped to
        /// 0–999,999 cents, and Digits is set to a six-digit, zero-padded cents string. Suffix is cleared.</remarks>
        /// <param name="itemAmount">Reference parameter set to the computed item amount in decimal dollars; if the parsed amount is zero, it is
        /// replaced with the previous nonzero amount. Assumption that the user has multiple items of the same price.</param>
        /// <returns>true when _taxSelection equals TaxKey.Taxable; otherwise false.</returns>
        private bool GetItemEntry(ref decimal itemAmount)
        {
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
            if (itemAmount == 0m && _lastAmount > 0m)
            {
                itemAmount = _lastAmount;
            }
            else
            {
                _lastAmount = itemAmount;
            }

            long totalCents = (long)Math.Round(itemAmount * 100m, MidpointRounding.AwayFromZero);
            totalCents = Math.Clamp(totalCents, 0, 999999);
            Digits = totalCents.ToString("D6");
            Suffix = "";

            if (_taxSelection == TaxKey.Taxable) return true;
            return false;
        }

        /// <summary>
        /// Reset internal state to initial, transaction-ready values.
        /// </summary>
        /// <remarks>Disables the total button, clears the last amount, creates a new Transaction using
        /// ReceiptTemplate.TaxRate, resets all input keys, and restores the display digit count to
        /// DisplayTotalDigits.</remarks>
        private void HandleReset()
        {
            TotalButtonDisabled = true;
            _lastAmount = 0m;
            _currentTransaction = new Transaction(ReceiptTemplate.TaxRate);
            ResetAllKeys();
            Digits = DisplayTotalDigits;
        }

        /// <summary>
        /// Processes the current item entry, adds a taxable or non-taxable Item with quantity 1 to the current
        /// transaction's ItemList, enables the total button, sets display digits to subtotal digits, sets the suffix to
        /// ST, and resets input keys.
        /// </summary>
        /// <remarks>Determines amount and taxability via GetItemEntry(ref itemAmount). Adds an Item using
        /// Item.TAXABLE or Item.NON_TAX with descriptions TX Item or NT Item and quantity 1. Mutates
        /// _currentTransaction, TotalButtonDisabled, Digits, Suffix, and input key state.</remarks>
        private void HandleItem()
        {
            TotalButtonDisabled = false;
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

        /// <summary>
        /// Record a voided item using the specified tax key by adding a negative taxable or non-taxable item entry to
        /// the current transaction, update subtotal display state, and reset input keys.
        /// </summary>
        /// <remarks>Retrieves the item amount via GetItemEntry, negates it, and appends an Item with
        /// description "Void TX" for taxable items or "Void NT" for non-taxable items. Updates _taxSelection, sets
        /// Suffix to "ST" and Digits to DisplaySubtotalDigits, and calls ResetAllKeys(). Modifies
        /// _currentTransaction.ItemList.</remarks>
        /// <param name="taxKey">Tax key used to select the tax treatment for the voided item.</param>
        private void HandleItemVoid(TaxKey taxKey)
        {
            _taxSelection = taxKey;
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

        /// <summary>
        /// Sets the Suffix property to "ST" and the Digits property to DisplayTotalDigits.
        /// </summary>
        /// <remarks>Applies display settings for subtotal values by selecting the subtotal suffix and the
        /// number of display digits.</remarks>
        private void HandleSubtotal()
        {
            Suffix = "ST";
            Digits = DisplayTotalDigits;
        }

        /// <summary>
        /// Sets Suffix to 'TL', configures Digits to DisplayTotalDigits, resets input keys, shows the receipt
        /// asynchronously, and resets the state.
        /// </summary>
        /// <remarks>Exceptions are caught and reported via DialogService.ShowMessageBoxAsync.</remarks>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        private async Task HandleTotal()
        {
            try
            {
                Suffix = "TL";
                Digits = DisplayTotalDigits;
                ResetAllKeys();
                await ShowReceipt();
                HandleReset();
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBoxAsync("Error", $"An error occurred while generating the receipt: {ex.Message}");
            }
        }

        private void HandleReports()
        {
            NavigationManager.NavigateTo("/reports");
        }

        private void HandleSettings()
        {
            NavigationManager.NavigateTo("/settings");
        }

        /// <summary>
        /// Gets a six-digit, zero-padded string representing the current transaction total in cents.
        /// </summary>
        /// <remarks>Total is computed by multiplying the transaction total by 100, rounding to the
        /// nearest cent using MidpointRounding.AwayFromZero, clamped to the range 0 to 999,999, and formatted with the
        /// D6 numeric format.</remarks>
        private string DisplayTotalDigits
        {
            get
            {
                long totalCents = (long)Math.Round(_currentTransaction.CalculateTotal() * 100m, MidpointRounding.AwayFromZero);
                totalCents = Math.Clamp(totalCents, 0, 999999);
                return totalCents.ToString("D6");
            }
        }

        /// <summary>
        /// String containing the transaction subtotal in cents, rounded away from zero, clamped to 0–999999 and
        /// formatted as a six-digit decimal with leading zeros.
        /// </summary>
        /// <remarks>Calculated from _currentTransaction.CalculateSubtotal(), multiplied by 100 and
        /// rounded using MidpointRounding.AwayFromZero. The resulting cent value is clamped to the range 0 to 999999
        /// and formatted with the "D6" specifier.</remarks>
        private string DisplaySubtotalDigits
        {
            get
            {
                long totalCents = (long)Math.Round(_currentTransaction.CalculateSubtotal() * 100m, MidpointRounding.AwayFromZero);
                totalCents = Math.Clamp(totalCents, 0, 999999);
                return totalCents.ToString("D6");
            }
        }

        /// <summary>
        /// Gets the digit at the specified index from Digits, or '0' if the index is out of range.
        /// </summary>
        /// <remarks>A negative index will cause an IndexOutOfRangeException when accessing
        /// Digits.</remarks>
        /// <param name="index">Index of the digit to retrieve.</param>
        /// <returns>The digit character at the specified index, or '0' if the index is out of range.</returns>
        private char GetDigit(int index) => index < Digits.Length ? Digits[index] : '0';

        /// <summary>
        /// Generate and display a receipt dialog for the current transaction; save the receipt to a timestamped file
        /// and append a summary line to the daily report.
        /// </summary>
        /// <remarks>Creates receipt text from the current transaction, writes a timestamped receipt file,
        /// appends a CSV summary to a daily report file, and shows the receipt dialog. Exceptions are caught and
        /// reported via a message box.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ShowReceipt()
        {
            try
            {
                //save transaction detail to disk
                string receiptFileName = $"{DateTime.UtcNow:yyMMdd_HHmmss}.txt";
                string reportFileName = $"{DateTime.UtcNow:yyMMdd}.txt";
                string receiptText = ReceiptGenerator.GenerateReceipt(_currentTransaction, ReceiptTemplate.Header, ReceiptTemplate.Footer);
                string path = RecordTransaction.WriteTranaction($"{receiptFileName}", receiptText);

                // Append transaction summary to report file
                var roundingItem = _currentTransaction.ItemList.FirstOrDefault(i => i.ItemName == "RND");
                decimal roundingAmount = roundingItem?.ItemAmount ?? 0m;

                string reportText = $"{Path.GetFileNameWithoutExtension(receiptFileName)},{_currentTransaction.TaxableTotal},{_currentTransaction.NonTaxTotal}, {_currentTransaction.TaxAmount},{roundingAmount}{Environment.NewLine}";
                RecordTransaction.AppendReport(reportFileName, reportText);

                var parameters = new DialogParameters<ReceiptDialog> { { x => x.ReceiptText, receiptText } };

                var options = new DialogOptions{ CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

                await DialogService.ShowAsync<ReceiptDialog>("Receipt", parameters, options);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBoxAsync("Error", $"An error occurred while generating the receipt: {ex.Message}");
            }
        }
    }
}
