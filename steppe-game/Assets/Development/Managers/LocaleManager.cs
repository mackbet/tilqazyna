using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    public static event Action OnChangeQuestionUI;

    private const string LocaleKey = "SelectedLocaleId";
    private bool isActive;

    private void Start()
    {
        if (PlayerPrefs.HasKey(LocaleKey))
        {
            int savedLocaleId = PlayerPrefs.GetInt(LocaleKey);
            StartCoroutine(SetLocale(savedLocaleId, false));
        }
    }

    public void ChangeLocale(int localeId)
    {
        if (isActive)
        {
            Debug.LogWarning("Locale change is already in progress.");
            return;
        }

        if (!IsValidLocaleId(localeId))
        {
            Debug.LogError($"Invalid locale ID: {localeId}. Please use a valid ID.");
            return;
        }

        StartCoroutine(SetLocale(localeId, true));
    }

    private IEnumerator SetLocale(int localeId, bool save)
    {
        isActive = true;

        // Wait for localization settings to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Set the selected locale
        var locales = LocalizationSettings.AvailableLocales.Locales;
        LocalizationSettings.SelectedLocale = locales[localeId];

        if (save)
        {
            PlayerPrefs.SetInt(LocaleKey, localeId);
            PlayerPrefs.Save();
        }

        Debug.Log($"Locale changed to: {LocalizationSettings.SelectedLocale.LocaleName}");
        isActive = false;

        OnChangeQuestionUI?.Invoke();
    }

    private bool IsValidLocaleId(int localeId)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        return localeId >= 0 && localeId < locales.Count;
    }
}
