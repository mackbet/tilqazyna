using UnityEngine;

[CreateAssetMenu(fileName = "FindTheItemVariant", menuName = "SO/Find The Item Variant")]
public class FindTheItemVariant : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private string itemName;

    public Sprite Sprite => sprite;
    public AudioClip AudioClip => audioClip;
    public string ItemName => itemName;
}
