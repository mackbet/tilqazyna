using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrophyPanel : CanvasView, IDragHandler, IPointerDownHandler
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Image _background;
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _rotationSpeed = 0.5f;
    [SerializeField] private AudioClip _openSound;

    private const float TargetAlpha = 0.85f;

    private void Awake() => _closeButton.onClick.AddListener(Close);

    public void Show(GameObject trophyPrefab)
    {
        Podium.Instance.Show(trophyPrefab);
    }

    public void Close()
    {
        Podium.Instance.Hide();
        CanvasViewManager.Instance.Unload(this);
    }

    protected override void ShowView()
    {
        _background.DOKill();
        _background.DOFade(TargetAlpha, _fadeDuration).From(0f);
        SoundManager.Instance.PlaySound(_openSound);
    }

    protected override void HideView()
    {
        _background.DOKill();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        Podium.Instance.Rotate(-eventData.delta.x * _rotationSpeed);
    }
}
