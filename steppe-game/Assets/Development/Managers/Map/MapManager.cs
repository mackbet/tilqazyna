using System;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Locks")]
    [SerializeField] private GameObject _bayterekMonumentLock;
    [SerializeField] private GameObject _shatyrMonumentLock;
    [SerializeField] private GameObject _acordaMonumentLock;
    [SerializeField] private GameObject _expoMonumentLock;
    [SerializeField] private GameObject _sarayMonumentLock;

    private StateManager _stateManager;

    public event Action OnClickCookingButtonAction;
    public event Action OnClickQuizButtonAction;
    public event Action OnClickMuseumButtonAction;
    public static event Action OnClickMuseumFiveWeaponsButtonAction;
    public static event Action OnClickMuseumSevenTreasuresButtonAction;
    public static event Action OnBozokButtonAction;


    // private void Awake()
    // {
    //     _stateManager = FindAnyObjectByType<StateManager>();
    //     StateManager.StateChanged += UpdateMapBuildings;
    //
    //     UpdateMapBuildings();
    // }

    public void Initialize(StateManager stateManager)
    {
        _stateManager = stateManager;
        StateManager.StateChanged += UpdateMapBuildings;

        UpdateMapBuildings();
    }

    private void UpdateMapBuildings()
    {
        if (_stateManager.IsTutorialTiltabetComplete)
        {
            _bayterekMonumentLock.SetActive(false);
        }

        if (_stateManager.ExperienceAmount >= _stateManager.basicLevelExperienceNeeded)
        {
            _shatyrMonumentLock.SetActive(false);
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 2))
        {
            _acordaMonumentLock.SetActive(false);
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 3))
        {
            _expoMonumentLock.SetActive(false);
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 4))
        {
            _sarayMonumentLock.SetActive(false);
        }
    }

    public void OnClickCookingButton()
    {
        OnClickCookingButtonAction?.Invoke();
    }

    private void OnClickQuizButton(CityEnum place)
    {
        if (_stateManager.IsTutorialTiltabetComplete && place == CityEnum.bayterek)
        {
            _stateManager.CurrentQuiz = "«Бәйтерек» монументі";
            OnClickQuizButtonAction?.Invoke();
        }

        if (_stateManager.ExperienceAmount >= _stateManager.basicLevelExperienceNeeded && place == CityEnum.shatyr)
        {
            _stateManager.CurrentQuiz = "Хан Шатыр";
            OnClickQuizButtonAction?.Invoke();
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 2) && place == CityEnum.acorda)
        {
            _stateManager.CurrentQuiz = "Ақорда";
            OnClickQuizButtonAction?.Invoke();
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 3) && place == CityEnum.expo)
        {
            _stateManager.CurrentQuiz = "EXPO Астана";
            OnClickQuizButtonAction?.Invoke();
        }

        if (_stateManager.ExperienceAmount >= (_stateManager.basicLevelExperienceNeeded * 4) && place == CityEnum.saray)
        {
            _stateManager.CurrentQuiz = "Бейбітшілік және келісім сарайы";
            OnClickQuizButtonAction?.Invoke();
        }
    }

    public void OnBayterekClick() => OnClickQuizButton(CityEnum.bayterek);
    public void OnShatyrClick() => OnClickQuizButton(CityEnum.shatyr);
    public void OnAcordaClick() => OnClickQuizButton(CityEnum.acorda);
    public void OnExpoClick() => OnClickQuizButton(CityEnum.expo);
    public void OnSarayClick() => OnClickQuizButton(CityEnum.saray);

    public void OnMuseumClick() => OnClickMuseumButtonAction?.Invoke();
    public void OnMuseumFiveWeaponsClick() => OnClickMuseumFiveWeaponsButtonAction?.Invoke();
    public void OnMuseumSevenTreasuresClick() => OnClickMuseumSevenTreasuresButtonAction?.Invoke();

    public void OnBozokClick() => OnBozokButtonAction?.Invoke();
}