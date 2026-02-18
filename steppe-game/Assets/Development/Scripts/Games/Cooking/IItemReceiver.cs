public interface IItemReceiver
{
    bool CanAccept(ItemData item);
    void AddItem(ItemData item);
}
