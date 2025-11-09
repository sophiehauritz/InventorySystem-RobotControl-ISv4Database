using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using InventorySystem.Models;

namespace InventorySystem.Data
{
    public static class Db
    {
        private const string Cs = "Server=127.0.0.1;Port=3306;Database=inventory;User ID=root;";

        // Loads items from MariaDB and returns UnitItem instances.
        public static async Task<List<UnitItem>> LoadItemsAsync()
        {
            var list = new List<UnitItem>();

            await using var conn = new MySqlConnection(Cs);
            await conn.OpenAsync();

            const string sql = @"
                SELECT Name, PricePerUnit, IFNULL(Weight, 0)
                FROM items
                ORDER BY Id;";

            await using var cmd = new MySqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                string name = rd.GetString(0);
                double price = rd.GetDouble(1);
                double weight = rd.GetDouble(2);

                list.Add(new UnitItem(name, price, weight));
            }

            return list;
        }
    }
}
