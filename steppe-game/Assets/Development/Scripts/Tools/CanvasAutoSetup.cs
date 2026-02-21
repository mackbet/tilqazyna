using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Автоматически конвертирует все Canvas в Screen Space - Camera mode при запуске
/// </summary>
public class CanvasAutoSetup : MonoBehaviour
{
    [SerializeField] private float planeDistance = 100f;
    [SerializeField] private bool runOnAwake = true;
    [SerializeField] private bool includeInactive = true;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        if (runOnAwake)
        {
            SetupAllCanvas();
        }
    }

    [ContextMenu("Setup All Canvas")]
    public void SetupAllCanvas()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();

        Canvas[] allCanvas = includeInactive
            ? FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            : FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        int converted = 0;
        foreach (Canvas canvas in allCanvas)
        {
            // Конвертируем только Overlay canvas
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = _camera;
                canvas.planeDistance = planeDistance;
                converted++;
            }
            // Если уже в Camera mode, просто обновляем камеру
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = _camera;
                    converted++;
                }
            }
        }

        Debug.Log($"[CanvasAutoSetup] Настроено {converted} Canvas на камеру {_camera.name}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();
    }
#endif
}