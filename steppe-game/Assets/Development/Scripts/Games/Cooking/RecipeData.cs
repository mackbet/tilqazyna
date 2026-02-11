using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [SerializeField] private ItemData[] _ingredients;
    [SerializeField] private ItemData _result;
    [SerializeField] private ItemData[] _extraResults;

    public ItemData[] Ingredients => _ingredients;
    public ItemData Result => _result;
    public ItemData[] ExtraResults => _extraResults;

    public bool IsMatch(System.Collections.Generic.ICollection<ItemData> items)
    {
        if (items.Count < _ingredients.Length) return false;

        foreach (var ingredient in _ingredients)
        {
            if (!items.Contains(ingredient)) return false;
        }

        return true;
    }
}
