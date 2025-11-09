using System.Collections.Generic;

namespace InventorySystem.Models
{

    public static class DatabaseInit
    {
        public static IReadOnlyList<Item> SampleCatalog { get; } = new List<Item>
        {
            new UnitItem("hydraulic pump", 8500, 0),
            new UnitItem("PLC module",     1200, 0),
            new UnitItem("servo motor",    4300, 0),
        };

        public static void EnsureCreatedAndSeed()
        {

        }
    }
}
