using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleLoader : MonoBehaviour
{
    private const string LocaleKey = "SelectedLocaleId";

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (PlayerPrefs.HasKey(LocaleKey))
        {
            int savedLocaleId = PlayerPrefs.GetInt(LocaleKey);
            var locales = LocalizationSettings.AvailableLocales.Locales;

            if (savedLocaleId >= 0 && savedLocaleId < locales.Count)
                LocalizationSettings.SelectedLocale = locales[savedLocaleId];
        }
    }
}
