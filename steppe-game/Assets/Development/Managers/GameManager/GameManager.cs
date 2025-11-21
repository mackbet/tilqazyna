using System;
using System.Threading.Tasks;
using Development.Managers.Bozok;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")][SerializeField] private TutorialManager _tutorialManager;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private HUDManager _hudManager;
    [SerializeField] private MapManager _astanaMapManager;
    [SerializeField] private AlmatyMapManager _almatyMapManager;
    [SerializeField] private GameObject _southKazakhstan;
    [SerializeField] private GameObject _eastKazakhstan;
    [SerializeField] private GameObject _altaiMapManager;
    [SerializeField] private GameObject _mangystauMapManager;
    [SerializeField] private GameObject _northMapManager;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private QuizController _quizController;
    [SerializeField] private FiveWeaponPointAndClickManager _fiveWeaponPointAndClickManager;
    private RealtimeManager _realtimeManager;
    [SerializeField] private EnergyManager _energyManager;
    [SerializeField] private LeaderboardManager _leaderboardManager;
    [SerializeField] private SharpGameSecondSceneNewManager _sharpGameSecondSceneNewManager;

    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private CookingViewManager _cookingViewManager;
    [SerializeField] private DragDropManager _dragDropManager;
    [SerializeField] private FiveWeaponsManager _fiveWeaponsManager;

    [Header("Game Scenes")]
    [SerializeField]
    private ChooseCityManager _chooseCityManager;

    [SerializeField] private ChooseCharacterManager _chooseCharacterManager;
    [SerializeField] private Canvas _cookingCanvas;
    [SerializeField] private Canvas _museumCanvas;
    [SerializeField] private Canvas _museumWeaponPointAndClickCanvas;
    //[SerializeField] private BozokIntroManager _bozokIntro;
    //[SerializeField] private GameObject _bozokMap;
    private GameObject _museumFiveWeapons;
    private GameObject _museumSevenTreasures;

    [SerializeField] private Canvas _quizCanvas;

    [SerializeField] private GameObject _almatyMuseumCanvas;
    private GameObject _altynAdamCanvas;
    private GameObject _traditionalLifeCanvas;

    [Header("Loading")][SerializeField] private Canvas _loadingCanvas;
    [SerializeField] private Slider _progressBar;

    [Header("Debug options")]
    [SerializeField]
    private bool openAllQuiz;

    // [Header("Bozok Managers")] [SerializeField]
    // private BozokGameManager _bozokGameManager;

    //[SerializeField] private BozokHorseQuizManager _horseQuizManager;
    //[SerializeField] private BozokBallQuizManager _ballQuizManager;
    //[SerializeField] private BozokBowQuizManager _bowQuizManager;
    //[SerializeField] private BozokBoneQuizManager _boneQuizManager;

    private GameScene _currentGameScene;
    private GameScene _previousGameScene;

    private GameScene CurrentGameScene
    {
        get => _currentGameScene;
        set
        {
            _soundManager.OnSceneChanged(value);

            if (value == GameScene.Astana)
            {
                if (!_stateManager.IsStartTutorialShown)
                {
                    _tutorialManager.StartTutorial();
                    _tutorialManager.StartTutorialComplete += () => _stateManager.IsStartTutorialShown = true;
                }

                if (_stateManager.IsTutorialTiltabetComplete && !_stateManager.IsQuizTutorialShown)
                {
                    _tutorialManager.QuizTutorial();
                    _tutorialManager.QuizTutorialComplete += () => _stateManager.IsQuizTutorialShown = true;
                }

                _stateManager.City = GameScene.Astana;
            }
            else if (value == GameScene.Almaty)
            {
                _stateManager.City = GameScene.Almaty;
            }
            else if (value == GameScene.Cooking && !_stateManager.IsTutorialTiltabetComplete)
            {
                _stateManager.IsTutorialTiltabetComplete = true;
            }

            if (value is GameScene.Quiz or GameScene.Cooking)
            {
                _hudManager.SetLeaderboardUIActive(false);
            }
            else
            {
                _hudManager.SetLeaderboardUIActive(true);
            }

            if (value == GameScene.Cooking)
            {
                _levelManager.StartNewLevel();
                _hudManager.CookingMode(true);
            }
            else
            {
                _hudManager.CookingMode(false);
            }

            _currentGameScene = value;
        }
    }

    private GameScene PreviousGameScene
    {
        get => _previousGameScene;
        set
        {
            if (value == GameScene.Astana)
            {
                if (!_stateManager.IsStartTutorialShown)
                {
                    _tutorialManager.StartTutorial();
                    _tutorialManager.StartTutorialComplete += () => _stateManager.IsStartTutorialShown = true;
                }

                if (_stateManager.IsTutorialTiltabetComplete && !_stateManager.IsQuizTutorialShown)
                {
                    _tutorialManager.QuizTutorial();
                    _tutorialManager.QuizTutorialComplete += () => _stateManager.IsQuizTutorialShown = true;
                }
            }
            else if (value == GameScene.Cooking && !_stateManager.IsTutorialTiltabetComplete)
            {
                _stateManager.IsTutorialTiltabetComplete = true;
            }

            if (value is GameScene.Quiz or GameScene.Cooking)
            {
                _hudManager.SetLeaderboardUIActive(false);
            }
            else
            {
                _hudManager.SetLeaderboardUIActive(true);
            }

            if (value == GameScene.Cooking)
            {
                _levelManager.StartNewLevel();
                _hudManager.CookingMode(true);
            }
            else
            {
                _hudManager.CookingMode(false);
            }

            _previousGameScene = value;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PutToDontDestroyOnLoad(gameObject);

        InitializeManagers();

        _museumFiveWeapons = GameObject.FindWithTag("MuseumFiveWeaponsCanvas");
        _museumSevenTreasures = GameObject.FindWithTag("MuseumSevenTreasuresCanvas");
        _almatyMuseumCanvas = GameObject.FindWithTag("AlmatyMuseumCanvas");
        _altynAdamCanvas = GameObject.FindWithTag("AltynAdamCanvas");
        _traditionalLifeCanvas = GameObject.FindWithTag("TraditionalLifeCanvas");

        if (openAllQuiz)
        {
            _stateManager.ExperienceAmount = 10000;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void PutToDontDestroyOnLoad(GameObject go)
    {
        if (go != null && go.transform.parent != null)
        {
            go.transform.SetParent(null);
        }
        DontDestroyOnLoad(go.gameObject);
    }

    /// <summary>
    /// Метод, вызываемый после загрузки новой сцены.
    /// Можно выполнять дополнительные настройки сцены.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Если нужно, например, включить общий HUD, который является DontDestroyOnLoad,
        // или выполнить инициализацию сцены «Бозок»
        if (CurrentGameScene == GameScene.Bozok)
        {
            // Если мы на сцене BozokIntro или BozokMap, дополнительная логика может быть здесь.
            _hudManager.gameObject.SetActive(true);
        }
        // Другие кейсы...
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(_stateManager.PlayerName))
        {
            OpenNewScene(GameScene.ChooseCharacter);
        }
        else
        {
            OpenNewScene(_stateManager.City);
        }
    }

    private void InitializeManagers()
    {
        _realtimeManager = new();
        _realtimeManager.Initialize();

        _energyManager.Initialize(_stateManager);

        _leaderboardManager.Initialize(realtimeManager: _realtimeManager, stateManager: _stateManager);

        _astanaMapManager.Initialize(_stateManager);
        _astanaMapManager.OnClickCookingButtonAction += () => OpenNewScene(GameScene.Cooking);
        _astanaMapManager.OnClickQuizButtonAction += () => OpenNewScene(GameScene.Quiz);
        _astanaMapManager.OnClickMuseumButtonAction += () => OpenNewScene(GameScene.Museum);
        MapManager.OnClickMuseumFiveWeaponsButtonAction += () => OpenNewScene(GameScene.MuseumFiveWeapons);
        MapManager.OnClickMuseumSevenTreasuresButtonAction += () => OpenNewScene(GameScene.MuseumSevenTreasures);
        MapManager.OnBozokButtonAction += () => OpenNewScene(GameScene.Bozok);

        _fiveWeaponsManager.Initialize(_stateManager);
        FiveWeaponsManager.OnClickMuseumWeaponButtonAction += () => OpenNewScene(GameScene.MuseumWeapon);

        _almatyMapManager.Initialize(_stateManager);
        _almatyMapManager.OnClickCookingButtonAction += () => OpenNewScene(GameScene.Cooking);
        _almatyMapManager.OnClickQuizButtonAction += () => OpenNewScene(GameScene.Quiz);
        AlmatyMapManager.OnClickMuseumButtonAction += () => OpenNewScene(GameScene.AlmatyMuseum);
        AlmatyMapManager.OnClickAltynAdamButtonAction += () => OpenNewScene(GameScene.AltynAdam);
        AlmatyMapManager.OnClickTraditionalLifeButtonAction += () => OpenNewScene(GameScene.TraditionalLife);

        _chooseCharacterManager.CharacterChoosen +=
            isBoy => _stateManager.CharacterSex = isBoy ? CharacterSex.Boy : CharacterSex.Girl;

        _chooseCharacterManager.NameSubmitted += name =>
        {
            _stateManager.PlayerName = name;
            OpenNewScene(GameScene.ChooseCity);
        };

        //_levelManager.Initialize(_stateManager);
        LevelManager.OnBackClick += () => OpenNewScene(_stateManager.City);

        _soundManager.Initialize(_stateManager, _cookingViewManager, _dragDropManager, _hudManager);

        _tutorialManager.Initialize(_soundManager);

        _fiveWeaponPointAndClickManager.Initialize(_stateManager, _soundManager);
        FiveWeaponPointAndClickManager.OnClickBackToCityButtonAction += () => OpenNewScene(_stateManager.City);
        FiveWeaponPointAndClickManager.OnClickBackToMuseumButtonAction +=
            () => OpenNewScene(GameScene.MuseumFiveWeapons);

        _quizController.Initialize(_stateManager, _soundManager);
        _quizController.OnBackToMapClickAction += ProcessBackClick;

        _hudManager.Initialize(_stateManager, _leaderboardManager, _soundManager);
        HUDManager.OnBackClickAction += ProcessBackClick;

        _tutorialManager.OnTutorialStarted += _soundManager.OnTutorialSound;

        //_bozokIntro.Initialize(_stateManager);
        BozokIntroManager.OnEnterButtonAction += () => OpenNewScene(GameScene.Bozok);

        //_bozokGameManager.Initialize(_stateManager, _hudManager);
        BozokGameManager.OnBackToBozokMap += () => OpenNewScene(GameScene.Bozok);
        BozokGameManager.OnBackToMap += () => OpenNewScene(_stateManager.City);

        _sharpGameSecondSceneNewManager.Initialize(_stateManager, _soundManager);

        //_horseQuizManager.Initialize(_stateManager, _soundManager);
        //_ballQuizManager.Initialize(_stateManager, _soundManager);
        //_bowQuizManager.Initialize(_stateManager, _soundManager);
        //_boneQuizManager.Initialize(_stateManager, _soundManager);
    }

    public async void OpenNewScene(GameScene scene)
    {
        _soundManager.PlayButtonSound();
        CloseAllScenes();
        //await LoadSceneWithLoadingScreen(scene);

        // Получаем имя сцены (оно должно соответствовать названию сцены в Build Settings)
        string sceneName = GetSceneName(scene);
        // if (string.IsNullOrEmpty(sceneName))
        // {
        //     Debug.LogError($"Не найдено имя сцены для {scene}");
        //     return;
        // }

        if (sceneName == "BozokGames" || sceneName == "BozokIntro" ||
            (sceneName == "GameScene" && CurrentGameScene == GameScene.Bozok))
        {
            _loadingCanvas.enabled = true;
            _progressBar.value = 0;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                _progressBar.value = asyncLoad.progress;
                await Task.Yield();
            }
            _progressBar.value = 1f;

            if (_stateManager.PlayerName != "")
            {
                await _realtimeManager.SaveUserData(
                    userName: _stateManager.PlayerName,
                    userSex: _stateManager.CharacterSex,
                    userLevel: _stateManager.ExperienceAmount / 80,
                    userPoints: _stateManager.PointsAmount);
            }

            asyncLoad.allowSceneActivation = true;
            ActivateScene(scene);
            PreviousGameScene = CurrentGameScene;
            CurrentGameScene = scene;
            _stateManager.City = scene;
            _hudManager.gameObject.SetActive(true);
            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }
            _loadingCanvas.enabled = false;
        }
        else
        {
            CloseAllScenes();
            _loadingCanvas.enabled = true;
            _progressBar.value = 0;

            const float totalSteps = 10;
            for (var i = 0; i < totalSteps / 2; i++)
            {
                // yield return new WaitForSeconds(0.05f);
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                _progressBar.value = (i + 1) / totalSteps;
            }

            if (_stateManager.PlayerName != "")
            {
                await _realtimeManager.SaveUserData(userName: _stateManager.PlayerName, userSex: _stateManager.CharacterSex,
                    userLevel: _stateManager.ExperienceAmount / 80, userPoints: _stateManager.PointsAmount);
            }

            for (var i = 4; i < totalSteps; i++)
            {
                // yield return new WaitForSeconds(0.05f);
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                _progressBar.value = (i + 1) / totalSteps;
            }

            ActivateScene(scene);
            PreviousGameScene = CurrentGameScene;
            CurrentGameScene = scene;
            _stateManager.City = scene;
            _loadingCanvas.enabled = false;
        }
    }

    // private async Task LoadSceneWithLoadingScreen(GameScene scene)
    // {
    //     _soundManager.PlayLogoSound();
    //
    //     _loadingCanvas.enabled = true;
    //     _progressBar.value = 0;
    //
    //     const float totalSteps = 10;
    //     for (var i = 0; i < totalSteps / 2; i++)
    //     {
    //         // yield return new WaitForSeconds(0.05f);
    //         await Task.Delay(TimeSpan.FromSeconds(0.05f));
    //         _progressBar.value = (i + 1) / totalSteps;
    //     }
    //
    //     if (_stateManager.PlayerName != "")
    //     {
    //         await _realtimeManager.SaveUserData(userName: _stateManager.PlayerName, userSex: _stateManager.CharacterSex,
    //             userLevel: _stateManager.ExperienceAmount / 80, userPoints: _stateManager.PointsAmount);
    //     }
    //
    //     for (var i = 4; i < totalSteps; i++)
    //     {
    //         // yield return new WaitForSeconds(0.05f);
    //         await Task.Delay(TimeSpan.FromSeconds(0.05f));
    //         _progressBar.value = (i + 1) / totalSteps;
    //     }
    //
    //     ActivateScene(scene);
    //     PreviousGameScene = CurrentGameScene;
    //     CurrentGameScene = scene;
    //     _stateManager.City = scene;
    //     _loadingCanvas.enabled = false;
    // }

    private string GetSceneName(GameScene scene)
    {
        switch (scene)
        {
            // case GameScene.Cooking:
            //     return "CookingScene";
            // case GameScene.Quiz:
            //     return "QuizScene";
            case GameScene.Astana:
                return "GameScene";
            // case GameScene.Almaty:
            //     return "AlmatyScene";
            // case GameScene.ChooseCharacter:
            //     return "ChooseCharacterScene";
            // case GameScene.ChooseCity:
            //     return "ChooseCityScene";
            // case GameScene.Museum:
            //     return "MuseumScene";
            case GameScene.Bozok:
                return _stateManager.IsIntroBozokComplete ? "BozokGames" : "BozokIntro";
            default:
                return "";
        }
    }

    private void ProcessBackClick()
    {
        switch (CurrentGameScene)
        {
            case GameScene.Cooking:
            case GameScene.Quiz:
                _soundManager.StopVoice();
                OpenNewScene(PreviousGameScene);
                break;
            case GameScene.Astana:
                OpenNewScene(GameScene.ChooseCity);
                break;
            case GameScene.Almaty:
                OpenNewScene(GameScene.ChooseCity);
                break;
            case GameScene.Museum:
                OpenNewScene(GameScene.Astana);
                break;
            case GameScene.MuseumFiveWeapons:
                OpenNewScene(GameScene.Museum);
                break;
            case GameScene.MuseumSevenTreasures:
                OpenNewScene(GameScene.Museum);
                break;
            case GameScene.MuseumWeapon:
                OpenNewScene(GameScene.MuseumFiveWeapons);
                _soundManager.StopMusic();
                _soundManager.PlayAstanaMusic();
                break;
            case GameScene.Bozok:
                OpenNewScene(_stateManager.City);
                break;
            case GameScene.AlmatyMuseum:
                OpenNewScene(GameScene.Almaty);
                break;
            case GameScene.AltynAdam:
                OpenNewScene(GameScene.AlmatyMuseum);
                break;
            case GameScene.TraditionalLife:
                OpenNewScene(GameScene.AlmatyMuseum);
                break;
        }
    }

    private void CloseAllScenes()
    {
        if (lastCity)
            lastCity.SetActive(false);
            
        if (CurrentGameScene == GameScene.Bozok)
        {
            return;
        }
        _cookingCanvas.enabled = false;
        _cookingCanvas.gameObject.SetActive(false);
        _quizCanvas.enabled = false;
        _quizCanvas.gameObject.SetActive(false);
        _astanaMapManager.gameObject.SetActive(false);
        _almatyMapManager.gameObject.SetActive(false);
        _hudManager.gameObject.SetActive(false);
        _chooseCharacterManager.gameObject.SetActive(false);
        _chooseCityManager.gameObject.SetActive(false);
        _museumCanvas.gameObject.SetActive(false);
        _museumFiveWeapons.SetActive(false);
        _museumSevenTreasures.SetActive(false);
        _museumWeaponPointAndClickCanvas.gameObject.SetActive(false);
        //_bozokIntro.gameObject.SetActive(false);
        //_bozokMap.gameObject.SetActive(false);
        _almatyMuseumCanvas.SetActive(false);
        _altynAdamCanvas.SetActive(false);
        _traditionalLifeCanvas.SetActive(false);
    }

    private void ActivateScene(GameScene scene)
    {
        switch (scene)
        {
            case GameScene.Cooking:
                _hudManager.gameObject.SetActive(true);
                _cookingCanvas.enabled = true;
                _cookingCanvas.gameObject.SetActive(true);
                break;
            case GameScene.Quiz:
                _quizController.StartNewQuiz();
                _quizCanvas.gameObject.SetActive(true);
                _hudManager.gameObject.SetActive(true);
                _quizCanvas.enabled = true;
                break;
            case GameScene.Astana:
                OpenCity(_astanaMapManager.gameObject);
                break;
            case GameScene.Almaty:
                OpenCity(_almatyMapManager.gameObject);
                break;
            case GameScene.SouthKazakhstan:
                OpenCity(_southKazakhstan);
                break;
            case GameScene.EastKazakhstan:
                OpenCity(_eastKazakhstan);
                break;
            case GameScene.Altai:
                OpenCity(_altaiMapManager);
                break;
            case GameScene.Mangystau:
                OpenCity(_mangystauMapManager);
                break;
            case GameScene.North:
                OpenCity(_northMapManager);
                break;
            case GameScene.ChooseCharacter:
                _chooseCharacterManager.gameObject.SetActive(true);
                _chooseCharacterManager.Initialize();
                break;
            case GameScene.ChooseCity:
                _chooseCityManager.gameObject.SetActive(true);
                break;
            case GameScene.Museum:
                _hudManager.gameObject.SetActive(true);
                _museumCanvas.gameObject.SetActive(true);
                break;
            case GameScene.MuseumFiveWeapons:
                _hudManager.gameObject.SetActive(true);
                _museumFiveWeapons.SetActive(true);
                break;
            case GameScene.MuseumSevenTreasures:
                _hudManager.gameObject.SetActive(true);
                _museumSevenTreasures.SetActive(true);
                break;
            case GameScene.MuseumWeapon:
                _fiveWeaponPointAndClickManager.StartNewGame();
                _hudManager.gameObject.SetActive(true);
                _museumWeaponPointAndClickCanvas.gameObject.SetActive(true);
                break;
            // case GameScene.Bozok:
            // {
            //     if (_stateManager.IsIntroBozokComplete)
            //     {
            //         _bozokMap.SetActive(true);
            //     }
            //     else
            //     {
            //         _bozokIntro.gameObject.SetActive(true);
            //     }
            //
            //     _hudManager.gameObject.SetActive(true);
            //     break;
            // }
            case GameScene.AlmatyMuseum:
                _hudManager.gameObject.SetActive(true);
                _almatyMuseumCanvas.gameObject.SetActive(true);
                break;
            case GameScene.AltynAdam:
                _hudManager.gameObject.SetActive(true);
                _altynAdamCanvas.gameObject.SetActive(true);
                break;
            case GameScene.TraditionalLife:
                _hudManager.gameObject.SetActive(true);
                _traditionalLifeCanvas.gameObject.SetActive(true);
                break;
        }
    }


    private GameObject lastCity = null;

    private void OpenCity(GameObject city)
    {
        _hudManager.gameObject.SetActive(true);
        city.gameObject.SetActive(true);
        lastCity = city;

        if (PreviousGameScene == GameScene.Quiz)
        {
            _soundManager.PlayEnterCitySound();
        }
    }
}