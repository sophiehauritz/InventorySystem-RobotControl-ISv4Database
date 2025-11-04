using System;

namespace InventorySystem.Models
{
    public sealed class Order
    {
        public Item Item { get; }
        public double Quantity { get; }
        public double TotalPrice { get; }
        public bool IsProcessed { get; private set; }

        public Order(Item item, double quantity)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = quantity;
            TotalPrice = item.Price * quantity;
            IsProcessed = false;
        }

        public void MarkProcessed() => IsProcessed = true;
    }
}