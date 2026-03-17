namespace UltimaMagic.Gameplay;

public sealed class Inventory
{
    private readonly List<IInventoryItem> _items = [];
    private readonly IReadOnlyList<IInventoryItem> _readOnlyItems;

    public Inventory()
    {
        _readOnlyItems = _items.AsReadOnly();
    }

    public IReadOnlyList<IInventoryItem> Items => _readOnlyItems;

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public bool RemoveItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _items.FindIndex(existingItem => MatchesIdentity(existingItem, item));
        if (index < 0)
        {
            return false;
        }

        _items.RemoveAt(index);
        return true;
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
