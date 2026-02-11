using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private static StateManager instance;
    public static StateManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<StateManager>();
            return instance;
        }
    }
    #region Constants

    private const string IsSwordBoughtKey = "IsSwordBought";
    private const string IsAxeBoughtKey = "IsAxeBought";
    private const string IsBowBoughtKey = "IsBowBought";
    private const string IsMazeBoughtKey = "IsMazeBought";
    private const string IsTutorialShownKey = "IsTutorialShown";
    private const string IsQuizTutorialShownKey = "IsQuizTutorialShown";
    private const string IsTutorialTiltabetCompleteKey = "IsTutorialTiltabetComplete";
    private const string IsIntroBozokCompleteKey = "IsIntroBozokComplete";
    private const string BozokBallGameCompleteKey = "BozokBallGameComplete";
    private const string BozokHorseGameCompleteKey = "BozokHorseGameComplete";
    private const string BozokBowGameCompleteKey = "BozokBowGameComplete";
    private const string BozokBoneGameCompleteKey = "BozokBoneGameComplete";
    private const string CharacterSexKey = "CharacterSex";
    private const string FiveWeaponPointAndClickGameTypeKey = "FiveWeaponPointAndClickGameTypeKey";
    private const string PLayerNameKey = "PLayerName";
    private const string CitySelectedKey = "CitySelected";
    private const string LanguageSelectedKey = "LanguageSelected";

    private const string AlmatyNameString = "Almaty";
    private const string AstanaNameString = "Astana";

    private const string KazakhLanguageString = "Kazakh";
    private const string RussianLanguageString = "Russian";

    private const string LevelNameKey = "LevelName";
    private const string SubLevelNameKey = "SubLevelName";

    private const string QuizNameKey = "QuizName";

    private const string EnergyValueKey = "EnergyValue";
    private const string CurrencyAmountKey = "CurrentCurrencyAmount";
    private const string PointsAmountKey = "CurrentPointsAmount";
    private const string ExperienceAmountKey = "ExperiencePointsAmount";


    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";
    private const string VoiceVolumeKey = "VoiceVolume";

    #endregion

    public int MaxEnergyValue = 10;

    public int basicLevelExperienceNeeded = 80;

    public static event Action StateChanged;

    public bool IsSwordBought
    {
        get => PlayerPrefs.GetInt(IsSwordBoughtKey, 0) != 0;
        set
        {
            if (IsSwordBought == value) return;
            PlayerPrefs.SetInt(IsSwordBoughtKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsAxeBought
    {
        get => PlayerPrefs.GetInt(IsAxeBoughtKey, 0) != 0;
        set
        {
            if (IsAxeBought == value) return;
            PlayerPrefs.SetInt(IsAxeBoughtKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsBowBought
    {
        get => PlayerPrefs.GetInt(IsBowBoughtKey, 0) != 0;
        set
        {
            if (IsBowBought == value) return;
            PlayerPrefs.SetInt(IsBowBoughtKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsMazeBought
    {
        get => PlayerPrefs.GetInt(IsMazeBoughtKey, 0) != 0;
        set
        {
            if (IsMazeBought == value) return;
            PlayerPrefs.SetInt(IsMazeBoughtKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsStartTutorialShown
    {
        get => PlayerPrefs.GetInt(IsTutorialShownKey, 0) != 0;
        set
        {
            if (IsStartTutorialShown == value) return;
            PlayerPrefs.SetInt(IsTutorialShownKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsQuizTutorialShown
    {
        get => PlayerPrefs.GetInt(IsQuizTutorialShownKey, 0) != 0;
        set
        {
            if (IsQuizTutorialShown == value) return;
            PlayerPrefs.SetInt(IsQuizTutorialShownKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsTutorialTiltabetComplete
    {
        get => PlayerPrefs.GetInt(IsTutorialTiltabetCompleteKey, 0) != 0;
        set
        {
            if (IsTutorialTiltabetComplete == value) return;
            PlayerPrefs.SetInt(IsTutorialTiltabetCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool IsIntroBozokComplete
    {
        get => PlayerPrefs.GetInt(IsIntroBozokCompleteKey, 0) != 0;
        set
        {
            if (IsIntroBozokComplete == value) return;
            PlayerPrefs.SetInt(IsIntroBozokCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool BozokBallGameComplete
    {
        get => PlayerPrefs.GetInt(BozokBallGameCompleteKey, 0) != 0;
        set
        {
            if (BozokBallGameComplete == value) return;
            PlayerPrefs.SetInt(BozokBallGameCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool BozokHorseGameComplete
    {
        get => PlayerPrefs.GetInt(BozokHorseGameCompleteKey, 0) != 0;
        set
        {
            if (BozokHorseGameComplete == value) return;
            PlayerPrefs.SetInt(BozokHorseGameCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool BozokBowGameComplete
    {
        get => PlayerPrefs.GetInt(BozokBowGameCompleteKey, 0) != 0;
        set
        {
            if (BozokBowGameComplete == value) return;
            PlayerPrefs.SetInt(BozokBowGameCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public bool BozokBoneGameComplete
    {
        get => PlayerPrefs.GetInt(BozokBoneGameCompleteKey, 0) != 0;
        set
        {
            if (BozokBoneGameComplete == value) return;
            PlayerPrefs.SetInt(BozokBoneGameCompleteKey, value ? 1 : 0);
            StateChanged?.Invoke();
        }
    }

    public CharacterSex CharacterSex
    {
        get => PlayerPrefs.GetInt(CharacterSexKey, 0) == 0 ? CharacterSex.Boy : CharacterSex.Girl;
        set
        {
            if (CharacterSex == value) return;
            PlayerPrefs.SetInt(CharacterSexKey, value == CharacterSex.Boy ? 0 : 1);
            StateChanged?.Invoke();
        }
    }

    public FiveWeaponsGameType FiveWeaponPointAndClickGameType
    {
        get
        {
            string enumString = PlayerPrefs.GetString(FiveWeaponPointAndClickGameTypeKey, "spear");

            switch (enumString)
            {
                case "spear":
                    return FiveWeaponsGameType.spear;
                case "sword":
                    return FiveWeaponsGameType.sword;
                case "axe":
                    return FiveWeaponsGameType.axe;
                case "hammer":
                    return FiveWeaponsGameType.hammer;
                case "bow":
                    return FiveWeaponsGameType.bow;
                default: return FiveWeaponsGameType.spear;
            }
        }

        set
        {
            if (FiveWeaponPointAndClickGameType == value) return;

            switch (value)
            {
                case FiveWeaponsGameType.spear:
                    PlayerPrefs.SetString(FiveWeaponPointAndClickGameTypeKey, "spear");
                    break;
                case FiveWeaponsGameType.sword:
                    PlayerPrefs.SetString(FiveWeaponPointAndClickGameTypeKey, "sword");
                    break;
                case FiveWeaponsGameType.axe:
                    PlayerPrefs.SetString(FiveWeaponPointAndClickGameTypeKey, "axe");
                    break;
                case FiveWeaponsGameType.hammer:
                    PlayerPrefs.SetString(FiveWeaponPointAndClickGameTypeKey, "hammer");
                    break;
                case FiveWeaponsGameType.bow:
                    PlayerPrefs.SetString(FiveWeaponPointAndClickGameTypeKey, "bow");
                    break;
            }

            StateChanged?.Invoke();
        }
    }

    public string PlayerName
    {
        get => PlayerPrefs.GetString(PLayerNameKey);
        set
        {
            if (PlayerName == value) return;
            PlayerPrefs.SetString(PLayerNameKey, value);
            StateChanged?.Invoke();
        }
    }

    public string CurrentQuiz
    {
        get => PlayerPrefs.GetString(QuizNameKey);
        set
        {
            if (CurrentQuiz == value) return;
            PlayerPrefs.SetString(QuizNameKey, value);
            StateChanged?.Invoke();
        }
    }

    public GameScene City
    {
        get
        {
            var city = PlayerPrefs.GetString(CitySelectedKey, string.Empty);

            if (city == AstanaNameString)
            {
                return GameScene.Astana;
            }

            if (city == AlmatyNameString)
            {
                return GameScene.Almaty;
            }

            return GameScene.ChooseCity;
        }

        set
        {
            if (City == value) return;
            switch (value)
            {
                case GameScene.Almaty:
                    PlayerPrefs.SetString(CitySelectedKey, AlmatyNameString);
                    break;
                case GameScene.Astana:
                    PlayerPrefs.SetString(CitySelectedKey, AstanaNameString);
                    break;
            }

            StateChanged?.Invoke();
        }
    }

    public Language Language
    {
        get
        {
            var language = PlayerPrefs.GetString(LanguageSelectedKey, KazakhLanguageString);

            if (language == RussianLanguageString)
            {
                return Language.Russian;
            }

            if (language == KazakhLanguageString)
            {
                return Language.Kazakh;
            }

            return Language.Kazakh;
        }

        set
        {
            if (Language == value) return;
            switch (value)
            {
                case Language.Russian:
                    PlayerPrefs.SetString(LanguageSelectedKey, RussianLanguageString);
                    break;
                case Language.Kazakh:
                    PlayerPrefs.SetString(LanguageSelectedKey, KazakhLanguageString);
                    break;
                default:
                    PlayerPrefs.SetString(LanguageSelectedKey, value.ToString());
                    break;
            }

            StateChanged?.Invoke();
        }
    }

    public static int Level
    {
        get => PlayerPrefs.GetInt(LevelNameKey, 1);
        set
        {
            if (Level == value) return;
            PlayerPrefs.SetInt(LevelNameKey, value);
            StateChanged?.Invoke();
        }
    }

    public static int SubLevel
    {
        get => PlayerPrefs.GetInt(SubLevelNameKey, 1);
        set
        {
            if (SubLevel == value) return;
            PlayerPrefs.SetInt(SubLevelNameKey, value);
            StateChanged?.Invoke();
        }
    }

    public int EnergyAmount
    {
        get => PlayerPrefs.GetInt(EnergyValueKey, 0);
        set
        {
            if (EnergyAmount == value) return;
            PlayerPrefs.SetInt(EnergyValueKey, value);
            StateChanged?.Invoke();
        }
    }

    public int CurrencyAmount
    {
        get => PlayerPrefs.GetInt(CurrencyAmountKey, 0);
        set
        {
            if (CurrencyAmount == value) return;
            PlayerPrefs.SetInt(CurrencyAmountKey, value);
            StateChanged?.Invoke();
        }
    }

    public int PointsAmount
    {
        get => PlayerPrefs.GetInt(PointsAmountKey, 0);
        set
        {
            if (PointsAmount == value) return;
            PlayerPrefs.SetInt(PointsAmountKey, value);
            StateChanged?.Invoke();
        }
    }

    public int ExperienceAmount
    {
        get => PlayerPrefs.GetInt(ExperienceAmountKey, 0);
        set
        {
            if (ExperienceAmount == value) return;
            PlayerPrefs.SetInt(ExperienceAmountKey, value);
            StateChanged?.Invoke();
        }
    }

    public int MusicVolume
    {
        get => PlayerPrefs.GetInt(MusicVolumeKey, 5);
        set
        {
            if (MusicVolume == value) return;
            PlayerPrefs.SetInt(MusicVolumeKey, value);
            StateChanged?.Invoke();
        }
    }

    public int SoundVolume
    {
        get => PlayerPrefs.GetInt(SoundVolumeKey, 5);
        set
        {
            if (SoundVolume == value) return;
            PlayerPrefs.SetInt(SoundVolumeKey, value);
            StateChanged?.Invoke();
        }
    }

    public int VoiceVolume
    {
        get => PlayerPrefs.GetInt(VoiceVolumeKey, 5);
        set
        {
            if (VoiceVolume == value) return;
            PlayerPrefs.SetInt(VoiceVolumeKey, value);
            StateChanged?.Invoke();
        }
    }
}