using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerView : MonoBehaviour
{
    [Serializable]
    public class ItemView
    {
        public ItemData item;
        public GameObject visual;
    }

    [SerializeField] private ItemContainer _container;
    [SerializeField] private List<ItemView> _itemViews;

    private void OnEnable()
    {
        _container.ItemChanged += OnItemChanged;
        OnItemChanged(_container.Item);
    }

    private void OnDisable()
    {
        _container.ItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(ItemData item)
    {
        foreach (var view in _itemViews)
        {
            if (view.visual != null)
                view.visual.SetActive(view.item == item);
        }
    }
}
