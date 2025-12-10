using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class WordDataGenerator : EditorWindow
{
    private TextAsset sourceFile;
    private string outputPath = "Assets/Development/ScriptableObjects/Words";
    private string categoryPath = "Assets/Development/ScriptableObjects/Categories";

    private enum FileFormat
    {
        CategoryBlocks,  // [Категория]\nслово\nслово
        InlineCategories // слово | категория1, категория2
    }

    private FileFormat fileFormat = FileFormat.CategoryBlocks;

    [MenuItem("Tools/Word Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<WordDataGenerator>("Word Data Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Word Data Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        sourceFile = (TextAsset)EditorGUILayout.ObjectField("Source TXT File", sourceFile, typeof(TextAsset), false);

        GUILayout.Space(10);

        fileFormat = (FileFormat)EditorGUILayout.EnumPopup("File Format", fileFormat);

        GUILayout.Space(5);

        if (fileFormat == FileFormat.CategoryBlocks)
        {
            EditorGUILayout.HelpBox("Format:\n[CategoryName]\nword1\nword2\n\n[AnotherCategory]\nword3", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Format:\nword1 | Category1, Category2\nword2 | Category1", MessageType.Info);
        }

        GUILayout.Space(10);

        outputPath = EditorGUILayout.TextField("Words Output Path", outputPath);
        categoryPath = EditorGUILayout.TextField("Categories Output Path", categoryPath);

        GUILayout.Space(20);

        GUI.enabled = sourceFile != null;
        if (GUILayout.Button("Generate Word Data", GUILayout.Height(40)))
        {
            GenerateWordData();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (GUILayout.Button("Clear All Generated Assets", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Assets",
                "Are you sure you want to delete all generated WordData and WordCategory assets?",
                "Yes", "No"))
            {
                ClearGeneratedAssets();
            }
        }
    }

    private void GenerateWordData()
    {
        if (sourceFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a source text file!", "OK");
            return;
        }

        // Создаем папки если их нет
        CreateDirectoryIfNotExists(outputPath);
        CreateDirectoryIfNotExists(categoryPath);

        // Парсим файл
        Dictionary<string, List<string>> categoryWordsMap;

        if (fileFormat == FileFormat.CategoryBlocks)
        {
            categoryWordsMap = ParseCategoryBlocks(sourceFile.text);
        }
        else
        {
            categoryWordsMap = ParseInlineCategories(sourceFile.text);
        }

        if (categoryWordsMap.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No valid data found in file!", "OK");
            return;
        }

        // Создаем категории
        Dictionary<string, WordCategory> categories = CreateCategories(categoryWordsMap.Keys.ToList());

        // Создаем слова
        var (wordsCreated, wordsUpdated) = CreateWords(categoryWordsMap, categories);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = $"Generated {categories.Count} categories\n" +
                        $"Created {wordsCreated} new words\n" +
                        $"Updated {wordsUpdated} existing words";

        EditorUtility.DisplayDialog("Success", message, "OK");
    }

    private Dictionary<string, List<string>> ParseCategoryBlocks(string text)
    {
        var result = new Dictionary<string, List<string>>();
        var lines = text.Split('\n');

        string currentCategory = null;

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Проверяем категорию [CategoryName]
            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentCategory = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                if (!result.ContainsKey(currentCategory))
                {
                    result[currentCategory] = new List<string>();
                }
            }
            else if (currentCategory != null)
            {
                // Добавляем слово к текущей категории
                result[currentCategory].Add(trimmedLine);
            }
        }

        return result;
    }

    private Dictionary<string, List<string>> ParseInlineCategories(string text)
    {
        // Инвертированная структура: word -> [categories]
        var wordCategories = new Dictionary<string, List<string>>();
        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Парсим формат: word | category1, category2
            string[] parts = trimmedLine.Split('|');
            if (parts.Length != 2)
                continue;

            string word = parts[0].Trim();
            string[] categories = parts[1].Split(',');

            wordCategories[word] = categories.Select(c => c.Trim()).ToList();
        }

        // Конвертируем в формат category -> [words]
        var result = new Dictionary<string, List<string>>();

        foreach (var kvp in wordCategories)
        {
            string word = kvp.Key;
            foreach (string category in kvp.Value)
            {
                if (!result.ContainsKey(category))
                {
                    result[category] = new List<string>();
                }
                result[category].Add(word);
            }
        }

        return result;
    }

    private Dictionary<string, WordCategory> CreateCategories(List<string> categoryNames)
    {
        var categories = new Dictionary<string, WordCategory>();

        foreach (string categoryName in categoryNames)
        {
            // Проверяем, существует ли уже такая категория
            string assetPath = $"{categoryPath}/{categoryName}.asset";
            WordCategory category = AssetDatabase.LoadAssetAtPath<WordCategory>(assetPath);

            if (category == null)
            {
                // Создаем новую категорию
                category = ScriptableObject.CreateInstance<WordCategory>();

                // Используем рефлексию для установки приватного поля
                var field = typeof(WordCategory).GetField("categoryName",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(category, categoryName);
                }

                AssetDatabase.CreateAsset(category, assetPath);
                Debug.Log($"Created category: {categoryName}");
            }

            categories[categoryName] = category;
        }

        return categories;
    }

    private (int created, int updated) CreateWords(Dictionary<string, List<string>> categoryWordsMap, Dictionary<string, WordCategory> categories)
    {
        // Собираем все уникальные слова с их категориями
        var wordCategoriesMap = new Dictionary<string, List<WordCategory>>();

        foreach (var kvp in categoryWordsMap)
        {
            string categoryName = kvp.Key;
            WordCategory category = categories[categoryName];

            foreach (string word in kvp.Value)
            {
                if (!wordCategoriesMap.ContainsKey(word))
                {
                    wordCategoriesMap[word] = new List<WordCategory>();
                }

                if (!wordCategoriesMap[word].Contains(category))
                {
                    wordCategoriesMap[word].Add(category);
                }
            }
        }

        // Создаем WordData для каждого уникального слова
        int wordsCreated = 0;
        int wordsUpdated = 0;

        foreach (var kvp in wordCategoriesMap)
        {
            string word = kvp.Key;
            List<WordCategory> newCategories = kvp.Value;

            // Безопасное имя файла
            string safeFileName = word.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            string assetPath = $"{outputPath}/{safeFileName}.asset";

            WordData wordData = AssetDatabase.LoadAssetAtPath<WordData>(assetPath);

            if (wordData == null)
            {
                // Создаем новое слово
                wordData = ScriptableObject.CreateInstance<WordData>();

                // Используем рефлексию для установки полей
                var wordField = typeof(WordData).GetField("word",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var categoriesField = typeof(WordData).GetField("categories",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (wordField != null)
                    wordField.SetValue(wordData, word);

                if (categoriesField != null)
                    categoriesField.SetValue(wordData, newCategories.ToArray());

                AssetDatabase.CreateAsset(wordData, assetPath);
                wordsCreated++;
                Debug.Log($"Created word: {word} with {newCategories.Count} categories");
            }
            else
            {
                // Слово уже существует - дополняем категории
                var categoriesField = typeof(WordData).GetField("categories",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (categoriesField != null)
                {
                    // Получаем текущие категории
                    WordCategory[] existingCategories = (WordCategory[])categoriesField.GetValue(wordData);
                    List<WordCategory> allCategories = new List<WordCategory>(existingCategories ?? new WordCategory[0]);

                    // Добавляем только новые категории
                    int addedCount = 0;
                    foreach (var newCategory in newCategories)
                    {
                        if (!allCategories.Contains(newCategory))
                        {
                            allCategories.Add(newCategory);
                            addedCount++;
                        }
                    }

                    // Обновляем категории только если были добавлены новые
                    if (addedCount > 0)
                    {
                        categoriesField.SetValue(wordData, allCategories.ToArray());
                        EditorUtility.SetDirty(wordData);
                        wordsUpdated++;
                        Debug.Log($"Updated word: {word} - added {addedCount} new categories (total: {allCategories.Count})");
                    }
                }
            }
        }

        Debug.Log($"Words created: {wordsCreated}, Words updated: {wordsUpdated}");
        return (wordsCreated, wordsUpdated);
    }

    private void CreateDirectoryIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }

    private void ClearGeneratedAssets()
    {
        // Удаляем все WordData
        string[] wordAssets = AssetDatabase.FindAssets("t:WordData", new[] { outputPath });
        foreach (string guid in wordAssets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }

        // Удаляем все WordCategory
        string[] categoryAssets = AssetDatabase.FindAssets("t:WordCategory", new[] { categoryPath });
        foreach (string guid in categoryAssets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.Refresh();
        Debug.Log("Cleared all generated assets");
    }
}
