#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class PuzzleTextureGenerator : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Texture2D sourceImage;
    
    [Header("Masks")]
    [SerializeField] private Texture2D[] masks;
    
    [Header("Output Settings")]
    [SerializeField] private string outputFolderPath = "Assets/Puzzles/GeneratedTextures";
    [SerializeField] private string texturePrefix = "PuzzlePiece_";
    
    [Header("Texture Settings")]
    [SerializeField] private TextureFormat textureFormat = TextureFormat.RGBA32;
    [SerializeField] private bool generateMipmaps = false;

    [ContextMenu("Generate and Save Textures")]
    public void GenerateAndSaveTextures()
    {
        if (sourceImage == null)
        {
            Debug.LogError("PuzzleTextureGenerator: Source image не задан!");
            return;
        }

        if (masks == null || masks.Length == 0)
        {
            Debug.LogError("PuzzleTextureGenerator: Массив масок пуст!");
            return;
        }

        // Создаем папку если её нет
        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
            AssetDatabase.Refresh();
        }

        int successCount = 0;

        for (int i = 0; i < masks.Length; i++)
        {
            Texture2D mask = masks[i];
            
            if (mask == null)
            {
                Debug.LogWarning($"PuzzleTextureGenerator: Маска #{i} равна null, пропускаем.");
                continue;
            }

            // Генерируем замаскированную текстуру
            Texture2D maskedTexture = CreateMaskedTexture(sourceImage, mask);

            if (maskedTexture == null)
            {
                Debug.LogError($"PuzzleTextureGenerator: Не удалось создать текстуру для маски #{i}");
                continue;
            }

            // Сохраняем текстуру
            string fileName = $"{texturePrefix}{i:D3}.png";
            string fullPath = Path.Combine(outputFolderPath, fileName);

            if (SaveTexture(maskedTexture, fullPath))
            {
                successCount++;
                Debug.Log($"Сохранена текстура: {fullPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Генерация завершена! Успешно создано {successCount}/{masks.Length} текстур.");
    }

    private Texture2D CreateMaskedTexture(Texture2D source, Texture2D mask)
    {
        // Проверяем что текстуры readable
        if (!source.isReadable)
        {
            Debug.LogError($"У текстуры {source.name} не включен Read/Write Enable!");
            return null;
        }

        if (!mask.isReadable)
        {
            Debug.LogError($"У маски {mask.name} не включен Read/Write Enable!");
            return null;
        }

        int width = Mathf.Min(source.width, mask.width);
        int height = Mathf.Min(source.height, mask.height);

        Texture2D maskedTexture = new Texture2D(width, height, textureFormat, generateMipmaps);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color sourceColor = source.GetPixel(x, y);
                Color maskColor = mask.GetPixel(x, y);

                // Применяем альфа-канал маски
                sourceColor.a *= maskColor.a;
                maskedTexture.SetPixel(x, y, sourceColor);
            }
        }

        maskedTexture.Apply();
        return maskedTexture;
    }

    private bool SaveTexture(Texture2D texture, string path)
    {
        try
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            
            // Импортируем текстуру с правильными настройками
            AssetDatabase.ImportAsset(path);
            
            // Настраиваем импорт
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.isReadable = true;
                importer.mipmapEnabled = generateMipmaps;
                
                AssetDatabase.WriteImportSettingsIfDirty(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении текстуры {path}: {e.Message}");
            return false;
        }
    }

    [ContextMenu("Clear Generated Textures")]
    public void ClearGeneratedTextures()
    {
        if (!Directory.Exists(outputFolderPath))
        {
            Debug.Log("Папка не существует, нечего удалять.");
            return;
        }

        string[] files = Directory.GetFiles(outputFolderPath, $"{texturePrefix}*.png");
        
        foreach (string file in files)
        {
            AssetDatabase.DeleteAsset(file);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Удалено {files.Length} текстур.");
    }

    [ContextMenu("Create Sprites from Generated Textures")]
    public Sprite[] CreateSpritesFromGeneratedTextures()
    {
        if (!Directory.Exists(outputFolderPath))
        {
            Debug.LogError("Папка с текстурами не существует!");
            return null;
        }

        string[] texturePaths = Directory.GetFiles(outputFolderPath, $"{texturePrefix}*.png");
        Sprite[] sprites = new Sprite[texturePaths.Length];

        for (int i = 0; i < texturePaths.Length; i++)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePaths[i]);
            if (texture != null)
            {
                string spritePath = texturePaths[i];
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                
                if (sprites[i] == null)
                {
                    Debug.LogWarning($"Не удалось загрузить спрайт: {spritePath}");
                }
            }
        }

        Debug.Log($"Загружено {sprites.Length} спрайтов.");
        return sprites;
    }
}

[CustomEditor(typeof(PuzzleTextureGenerator))]
public class PuzzleTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PuzzleTextureGenerator generator = (PuzzleTextureGenerator)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate and Save Textures", GUILayout.Height(40)))
        {
            generator.GenerateAndSaveTextures();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Clear Generated Textures", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Подтверждение",
                "Вы уверены что хотите удалить все сгенерированные текстуры?",
                "Да", "Отмена"))
            {
                generator.ClearGeneratedTextures();
            }
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create Sprites from Textures", GUILayout.Height(30)))
        {
            generator.CreateSpritesFromGeneratedTextures();
        }
    }
}
#endif