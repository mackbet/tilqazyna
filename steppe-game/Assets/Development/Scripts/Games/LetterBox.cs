using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private Image background;

    private void Awake()
    {
        // Автоматически находим компоненты если не назначены
        if (letterText == null)
        {
            letterText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (background == null)
        {
            background = GetComponent<Image>();
        }
    }

    public void SetLetter(char letter)
    {
        letterText.text = letter.ToString();
    }

    public void SetColor(Color color)
    {
        if (background != null)
        {
            background.color = color;
        }
    }
}
