using System;
using UnityEngine;

public class Seat : MonoBehaviour
{
    [SerializeField] private Customer _customer;

    public Customer Customer => _customer;

    public event Action<Seat> CustomerCompleted;

    private void OnEnable()
    {
        _customer.AllOrdersFulfilled += OnAllOrdersFulfilled;
    }

    private void OnDisable()
    {
        _customer.AllOrdersFulfilled -= OnAllOrdersFulfilled;
    }

    public void ActivateCustomer(ItemData[] possibleOrders, int maxOrderCount)
    {
        _customer.gameObject.SetActive(true);
        _customer.Activate(possibleOrders, maxOrderCount);
    }

    public void DeactivateCustomer()
    {
        _customer.Deactivate();
        _customer.gameObject.SetActive(false);
    }

    private void OnAllOrdersFulfilled(Customer customer)
    {
        CustomerCompleted?.Invoke(this);
    }
}
