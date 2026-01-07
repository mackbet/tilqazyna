using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class WordCategoryAnalyzer : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WordData[] allWords;
    [SerializeField] private WordCategory[] categories;

    [Header("Settings")]
    [SerializeField] private bool analyzeOnStart = false;
    [SerializeField] private bool showInConsole = true;
    [SerializeField] private int minWordsWarningThreshold = 5;

    [ContextMenu("Analyze Categories")]
    public void AnalyzeCategories()
    {
        if (allWords == null || allWords.Length == 0)
        {
            Debug.LogError("No words assigned!");
            return;
        }

        if (categories == null || categories.Length == 0)
        {
            Debug.LogError("No categories assigned!");
            return;
        }

        Dictionary<WordCategory, List<WordData>> categoryWords = new Dictionary<WordCategory, List<WordData>>();

        // Инициализируем словарь
        foreach (var category in categories)
        {
            if (category != null)
            {
                categoryWords[category] = new List<WordData>();
            }
        }

        // Распределяем слова по категориям
        foreach (var word in allWords)
        {
            if (word == null) continue;

            foreach (var category in categories)
            {
                if (category != null && word.HasCategory(category))
                {
                    categoryWords[category].Add(word);
                }
            }
        }

        // Формируем отчет
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== WORD CATEGORY ANALYSIS ===");
        report.AppendLine($"Total Words: {allWords.Length}");
        report.AppendLine($"Total Categories: {categories.Length}");
        report.AppendLine();

        int totalCategorized = 0;
        List<WordCategory> warningCategories = new List<WordCategory>();
        List<WordCategory> emptyCategories = new List<WordCategory>();

        foreach (var category in categories)
        {
            if (category == null) continue;

            int wordCount = categoryWords[category].Count;
            totalCategorized += wordCount;

            string status = "";
            if (wordCount == 0)
            {
                status = " ⚠️ EMPTY";
                emptyCategories.Add(category);
            }
            else if (wordCount < minWordsWarningThreshold)
            {
                status = $" ⚠️ LOW (< {minWordsWarningThreshold})";
                warningCategories.Add(category);
            }
            else
            {
                status = " ✓";
            }

            report.AppendLine($"• {category.CategoryName}: {wordCount} words{status}");
            
            // Опционально: показываем список слов для маленьких категорий
            if (wordCount > 0 && wordCount < minWordsWarningThreshold && showInConsole)
            {
                report.Append("  └─ Words: ");
                for (int i = 0; i < categoryWords[category].Count; i++)
                {
                    report.Append(categoryWords[category][i].Word);
                    if (i < categoryWords[category].Count - 1)
                        report.Append(", ");
                }
                report.AppendLine();
            }
        }

        report.AppendLine();
        report.AppendLine("=== SUMMARY ===");
        report.AppendLine($"Empty Categories: {emptyCategories.Count}");
        report.AppendLine($"Low Word Categories (< {minWordsWarningThreshold}): {warningCategories.Count}");
        
        // Находим слова без категорий
        List<WordData> wordsWithoutCategories = new List<WordData>();
        foreach (var word in allWords)
        {
            if (word == null) continue;
            
            bool hasCategory = false;
            foreach (var category in categories)
            {
                if (category != null && word.HasCategory(category))
                {
                    hasCategory = true;
                    break;
                }
            }
            
            if (!hasCategory)
            {
                wordsWithoutCategories.Add(word);
            }
        }

        report.AppendLine($"Words Without Categories: {wordsWithoutCategories.Count}");

        if (wordsWithoutCategories.Count > 0 && showInConsole)
        {
            report.AppendLine();
            report.AppendLine("=== WORDS WITHOUT CATEGORIES ===");
            foreach (var word in wordsWithoutCategories)
            {
                report.AppendLine($"• {word.Word}");
            }
        }

        // Выводим предупреждения
        if (emptyCategories.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("⚠️ WARNING: Empty Categories:");
            foreach (var category in emptyCategories)
            {
                report.AppendLine($"  • {category.CategoryName}");
            }
        }

        if (warningCategories.Count > 0)
        {
            report.AppendLine();
            report.AppendLine($"⚠️ WARNING: Categories with less than {minWordsWarningThreshold} words:");
            foreach (var category in warningCategories)
            {
                report.AppendLine($"  • {category.CategoryName} ({categoryWords[category].Count} words)");
            }
        }

        report.AppendLine();
        report.AppendLine("=== END OF ANALYSIS ===");

        Debug.Log(report.ToString());
    }

    [ContextMenu("Analyze and Save to File")]
    public void AnalyzeAndSaveToFile()
    {
        if (allWords == null || allWords.Length == 0 || categories == null || categories.Length == 0)
        {
            Debug.LogError("No words or categories assigned!");
            return;
        }

        Dictionary<WordCategory, List<WordData>> categoryWords = new Dictionary<WordCategory, List<WordData>>();

        // Инициализируем словарь
        foreach (var category in categories)
        {
            if (category != null)
            {
                categoryWords[category] = new List<WordData>();
            }
        }

        // Распределяем слова по категориям
        foreach (var word in allWords)
        {
            if (word == null) continue;

            foreach (var category in categories)
            {
                if (category != null && word.HasCategory(category))
                {
                    categoryWords[category].Add(word);
                }
            }
        }

        // Формируем детальный отчет
        StringBuilder report = new StringBuilder();
        report.AppendLine("WORD CATEGORY DETAILED ANALYSIS");
        report.AppendLine($"Generated: {System.DateTime.Now}");
        report.AppendLine($"Total Words: {allWords.Length}");
        report.AppendLine($"Total Categories: {categories.Length}");
        report.AppendLine();

        foreach (var category in categories)
        {
            if (category == null) continue;

            report.AppendLine($"=== {category.CategoryName.ToUpper()} ===");
            report.AppendLine($"Word Count: {categoryWords[category].Count}");
            report.AppendLine("Words:");

            if (categoryWords[category].Count > 0)
            {
                foreach (var word in categoryWords[category])
                {
                    report.AppendLine($"  • {word.Word}");
                }
            }
            else
            {
                report.AppendLine("  (No words in this category)");
            }

            report.AppendLine();
        }

        string fileName = $"WordCategoryAnalysis_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string path = System.IO.Path.Combine(Application.dataPath, fileName);
        
        try
        {
            System.IO.File.WriteAllText(path, report.ToString());
            Debug.Log($"Analysis saved to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save file: {e.Message}");
        }
    }

    public Dictionary<WordCategory, int> GetCategoryWordCounts()
    {
        Dictionary<WordCategory, int> counts = new Dictionary<WordCategory, int>();

        if (allWords == null || categories == null) return counts;

        foreach (var category in categories)
        {
            if (category == null) continue;
            counts[category] = 0;
        }

        foreach (var word in allWords)
        {
            if (word == null) continue;

            foreach (var category in categories)
            {
                if (category != null && word.HasCategory(category))
                {
                    counts[category]++;
                }
            }
        }

        return counts;
    }

    private void Start()
    {
        if (analyzeOnStart)
        {
            AnalyzeCategories();
        }
    }
}