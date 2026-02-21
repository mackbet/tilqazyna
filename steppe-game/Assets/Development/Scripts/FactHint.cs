using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class Fact
{
    [TextArea] public string text;
    public Sprite characterSprite; // если не задан — берётся случайный из _defaultSprites
}

public class FactHint : MonoBehaviour
{
    [SerializeField] private Fact[] _facts;
    [SerializeField] private Sprite[] _defaultSprites;

    [SerializeField] private int _cost = 10;
    [SerializeField] private Button _hintButton;

    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _factText;
    [SerializeField] private Image _characterImage;
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _hintButton.onClick.AddListener(OnHintButtonClicked);
        _closeButton.onClick.AddListener(ClosePanel);
        _panel.SetActive(false);
    }

    private void OnHintButtonClicked()
    {
        if (StateManager.Instance.CurrencyAmount < _cost)
            return;

        StateManager.Instance.CurrencyAmount -= _cost;
        _hintButton.interactable = false;

        ShowFact();
    }

    private void ShowFact()
    {
        if (_facts == null || _facts.Length == 0) return;

        var fact = _facts[Random.Range(0, _facts.Length)];

        _factText.text = fact.text;

        Sprite sprite = fact.characterSprite;
        if (sprite == null && _defaultSprites != null && _defaultSprites.Length > 0)
            sprite = _defaultSprites[Random.Range(0, _defaultSprites.Length)];

        _characterImage.sprite = sprite;
        _characterImage.gameObject.SetActive(sprite != null);

        _panel.SetActive(true);

        _hintButton.gameObject.SetActive(false);
    }

    private void ClosePanel()
    {
        _panel.SetActive(false);
        _hintButton.gameObject.SetActive(true);
    }
}
