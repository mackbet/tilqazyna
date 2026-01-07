using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "SO/Profession")]
public class CraftToolsProfession : ScriptableObject
{
    [field: SerializeField] public LocalizedString Name { get; private set; }
    [field: SerializeField] public Sprite WaitingSprite { get; private set; }
    [field: SerializeField] public Sprite FunSprite { get; private set; }
    [field: SerializeField] public Sprite SadSprite { get; private set; }
    [field: SerializeField] public Sprite FinalSprite { get; private set; }
    [field: SerializeField] public CraftStuffVariant[] Stuff { get; private set; }
}
