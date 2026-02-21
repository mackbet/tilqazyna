using UnityEngine;

public class VesselSound : MonoBehaviour
{
    [SerializeField] private Vessel _vessel;
    [SerializeField] private AudioClip _itemAddedClip;
    [SerializeField] private AudioClip _itemRemovedClip;
    [SerializeField] private AudioClip _recipeCompletedClip;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_vessel)
            _vessel = GetComponent<Vessel>();
    }
#endif

    private void OnEnable()
    {
        _vessel.ItemAdded += OnItemAdded;
        _vessel.ItemRemoved += OnItemRemoved;
        _vessel.RecipeCompleted += OnRecipeCompleted;
    }

    private void OnDisable()
    {
        _vessel.ItemAdded -= OnItemAdded;
        _vessel.ItemRemoved -= OnItemRemoved;
        _vessel.RecipeCompleted -= OnRecipeCompleted;
    }

    private void OnItemAdded(ItemData item) => SoundManager.Instance?.PlaySound(_itemAddedClip);
    private void OnItemRemoved(ItemData item) => SoundManager.Instance?.PlaySound(_itemRemovedClip);
    private void OnRecipeCompleted(RecipeData recipe, ItemData result) => SoundManager.Instance?.PlaySound(_recipeCompletedClip);
}
