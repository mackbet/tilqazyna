using UnityEngine;

public class ClickVesselSound : MonoBehaviour
{
    [SerializeField] private ClickVessel _vessel;
    [SerializeField] private AudioClip _readyClip;     // когда все ингредиенты добавлены
    [SerializeField] private AudioClip _completedClip; // когда нажали и рецепт выполнен

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_vessel)
            _vessel = GetComponent<ClickVessel>();
    }
#endif

    private void OnEnable()
    {
        _vessel.RecipeReady += OnRecipeReady;
        _vessel.RecipeCompleted += OnRecipeCompleted;
    }

    private void OnDisable()
    {
        _vessel.RecipeReady -= OnRecipeReady;
        _vessel.RecipeCompleted -= OnRecipeCompleted;
    }

    private void OnRecipeReady(RecipeData recipe)
    {
        SoundManager.Instance?.PlaySound(_readyClip);
    }

    private void OnRecipeCompleted(RecipeData recipe, ItemData result)
    {
        SoundManager.Instance?.PlaySound(_completedClip);
    }
}
