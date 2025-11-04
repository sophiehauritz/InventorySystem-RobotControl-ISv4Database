using System;

namespace InventorySystem.Models
{
    // Base type for all items
    public abstract class Item
    {
        public string Name { get; }
        public double Price { get; }   // price per unit (or per kg for bulk)

        protected Item(string name, double price)
        {
            Name  = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
        }

        public override string ToString() => $"{Name} ({Price:0.##})";
    }
}