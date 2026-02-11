using UnityEngine;

public class ClickVesselAnimator : MonoBehaviour
{
    [SerializeField] private ClickVessel _vessel;
    [SerializeField] private ImageAnimator _animator;

    private void OnEnable()
    {
        _vessel.RecipeCompleted += OnRecipeCompleted;
    }

    private void OnDisable()
    {
        _vessel.RecipeCompleted -= OnRecipeCompleted;
    }

    private void OnRecipeCompleted(RecipeData recipe, ItemData result)
    {
        _animator.StartAnimation();
    }
}
