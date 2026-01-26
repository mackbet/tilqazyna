using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExcavationCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image groundImage;
    [SerializeField] private Image artifactImage;
    [SerializeField] private TextMeshProUGUI neighborCountText;
    [SerializeField] private ParticleSystem digEffect;
    [SerializeField] private RectTransform digRectTransform;
    [SerializeField] private Material completedMaterial;
    [SerializeField] private Material defaultMaterial;

    [Header("Number Colors")]
    [SerializeField] private Color color1 = Color.blue;
    [SerializeField] private Color color2 = Color.green;
    [SerializeField] private Color color3 = Color.red;
    [SerializeField] private Color color4 = new Color(0.5f, 0f, 0.5f);
    [SerializeField] private Color colorDefault = Color.black;

    private Vector2Int gridPosition;
    private bool hasArtifact;
    private bool isRevealed;
    private Sprite artifactSprite;

    public event Action<ExcavationCell> OnCellClicked;

    public Vector2Int GridPosition => gridPosition;
    public bool HasArtifact => hasArtifact;
    public bool IsRevealed => isRevealed;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
        digRectTransform.SetParent(transform.parent);
        digRectTransform.SetAsLastSibling();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Initialize(int col, int row)
    {
        gridPosition = new Vector2Int(col, row);
        Reset();
    }

    public void Reset()
    {
        isRevealed = false;
        hasArtifact = false;
        artifactSprite = null;

        groundImage.gameObject.SetActive(true);
        artifactImage.gameObject.SetActive(false);
        artifactImage.material = defaultMaterial;
        neighborCountText.gameObject.SetActive(false);

        button.interactable = true;
    }

    public void SetArtifact(Sprite sprite)
    {
        hasArtifact = true;
        artifactSprite = sprite;
        artifactImage.sprite = sprite;
    }

    public void Reveal(int neighborArtifactCount, bool byPlayer = false)
    {
        if (isRevealed) return;

        isRevealed = true;
        button.interactable = false;

        digEffect.Play();

        // Скрываем землю
        groundImage.gameObject.SetActive(false);

        if (hasArtifact)
        {
            // Показываем артефакт
            artifactImage.gameObject.SetActive(true);
            // Применяем completedMaterial только если игрок сам открыл ячейку
            if (byPlayer)
            {
                artifactImage.material = completedMaterial;
            }
        }
        else
        {
            // Показываем число соседних артефактов
            if (neighborArtifactCount > 0)
            {
                neighborCountText.text = neighborArtifactCount.ToString();
                neighborCountText.gameObject.SetActive(true);
                SetNumberColor(neighborArtifactCount);
            }
        }
    }

    private void SetNumberColor(int count)
    {
        neighborCountText.color = count switch
        {
            1 => color1,
            2 => color2,
            3 => color3,
            4 => color4,
            _ => colorDefault
        };
    }

    private void OnClick()
    {
        if (!isRevealed)
        {
            OnCellClicked?.Invoke(this);
        }
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable && !isRevealed;
    }
}
