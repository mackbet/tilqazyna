using UnityEngine;

[CreateAssetMenu(fileName = "ThreeInRowItem", menuName = "ThreeInRow/Item Data")]
public class ThreeInRowItemData : ScriptableObject
{
    [Header("Visual")]
    public Sprite icon;
    
    [Header("Audio")]
    public AudioClip audioClip;
    
    [Header("Text")]
    [TextArea(2, 4)]
    public string itemText;
}