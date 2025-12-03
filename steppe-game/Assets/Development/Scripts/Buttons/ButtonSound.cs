using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private AudioClip clickedSound;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!button)
            button = GetComponent<Button>();
    }
#endif

    private void OnEnable()
    {
        button.onClick.AddListener(ClickSound);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(ClickSound);
    }

    private void ClickSound()
    {
        AudioManager.Instance.PlaySound(clickedSound);
    }
}
