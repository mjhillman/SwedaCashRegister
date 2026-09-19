namespace SwedaCashRegister.Components.Services
{
    public interface IReceiptTemplateProvider
    {
        string Header { get; }
        string Footer { get; }
        decimal TaxRate { get; }

        void SaveHeader(string text);
        void SaveFooter(string text);
        void SaveTaxRate(decimal rate);
    }

    public sealed class ReceiptTemplateProvider : IReceiptTemplateProvider
    {
        private readonly string _settingsPath;

        public string Header { get; private set; }
        public string Footer { get; private set; }
        public decimal TaxRate { get; private set; } = .0665m;

        public ReceiptTemplateProvider(IWebHostEnvironment env)
        {
            _settingsPath = Path.Combine(env.WebRootPath, "receiptSettings");
            Directory.CreateDirectory(_settingsPath); // ensure it exists on first run

            Header = ReadOrDefault("header.txt", "");
            Footer = ReadOrDefault("footer.txt", "");

            string taxRateText = ReadOrDefault("taxrate.txt", "0.06625");
            TaxRate = decimal.TryParse(taxRateText, out var rate) ? rate : 0.06625m;
        }

        private string ReadOrDefault(string fileName, string defaultValue)
        {
            string path = Path.Combine(_settingsPath, fileName);
            return File.Exists(path) ? File.ReadAllText(path).TrimEnd() : defaultValue;
        }

        public void SaveHeader(string text)
        {
            Header = text;
            File.WriteAllText(Path.Combine(_settingsPath, "header.txt"), text);
        }

        public void SaveFooter(string text)
        {
            Footer = text;
            File.WriteAllText(Path.Combine(_settingsPath, "footer.txt"), text);
        }

        public void SaveTaxRate(decimal rate)
        {
            TaxRate = rate;
            File.WriteAllText(Path.Combine(_settingsPath, "taxrate.txt"), rate.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}