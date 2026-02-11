using UnityEngine;
using UnityEngine.Localization;

public enum ItemCategory
{
    Raw,
    Container,
    Cooked,
    Tool
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Cooking/Item")]
public class ItemData : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private LocalizedString _name;
    [SerializeField] private ItemCategory _category;
    [SerializeField] private ItemData[] _prerequisites;

    public Sprite Icon => _icon;
    public LocalizedString Name => _name;
    public ItemCategory Category => _category;
    public ItemData[] Prerequisites => _prerequisites;
}
