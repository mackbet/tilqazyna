using DG.Tweening;
using UnityEngine;

public class Podium : MonoBehaviour
{
    public static Podium Instance { get; private set; }

    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private float _showDuration = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutBack;
    private Vector3 _targetScale;
    private GameObject _current;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _targetScale = transform.localScale;
    }

    public GameObject Show(GameObject prefab)
    {
        if (_current != null)
            Destroy(_current);

        _current = Instantiate(prefab, transform);
        _current.transform.localScale = Vector3.zero;
        _current.transform.DOScale(_targetScale, _showDuration).SetEase(_ease);

        if (_particles != null)
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particles.Play();
        }
        return _current;
    }

    public void Rotate(float deltaY)
    {
        if (_current == null) return;
        _current.transform.Rotate(Vector3.up, deltaY, Space.World);
    }

    public void Hide()
    {
        if (_current != null)
            Destroy(_current);
        _current = null;

        if (_particles != null)
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
