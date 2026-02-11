using System;
using System.Collections.Generic;
using UnityEngine;

public class VesselView : MonoBehaviour
{
    [Serializable]
    public class ItemView
    {
        public ItemData item;
        public GameObject visual;
    }

    [SerializeField] private Vessel _vessel;
    [SerializeField] private List<ItemView> _itemViews;

    private void OnEnable()
    {
        _vessel.ItemAdded += OnItemAdded;
        _vessel.ItemRemoved += OnItemRemoved;
        _vessel.RecipeCompleted += OnRecipeCompleted;
        SyncVisuals();
    }

    private void OnDisable()
    {
        _vessel.ItemAdded -= OnItemAdded;
        _vessel.ItemRemoved -= OnItemRemoved;
        _vessel.RecipeCompleted -= OnRecipeCompleted;
    }

    private void OnItemAdded(ItemData item)
    {
        SetVisual(item, true);
    }

    private void OnItemRemoved(ItemData item)
    {
        SetVisual(item, false);
    }

    private void OnRecipeCompleted(RecipeData recipe, ItemData result)
    {
        SyncVisuals();
    }

    private void SyncVisuals()
    {
        HideAll();

        foreach (var item in _vessel.Items)
            SetVisual(item, true);
    }

    public void HideAll()
    {
        foreach (var view in _itemViews)
        {
            if (view.visual != null)
                view.visual.SetActive(false);
        }
    }

    private void SetVisual(ItemData item, bool active)
    {
        foreach (var view in _itemViews)
        {
            if (view.item == item && view.visual != null)
            {
                view.visual.SetActive(active);
                return;
            }
        }
    }
}
