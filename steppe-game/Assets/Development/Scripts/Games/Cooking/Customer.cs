using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class CustomerSprite
{
    public Sprite sprite;
    public bool isFemale;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Customer : MonoBehaviour, IItemReceiver
{
    [SerializeField] private Image _image;
    [SerializeField] private List<CustomerSprite> _sprites;
    [SerializeField] private List<OrderView> _orderViews;
    [SerializeField] private Slider _waitSlider;
    [SerializeField] private float _waitTime = 30f;
    [SerializeField] private TMP_Text _totalText;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private TMP_Text _speechText;
    [SerializeField] private float _speechDuration = 4f;
    [SerializeField] private Color _totalDefaultColor = Color.white;
    [SerializeField] private Color _totalExpiredColor = Color.red;

    private readonly List<OrderItem> _orders = new();
    private float _elapsed;
    private bool _isWaiting;
    private int _total;
    private bool _isFemale;
    private Coroutine _speechCoroutine;

    public IReadOnlyList<OrderItem> Orders => _orders;
    public int Total => _total;
    public int MaxTotal { get; private set; }

    public event Action<Customer, OrderItem> OrderItemFulfilled;
    public event Action<Customer> AllOrdersFulfilled;
    public event Action<Customer> WaitTimeExpired;

    public void Activate(ItemData[] possibleOrders, int maxOrderCount)
    {
        _orders.Clear();
        HideAll();

        if (_image != null && _sprites.Count > 0)
        {
            var selected = _sprites[Random.Range(0, _sprites.Count)];
            _image.sprite = selected.sprite;
            _isFemale = selected.isFemale;
        }

        var available = new List<ItemData>(possibleOrders);
        int count = Mathf.Min(maxOrderCount, Mathf.Min(_orderViews.Count, available.Count));

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            var item = available[index];
            available.RemoveAt(index);

            _orders.Add(new OrderItem(item));
            _orderViews[i].gameObject.SetActive(true);
            _orderViews[i].HideAll();
            _orderViews[i].Show(item);
        }

        _total = 0;
        foreach (var order in _orders)
            _total += order.Item.Price;

        MaxTotal = _total;
        UpdateTotalText();

        _elapsed = 0f;
        _isWaiting = true;

        if (_waitSlider != null)
        {
            _waitSlider.gameObject.SetActive(true);
            _waitSlider.value = 1f;
        }

        if (_orders.Count > 0)
            ShowOrderSpeech(_orders[0].Item);
    }

    public void Deactivate()
    {
        _isWaiting = false;
        _orders.Clear();
        HideAll();

        if (_waitSlider != null)
            _waitSlider.gameObject.SetActive(false);

        if (_speechCoroutine != null)
        {
            StopCoroutine(_speechCoroutine);
            _speechCoroutine = null;
        }

        if (_totalText != null)
            _totalText.color = _totalDefaultColor;

        if (_speechBubble != null)
            _speechBubble.SetActive(false);
    }

    private void ShowOrderSpeech(ItemData item)
    {
        if (_speechBubble != null)
            _speechBubble.SetActive(true);

        if (_speechText != null && item.OrderText != null)
            item.OrderText.GetLocalizedStringAsync().Completed +=
                op => { if (_speechText != null) _speechText.text = op.Result; };

        var audio = _isFemale ? item.OrderAudioFemale : item.OrderAudioMale;
        SoundManager.Instance?.PlayVoice(audio);

        if (_speechCoroutine != null)
            StopCoroutine(_speechCoroutine);
        _speechCoroutine = StartCoroutine(HideSpeechAfterDelay());
    }

    private IEnumerator HideSpeechAfterDelay()
    {
        yield return new WaitForSeconds(_speechDuration);
        if (_speechBubble != null)
            _speechBubble.SetActive(false);
        _speechCoroutine = null;
    }

    private void Update()
    {
        if (!_isWaiting) return;

        _elapsed += Time.deltaTime;

        if (_waitSlider != null)
            _waitSlider.value = 1f - _elapsed / _waitTime;

        if (_elapsed >= _waitTime)
        {
            _isWaiting = false;
            _total /= 2;
            if (_totalText != null)
                _totalText.color = _totalExpiredColor;
            UpdateTotalText();
            _waitSlider.gameObject.SetActive(false);
            WaitTimeExpired?.Invoke(this);
        }
    }

    private void HideAll()
    {
        foreach (var view in _orderViews)
        {
            view.HideAll();
            view.gameObject.SetActive(false);
        }
    }

    public bool CanAccept(ItemData item)
    {
        if (item == null) return false;

        foreach (var order in _orders)
        {
            if (order.Item == item && !order.Fulfilled)
                return true;
        }

        return false;
    }

    public void AddItem(ItemData item)
    {
        foreach (var order in _orders)
        {
            if (order.Item == item && !order.Fulfilled)
            {
                order.Fulfilled = true;
                int orderIndex = _orders.IndexOf(order);
                _orderViews[orderIndex].Check();
                OrderItemFulfilled?.Invoke(this, order);

                if (IsAllFulfilled())
                {
                    _isWaiting = false;
                    AllOrdersFulfilled?.Invoke(this);
                }

                return;
            }
        }
    }

    private void UpdateTotalText()
    {
        if (_totalText != null)
            _totalText.text = _total.ToString();
    }

    private bool IsAllFulfilled()
    {
        foreach (var order in _orders)
        {
            if (!order.Fulfilled)
                return false;
        }

        return true;
    }
}
