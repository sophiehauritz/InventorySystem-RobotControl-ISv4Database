using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MySqlConnector;  // DB driver

namespace InventorySystem
{
    internal class Program
    {
        // Fire-and-forget DB test (runs on a background thread)
        private static async Task TestDatabaseConnection()
        {
            try
            {
                string cs = "Server=127.0.0.1;Port=3306;Database=inventory;User ID=root;";

                using var conn = new MySqlConnection(cs);
                await conn.OpenAsync();

                Console.WriteLine("✅ Connected to MariaDB!");

                using var cmd = new MySqlCommand("SELECT COUNT(*) FROM items;", conn);
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                Console.WriteLine($"📦 Items in database: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ DB error: " + ex.Message);
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine("🚀 Starting application…");

            // Run DB test in background so the UI starts on the main thread
            _ = Task.Run(TestDatabaseConnection);

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}