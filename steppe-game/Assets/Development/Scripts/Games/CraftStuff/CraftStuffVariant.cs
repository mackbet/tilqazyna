using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stuff")]
public class CraftStuffVariant : ScriptableObject
{
    [field: SerializeField] public Phrase Phrase { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
