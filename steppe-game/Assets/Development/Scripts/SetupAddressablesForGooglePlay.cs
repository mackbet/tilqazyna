using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// ВАЖНО: Addressables должны быть установлены через Package Manager
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

public class SetupAddressablesForGooglePlay
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Addressables for Google Play")]
    static void Setup()
    {
        Debug.Log("=== Starting setup ===");
        
        // Проверка что Addressables установлен
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not initialized! Steps:");
            Debug.LogError("1. Window → Package Manager → Unity Registry → Addressables → Install");
            Debug.LogError("2. Window → Asset Management → Addressables → Groups → Create Addressables Settings");
            return;
        }
        Debug.Log("✓ Addressables settings found");

        // Найти или создать группу
        AddressableAssetGroup group = settings.FindGroup("LargeAssets");
        if (group == null)
        {
            group = settings.CreateGroup("LargeAssets", false, false, false, null);
            Debug.Log("✓ Created new group: LargeAssets");
        }
        else
        {
            Debug.Log("✓ Found existing group: LargeAssets");
        }

        // Настроить компрессию
        BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema != null)
        {
            schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            Debug.Log("✓ Configured compression: LZ4, PackTogether");
        }

        // Найти большие файлы
        long minSize = 500 * 1024; // 500 KB
        int totalCount = 0;
        List<string> addedFiles = new List<string>();

        // Текстуры
        Debug.Log("\n--- Searching Textures ---");
        int textureCount = AddAssetsOfType(settings, group, "t:Texture2D", minSize, addedFiles);
        totalCount += textureCount;
        
        // Аудио
        Debug.Log("\n--- Searching Audio ---");
        int audioCount = AddAssetsOfType(settings, group, "t:AudioClip", minSize, addedFiles);
        totalCount += audioCount;
        
        // Модели/Префабы
        Debug.Log("\n--- Searching Prefabs ---");
        int prefabCount = AddAssetsOfType(settings, group, "t:GameObject", minSize, addedFiles);
        totalCount += prefabCount;

        // Сохранить
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        AssetDatabase.SaveAssets();
        
        Debug.Log("\n=== Setup Complete! ===");
        Debug.Log($"Total assets marked as Addressable: {totalCount}");
        
        if (totalCount > 0)
        {
            Debug.Log("\nAdded files (first 10):");
            for (int i = 0; i < Mathf.Min(10, addedFiles.Count); i++)
            {
                Debug.Log($"  ✓ {addedFiles[i]}");
            }
            
            Debug.Log("\nNEXT STEPS:");
            Debug.Log("1. Window → Asset Management → Addressables → Groups");
            Debug.Log("2. Build → New Build → Default Build Script");
            Debug.Log("3. Wait 2-5 minutes");
            Debug.Log("4. File → Build Settings → Build (AAB)");
        }
        else
        {
            Debug.LogWarning("\n⚠ NO FILES ADDED!");
            Debug.LogWarning("Possible reasons:");
            Debug.LogWarning("• All files are < 500 KB");
            Debug.LogWarning("• Files are in Resources folder (can't be Addressable)");
            Debug.LogWarning("• Files are in Packages folder (system files)");
            Debug.LogWarning("\nTry: Change minSize to 100 KB in the script");
        }
    }

    static int AddAssetsOfType(AddressableAssetSettings settings, AddressableAssetGroup group, 
                               string assetType, long minSize, List<string> addedFiles)
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets(assetType);
        
        Debug.Log($"Found {guids.Length} total assets of type {assetType}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Пропустить системные файлы
            if (path.StartsWith("Packages/")) continue;
            if (path.Contains("TextMesh Pro")) continue;
            if (path.Contains("Resources")) continue;
            if (path.Contains("Editor")) continue; // Editor-only ресурсы
            
            // Проверить размер файла
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) continue;
            
            long fileSizeKB = fileInfo.Length / 1024;
            
            // Показать первые 5 для диагностики
            if (count < 5)
            {
                Debug.Log($"  Checking: {Path.GetFileName(path)} → {fileSizeKB} KB");
            }
            
            if (fileInfo.Length < minSize) continue;
            
            try
            {
                // Добавить в Addressables
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = Path.GetFileNameWithoutExtension(path);
                
                addedFiles.Add($"{Path.GetFileName(path)} ({fileSizeKB} KB)");
                count++;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not add {path}: {e.Message}");
            }
        }
        
        Debug.Log($"→ Added {count} assets (> {minSize / 1024} KB)");
        return count;
    }
#endif
}