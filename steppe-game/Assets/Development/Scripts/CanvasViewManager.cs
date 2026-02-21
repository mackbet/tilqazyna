using System.Collections.Generic;
using UnityEngine;

public class CanvasViewManager : MonoBehaviour
{
    private static CanvasViewManager instance;
    public static CanvasViewManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CanvasViewManager");
                instance = go.AddComponent<CanvasViewManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Stack<CanvasView> viewStack = new Stack<CanvasView>();

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
    /// Загружает CanvasView из префаба
    /// </summary>
    /// <param name="viewPrefab">Префаб CanvasView</param>
    /// <param name="single">Если true - удаляет все предыдущие views</param>
    public T Load<T>(T viewPrefab, bool single = true) where T : CanvasView
    {
        if (single)
            UnloadAll();

        if (viewPrefab == null)
        {
            Debug.LogError("CanvasViewManager: Префаб null!");
            return null;
        }

        T view = Instantiate(viewPrefab, transform);

        // Настраиваем Canvas на UI камеру
        if (view.IsScreenSpaceCamera)
            SetupCanvasCamera(view);

        viewStack.Push(view);
        return view;
    }

    /// <summary>
    /// Загружает CanvasView из префаба (без generic)
    /// </summary>
    public CanvasView Load(CanvasView viewPrefab, bool single = true)
    {
        return Load<CanvasView>(viewPrefab, single);
    }

    /// <summary>
    /// Настраивает Canvas на использование UI камеры
    /// </summary>
    private void SetupCanvasCamera(CanvasView view)
    {
        if (view.Canvas == null)
        {
            Debug.LogWarning($"CanvasViewManager: CanvasView '{view.name}' не имеет Canvas компонента!");
            return;
        }

        Camera uiCamera = CameraUI.Instance?.UICamera;
        if (uiCamera == null)
        {
            Debug.LogError("CanvasViewManager: UI Camera не найдена! Убедитесь что CameraUI существует на сцене.");
            return;
        }

        // Переключаем на Screen Space - Camera mode
        view.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        view.Canvas.worldCamera = uiCamera;
        view.Canvas.planeDistance = 100f; // можно вынести в настройки если нужно
    }

    /// <summary>
    /// Выгружает текущий (верхний) view
    /// </summary>
    public void Unload()
    {
        if (viewStack.Count == 0) return;

        CanvasView view = viewStack.Pop();
        if (view != null)
            Destroy(view.gameObject);
    }

    /// <summary>
    /// Выгружает конкретный view
    /// </summary>
    public void Unload(CanvasView view)
    {
        if (view == null) return;

        Stack<CanvasView> tempStack = new Stack<CanvasView>();
        bool found = false;

        while (viewStack.Count > 0)
        {
            CanvasView current = viewStack.Pop();
            if (current == view)
            {
                Destroy(current.gameObject);
                found = true;
                break;
            }
            tempStack.Push(current);
        }

        while (tempStack.Count > 0)
            viewStack.Push(tempStack.Pop());

        if (!found)
            Debug.LogWarning("CanvasViewManager: View не найден в стеке!");
    }

    /// <summary>
    /// Выгружает все views
    /// </summary>
    public void UnloadAll()
    {
        while (viewStack.Count > 0)
        {
            CanvasView view = viewStack.Pop();
            if (view != null)
                Destroy(view.gameObject);
        }
    }

    /// <summary>
    /// Получить текущий (верхний) view
    /// </summary>
    public CanvasView GetCurrent()
    {
        return viewStack.Count > 0 ? viewStack.Peek() : null;
    }

    /// <summary>
    /// Получить текущий view с типом
    /// </summary>
    public T GetCurrent<T>() where T : CanvasView
    {
        return viewStack.Count > 0 ? viewStack.Peek() as T : null;
    }

    /// <summary>
    /// Получить количество активных views
    /// </summary>
    public int GetViewCount()
    {
        return viewStack.Count;
    }
}