namespace UltimaMagic.Gameplay;

public sealed class Inventory
{
    private readonly List<IInventoryItem> _items = [];
    private readonly IReadOnlyList<IInventoryItem> _readOnlyView;

    public Inventory()
    {
        _readOnlyView = _items.AsReadOnly();
    }

    public IReadOnlyList<IInventoryItem> Items => _readOnlyView;

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public bool RemoveItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        for (var index = 0; index < _items.Count; index++)
        {
            if (!MatchesIdentity(_items[index], item))
            {
                continue;
            }

            _items.RemoveAt(index);
            return true;
        }

        return false;
    }

    public int CountItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _items.Count(existingItem => MatchesIdentity(existingItem, item));
    }

    private static bool MatchesIdentity(IInventoryItem left, IInventoryItem right)
    {
        return left.GetType() == right.GetType()
            && string.Equals(left.Name, right.Name, StringComparison.Ordinal);
    }
}
