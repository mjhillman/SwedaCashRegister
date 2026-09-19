namespace SwedaCashRegister.Components.Services
{
    public class RecordTransaction
    {
        public static string WriteTranaction(string fileName, string text)
        {
            try
            {
                string folder = Path.Combine(Program.EXECUTING_DIRECTORY, "wwwroot", "receipts");
                Directory.CreateDirectory(folder); // Ensure the directory exists
                string path = Path.Combine(folder, fileName);
                File.WriteAllText(path, text);
                return path;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string AppendReport(string fileName, string text)
        {
            try
            {
                string folder = Path.Combine(Program.EXECUTING_DIRECTORY, "wwwroot", "reports");
                Directory.CreateDirectory(folder); // Ensure the directory exists
                string path = Path.Combine(folder, fileName);
                File.AppendAllText(path, text);
                return path;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
