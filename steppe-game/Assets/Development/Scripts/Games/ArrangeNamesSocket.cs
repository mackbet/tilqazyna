using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrangeNamesSocket : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private RectTransform attachPoint;
    [SerializeField] private List<ArrangeNamesPlug> validPlugs = new List<ArrangeNamesPlug>();
    [SerializeField] private Material acceptedColor;
    private Material material;
    private bool isOccupied = false;
    private ArrangeNamesPlug attachedPlug = null;

    public event Action<ArrangeNamesSocket> OnSocketEntered;

    public RectTransform AttachPoint => attachPoint != null ? attachPoint : GetComponent<RectTransform>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!image)
            image = GetComponent<Image>();

        if (!attachPoint)
            attachPoint = GetComponent<RectTransform>();
    }
#endif

    private void Start()
    {
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public bool CanAcceptPlug(ArrangeNamesPlug plug)
    {
        if (isOccupied)
            return false;

        return validPlugs.Contains(plug);
    }

    public void AttachPlug(ArrangeNamesPlug plug)
    {
        if (!CanAcceptPlug(plug))
            return;

        image.material = acceptedColor;
        isOccupied = true;
        attachedPlug = plug;

        OnSocketEntered?.Invoke(this);
    }

    public void DetachPlug()
    {
        isOccupied = false;
        if (attachedPlug != null)
        {
            attachedPlug.Reset();
            attachedPlug = null;
        }
    }

    public void Reset()
    {
        DetachPlug();
    }

}