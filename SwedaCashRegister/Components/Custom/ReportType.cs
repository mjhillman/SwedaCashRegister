using Microsoft.Extensions.Primitives;

namespace SwedaCashRegister.Components.Custom
{
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

        public ReportType(string csvLine)
        {
            string[] values = csvLine.Split(',');
            if (values.Length == 5)
            {
                TimeStamp = values[0];
                TaxableTotal = Convert.ToDecimal(values[1]);
                NonTaxableTotal = Convert.ToDecimal(values[2]);
                TaxCollected = Convert.ToDecimal(values[3]);
                RoundingAmount = Convert.ToDecimal(values[4]);
            }
        }
    }
}
