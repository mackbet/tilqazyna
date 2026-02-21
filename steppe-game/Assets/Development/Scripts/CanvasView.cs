using UnityEngine;

public class CanvasView : MonoBehaviour
{
    public Canvas Canvas => _canvas;
    [SerializeField] private Canvas _canvas;
    public bool IsScreenSpaceCamera => isScreenSpaceCamera;
    [SerializeField] private bool isScreenSpaceCamera = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_canvas)
            _canvas = GetComponent<Canvas>();
    }
#endif

    private void OnEnable()
    {
        ShowView();
    }

    private void OnDisable()
    {
        HideView();
    }

    protected virtual void ShowView()
    {

    }

    protected virtual void HideView()
    {

    }
}
