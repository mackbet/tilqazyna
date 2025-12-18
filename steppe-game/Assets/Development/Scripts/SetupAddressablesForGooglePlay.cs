using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class SetupAddressablesForGooglePlay
{
    [MenuItem("Tools/Setup Addressables for Google Play")]
    static void Setup()
    {
        Debug.Log("=== Starting setup ===");
        
        // ШАГ 1: Получить настройки Addressables
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not initialized! Go to: Window → Addressables → Groups → Create Settings");
            return;
        }
        Debug.Log("✓ Addressables settings found");

        // ШАГ 2: Создать или найти группу для больших файлов
        var group = settings.FindGroup("LargeAssets");
        if (group == null)
        {
            // Создать новую группу
            group = settings.CreateGroup("LargeAssets", false, false, false, null);
            Debug.Log("✓ Created new group: LargeAssets");
        }
        else
        {
            Debug.Log("✓ Found existing group: LargeAssets");
        }

        // ШАГ 3: Настроить компрессию для группы
        var schema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
        if (schema != null)
        {
            // LZ4 = быстрое сжатие, хорошо для runtime загрузки
            schema.Compression = UnityEngine.ResourceManagement.ResourceProviders.BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            
            // PackTogether = все ресурсы группы в один файл
            schema.BundleMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            
            Debug.Log("✓ Configured compression: LZ4, PackTogether");
        }

        // ШАГ 4: Найти и добавить большие файлы
        long minSize = 500 * 1024; // 500 KB - минимальный размер файла
        int totalCount = 0;

        // 4.1 Текстуры
        Debug.Log("Searching for large textures...");
        int textureCount = AddAssetsOfType(settings, group, "t:Texture2D", minSize);
        totalCount += textureCount;
        Debug.Log($"✓ Found {textureCount} large textures");

        // 4.2 Аудио
        Debug.Log("Searching for large audio clips...");
        int audioCount = AddAssetsOfType(settings, group, "t:AudioClip", minSize);
        totalCount += audioCount;
        Debug.Log($"✓ Found {audioCount} large audio clips");

        // 4.3 3D Модели
        Debug.Log("Searching for large models...");
        int modelCount = AddAssetsOfType(settings, group, "t:GameObject", minSize);
        totalCount += modelCount;
        Debug.Log($"✓ Found {modelCount} large models");

        // ШАГ 5: Сохранить изменения
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"=== Setup Complete! ===");
        Debug.Log($"Total assets marked as Addressable: {totalCount}");
        Debug.Log("");
        Debug.Log("NEXT STEPS:");
        Debug.Log("1. Window → Addressables → Groups → Build → New Build → Default Build Script");
        Debug.Log("2. Wait for build to complete (2-5 minutes)");
        Debug.Log("3. File → Build Settings → Build (AAB)");
    }

    // ВСПОМОГАТЕЛЬНАЯ ФУНКЦИЯ: добавить ресурсы определённого типа
    static int AddAssetsOfType(AddressableAssetSettings settings, AddressableAssetGroup group, string assetType, long minSize)
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets(assetType);

        foreach (string guid in guids)
        {
            // Получить путь к файлу
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // ПРОПУСТИТЬ системные файлы
            if (path.StartsWith("Packages/")) continue;          // Unity пакеты
            if (path.Contains("TextMesh Pro")) continue;          // TextMesh Pro
            if (path.Contains("Resources")) continue;             // Resources папка (нельзя сделать Addressable)
            
            // Проверить размер файла
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) continue;
            if (fileInfo.Length < minSize) continue; // Слишком маленький файл
            
            try
            {
                // Добавить в Addressables
                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                
                // Установить адрес = имя файла (для удобной загрузки)
                entry.address = Path.GetFileNameWithoutExtension(path);
                
                count++;
                
                // Логировать каждый 10-й файл для прогресса
                if (count % 10 == 0)
                {
                    Debug.Log($"  Processing... {count} files");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not add {path}: {e.Message}");
            }
        }

        return count;
    }
}