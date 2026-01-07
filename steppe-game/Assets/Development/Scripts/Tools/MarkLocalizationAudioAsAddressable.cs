using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.IO;

public class MarkLocalizationAudioAsAddressable
{
    [MenuItem("Tools/Localization/Mark All Audio As Addressable")]
    public static void MarkAllAudioAsAddressable()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("❌ Addressables settings not found! Please install Addressables package.");
            return;
        }

        // Получаем или создаем группу для аудио
        var group = settings.FindGroup("Default Local Group");
        if (group == null)
        {
            group = settings.DefaultGroup;
        }

        Debug.Log("=== Starting to mark audio files as Addressable ===");

        // Получаем все коллекции Asset Tables
        var collections = LocalizationEditorSettings.GetAssetTableCollections();

        int totalProcessed = 0;
        int newlyAdded = 0;
        int alreadyAddressable = 0;
        int errors = 0;

        HashSet<string> processedGuids = new HashSet<string>();

        foreach (var collection in collections)
        {
            Debug.Log($"\n📁 Processing collection: {collection.name}");

            foreach (var tableReference in collection.Tables)
            {
                if (tableReference == null) continue;

                var table = LocalizationEditorSettings.GetAssetTableCollection(collection.TableCollectionNameGuid)
                    ?.GetTable(tableReference.LocaleIdentifier) as AssetTable;

                if (table == null) continue;

                Debug.Log($"  📋 Table: {table.LocaleIdentifier.Code}");

                foreach (var entry in table.Values)
                {
                    if (entry == null) continue;

                    string assetGuid = entry.Guid;

                    // Пропускаем если уже обработали этот GUID
                    if (string.IsNullOrEmpty(assetGuid) || processedGuids.Contains(assetGuid))
                        continue;

                    processedGuids.Add(assetGuid);

                    // Получаем путь к ассету
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        Debug.LogWarning($"    ⚠️ Entry '{entry.Key}' has invalid GUID: {assetGuid}");
                        errors++;
                        continue;
                    }

                    // Проверяем что это аудиофайл
                    var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (asset == null)
                    {
                        // Не аудио файл, пропускаем
                        continue;
                    }

                    totalProcessed++;

                    // Проверяем есть ли уже в Addressables
                    var existingEntry = settings.FindAssetEntry(assetGuid);

                    if (existingEntry == null)
                    {
                        // Добавляем в Addressables
                        var addressableEntry = settings.CreateOrMoveEntry(assetGuid, group);
                        addressableEntry.address = Path.GetFileNameWithoutExtension(assetPath);

                        Debug.Log($"    ✅ Added: {entry.Key} → {assetPath}");
                        newlyAdded++;
                    }
                    else
                    {
                        Debug.Log($"    ✓ Already Addressable: {entry.Key} → {existingEntry.address}");
                        alreadyAddressable++;
                    }
                }
            }
        }

        // Сохраняем изменения
        if (newlyAdded > 0)
        {
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(settings);
        }

        // Итоговый отчет
        Debug.Log("\n=== SUMMARY ===");
        Debug.Log($"📊 Total audio files processed: {totalProcessed}");
        Debug.Log($"✅ Newly added to Addressables: {newlyAdded}");
        Debug.Log($"✓ Already in Addressables: {alreadyAddressable}");

        if (errors > 0)
        {
            Debug.LogWarning($"⚠️ Errors/Warnings: {errors}");
        }

        if (newlyAdded > 0)
        {
            Debug.Log("\n🔨 Now rebuild Addressables:");
            Debug.Log("   Window → Asset Management → Addressables → Groups");
            Debug.Log("   → Build → New Build → Default Build Script");

            // Предлагаем автоматический rebuild
            if (EditorUtility.DisplayDialog("Rebuild Addressables?",
                $"Added {newlyAdded} audio files to Addressables.\n\nRebuild Addressables now?",
                "Yes", "Later"))
            {
                RebuildAddressables();
            }
        }
        else
        {
            Debug.Log("\n✓ All audio files are already in Addressables. No rebuild needed.");
        }
    }

    [MenuItem("Tools/Localization/Show Audio Files In Tables")]
    public static void ShowAudioFilesInTables()
    {
        Debug.Log("=== Audio Files in Localization Tables ===\n");

        var collections = LocalizationEditorSettings.GetAssetTableCollections();
        int totalAudio = 0;

        foreach (var collection in collections)
        {
            Debug.Log($"📁 Collection: {collection.name}");

            HashSet<string> uniqueAudioGuids = new HashSet<string>();

            foreach (var tableReference in collection.Tables)
            {
                if (tableReference == null) continue;

                var table = LocalizationEditorSettings.GetAssetTableCollection(collection.TableCollectionNameGuid)
                    ?.GetTable(tableReference.LocaleIdentifier) as AssetTable;

                if (table == null) continue;

                foreach (var entry in table.Values)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Guid)) continue;

                    string assetPath = AssetDatabase.GUIDToAssetPath(entry.Guid);
                    if (string.IsNullOrEmpty(assetPath)) continue;

                    var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (asset != null)
                    {
                        uniqueAudioGuids.Add(entry.Guid);
                    }
                }
            }

            foreach (var guid in uniqueAudioGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"  🎵 {Path.GetFileName(path)} → {path}");
                totalAudio++;
            }

            Debug.Log("");
        }

        Debug.Log($"Total unique audio files: {totalAudio}");
    }

    private static void RebuildAddressables()
    {
        Debug.Log("\n🔨 Starting Addressables rebuild...");

        try
        {
            // Очистка старых билдов
            AddressableAssetSettings.CleanPlayerContent(
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);

            Debug.Log("  ✓ Cleaned old builds");

            // Новый билд
            AddressableAssetSettings.BuildPlayerContent();

            Debug.Log("✅ Addressables rebuild complete!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Rebuild failed: {e.Message}");
        }
    }
}
