using UnityEngine;

public class CustomerSound : MonoBehaviour
{
    [SerializeField] private Customer _customer;
    [SerializeField] private AudioClip _orderFulfilledClip;
    [SerializeField] private AudioClip _allOrdersFulfilledClip;
    [SerializeField] private AudioClip _waitExpiredClip;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_customer)
            _customer = GetComponent<Customer>();
    }
#endif

    private void OnEnable()
    {
        _customer.OrderItemFulfilled += OnOrderItemFulfilled;
        _customer.AllOrdersFulfilled += OnAllOrdersFulfilled;
        _customer.WaitTimeExpired += OnWaitTimeExpired;
    }

    private void OnDisable()
    {
        _customer.OrderItemFulfilled -= OnOrderItemFulfilled;
        _customer.AllOrdersFulfilled -= OnAllOrdersFulfilled;
        _customer.WaitTimeExpired -= OnWaitTimeExpired;
    }

    private void OnOrderItemFulfilled(Customer customer, OrderItem order) => SoundManager.Instance?.PlaySound(_orderFulfilledClip);
    private void OnAllOrdersFulfilled(Customer customer) => SoundManager.Instance?.PlaySound(_allOrdersFulfilledClip);
    private void OnWaitTimeExpired(Customer customer) => SoundManager.Instance?.PlaySound(_waitExpiredClip);
}
