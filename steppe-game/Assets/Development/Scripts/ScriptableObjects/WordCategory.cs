using UnityEngine;

[CreateAssetMenu(fileName = "WordCategory", menuName = "Game/Word Category")]
public class WordCategory : ScriptableObject
{
    [SerializeField] private string categoryName; // Название категории
    [SerializeField] private Sprite categoryIcon; // Иконка категории (опционально)
    [SerializeField] private Color categoryColor = Color.white; // Цвет категории

    public string CategoryName => categoryName;
    public Sprite CategoryIcon => categoryIcon;
    public Color CategoryColor => categoryColor;
}
