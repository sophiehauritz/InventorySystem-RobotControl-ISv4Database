using System;

namespace InventorySystem.Models
{
    // A normal piece/box unit item
    public sealed class UnitItem : Item
    {
        public double Weight { get; } // optional; can be 0

        public UnitItem(string name, double pricePerUnit, double weight = 0)
            : base(name, pricePerUnit)
        {
            Weight = weight;
        }
    }
}