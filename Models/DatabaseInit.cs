using System.Collections.Generic;

namespace InventorySystem.Models
{
    /// <summary>
    /// Simple in-memory seed so the app compiles and runs.
    /// Later (DB lectures) you can replace this with real create/seed logic.
    /// </summary>
    public static class DatabaseInit
    {
        // --- used by the UI to populate the catalog ---
        public static IReadOnlyList<Item> SampleCatalog { get; } = new List<Item>
        {
            new UnitItem("hydraulic pump", 8500, 0),
            new UnitItem("PLC module",     1200, 0),
            new UnitItem("servo motor",    4300, 0),
        };

        // --- kept to satisfy Program.cs (currently a no-op) ---
        public static void EnsureCreatedAndSeed()
        {
            // No database yet. When you wire MariaDB, create schema + seed here.
            // Keeping the method so Program.cs builds without changes.
        }
    }
}