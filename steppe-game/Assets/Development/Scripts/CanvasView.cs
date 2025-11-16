using UnityEngine;

public class CanvasView : MonoBehaviour
{


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
