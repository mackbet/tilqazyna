using UnityEngine;

public class ArrowWobble : MonoBehaviour
{
    [SerializeField] private float _amplitude = 15f;
    [SerializeField] private float _speed = 3f;

    private Vector3 _originPosition;
    private float _phase;

    private void Start()
    {
        _originPosition = transform.position;
        _phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        _phase += Time.deltaTime * _speed;
        float offset = Mathf.Sin(_phase) * _amplitude;
        transform.position = _originPosition + (-transform.up) * offset;
    }

    private void OnDisable()
    {
        if (_originPosition != Vector3.zero)
            transform.position = _originPosition;
    }
}
