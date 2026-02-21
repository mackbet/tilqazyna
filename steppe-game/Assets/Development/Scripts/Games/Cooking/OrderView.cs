using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderView : MonoBehaviour
{
    [Serializable]
    public class ItemView
    {
        public ItemData item;
        public GameObject visual;
    }

    [SerializeField] private GameObject _check;
    [SerializeField] private List<ItemView> _itemViews;

    public void Show(ItemData item)
    {
        foreach (var view in _itemViews)
        {
            if (view.item == item && view.visual != null)
            {
                view.visual.SetActive(true);
                return;
            }
        }
    }

    public void Check()
    {
        if (_check != null)
            _check.SetActive(true);
    }

    public void HideAll()
    {
        foreach (var view in _itemViews)
        {
            if (view.visual != null)
                view.visual.SetActive(false);
        }

        if (_check != null)
            _check.SetActive(false);
    }
}
