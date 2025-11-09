using System;

namespace InventorySystem.Models
{
    // One line in an order: a chosen Item and how many units of it.
    public sealed class OrderLine
    {
        public Item Item { get; }
        public double Quantity { get; }

        public double LineTotal => Item.Price * Quantity;

        public OrderLine(Item item, double quantity)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = quantity;
        }

        public override string ToString() => $"{Quantity} x {Item.Name}";
    }
}
