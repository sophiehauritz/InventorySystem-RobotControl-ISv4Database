using System;

namespace InventorySystem.Models
{
    // Example of a bulk item (priced per kg, meter, etc.)
    public sealed class BulkItem : Item
    {
        public double WeightPerUnit { get; } // if you need it for calc/display

        public BulkItem(string name, double pricePerKg, double weightPerUnit)
            : base(name, pricePerKg)
        {
            WeightPerUnit = weightPerUnit;
        }
    }
}