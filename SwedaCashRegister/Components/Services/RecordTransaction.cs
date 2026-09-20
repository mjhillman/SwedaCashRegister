namespace SwedaCashRegister.Components.Services
{
    /// <summary>
    /// Provides static helper methods to write transaction receipt files and append report files under the
    /// application's wwwroot directory.
    /// </summary>
    /// <remarks>Performs file system I/O: each method ensures the target directory exists and returns the
    /// full path of the written file. I/O exceptions (for example IOException or UnauthorizedAccessException) are
    /// propagated to the caller. The type contains only static members and can be declared static to reflect its
    /// usage.</remarks>
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
