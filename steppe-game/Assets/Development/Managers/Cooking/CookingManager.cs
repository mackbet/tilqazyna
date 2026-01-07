using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Development.Managers.Cooking;
using Development.Objects.Cooking;
using Unity.VisualScripting;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    [SerializeField] private CookingViewManager _cookingView;
    [SerializeField] private DragDropManager _dragDropManager;
    [SerializeField] private CookingTipManager _cookingTipManager;
    [SerializeField] private SoundManager _soundManager;
    private IngredientPrerequisites _prerequisites;

    [Space, SerializeField] private GameObject _trashBin;
    [SerializeField] private GameObject _fireExtinguisher;
    [SerializeField] private GameObject _fireExtinguisherPot;
    [SerializeField] private SpriteAnimator _fireExtinguisherPotAnimator;
    [SerializeField] private GameObject _fireExtinguisherPot2;
    [SerializeField] private SpriteAnimator _fireExtinguisherPot2Animator;

    [Space, SerializeField] private SpriteAnimator _fireStovePotAnimator;
    [SerializeField] private SpriteAnimator _fireStovePot2Animator;
    [SerializeField] private SpriteAnimator _fireStoveCauldronAnimator;
    [SerializeField] private SpriteAnimator _plateRevealAnimator;
    [SerializeField] private SpriteAnimator _smokePotAnimator;
    [SerializeField] private SpriteAnimator _smokePot2Animator;
    [SerializeField] private SpriteAnimator _firePotAnimator;
    [SerializeField] private SpriteAnimator _firePot2Animator;

    [SerializeField] private RectTransform _kumisRect;
    [SerializeField] private Kumis _kumis;

    private readonly Dictionary<IngredientType, List<IngredientType>> _ingredients = new();
    private RecipeManager _recipeManager;

    private IngredientType? _activeFireContainer;

    public event Action OnDoughPlateClickedAction;
    public event Action OnOnionClickedAction;
    //public static event Action OnKumisBottleClickedAction;
    public event Action OnCauldronClickedAction;
    public event Action OnPotClickedAction;
    public static event Action OnPotTimerStarted;

    private void Awake()
    {
        _recipeManager = new RecipeManager();
        _prerequisites = new IngredientPrerequisites();
        _cookingTipManager.Initialize();
    }

    private void OnEnable()
    {
        _dragDropManager.OnIngredientAdded += AddIngredient;
        _dragDropManager.OnOrderCompleted += OnOrderCompleted;

        CookingViewManager.OnTimerFinished += TimerFinished;
        _cookingView.OnFireStarted += FireStarted;
        _cookingView.OnDishBurned += DishBurned;
    }

    private void OnDisable()
    {
        _dragDropManager.OnIngredientAdded -= AddIngredient;
        _dragDropManager.OnOrderCompleted -= OnOrderCompleted;

        CookingViewManager.OnTimerFinished -= TimerFinished;
        _cookingView.OnFireStarted -= FireStarted;
        _cookingView.OnDishBurned -= DishBurned;
    }

    private void DishBurned(IngredientType container)
    {
        if (!_ingredients.TryGetValue(container, out var ingredients))
        {
            return;
        }

        if (container == IngredientType.Pot)
        {
            _fireStovePotAnimator.StopAnimation();
            _smokePotAnimator.StartAnimation();
        }
        else if (container == IngredientType.Pot2)
        {
            _fireStovePot2Animator.StopAnimation();
            _smokePot2Animator.StartAnimation();
        }
        else if (container == IngredientType.Cauldron)
        {
            _fireStoveCauldronAnimator.StopAnimation();
        }

        if ((ingredients.Contains(IngredientType.MilkCooked) || ingredients.Contains(IngredientType.Milk)) &&
            ingredients.Contains(IngredientType.Salt))
        {
            _ingredients.Remove(container);
            AddIngredient(container, IngredientType.MilkBurned);
        }
        else if (ingredients.Contains(IngredientType.BaursakCooked))
        {
            _ingredients.Remove(container);
            AddIngredient(container, IngredientType.BaursakBurned);
        }

        _trashBin.SetActive(true);
    }

    private void FireStarted(IngredientType container)
    {
        _activeFireContainer = container;

        _trashBin.SetActive(false);
        _fireExtinguisher.SetActive(true);

        if (container == IngredientType.Pot)
        {
            _firePotAnimator.StartAnimation();
        }
        else if (container == IngredientType.Pot2)
        {
            _firePot2Animator.StartAnimation();
        }

        _cookingTipManager.StartFire();
    }

    public void OnFireExtinguisherClicked()
    {
        if (_activeFireContainer == IngredientType.Pot)
        {
            _fireExtinguisherPot.SetActive(true);
            _fireExtinguisherPotAnimator.StartAnimation();
        }
        else if (_activeFireContainer == IngredientType.Pot2)
        {
            _fireExtinguisherPot2.SetActive(true);
            _fireExtinguisherPot2Animator.StartAnimation();
        }

        _fireExtinguisher.SetActive(false);
        StopAllFireAnimations();

        StartCoroutine(HideExtinguishers());

        _activeFireContainer = null;
        _trashBin.SetActive(true);

        _cookingTipManager.EndFire();
    }

    private IEnumerator HideExtinguishers()
    {
        yield return new WaitForSeconds(2f);
        _fireExtinguisherPot.SetActive(false);
        _fireExtinguisherPot2.SetActive(false);
        _fireExtinguisherPotAnimator.StopAnimation();
        _fireExtinguisherPot2Animator.StopAnimation();
    }

    public void OnDoughPlateClicked()
    {
        if (_ingredients.TryGetValue(IngredientType.DoughBowl, out var ingredients))
        {
            var recipe = _recipeManager.GetRecipeForIngredients(IngredientType.DoughBowl, ingredients);
            if (recipe != null && (recipe.Result == IngredientType.DoughBaursak ||
                                   recipe.Result == IngredientType.DoughBershmak))
            {
                OnDoughPlateClickedAction?.Invoke();
                ProcessRecipe(IngredientType.DoughBowl, recipe);
                _plateRevealAnimator.StartAnimation();
                StartCoroutine(HideReveal());
            }
        }
    }

    private IEnumerator HideReveal()
    {
        _soundManager.PlayRevealSound();
        yield return new WaitForSeconds(1.2f);
        _plateRevealAnimator.StopAnimation();
    }

    public void OnRollingPinClicked()
    {
        if (_ingredients.TryGetValue(IngredientType.Board, out var ingredients))
        {
            // var recipe = _recipeManager.GetRecipeForIngredients(IngredientType.Board, ingredients);
            // if (recipe != null && recipe.Result == IngredientType.Baursak)
            // {
            //     OnRollingPinClickedAction?.Invoke();
            //     ProcessRecipe(IngredientType.Board, recipe);
            //     _soundManager.PlayRollingPinSound();
            // }
        }
    }

    public void OnKnifeClicked()
    {
        if (_ingredients.TryGetValue(IngredientType.Board, out var ingredients))
        {
            var recipe = _recipeManager.GetRecipeForIngredients(IngredientType.Board, ingredients);
            if (recipe != null && (recipe.Result == IngredientType.DoughBershmakCutted ||
                                    recipe.Result == IngredientType.BershmakMeatCutted))
            {
                ProcessRecipe(IngredientType.Board, recipe);
                _soundManager.PlayKnifeSound();

                if (recipe.Result == IngredientType.BershmakMeatCutted)
                {
                    _soundManager.PlayMeatCutSound();
                }

                if (recipe.Result == IngredientType.BershmakMeatCutted)
                {
                    _soundManager.PlayDoughCutSound();
                }
            }
            else if (recipe != null && recipe.Result == IngredientType.Baursak)
            {
                ProcessRecipe(IngredientType.Board, recipe);
                _soundManager.PlayRollingPinSound();
            }
        }
    }

    public void OnOnionClicked()
    {
        _ingredients[IngredientType.Board] = new List<IngredientType>();
        _cookingView.ClearView(IngredientType.Board);
        AddIngredient(IngredientType.Board, IngredientType.OnionPeeled);
        OnOnionClickedAction?.Invoke();
        _soundManager.PlaySoundOnion();
    }

    public void OnKumisBottleClicked()
    {
        //OnKumisBottleClickedAction?.Invoke();
        _dragDropManager.PlayIngredientAnimation(_kumisRect, IngredientType.KumisCup, IngredientType.Kumis);
        _kumis.SetLidOff();
        //AddIngredient(IngredientType.KumisCup, IngredientType.Kumis);
    }

    public void OnCauldronClicked()
    {
        if (_ingredients.TryGetValue(IngredientType.Cauldron, out var ingredients) &&
            ingredients.Contains(IngredientType.BershmakBoiled) &&
            (!_ingredients.ContainsKey(IngredientType.Bowl1) || _ingredients[IngredientType.Bowl1].Count == 0 ||
             !_ingredients.ContainsKey(IngredientType.Bowl2) || _ingredients[IngredientType.Bowl2].Count == 0) &&
            (!_ingredients.ContainsKey(IngredientType.Board) || _ingredients[IngredientType.Board].Count == 0))
        {
            _cookingView.ClearView(IngredientType.Cauldron);
            if (!_ingredients.ContainsKey(IngredientType.Bowl1))
            {
                OnCauldronClickedAction?.Invoke();
                AddIngredient(IngredientType.Bowl1, IngredientType.BershmakBoiled);
                AddIngredient(IngredientType.Board, IngredientType.MeatCooked);
            }
            else if (!_ingredients.ContainsKey(IngredientType.Bowl2))
            {
                OnCauldronClickedAction?.Invoke();
                AddIngredient(IngredientType.Bowl2, IngredientType.BershmakBoiled);
                AddIngredient(IngredientType.Board, IngredientType.MeatCooked);
            }
        }
    }

    public void OnPotClicked(string containerText)
    {
        var container = IngredientType.Pot;
        if (containerText == "Pot2")
        {
            container = IngredientType.Pot2;
        }
        else if (containerText != "Pot")
        {
            return;
        }

        if (_ingredients.TryGetValue(container, out var ingredients) &&
            ingredients.Contains(IngredientType.MilkCooked) &&
            ingredients.Contains(IngredientType.Salt))
        {
            OnPotClickedAction?.Invoke();
            _cookingView.ClearView(container);
            _ingredients.Remove(container);
            AddIngredient(container, IngredientType.KurutCooked);
            _cookingView.StopTimer(container);
            _cookingView.StopBurnTimer(container);
        }
    }

    private void TimerFinished(IngredientType ingredient)
    {
        if (_ingredients.TryGetValue(ingredient, out var ingredients))
        {
            if (ingredients.Contains(IngredientType.Baursak))
            {
                _ingredients[ingredient] = new List<IngredientType>();
                AddIngredient(ingredient, IngredientType.BaursakCooked);
                //_cookingView.StartBurningTimer(ingredient);
            }
            else if (ingredients.Contains(IngredientType.Meat) && ingredients.Contains(IngredientType.Sausage) &&
                     ingredients.Contains(IngredientType.OnionPeeled) &&
                     ingredients.Contains(IngredientType.DoughBershmakCutted))
            {
                _ingredients[ingredient] = new List<IngredientType>();
                AddIngredient(ingredient, IngredientType.BershmakBoiled);
            }
            else if (ingredients.Contains(IngredientType.Milk) && ingredients.Contains(IngredientType.Salt))
            {
                _ingredients[ingredient] = new List<IngredientType> { IngredientType.Salt };
                AddIngredient(ingredient, IngredientType.MilkCooked);
            }
        }
    }

    private void OnOrderCompleted(IngredientType ingredientType, int orderOriginal, Order[] order)
    {
        if (_ingredients.TryGetValue(ingredientType, out var ingr))
        {
            foreach (var i in ingr.Where(ingredient => order[orderOriginal] != null))
            {
                foreach (var dish in order[orderOriginal].GetUncompletedDishes())
                {
                    if (dish.DishData.dishType == i && dish.IsCompleted != true)
                    {
                        order[orderOriginal].MarkDishAsCompleted(i);
                        _cookingView.ClearView(ingredientType);
                        _ingredients.Remove(ingredientType);

                        StopFireIfEmpty(ingredientType);

                        _soundManager.PlayOrderCompletedSound();

                        return;
                    }
                }
            }

            var inedx = orderOriginal == 0 ? 1 : 0;
            foreach (var i in ingr.Where(ingredient => order[inedx] != null))
            {
                foreach (var dish in order[inedx].GetUncompletedDishes())
                {
                    if (dish.DishData.dishType == i && dish.IsCompleted != true)
                    {
                        order[inedx].MarkDishAsCompleted(i);
                        order[inedx].MarkDishAsCompleted(i);
                        _cookingView.ClearView(ingredientType);
                        _ingredients.Remove(ingredientType);

                        StopFireIfEmpty(ingredientType);

                        _soundManager.PlayOrderCompletedSound();

                        return;
                    }
                }
            }

        }
    }

    private void AddIngredient(IngredientType targetContainer, IngredientType ingredient)
    {
        if (_ingredients.TryGetValue(targetContainer, out var containerIngredients))
        {
            if (!_prerequisites.CheckPrerequisites(ingredient, targetContainer, containerIngredients))
            {
                //_cookingTipManager.ShowTip("Не все условия выполнены для добавления ингредиента!");
                return;
            }
        }
        else if (!_prerequisites.CheckPrerequisites(ingredient, targetContainer, new List<IngredientType>()))
        {
            //_cookingTipManager.ShowTip("Сначала добавьте необходимые ингредиенты!");
            return;
        }

        if (targetContainer == IngredientType.TrashBin)
        {
            if (_ingredients.TryGetValue(ingredient, out var ingredients) &&
                (ingredients.Contains(IngredientType.BaursakBurned) ||
                 ingredients.Contains(IngredientType.MilkBurned)))
            {
                _ingredients.Remove(ingredient);
                _cookingView.ClearView(ingredient);
                _trashBin.SetActive(false);
                if (ingredient == IngredientType.Pot)
                {
                    _smokePotAnimator.StopAnimation();
                }
                else if (ingredient == IngredientType.Pot2)
                {
                    _smokePot2Animator.StopAnimation();
                }

                return;
            }
        }

        for (int i = _ingredients.Keys.Count - 1; i >= 0; i--)
        {
            var sourceContainer = _ingredients.ElementAt(i).Key;
            var sourceIngredients = _ingredients[sourceContainer];

            for (int j = sourceIngredients.Count - 1; j >= 0; j--)
            {
                var ingredientType = sourceIngredients[j];

                if (IsDish(ingredientType) && _ingredients.TryGetValue(ingredient, out var innerIngredients))
                {
                    if (IsKitchenware(targetContainer) && IsKitchenware(sourceContainer) &&
                        CheckPossibility(targetContainer, ingredientType))
                    {
                        if (!_ingredients.ContainsKey(targetContainer))
                        {
                            _ingredients[targetContainer] = new List<IngredientType>();
                        }

                        _ingredients[targetContainer].Add(ingredientType);
                        _cookingView.AddIngredientView(targetContainer, ingredientType);
                        _cookingView.ClearView(ingredient);
                        _ingredients[ingredient] = new List<IngredientType>();

                        if ((!(sourceContainer is IngredientType.Bowl1 or IngredientType.Bowl2 &&
                               targetContainer is IngredientType.Bowl1 or IngredientType.Bowl2)
                             || targetContainer == IngredientType.Bowl1 && ingredient == IngredientType.Bowl2 ||
                             targetContainer == IngredientType.Bowl2 && ingredient == IngredientType.Bowl1))
                        {
                            sourceIngredients.RemoveAt(j);
                            _cookingView.ClearView(sourceContainer);
                            if (sourceContainer == IngredientType.Pot || sourceContainer == IngredientType.Pot2)
                            {
                                _ingredients.Remove(sourceContainer);
                            }
                        }

                        return;
                    }
                }

                if (!IsDish(ingredientType) || ingredient != ingredientType) continue;
                if (sourceContainer is IngredientType.Bowl1 or IngredientType.Bowl2 && IsKitchenware(targetContainer))
                {
                    break;
                }

                sourceIngredients.RemoveAt(j);
                _cookingView.ClearView(sourceContainer);

                foreach (var remainingIngredient in sourceIngredients)
                {
                    _cookingView.AddIngredientView(sourceContainer, remainingIngredient);
                }

                if ((sourceContainer == IngredientType.Pot || sourceContainer == IngredientType.Pot2) && sourceIngredients.Count == 0)
                {
                    _ingredients.Remove(sourceContainer);
                }

                break;
            }
        }

        if (!_ingredients.ContainsKey(targetContainer))
        {
            _ingredients[targetContainer] = new List<IngredientType>();
        }

        var targetIngredients = _ingredients[targetContainer];

        if (!targetIngredients.Contains(ingredient))
        {
            targetIngredients.Add(ingredient);
            _ingredients[targetContainer] = targetIngredients;

            _cookingView.AddIngredientView(targetContainer, ingredient);

            if (targetContainer == IngredientType.Cauldron)
            {
                _soundManager.PlayPutInWaterSound();
            }

            if ((targetContainer == IngredientType.Pot ||
                 targetContainer == IngredientType.Pot2 ||
                 targetContainer == IngredientType.Cauldron) &&
                targetIngredients.Count == 1)
            {
                StartFireForContainer(targetContainer);
            }

            if (targetContainer is IngredientType.Pot or IngredientType.Pot2 &&
                targetIngredients.Contains(IngredientType.Baursak) &&
                targetIngredients.Contains(IngredientType.Oil))
            {
                _cookingView.StartTimer(5, targetContainer);
                _soundManager.PlayCookPotSound();
                OnPotTimerStarted?.Invoke();
            }
            else if (targetContainer is IngredientType.Pot or IngredientType.Pot2 &&
                     targetIngredients.Contains(IngredientType.Salt) &&
                     targetIngredients.Contains(IngredientType.Milk))
            {
                _cookingView.StartTimer(5, targetContainer);
                _soundManager.PlayBoilPotSound();
                OnPotTimerStarted?.Invoke();
            }
            else if (targetContainer == IngredientType.Cauldron &&
                     targetIngredients.Contains(IngredientType.Meat) &&
                     targetIngredients.Contains(IngredientType.Sausage) &&
                     targetIngredients.Contains(IngredientType.OnionPeeled) &&
                     targetIngredients.Contains(IngredientType.DoughBershmakCutted))
            {
                _cookingView.StartTimer(5, targetContainer);
                _soundManager.PlayBoilCauldronSound();
            }
        }

        var recipe = _recipeManager.GetRecipeForIngredients(targetContainer, targetIngredients);
        if (recipe != null &&
            !(targetContainer == IngredientType.DoughBowl && (recipe.Result == IngredientType.DoughBaursak ||
                                                              recipe.Result == IngredientType.DoughBershmak)) &&
            !(targetContainer == IngredientType.Board &&
              (recipe.Result == IngredientType.DoughBershmakCutted ||
               recipe.Result == IngredientType.BershmakMeatCutted ||
               recipe.Result == IngredientType.Baursak)))
        {
            ProcessRecipe(targetContainer, recipe);
        }
    }

    private bool CheckPossibility(IngredientType targetContainer, IngredientType ingredientType)
    {
        if (_ingredients.TryGetValue(targetContainer, out var ingredients) && ingredients.Contains(ingredientType))
        {
            return false;
        }

        if (ingredientType is IngredientType.BaursakCooked or IngredientType.BershmakCooked
            or IngredientType.KurutCooked or IngredientType.MeatCooked or IngredientType.KurutBoiled)
        {
            if ((targetContainer != IngredientType.Bowl1 && targetContainer != IngredientType.Bowl2))
            {
                return false;
            }
        }

        if (ingredientType is IngredientType.BaursakBurned or IngredientType.MilkBurned)
        {
            if (targetContainer != IngredientType.TrashBin)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsKitchenware(IngredientType container)
    {
        return container is IngredientType.Bowl1 or IngredientType.Bowl2 or IngredientType.Pot or IngredientType.Pot2
            or IngredientType.Cauldron or IngredientType.DoughBowl or IngredientType.Board;
    }

    private bool IsDish(IngredientType ingredient)
    {
        return ingredient is IngredientType.Baursak or
            IngredientType.BaursakBurned or
            IngredientType.BaursakCooked or
            IngredientType.DoughBaursak or
            IngredientType.BershmakCooked or
            IngredientType.BershmakBoiled or
            IngredientType.BershmakMeatCutted or
            IngredientType.DoughBershmak or
            IngredientType.DoughBershmakCutted or
            IngredientType.KurutBoiled or
            IngredientType.KurutCooked;
    }

    private void ProcessRecipe(IngredientType container, Recipe recipe)
    {
        _cookingView.ClearView(container);
        _ingredients.Remove(container);

        StopFireIfEmpty(container);

        AddIngredient(container, recipe.Result);

        if (container == IngredientType.Pot)
        {
            _cookingView.StopTimer(IngredientType.Pot);
        }
        else if (container == IngredientType.Pot2)
        {
            _cookingView.StopTimer(IngredientType.Pot2);
        }
        else if (container == IngredientType.Cauldron)
        {
            _cookingView.StopTimer(IngredientType.Cauldron);
        }
    }

    private void StartFireForContainer(IngredientType container)
    {
        if (container == IngredientType.Pot)
        {
            _fireStovePotAnimator.StartAnimation();
        }
        else if (container == IngredientType.Pot2)
        {
            _fireStovePot2Animator.StartAnimation();
        }
        else if (container == IngredientType.Cauldron)
        {
            _fireStoveCauldronAnimator.StartAnimation();
        }
    }

    private void StopFireIfEmpty(IngredientType container)
    {
        if (!_ingredients.ContainsKey(container) || _ingredients[container].Count == 0)
        {
            if (container == IngredientType.Pot)
            {
                _fireStovePotAnimator.StopAnimation();
                _smokePotAnimator.StopAnimation();
            }
            else if (container == IngredientType.Pot2)
            {
                _fireStovePot2Animator.StopAnimation();
                _smokePot2Animator.StopAnimation();
            }
            else if (container == IngredientType.Cauldron)
            {
                _fireStoveCauldronAnimator.StopAnimation();
            }
        }
    }

    private void StopAllFireAnimations()
    {
        _firePotAnimator.StopAnimation();
        _smokePotAnimator.StopAnimation();
        _firePot2Animator.StopAnimation();
        _smokePot2Animator.StopAnimation();
        _fireStoveCauldronAnimator.StopAnimation();
        _fireStovePotAnimator.StopAnimation();
        _fireStovePot2Animator.StopAnimation();
    }

    public void Reset()
    {
        _ingredients.Clear();
        _cookingView.Reset();
        StopAllFireAnimations();
        _activeFireContainer = null;
        _fireExtinguisher.SetActive(false);
        _fireExtinguisherPot.SetActive(false);
        _fireExtinguisherPot2.SetActive(false);
    }

    public bool HasIngredient(IngredientType container, IngredientType ingredient)
    {
        if (!_ingredients.TryGetValue(container, out var ingredient1))
        {
            return false;
        }

        return ingredient1.Contains(ingredient);
    }

    public void PlayReciepeSound()
    {
        _soundManager.PlayReciepeSound();
    }
}