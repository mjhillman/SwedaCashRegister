using static MudBlazor.CategoryTypes;

namespace SwedaCashRegister.Components.Custom
{
    /// <summary>
    /// Represents a sales transaction that tracks items, taxable and non-taxable totals, tax amounts, and provides
    /// calculation and rounding utilities. Implements IDisposable to clear its item list when disposed.
    /// </summary>
    /// <remarks>Maintains an ItemList and properties for Price, TaxRate, TaxableTotal, NonTaxTotal, and
    /// TaxAmount. AddItem updates the appropriate totals based on item taxability. TaxAmount is derived from
    /// TaxableTotal and TaxRate and rounded to two decimal places. CalculateRounding returns the adjustment needed to
    /// align an amount to the nearest $0.05.</remarks>
    public sealed class Transaction : IDisposable
    {
        public decimal TaxRate = .06625m;
        public decimal Price { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal NonTaxTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Item> ItemList { get; set; }

        public Transaction()
        {
            ItemList = new List<Item>();
        }

        public void AddItem(Item item)
        {
            ItemList.Add(item);
            if (item.IsTaxable)
            {
                CalculateTaxableTotal();
            }
            else
            {
                CalculateNonTaxableTotal();
            }            
        }

        public void CalculateNonTaxableTotal()
        {
            NonTaxTotal = 0;
            foreach (Item i in ItemList)
            {
                if (!i.IsTaxable)
                {
                    NonTaxTotal += i.ItemAmount;
                }
            }
        }

        public void CalculateTaxableTotal()
        {
            TaxableTotal = 0;
            foreach (Item i in ItemList)
            {
                if (i.IsTaxable)
                {
                    TaxableTotal += i.ItemAmount;
                }
            }
        }

        public void CalculateTaxAmount()
        {
            CalculateTaxableTotal();
            TaxAmount = Math.Round(TaxableTotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalculateSubtotal()
        {
            CalculateNonTaxableTotal();
            CalculateTaxableTotal();
            CalculateTaxAmount();
            return TaxableTotal + NonTaxTotal + TaxAmount;
        }

        public decimal CalculateTotal()
        {
            CalculateNonTaxableTotal(); //includes rounding amount
            CalculateTaxableTotal();
            CalculateTaxAmount();
            decimal roundingAmount = CalculateRounding(NonTaxTotal + TaxableTotal + TaxAmount);
            ItemList.Add(new Item("Rounding", false, 1, "RND", roundingAmount));
            return TaxableTotal + NonTaxTotal + TaxAmount + roundingAmount;
        }

        /// <summary>
        /// Calculates the rounding adjustment needed to bring a currency amount
        /// to the nearest $0.05 (nickel). Returns the difference (rounded - original),
        /// which will be negative (round down), positive (round up), or zero.
        /// </summary>
        public decimal CalculateRounding(decimal amount)
        {
            decimal rounded = Math.Round(amount / 0.05m, 0, MidpointRounding.AwayFromZero) * 0.05m;
            return rounded - amount;
        }

        public void Dispose()
        {
            ItemList.Clear();
        }
    }
}
