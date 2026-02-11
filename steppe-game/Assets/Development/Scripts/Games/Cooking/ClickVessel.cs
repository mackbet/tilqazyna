using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickVessel : Vessel, IPointerClickHandler
{
    private RecipeData _pendingRecipe;

    public bool IsReady => _pendingRecipe != null;

    public event Action<RecipeData> RecipeReady;

    protected override void OnRecipeReady(RecipeData recipe)
    {
        _pendingRecipe = recipe;
        RecipeReady?.Invoke(recipe);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Process();
    }

    public void Process()
    {
        if (_pendingRecipe == null)
        {
            Debug.Log($"[ClickVessel] Process called but no pending recipe");
            return;
        }

        Debug.Log($"[ClickVessel] Processing recipe: {_pendingRecipe.name}");
        CompleteRecipe(_pendingRecipe);
        _pendingRecipe = null;
    }
}
