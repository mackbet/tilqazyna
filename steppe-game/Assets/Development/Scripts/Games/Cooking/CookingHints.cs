using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookingHints : MonoBehaviour
{
    [SerializeField] private List<Seat> _seats;
    [SerializeField] private List<ItemContainer> _containers; // ресурсы и result-контейнеры
    [SerializeField] private List<Vessel> _vessels;
    [SerializeField] private List<Draggable> _draggables;
    [SerializeField] private float _inactivityDelay = 5f;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private Material _outlineMaterial;
    [SerializeField] private Vector2 _arrowOffset = new Vector2(0f, 100f);
    [SerializeField] private GameObject _hintTextObject;
    [SerializeField] private TMP_Text _hintText;
    [SerializeField] private bool _debug;

    private struct HintStep
    {
        public ItemContainer Container; // подсветить outline, если не null
        public Transform Source;        // стрелка на источнике (контейнер или vessel)
        public Transform Target;        // стрелка на цели (vessel или seat), может быть null
        public HintData HintData;       // текст и аудио подсказки
    }

    private float _idleTime;
    private bool _hintsActive;
    private readonly List<GameObject> _arrows = new();
    private readonly List<(Image image, Material original, OutlineImageExtender extender)> _highlighted = new();

    private void OnEnable()
    {
        foreach (var draggable in _draggables)
            draggable.DragStarted += OnActivity;

        foreach (var vessel in _vessels)
        {
            vessel.ItemAdded += OnVesselActivity;
            vessel.RecipeCompleted += OnRecipeActivity;
        }
    }

    private void OnDisable()
    {
        foreach (var draggable in _draggables)
            draggable.DragStarted -= OnActivity;

        foreach (var vessel in _vessels)
        {
            vessel.ItemAdded -= OnVesselActivity;
            vessel.RecipeCompleted -= OnRecipeActivity;
        }

        HideHints();
    }

    private void OnActivity(Draggable draggable, PointerEventData eventData)
    {
        Log($"Активность: перетаскивание {draggable.name}");
        ResetIdle();
    }

    private void OnVesselActivity(ItemData item)
    {
        Log($"Активность: добавлен {item.name} в vessel");
        ResetIdle();
    }

    private void OnRecipeActivity(RecipeData recipe, ItemData result)
    {
        Log($"Активность: приготовлено {result.name}");
        ResetIdle();
    }

    private void ResetIdle()
    {
        if (_hintsActive) HideHints();
        _idleTime = 0f;
    }

    private void Update()
    {
        if (_hintsActive) return;

        _idleTime += Time.deltaTime;
        if (_idleTime >= _inactivityDelay)
            ShowHints();
    }

    private void ShowHints()
    {
        _hintsActive = true;

        var hint = FindHint();
        if (!hint.HasValue)
        {
            Log("Подсказка не найдена");
            return;
        }

        var sourceName = hint.Value.Container != null ? hint.Value.Container.name : hint.Value.Source?.name;
        Log($"Подсказка: {sourceName} → {hint.Value.Target?.name ?? "нет цели"}");

        if (hint.Value.Container != null)
        {
            var image = hint.Value.Container.GetComponent<Image>();
            if (image != null)
            {
                var extender = image.gameObject.AddComponent<OutlineImageExtender>();
                _highlighted.Add((image, image.material, extender));
                image.material = _outlineMaterial;
            }
            else
            {
                Log($"У {hint.Value.Container.name} нет Image — обводка не применена");
            }
        }

        if (hint.Value.Source != null)
            PlaceArrow(hint.Value.Source);

        if (hint.Value.Target != null)
            PlaceArrow(hint.Value.Target);

        PlayHint(hint.Value.HintData);
    }

    private void PlaceArrow(Transform target)
    {
        if (_arrowPrefab == null)
        {
            Log("Arrow Prefab не назначен");
            return;
        }

        var arrow = Instantiate(_arrowPrefab, transform);
        var arrowRect = arrow.GetComponent<RectTransform>();
        var parentRect = (RectTransform)transform;
        var canvas = GetComponentInParent<Canvas>();

        // Переводим позицию объекта в локальные координаты canvas,
        // корректно работает и в ScreenSpaceOverlay, и в ScreenSpaceCamera
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, screenPoint, canvas.worldCamera, out Vector2 localPoint);

        Vector2 arrowLocalPos = localPoint + _arrowOffset;
        arrowRect.anchoredPosition = arrowLocalPos;

        Vector2 dirToItem = (localPoint - arrowLocalPos).normalized;
        if (dirToItem != Vector2.zero)
        {
            float angle = Vector2.SignedAngle(Vector2.down, dirToItem);
            arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            Log($"Стрелка над {target.name}: поворот {angle:F1}°");
        }

        _arrows.Add(arrow);
    }

    private void HideHints()
    {
        Log("Скрыть подсказку");
        _hintsActive = false;

        foreach (var (image, original, extender) in _highlighted)
        {
            if (image != null) image.material = original;
            if (extender != null) Destroy(extender);
        }
        _highlighted.Clear();

        foreach (var arrow in _arrows)
            if (arrow != null) Destroy(arrow);
        _arrows.Clear();

        if (_hintTextObject != null)
            _hintTextObject.SetActive(false);

        if (_hintText != null)
            _hintText.text = string.Empty;

        SoundManager.Instance?.StopVoice();
    }

    private HintStep? FindHint()
    {
        foreach (var seat in _seats)
        {
            var customer = seat.Customer;
            if (customer == null || !customer.gameObject.activeSelf) continue;

            foreach (var order in customer.Orders)
            {
                if (order.Fulfilled) continue;

                Log($"Ищу подсказку для заказа: {order.Item.name} → {seat.name}");
                var hint = ResolveItem(order.Item, seat.transform, new HashSet<ItemData>());
                if (hint.HasValue) return hint;
            }
        }

        return null;
    }

    private HintStep? ResolveItem(ItemData target, Transform destination, HashSet<ItemData> visited)
    {
        if (visited.Contains(target)) return null;
        visited.Add(target);

        foreach (var container in _containers)
        {
            if (container.Item == target && container.gameObject.activeSelf)
            {
                Log($"Найден контейнер: {container.name} ({target.name}) → {destination.name}");
                return new HintStep { Container = container, Source = container.transform, Target = destination, HintData = container.Item.Hint };
            }
        }

        foreach (var vessel in _vessels)
        {
            foreach (var recipe in vessel.Recipes)
            {
                if (!RecipeProduces(recipe, target)) continue;

                if (vessel.Contains(target))
                {
                    Log($"{target.name} готов в {vessel.name}, нужно забрать → {destination.name}");
                    return new HintStep { Source = vessel.transform, Target = destination, HintData = vessel.Hint };
                }

                if (vessel is ClickVessel clickVessel && clickVessel.IsReady)
                {
                    Log($"ClickVessel {vessel.name} готов к нажатию");
                    return new HintStep { Source = vessel.transform, Target = null, HintData = vessel.Hint };
                }

                Log($"{target.name} нужно приготовить в {vessel.name}, ищу ингредиенты");

                foreach (var ingredient in recipe.Ingredients)
                {
                    if (vessel.Contains(ingredient))
                    {
                        Log($"{ingredient.name} уже в {vessel.name}, пропускаю");
                        continue;
                    }

                    if (!ArePrerequisitesMet(ingredient, vessel))
                    {
                        Log($"{ingredient.name}: prerequisites не выполнены, пропускаю");
                        continue;
                    }

                    Log($"Недостающий ингредиент: {ingredient.name} для {vessel.name}");
                    var hint = ResolveItem(ingredient, vessel.transform, visited);
                    if (hint.HasValue) return hint;
                }

                break;
            }
        }

        Log($"Не удалось найти шаг для {target.name}");
        return null;
    }

    private void PlayHint(HintData hintData)
    {
        if (hintData == null) return;

        if (_hintTextObject != null)
            _hintTextObject.SetActive(true);

        if (_hintText != null && hintData.Text != null)
            hintData.Text.GetLocalizedStringAsync().Completed +=
                op => { if (_hintText != null) _hintText.text = op.Result; };

        SoundManager.Instance?.PlayVoice(hintData.Audio);
    }

    private bool RecipeProduces(RecipeData recipe, ItemData target)
    {
        if (recipe.Result == target) return true;

        if (recipe.ExtraResults != null)
            foreach (var extra in recipe.ExtraResults)
                if (extra == target) return true;

        return false;
    }

    private bool ArePrerequisitesMet(ItemData ingredient, Vessel vessel)
    {
        if (ingredient.Prerequisites == null || ingredient.Prerequisites.Length == 0)
            return true;

        foreach (var prerequisite in ingredient.Prerequisites)
        {
            if (!vessel.Contains(prerequisite))
                return false;
        }

        return true;
    }

    private void Log(string message)
    {
        if (_debug)
            Debug.Log($"[CookingHints] {message}");
    }
}
