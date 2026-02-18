using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewHint", menuName = "Cooking/Hint")]
public class HintData : ScriptableObject
{
    [SerializeField] private LocalizedString _text;
    [SerializeField] private AudioClip _audio;

    public LocalizedString Text => _text;
    public AudioClip Audio => _audio;
}
