namespace SwedaCashRegister.Components.Custom
{
    /// <summary>
    /// Represents a financial report that captures a timestamp and aggregated monetary totals for taxable amounts,
    /// non-taxable amounts, tax collected, and rounding adjustments.
    /// </summary>
    /// <remarks>Used to transfer or serialize summarized transaction totals for a reporting period; monetary
    /// values are represented as decimal.</remarks>
    public class ReportType
    {
        public string TimeStamp { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal NonTaxableTotal { get; set; }
        public decimal TaxCollected { get; set; }
        public decimal RoundingAmount { get; set; }

        public ReportType()
        {

        }
    }
}
