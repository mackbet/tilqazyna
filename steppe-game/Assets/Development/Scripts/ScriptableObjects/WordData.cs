using UnityEngine;

[CreateAssetMenu(fileName = "WordData", menuName = "Game/Word Data")]
public class WordData : ScriptableObject
{
    [SerializeField] private string word; // Само слово
    [SerializeField] private WordCategory[] categories; // Категории слова (может быть несколько)
    [SerializeField] private AudioClip audioClip; // Звук слова (опционально)

    public string Word => word;
    public WordCategory[] Categories => categories;
    public AudioClip AudioClip => audioClip;

    // Проверяет, относится ли слово к указанной категории
    public bool HasCategory(WordCategory category)
    {
        if (categories == null || category == null) return false;

        foreach (var cat in categories)
        {
            if (cat == category) return true;
        }

        return false;
    }
}

