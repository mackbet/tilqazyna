using UnityEngine;

public class CameraUI : MonoBehaviour
{
    private static CameraUI instance;
    public static CameraUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<CameraUI>();

            return instance;
        }
    }

    public Camera UICamera => _uiCamera;
    [SerializeField] private Camera _uiCamera;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}