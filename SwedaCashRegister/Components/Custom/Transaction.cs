namespace SwedaCashRegister.Components.Custom
{
    public class Transaction
    {
        private decimal TaxRate = .07m;
        public decimal Price { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal NonTaxTotal { get; set; }
        public decimal TaxAmount { get; set; }

        public Transaction()
        {

        }

        public decimal GetSubtotal()
        {
            return TaxableTotal + NonTaxTotal;
        }

        public decimal GetTaxAmount()
        {
            return Math.Round(TaxableTotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        }

        public decimal GetTotal()
        {
            return TaxableTotal + NonTaxTotal + GetTaxAmount();
        }
    }
}
