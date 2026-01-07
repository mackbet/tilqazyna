using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizableText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [field: SerializeField] public LocalizedString LocalizedString { get; private set; }

    public void SetString(LocalizedString localization)
    {
        LocalizedString = localization;
        Localize();
    }

    private void OnEnable()
    {
        if (LocalizedString != null)
            Localize();

        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void OnSelectedLocaleChanged(Locale obj) => Localize();


    private void Localize()
    {
        if (LocalizedString.TableEntryReference.KeyId != 0)
            display.text = LocalizedString.GetLocalizedString();
    }

    // private void OnValidate()
    // {
    //     if (!display) display = GetComponent<TextMeshProUGUI>();

    //     if (display.text == "")
    //         Localize();
    // }
}