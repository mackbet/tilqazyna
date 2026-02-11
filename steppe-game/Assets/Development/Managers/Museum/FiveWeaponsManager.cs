using System;
using UnityEngine;

public class FiveWeaponsManager : MonoBehaviour
{
    [SerializeField] BuyWeaponPopupManager buyWeaponPopup;

    [Header("Lock's objects")]
    [SerializeField] GameObject swordLock;
    [SerializeField] GameObject axeLock;
    [SerializeField] GameObject bowLock;
    [SerializeField] GameObject mazeLock;

    private StateManager _stateManager => StateManager.Instance;

    public static event Action OnClickMuseumWeaponButtonAction;

    public void Initialize(StateManager stateManager)
    {
        buyWeaponPopup.Initialize(stateManager);
    }

    void OnEnable()
    {
        buyWeaponPopup.gameObject.SetActive(false);

        SetLocks();
    }

    private void SetLocks()
    {
        if (_stateManager == null) return;

        swordLock.SetActive(!_stateManager.IsSwordBought);

        axeLock.SetActive(!_stateManager.IsAxeBought);

        bowLock.SetActive(!_stateManager.IsBowBought);

        mazeLock.SetActive(!_stateManager.IsMazeBought);
    }

    private void OnClickMuseumWeaponButton(FiveWeaponsGameType fiveWeaponsGameType)
    {
        switch (fiveWeaponsGameType)
        {
            case FiveWeaponsGameType.spear:
                _stateManager.FiveWeaponPointAndClickGameType = FiveWeaponsGameType.spear;
                OnClickMuseumWeaponButtonAction?.Invoke();
                break;
            case FiveWeaponsGameType.sword:
                if (_stateManager.IsSwordBought)
                {
                    _stateManager.FiveWeaponPointAndClickGameType = FiveWeaponsGameType.sword;
                    OnClickMuseumWeaponButtonAction?.Invoke();
                }
                else
                {
                    buyWeaponPopup.SetCurrentWeaponType(FiveWeaponsGameType.sword);
                    buyWeaponPopup.gameObject.SetActive(true);
                }
                break;
            case FiveWeaponsGameType.axe:
                if (_stateManager.IsAxeBought)
                {
                    _stateManager.FiveWeaponPointAndClickGameType = FiveWeaponsGameType.axe;
                    OnClickMuseumWeaponButtonAction?.Invoke();
                }
                else
                {
                    buyWeaponPopup.SetCurrentWeaponType(FiveWeaponsGameType.axe);
                    buyWeaponPopup.gameObject.SetActive(true);
                }
                break;
            case FiveWeaponsGameType.bow:
                if (_stateManager.IsBowBought)
                {
                    _stateManager.FiveWeaponPointAndClickGameType = FiveWeaponsGameType.bow;
                    OnClickMuseumWeaponButtonAction?.Invoke();
                }
                else
                {
                    buyWeaponPopup.SetCurrentWeaponType(FiveWeaponsGameType.bow);
                    buyWeaponPopup.gameObject.SetActive(true);
                }
                break;
            case FiveWeaponsGameType.hammer:
                if (_stateManager.IsMazeBought)
                {
                    _stateManager.FiveWeaponPointAndClickGameType = FiveWeaponsGameType.hammer;
                    OnClickMuseumWeaponButtonAction?.Invoke();
                }
                else
                {
                    buyWeaponPopup.SetCurrentWeaponType(FiveWeaponsGameType.hammer);
                    buyWeaponPopup.gameObject.SetActive(true);
                }
                break;
        }
    }

    public void OnSpearClick() => OnClickMuseumWeaponButton(FiveWeaponsGameType.spear);
    public void OnSwordClick() => OnClickMuseumWeaponButton(FiveWeaponsGameType.sword);
    public void OnAxeClick() => OnClickMuseumWeaponButton(FiveWeaponsGameType.axe);
    public void OnBowClick() => OnClickMuseumWeaponButton(FiveWeaponsGameType.bow);
    public void OnHammerClick() => OnClickMuseumWeaponButton(FiveWeaponsGameType.hammer);
}
