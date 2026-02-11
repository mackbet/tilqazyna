using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;

    [Header("Settings")]
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _backBottomButton;
    [SerializeField] private GameObject _backTopButton;
    [SerializeField] private GameObject _settingsWindow;
    [SerializeField] private Sprite _activeButtonLanguageBackground;
    [SerializeField] private Sprite _unactiveButtonLanguageBackground;
    [SerializeField] private Button _ruButton;
    [SerializeField] private Button _kzButton;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _soundsVolumeSlider;
    [SerializeField] private Slider _voiceVolumeSlider;
    [SerializeField] private LocalizeStringEvent _versionText;

    [Header("Avatar")]
    [SerializeField] private Image _avatarLevelImage;
    [SerializeField] private Image _avatarLeaderboardImage;
    [SerializeField] private Sprite _girlAvatarSprite;
    [SerializeField] private Sprite _boyAvatarSprite;

    [Header("Settings icons")]
    [SerializeField] private GameObject _musicSlash;
    [SerializeField] private GameObject _soundSlash;
    [SerializeField] private GameObject _voiceSlash;

    [Header("Leaderboard")]
    [SerializeField] private GameObject _leaderWindow;
    [SerializeField] private GameObject _leaderboardUI;

    [Header("Player Parameters")]
    [SerializeField] private Slider _energySlider;
    [SerializeField] private LocalizeStringEvent _moneyText;
    [SerializeField] private LocalizeStringEvent _moneyTextWithoutLeaderboard;
    [SerializeField] private LocalizeStringEvent _pointsText;
    [SerializeField] private LocalizeStringEvent _playerLevelText;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Hud Objects")]
    [SerializeField] private GameObject _upperLeftCornerGroupEnergyBar;
    [SerializeField] private GameObject _upperRightCornerGroup;
    [SerializeField] private GameObject _upperRightCornerWithoutLeaderboardGroup;

    private StateManager _stateManager;
    private LeaderboardManager _leaderboardManager;
    private SoundManager _soundManager;

    public static event Action OnBackClickAction;
    public static event Action OnButtonClick;

    void OnEnable()
    {
        SharpGameSecondSceneManager.disableEnergyRefillOnHUD += SetUpperLeftGroupWithEnergyRefillDisabled;
        SharpGameSecondSceneManager.enableEnergyRefillOnHUD += SetUpperLeftGroupWithEnergyRefillActive;
    }

    void OnDisable()
    {
        SharpGameSecondSceneManager.disableEnergyRefillOnHUD -= SetUpperLeftGroupWithEnergyRefillDisabled;
        SharpGameSecondSceneManager.enableEnergyRefillOnHUD += SetUpperLeftGroupWithEnergyRefillActive;
    }

    public void Initialize(StateManager stateManager, LeaderboardManager leaderboardManager, SoundManager soundManager)
    {
        _soundManager = soundManager;

        _leaderboardManager = leaderboardManager;
        _stateManager = stateManager;
        StateManager.StateChanged += UpdateHUD;

        UpdateHUD();

        _settingsWindow.SetActive(false);

        _ruButton.onClick.AddListener(() => OnLanguageChange(Language.Russian));
        _kzButton.onClick.AddListener(() => OnLanguageChange(Language.Kazakh));

        _energySlider.maxValue = stateManager.MaxEnergyValue;
    }

    private void OnLanguageChange(Language language)
    {
        _stateManager.Language = language;

        if (language == Language.Kazakh)
        {
            _kzButton.image.sprite = _activeButtonLanguageBackground;
            _ruButton.image.sprite = _unactiveButtonLanguageBackground;
        }
        else if (language == Language.Russian)
        {
            _kzButton.image.sprite = _unactiveButtonLanguageBackground;
            _ruButton.image.sprite = _activeButtonLanguageBackground;
        }

        OnButtonClick?.Invoke();
    }

    private void UpdateHUD()
    {
        _levelText.text = _stateManager.PointsAmount.ToString();

        _playerNameText.text = _stateManager.PlayerName;

        if (_stateManager.CharacterSex == CharacterSex.Boy)
        {
            _avatarLevelImage.sprite = _boyAvatarSprite;
            _avatarLeaderboardImage.sprite = _boyAvatarSprite;
        }
        else
        {
            _avatarLevelImage.sprite = _girlAvatarSprite;
            _avatarLeaderboardImage.sprite = _girlAvatarSprite;
        }

        _energySlider.value = _stateManager.EnergyAmount;

        _versionText.StringReference.Arguments = new object[] { new VersionUtility().GetVersionNumber() };
        _versionText.RefreshString();

        _moneyText.StringReference.Arguments = new object[] { _stateManager.CurrencyAmount };
        _moneyText.RefreshString();

        _moneyTextWithoutLeaderboard.StringReference.Arguments = new object[] { _stateManager.CurrencyAmount };
        _moneyTextWithoutLeaderboard.RefreshString();

        _pointsText.StringReference.Arguments = new object[] { _stateManager.PointsAmount };
        _pointsText.RefreshString();

        _playerLevelText.StringReference.Arguments = new object[] { _stateManager.ExperienceAmount / 80 };
        _playerLevelText.RefreshString();

        _soundsVolumeSlider.value = _stateManager.SoundVolume;
        _musicVolumeSlider.value = _stateManager.MusicVolume;

        if (_voiceVolumeSlider != null)
            _voiceVolumeSlider.value = _stateManager.VoiceVolume;

        SetMusicSoundButtonsImage();

        var currentLocale = LocalizationSettings.SelectedLocale;
        bool isRussian = currentLocale != null && currentLocale.Identifier.Code == "ru";
        _ruButton.image.sprite = isRussian ? _activeButtonLanguageBackground : _unactiveButtonLanguageBackground;
        _kzButton.image.sprite = isRussian ? _unactiveButtonLanguageBackground : _activeButtonLanguageBackground;
    }

    public void OnBuyEnergyClick()
    {
        if (_stateManager.CurrencyAmount >= 100)
        {
            _stateManager.CurrencyAmount -= 100;
            _stateManager.EnergyAmount = _stateManager.MaxEnergyValue;

            _soundManager.PlayEnergyRefillButtonSound();
        }
        else
        {
            _soundManager.PlayUnavailableButtonSound();
        }
    }

    public void OnBackClick()
    {
        _soundManager.StopVoice();
        _soundManager.StopEffects();
        OnBackClickAction?.Invoke();
        OnButtonClick?.Invoke();
        SetUpperLeftGroupWithEnergyRefillActive();
    }

    public void OnLeaderClick()
    {
        _leaderWindow.SetActive(true);
        OnButtonClick?.Invoke();
    }

    public void OnSettingsClick()
    {
        _settingsWindow.SetActive(true);
        OnButtonClick?.Invoke();
    }

    public void OnCloseSettingsWindow()
    {
        _settingsWindow.SetActive(false);
        OnButtonClick?.Invoke();
    }

    public void SetLeaderboardUIActive(bool value)
    {
        _leaderboardUI.SetActive(value);
    }

    public async void SetLeaderboardActive(bool value)
    {
        _leaderWindow.SetActive(value);

        if (value)
        {
            await _leaderboardManager.ReadData();
        }
        else
        {
            _leaderboardManager.ClearCards();
        }
    }

    public void CookingMode(bool isActive)
    {
        _settingsButton.SetActive(!isActive);
        _backTopButton.SetActive(isActive);
    }

    public void OnMusicVolumeChanged()
    {
        _soundManager.PlayDefaultButtonSound();

        _stateManager.MusicVolume = (int)_musicVolumeSlider.value;

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    public void OnSoundVolumeChanged()
    {
        _soundManager.PlayDefaultButtonSound();

        _stateManager.SoundVolume = (int)_soundsVolumeSlider.value;

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    public void OnMusicVolumeClick()
    {
        _soundManager.PlayDefaultButtonSound();

        if (_stateManager.MusicVolume == 0)
        {
            _stateManager.MusicVolume = 5;
            _musicVolumeSlider.value = 5;
        }
        else
        {
            _stateManager.MusicVolume = 0;
            _musicVolumeSlider.value = 0;
        }

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    public void OnSoundVolumeClick()
    {
        _soundManager.PlayDefaultButtonSound();

        if (_stateManager.SoundVolume == 0)
        {
            _stateManager.SoundVolume = 5;
            _soundsVolumeSlider.value = 5;
        }
        else
        {
            _stateManager.SoundVolume = 0;
            _soundsVolumeSlider.value = 0;
        }

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    public void OnVoiceVolumeChanged()
    {
        _soundManager.PlayDefaultButtonSound();

        _stateManager.VoiceVolume = (int)_voiceVolumeSlider.value;

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    public void OnVoiceVolumeClick()
    {
        _soundManager.PlayDefaultButtonSound();

        if (_stateManager.VoiceVolume == 0)
        {
            _stateManager.VoiceVolume = 5;
            _voiceVolumeSlider.value = 5;
        }
        else
        {
            _stateManager.VoiceVolume = 0;
            _voiceVolumeSlider.value = 0;
        }

        SetMusicSoundButtonsImage();

        OnButtonClick?.Invoke();
    }

    private void SetMusicSoundButtonsImage()
    {
        _soundSlash.SetActive(_soundsVolumeSlider.value == 0);
        _musicSlash.SetActive(_musicVolumeSlider.value == 0);

        if (_voiceSlash != null)
            _voiceSlash.SetActive(_voiceVolumeSlider.value == 0);
    }

    public void SetUpperRightGroupWithLeaderboardActive(bool setActive)
    {
        _upperRightCornerGroup.SetActive(setActive);
        _upperRightCornerWithoutLeaderboardGroup.SetActive(!setActive);
    }

    public void SetUpperLeftGroupWithEnergyRefillActive()
    {
        _upperLeftCornerGroupEnergyBar.SetActive(true);
    }

    public void SetUpperLeftGroupWithEnergyRefillDisabled()
    {
        _upperLeftCornerGroupEnergyBar.SetActive(false);
    }
}