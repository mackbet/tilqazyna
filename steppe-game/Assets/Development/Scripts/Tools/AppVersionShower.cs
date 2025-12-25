using TMPro;
using UnityEngine;

public class AppVersionShower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionField;

    private void OnValidate()
    {
        versionField.text = $"v{Application.version}";
    }

    private void OnEnable()
    {
        versionField.text = $"v{Application.version}";
    }
}
