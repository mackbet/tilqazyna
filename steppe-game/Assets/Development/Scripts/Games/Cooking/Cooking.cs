using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Cooking : GameController
{
    [Serializable]
    public class LevelData
    {
        public int customerCount;
        public int maxOrderCount;
        public ItemData[] possibleOrders;
    }

    private const string KEY_COOKING_LEVEL = "cookingLevel";
    private const string PREFS_COOKING_LEVEL = "cookingLevel";

    [SerializeField] private List<Seat> _seats;
    [SerializeField] private LevelData[] _levels;

    private int _currentLevel;
    private int _customersServed;
    private int _customersSpawned;
    private int _totalCoins;
    private int _maxCoins;
    private LevelData _currentLevelData;
    private readonly Queue<int> _freeSeats = new();

    public int TotalCoins => _totalCoins;

    protected override async void InitializeGame()
    {
        await LoadLevel();
        _currentLevel = Mathf.Clamp(_currentLevel, 0, _levels.Length - 1);
        StartLevel(_currentLevel);
    }

    private async Task LoadLevel()
    {
        _currentLevel = PlayerPrefs.GetInt(PREFS_COOKING_LEVEL, 0);

        var rm = RealtimeManager.Instance;
        if (rm == null) return;

        var data = await rm.LoadData(new HashSet<string> { KEY_COOKING_LEVEL });
        if (data != null && data.TryGetValue(KEY_COOKING_LEVEL, out var val))
        {
            _currentLevel = Convert.ToInt32(val);
            PlayerPrefs.SetInt(PREFS_COOKING_LEVEL, _currentLevel);
        }
    }

    private async void SaveLevel()
    {
        PlayerPrefs.SetInt(PREFS_COOKING_LEVEL, _currentLevel);
        PlayerPrefs.Save();

        var rm = RealtimeManager.Instance;
        if (rm != null)
            await rm.SaveData(new Dictionary<string, object> { { KEY_COOKING_LEVEL, _currentLevel } });
    }

    private void StartLevel(int levelIndex)
    {
        if (levelIndex >= _levels.Length) return;

        _currentLevelData = _levels[levelIndex];
        _customersServed = 0;
        _customersSpawned = 0;
        _totalCoins = 0;
        _maxCoins = 0;
        _freeSeats.Clear();

        for (int i = 0; i < _seats.Count; i++)
        {
            _seats[i].DeactivateCustomer();
            _freeSeats.Enqueue(i);
        }

        SpawnCustomers();
    }

    private void SpawnCustomers()
    {
        while (_freeSeats.Count > 0 && _customersSpawned < _currentLevelData.customerCount)
        {
            int seatIndex = _freeSeats.Dequeue();
            var seat = _seats[seatIndex];

            seat.CustomerCompleted -= OnCustomerCompleted;
            seat.CustomerCompleted += OnCustomerCompleted;

            seat.ActivateCustomer(_currentLevelData.possibleOrders, _currentLevelData.maxOrderCount);
            _customersSpawned++;
        }
    }

    private void OnCustomerCompleted(Seat seat)
    {
        _totalCoins += seat.Customer.Total;
        _maxCoins += seat.Customer.MaxTotal;
        _customersServed++;

        if (_customersServed >= _currentLevelData.customerCount)
        {
            finishPanel.SetTitle($"Уровень {_currentLevel + 1}");

            _currentLevel++;
            SaveLevel();
            finishPanel.SetReward(_totalCoins, 60, 50);
            finishPanel.SetStars(CalculateStars());
            finishPanel.SetState(true);
            FinishGame();
            return;
        }

        if (_customersSpawned < _currentLevelData.customerCount)
        {
            seat.CustomerCompleted -= OnCustomerCompleted;
            seat.DeactivateCustomer();

            int seatIndex = _seats.IndexOf(seat);
            _freeSeats.Enqueue(seatIndex);

            SpawnCustomers();
        }
    }

    private int CalculateStars()
    {
        if (_maxCoins == 0) return 1;

        float ratio = (float)_totalCoins / _maxCoins;

        if (ratio >= 0.9f) return 3;
        if (ratio >= 0.6f) return 2;
        return 1;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        foreach (var seat in _seats)
            seat.CustomerCompleted -= OnCustomerCompleted;
    }
}
