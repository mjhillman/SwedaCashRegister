namespace SwedaCashRegister.Components.Services
{
    public interface IReceiptTemplateProvider
    {
        string Header { get; }
        string Footer { get; }
    }

    public sealed class ReceiptTemplateProvider : IReceiptTemplateProvider
    {
        public string Header { get; }
        public string Footer { get; }

        public ReceiptTemplateProvider()
        {
            Header = File.ReadAllText(Path.Combine(Program.EXECUTING_DIRECTORY, "wwwroot", "receipt", "header.txt")).TrimEnd();

            Footer = File.ReadAllText(Path.Combine(Program.EXECUTING_DIRECTORY, "wwwroot", "receipt", "footer.txt")).TrimEnd();
        }
    }
}
