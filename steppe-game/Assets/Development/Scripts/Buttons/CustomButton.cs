using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    [SerializeField] private Button button;
    public event Action OnButtonClicked;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (!button)
            button = GetComponent<Button>();
    }
#endif

    private void OnEnable()
    {
        button.onClick.AddListener(Clicked);
    }

    private void OnDisable()
    {
        button.onClick.AddListener(Clicked);
    }

    protected virtual void Clicked()
    {
        OnButtonClicked?.Invoke();
    }
}
