using UnityEngine;
using UnityEngine.UI;

public class RecipeViewManager : MonoBehaviour
{
    [Header("Recipe Buttons")]
    public Button[] recipeButtons; // Assign your recipe buttons here

    [Header("Recipe Windows")]
    public GameObject[] recipeWindows; // Assign your recipe windows here

    [Header("Close Buttons")]
    public Button[] closeButtons; // Assign close buttons for each window
    
    [SerializeField] private CookingManager cookingManager;

    private GameObject activeWindow;

    void Start()
    {
        // Link recipe buttons to their respective actions
        for (var i = 0; i < recipeButtons.Length; i++)
        {
            var index = i; // Avoid closure issue
            recipeButtons[i].onClick.AddListener(() => ShowWindow(index));
        }

        // Link close buttons to their respective actions
        for (var i = 0; i < closeButtons.Length; i++)
        {
            var index = i; // Avoid closure issue
            closeButtons[i].onClick.AddListener(() => CloseWindow(index));
        }

        // Ensure all windows are initially hidden
        HideAllWindows();
    }

    public void ShowWindow(int index)
    {
        if (index < 0 || index >= recipeWindows.Length)
        {
            Debug.LogError("Invalid window index.");
            return;
        }

        //cookingManager.PlayReciepeSound();
        // Hide the currently active window if any
        if (activeWindow != null)
        {
            activeWindow.SetActive(false);
        }

        // Show the selected window
        activeWindow = recipeWindows[index];
        activeWindow.SetActive(true);

        // Bring the window to the front of the UI
        var windowTransform = activeWindow.GetComponent<RectTransform>();
        if (windowTransform != null)
        {
            windowTransform.SetAsLastSibling();
        }
    }

    public void CloseWindow(int index)
    {
        if (index < 0 || index >= recipeWindows.Length)
        {
            Debug.LogError("Invalid window index.");
            return;
        }

        // Hide the window
        recipeWindows[index].SetActive(false);

        // Clear the active window if it's the one being closed
        if (activeWindow == recipeWindows[index])
        {
            activeWindow = null;
        }
    }

    public void HideAllWindows()
    {
        foreach (var window in recipeWindows)
        {
            window.SetActive(false);
        }
        activeWindow = null;
    }
}
