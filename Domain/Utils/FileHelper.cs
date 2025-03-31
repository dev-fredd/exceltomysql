namespace Exceltomysql.Domain.Utils
{
    public class FileHelper
    {
        public string GetTempFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is empty or null");
                }

                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return tempFilePath;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating temp file: {e.Message}");
                return null;
            }
        }

        public void TryDeleteFile(string path, int maxAttempts = 3, int delayMs = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        return;
                    }
                }
                catch (IOException) when (i < maxAttempts - 1)
                {
                    Thread.Sleep(delayMs);
                }
            }
        }
    }
}