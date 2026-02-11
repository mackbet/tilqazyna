using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Vessel : MonoBehaviour
{
    [SerializeField] private RecipeData[] _recipes;
    [SerializeField] private ItemContainer _resultContainer;
    [SerializeField] private float _resultAppearDuration = 0.4f;
    [SerializeField] private List<ItemData> _items = new();

    private ItemData _lastSpawnedResult;
    private Tweener _resultTween;

    public IReadOnlyList<ItemData> Items => _items;

    public event Action<ItemData> ItemAdded;
    public event Action<ItemData> ItemRemoved;
    public event Action<RecipeData, ItemData> RecipeCompleted;

    private void Start()
    {
        if (_items.Count > 0)
            CheckRecipes();
    }

    public bool HasResult => _items.Count == 1 && IsResult(_items[0]);

    public bool CanAccept(ItemData item)
    {
        if (item == null || _items.Contains(item) || HasResult) return false;

        foreach (var recipe in _recipes)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient == item)
                    return HasPrerequisites(item);
            }
        }

        return false;
    }

    private bool HasPrerequisites(ItemData item)
    {
        if (item.Prerequisites == null || item.Prerequisites.Length == 0)
            return true;

        foreach (var prerequisite in item.Prerequisites)
        {
            if (!_items.Contains(prerequisite))
            {
                Debug.Log($"[Vessel] {item.name} requires {prerequisite.name} in vessel first");
                return false;
            }
        }

        return true;
    }

    private bool IsResult(ItemData item)
    {
        foreach (var recipe in _recipes)
        {
            if (recipe.Result == item) return true;
        }

        return false;
    }

    public bool Contains(ItemData item) => _items.Contains(item);

    public void Clear()
    {
        _items.Clear();
    }

    public void AddItem(ItemData item)
    {
        _items.Add(item);
        ItemAdded?.Invoke(item);
        CheckRecipes();
    }

    public void RemoveItem(ItemData item)
    {
        if (_items.Remove(item))
            ItemRemoved?.Invoke(item);
    }

    protected void CompleteRecipe(RecipeData recipe)
    {
        var result = recipe.Result;
        _items.Clear();
        _items.Add(result);

        if (recipe.ExtraResults != null)
        {
            foreach (var extra in recipe.ExtraResults)
                _items.Add(extra);
        }

        RecipeCompleted?.Invoke(recipe, result);
        SpawnResult(result);
    }

    private void SpawnResult(ItemData result)
    {
        if (_resultContainer == null) return;

        _lastSpawnedResult = result;
        _resultContainer.Put(result);

        var rt = _resultContainer.transform;
        rt.localScale = Vector3.zero;
        _resultContainer.gameObject.SetActive(true);

        KillResultTween();
        _resultTween = rt
            .DOScale(Vector3.one, _resultAppearDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => _resultTween = null);

        _resultContainer.ItemChanged += OnResultTaken;
    }

    private void KillResultTween()
    {
        if (_resultTween != null && _resultTween.IsActive())
        {
            _resultTween.Kill();
            _resultTween = null;
        }
    }

    private void OnResultTaken(ItemData item)
    {
        if (item != null) return;

        _resultContainer.ItemChanged -= OnResultTaken;
        _resultContainer.gameObject.SetActive(false);
        _items.Remove(_lastSpawnedResult);
        _lastSpawnedResult = null;
    }

    private void CheckRecipes()
    {
        foreach (var recipe in _recipes)
        {
            if (!recipe.IsMatch(_items)) continue;
            OnRecipeReady(recipe);
            return;
        }
    }

    protected virtual void OnRecipeReady(RecipeData recipe)
    {
        CompleteRecipe(recipe);
    }
}
