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
    [SerializeField] private int _price;
    [SerializeField] private HintData _hint;
    [SerializeField] private LocalizedString _orderText;
    [SerializeField] private AudioClip _orderAudioMale;
    [SerializeField] private AudioClip _orderAudioFemale;

    public Sprite Icon => _icon;
    public LocalizedString Name => _name;
    public ItemCategory Category => _category;
    public ItemData[] Prerequisites => _prerequisites;
    public int Price => _price;
    public HintData Hint => _hint;
    public LocalizedString OrderText => _orderText;
    public AudioClip OrderAudioMale => _orderAudioMale;
    public AudioClip OrderAudioFemale => _orderAudioFemale;
}
