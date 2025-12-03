using UnityEngine;

[CreateAssetMenu(fileName = "FishingVariant", menuName = "SO/Fishing Variant")]
public class FishingVariant : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private string fishName;
    [SerializeField] private float moveSpeed = 2f; // Скорость движения рыбы

    public Sprite Sprite => sprite;
    public AudioClip AudioClip => audioClip;
    public string FishName => fishName;
    public float MoveSpeed => moveSpeed;
}
