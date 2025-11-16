using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private static CanvasManager instance;
    public static CanvasManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CanvasManager");
                instance = go.AddComponent<CanvasManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private List<Canvas> activeCanvases = new List<Canvas>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Загружает канвас из префаба в режиме Single (удаляет все предыдущие канвасы)
    /// </summary>
    public Canvas LoadCanvas(GameObject canvasPrefab, bool single = true)
    {
        if (single)
            UnloadAllCanvases();

        if (canvasPrefab == null)
        {
            Debug.LogError("CanvasManager: Префаб канваса null!");
            return null;
        }

        GameObject canvasObject = Instantiate(canvasPrefab, transform);
        Canvas canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("CanvasManager: На префабе нет компонента Canvas!");
            Destroy(canvasObject);
            return null;
        }

        activeCanvases.Add(canvas);
        return canvas;
    }

    /// <summary>
    /// Создает новый пустой канвас
    /// </summary>
    public Canvas CreateCanvas(string canvasName = "Canvas", bool single = true)
    {
        if (single)
            UnloadAllCanvases();

        GameObject canvasObject = new GameObject(canvasName);
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        activeCanvases.Add(canvas);
        return canvas;
    }

    /// <summary>
    /// Выгружает конкретный канвас
    /// </summary>
    public void UnloadCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        activeCanvases.Remove(canvas);
        Destroy(canvas.gameObject);
    }

    /// <summary>
    /// Выгружает все канвасы
    /// </summary>
    public void UnloadAllCanvases()
    {
        foreach (Canvas canvas in activeCanvases)
        {
            if (canvas != null)
                Destroy(canvas.gameObject);
        }
        activeCanvases.Clear();
    }

    /// <summary>
    /// Получить все активные канвасы
    /// </summary>
    public List<Canvas> GetActiveCanvases()
    {
        activeCanvases.RemoveAll(c => c == null);
        return new List<Canvas>(activeCanvases);
    }

    /// <summary>
    /// Получить канвас по имени
    /// </summary>
    public Canvas GetCanvasByName(string name)
    {
        return activeCanvases.Find(c => c != null && c.gameObject.name == name);
    }
}