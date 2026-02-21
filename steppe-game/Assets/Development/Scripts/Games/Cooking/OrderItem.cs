public class OrderItem
{
    public ItemData Item { get; }
    public bool Fulfilled { get; set; }

    public OrderItem(ItemData item)
    {
        Item = item;
    }
}
