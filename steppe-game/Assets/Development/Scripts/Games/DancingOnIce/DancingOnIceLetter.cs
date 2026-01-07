using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DancingOnIceLetter : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private Image background;

    [Header("Visual Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

    private char letter;
    private int index;
    private DancingOnIce gameController;
    private bool isSelected = false;

    public int Index => index;
    public char Letter => letter;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (letterText == null)
            letterText = GetComponentInChildren<TextMeshProUGUI>();

        if (background == null)
            background = GetComponent<Image>();
    }
#endif

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (letterText == null)
            letterText = GetComponentInChildren<TextMeshProUGUI>();

        if (background == null)
            background = GetComponent<Image>();

        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
    }

    public void Initialize(char letterChar, int letterIndex, DancingOnIce controller)
    {
        letter = letterChar;
        index = letterIndex;
        gameController = controller;

        if (letterText != null)
        {
            letterText.text = letter.ToString();
        }

        SetSelected(false);
    }

    private void OnClicked()
    {
        if (gameController != null && !isSelected)
        {
            gameController.OnLetterClicked(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Меняем спрайт фона
        if (background != null)
        {
            background.sprite = selected ? selectedSprite : normalSprite;
        }

        // Меняем цвет текста
        if (letterText != null)
        {
            letterText.color = selected ? selectedTextColor : normalTextColor;
        }

        if (button != null)
        {
            button.interactable = !selected;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
        }
    }
}
