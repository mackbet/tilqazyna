using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class QuizController : MonoBehaviour
{
    [Header("Quiz info")]
    private QuizInfo quizInfo;

    [Header("Text quiz elements")]
    [SerializeField] private TextMeshProUGUI questionTitleText;
    [SerializeField] private TextMeshProUGUI questionDescriptionText;
    [SerializeField] private TextMeshProUGUI quizTitleText;

    [Header("Question buttons game object")]
    [SerializeField] private Button questionButton1;
    [SerializeField] private Button questionButton2;
    [SerializeField] private Button questionButton3;
    [SerializeField] private Button questionButton4;
    [SerializeField] private Button removeAnswers;

    [Header("Question buttons images")]
    [SerializeField] private Image questionButton1Image;
    [SerializeField] private Image questionButton2Image;
    [SerializeField] private Image questionButton3Image;
    [SerializeField] private Image questionButton4Image;

    [Header("Quiz back images")]
    [SerializeField] private Sprite bayterekImage;
    [SerializeField] private Sprite shatyrImage;
    [SerializeField] private Sprite acordaImage;
    [SerializeField] private Sprite expoImage;
    [SerializeField] private Sprite kekImage;
    [SerializeField] private Sprite medeyImage;
    [SerializeField] private Sprite konakImage;
    [SerializeField] private Sprite sarayImage;
    [SerializeField] private Image quizCurrentBack;
    [SerializeField] private GameObject tower;

    [Header("Buttons text elements")]
    [SerializeField] private TextMeshProUGUI questionButton1Text;
    [SerializeField] private TextMeshProUGUI questionButton2Text;
    [SerializeField] private TextMeshProUGUI questionButton3Text;
    [SerializeField] private TextMeshProUGUI questionButton4Text;

    [Header("Buttons back sprites")]
    [SerializeField] private Sprite correctAnswerBack;
    [SerializeField] private Sprite wrongAnswerBack;
    [SerializeField] private Sprite regularAnwserBack;

    [Header("Text back sprites")]
    [SerializeField] private GameObject wrongAnwserPanelBack;
    [SerializeField] private GameObject rightAnwserPanelBack;
    // [SerializeField] private GameObject tazyPanelBack;

    [Header("Right Wrong Answer Canvas")]
    // [SerializeField] private Canvas rightWrongAnswerCanvas;
    [SerializeField] private TextMeshProUGUI answerRightContextText;
    [SerializeField] private TextMeshProUGUI answerWrongContextText;
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private Canvas energyBarCanvas;

    [Header("Hint and Continue Buttons")]
    [SerializeField] private Button hintButton;
    [SerializeField] private Button continueButton;

    [Header("Question index and amount of right answers")]
    private int currentQuestionIndex;
    private int rightAnswers;

    [Header("End game UI")]
    [SerializeField] private TextMeshProUGUI endGameLevelName;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Canvas endLevelCanvas;
    [SerializeField] private TextMeshProUGUI endLevelTitle;

    [Header("Star groups")]
    [SerializeField] private GameObject starGroupOne;
    [SerializeField] private GameObject starGroupTwo;
    [SerializeField] private GameObject starGroupThree;

    [Header("Info group text UI")]
    [SerializeField] private TextMeshProUGUI coins;
    [SerializeField] private TextMeshProUGUI points;

    [Header("Quiz Info")]
    [SerializeField] private QuizInfo bayterekInfo;
    [SerializeField] private QuizInfo shatyrInfo;
    [SerializeField] private QuizInfo acordaInfo;
    [SerializeField] private QuizInfo expoInfo;
    [SerializeField] private QuizInfo sarayInfo;
    [SerializeField] private QuizInfo kokInfo;
    [SerializeField] private QuizInfo medeyInfo;
    [SerializeField] private QuizInfo konakInfo;

    public event Action OnBackToMapClickAction;

    private StateManager _stateManager;
    private SoundManager _soundManager;

    private bool lastAnswerAnsweredCorrectly = false;

    public void Initialize(StateManager stateManager, SoundManager soundManager)
    {
        _stateManager = stateManager;
        _soundManager = soundManager;

        LocaleManager.OnChangeQuestionUI += LocalizeQuestionUI;
    }

    private void DisableAllBack()
    {
        quizCurrentBack.sprite = null;
        tower.SetActive(false);
    }

    private void SetCurrentBack()
    {
        if (_stateManager.CurrentQuiz == "«Бәйтерек» монументі")
        {
            quizCurrentBack.sprite = bayterekImage;
            tower.SetActive(true);
        }
        else if (_stateManager.CurrentQuiz == "Хан Шатыр")
        {
            quizCurrentBack.sprite = shatyrImage;
        }
        else if (_stateManager.CurrentQuiz == "Ақорда")
        {
            quizCurrentBack.sprite = acordaImage;
        }
        else if (_stateManager.CurrentQuiz == "EXPO Астана")
        {
            quizCurrentBack.sprite = expoImage;
        }
        else if (_stateManager.CurrentQuiz == "Көктөбе")
        {
            quizCurrentBack.sprite = kekImage;
        }
        else if (_stateManager.CurrentQuiz == "Медеу мұз айдыны")
        {
            quizCurrentBack.sprite = medeyImage;
        }
        else if (_stateManager.CurrentQuiz == "«Қазақстан» қонақүйі")
        {
            quizCurrentBack.sprite = konakImage;
        }
        else if (_stateManager.CurrentQuiz == "Бейбітшілік және келісім сарайы")
        {
            quizCurrentBack.sprite = sarayImage;
        }
    }

    private void SetCurrentQuiz()
    {
        if (_stateManager.CurrentQuiz == "«Бәйтерек» монументі")
        {
            quizInfo = bayterekInfo;
            _soundManager.PlayMusicQuizBayterek();
        }
        else if (_stateManager.CurrentQuiz == "Хан Шатыр")
        {
            quizInfo = shatyrInfo;
            _soundManager.PlayMusicQuizShatyr();
        }
        else if (_stateManager.CurrentQuiz == "Ақорда")
        {
            quizInfo = acordaInfo;
            _soundManager.PlayMusicQuizAkorda();
        }
        else if (_stateManager.CurrentQuiz == "EXPO Астана")
        {
            quizInfo = expoInfo;
            _soundManager.PlayMusicQuizExpo();
        }
        else if (_stateManager.CurrentQuiz == "Көктөбе")
        {
            quizInfo = kokInfo;
            _soundManager.PlayMusicQuizKekTobe();
        }
        else if (_stateManager.CurrentQuiz == "Медеу мұз айдыны")
        {
            quizInfo = medeyInfo;
            _soundManager.PlayMusicQuizMedey();
        }
        else if (_stateManager.CurrentQuiz == "«Қазақстан» қонақүйі")
        {
            quizInfo = konakInfo;
            _soundManager.PlayMusicQuizKonak();
        }
        else if (_stateManager.CurrentQuiz == "Бейбітшілік және келісім сарайы")
        {
            quizInfo = sarayInfo;
            _soundManager.PlayMusicQuizBeybitishlik();
        }
    }

    public void StartNewQuiz()
    {
        DisableAllBack();

        SetCurrentBack();

        endGamePanel.SetActive(false);

        quizTitleText.text = _stateManager.CurrentQuiz;
        endLevelTitle.text = _stateManager.CurrentQuiz;

        starGroupOne.SetActive(false);
        starGroupTwo.SetActive(false);
        starGroupThree.SetActive(false);

        // rightWrongAnswerCanvas.sortingOrder = 10;

        ProcessQuizStart();

        SetCurrentQuiz();

        SetCurrentUI();
    }

    public void OnAnswerClick(int buttonIndex)
    {
        DisableButtons();

        _soundManager.StopVoice();

        // Скрыть подсказку, показать кнопку продолжения
        hintButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);

        if (quizInfo.questions[currentQuestionIndex].answers[buttonIndex].correct)
        {
            // Question answered correctly
            ChangeButtonColor(buttonIndex: buttonIndex, newImage: correctAnswerBack);

            _soundManager.PlayRightAnswerSound();

            SetAnswerBackPanelActive(answerIsRight: true, active: true);

            answerRightContextText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].rightAnswerContent);

            _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].answerAudio);

            rightAnswers += 1;

            lastAnswerAnsweredCorrectly = true;
        }
        else
        {
            // Question answered incorrectly
            ChangeButtonColor(buttonIndex: buttonIndex, newImage: wrongAnswerBack);

            for (int i = 0; i < quizInfo.questions[currentQuestionIndex].answers.Length; i++)
            {
                if (quizInfo.questions[currentQuestionIndex].answers[i].correct)
                {
                    ChangeButtonColor(buttonIndex: i, newImage: correctAnswerBack);
                    break;
                }
            }

            _soundManager.PlayWrongAnswerSound();

            if (_stateManager.EnergyAmount > 0)
            {
                _stateManager.EnergyAmount -= 1;
            }

            SetAnswerBackPanelActive(answerIsRight: false, active: true);

            // Change panel text according to energy amount
            if (_stateManager.EnergyAmount == 0)
            {
                var translatedValue = LocalizationSettings.StringDatabase.GetLocalizedString("Main", "end_game_not_enough_energy");
                answerWrongContextText.text = translatedValue;
                arrowObject.SetActive(true);
                energyBarCanvas.overrideSorting = true;
                energyBarCanvas.sortingOrder = 11;
            }
            else
            {
                var translatedValue = LocalizationSettings.StringDatabase.GetLocalizedString("Main", "wrong_answer_context_text", arguments: _stateManager.EnergyAmount);
                answerWrongContextText.text = translatedValue;
                arrowObject.SetActive(false);
            }

            lastAnswerAnsweredCorrectly = false;
        }

        currentQuestionIndex += 1;
    }

    private void ChangeButtonColor(int buttonIndex, Sprite newImage)
    {
        switch (buttonIndex)
        {
            case 0:
                questionButton1Image.sprite = newImage;
                break;
            case 1:
                questionButton2Image.sprite = newImage;
                break;
            case 2:
                questionButton3Image.sprite = newImage;
                break;
            case 3:
                questionButton4Image.sprite = newImage;
                break;
        }
    }

    private void SetCurrentUI()
    {
        DeactivateAnswerBackPanel();

        LocalizeQuestionUI();

        _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].questionAudio);

        ActivateAllButtons();

        removeAnswers.gameObject.SetActive(true);

        // Показать подсказку, скрыть кнопку продолжения
        hintButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);
    }

    private void LocalizeQuestionUI()
    {
        // Проверка что квиз активен и вопрос существует
        if (quizInfo == null || quizInfo.questions == null || currentQuestionIndex >= quizInfo.questions.Length)
            return;

        Debug.Log($"LocalizeQuestionUI: {LocalizationSettings.SelectedLocale.LocaleName}");
        questionTitleText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", "Вопрос") + " " + (currentQuestionIndex + 1) + " / " + quizInfo.questions.Length;
        questionDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].description);

        questionButton1Text.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].answers[0].answer);
        questionButton2Text.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].answers[1].answer);
        questionButton3Text.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].answers[2].answer);
        questionButton4Text.text = LocalizationSettings.StringDatabase.GetLocalizedString("Main", quizInfo.questions[currentQuestionIndex].answers[3].answer);
    }

    private void SetCurrentUIToEndGame()
    {
        DeactivateAnswerBackPanel();

        // endLevelCanvas.overrideSorting = true;
        // endLevelCanvas.sortingOrder = 20;

        // endGameLevelName.text = "";

        endGamePanel.SetActive(true);

        GameEnd();
    }

    private void DisableButtons()
    {
        questionButton1.enabled = false;
        questionButton2.enabled = false;
        questionButton3.enabled = false;
        questionButton4.enabled = false;
        removeAnswers.enabled = false;
    }

    private void EnableButtons()
    {
        questionButton1.enabled = true;
        questionButton2.enabled = true;
        questionButton3.enabled = true;
        questionButton4.enabled = true;
        removeAnswers.enabled = true;
    }

    private void ActivateAllButtons()
    {
        questionButton1.gameObject.SetActive(true);
        questionButton2.gameObject.SetActive(true);
        questionButton3.gameObject.SetActive(true);
        questionButton4.gameObject.SetActive(true);

        questionButton1Image.sprite = regularAnwserBack;
        questionButton2Image.sprite = regularAnwserBack;
        questionButton3Image.sprite = regularAnwserBack;
        questionButton4Image.sprite = regularAnwserBack;

        EnableButtons();
    }

    private void DeactivateAnswerBackPanel()
    {
        rightAnwserPanelBack.SetActive(false);
        wrongAnwserPanelBack.SetActive(false);
        // tazyPanelBack.SetActive(false);
    }

    public void RemoveTwoAnswers()
    {
        if (_stateManager.CurrencyAmount >= 2)
        {
            _soundManager.PlayRemoveAnswerSound();

            _stateManager.CurrencyAmount -= 2;

            var rightAnswerIndex = 0;

            for (var index = 0; index < quizInfo.questions[currentQuestionIndex].answers.Length; index++)
            {
                if (quizInfo.questions[currentQuestionIndex].answers[index].correct)
                {
                    rightAnswerIndex = index;
                }
            }

            if (rightAnswerIndex == 0)
            {
                questionButton3.gameObject.SetActive(false);
                questionButton4.gameObject.SetActive(false);
            }
            else if (rightAnswerIndex == 1)
            {
                questionButton3.gameObject.SetActive(false);
                questionButton4.gameObject.SetActive(false);
            }
            else if (rightAnswerIndex == 2)
            {
                questionButton1.gameObject.SetActive(false);
                questionButton2.gameObject.SetActive(false);
            }
            else if (rightAnswerIndex == 3)
            {
                questionButton1.gameObject.SetActive(false);
                questionButton2.gameObject.SetActive(false);
            }

            removeAnswers.gameObject.SetActive(false);
        }
        else
        {
            _soundManager.PlayUnavailableButtonSound();
        }
    }

    public void NextButtonTapped()
    {
        _soundManager.StopVoice();

        energyBarCanvas.overrideSorting = false;

        if (currentQuestionIndex < quizInfo.questions.Length)
        {
            if (_stateManager.EnergyAmount == 0 && !lastAnswerAnsweredCorrectly)
            {
                OnBackToCityClick();
            }
            else
            {
                // Quiz is not yet finished
                SetCurrentUI();
            }
        }
        else
        {
            // Quiz is finished
            SetCurrentUIToEndGame();
        }
    }

    public void OnBackToCityClick()
    {
        _soundManager.StopVoice();

        DeactivateAnswerBackPanel();
        endLevelCanvas.overrideSorting = false;

        endGamePanel.SetActive(false);

        OnBackToMapClickAction?.Invoke();
    }

    public void GameEnd()
    {
        if (rightAnswers <= 4)
        {
            coins.text = "2 тиын";
            points.text = "20 ұпай";

            starGroupOne.SetActive(true);
            ProcessQuizWinBad();
        }
        else if (rightAnswers == 5)
        {
            coins.text = "3 тиын";
            points.text = "40 ұпай";

            starGroupTwo.SetActive(true);
            ProcessQuizWinNormal();
        }
        else if (rightAnswers == 6)
        {
            coins.text = "4 тиын";
            points.text = "60 ұпай";

            starGroupThree.SetActive(true);
            ProcessQuizWinBest();
        }

        _soundManager.PlayExpGainSound();
    }

    private void ProcessQuizWinBest()
    {
        _stateManager.EnergyAmount = _stateManager.MaxEnergyValue;

        _stateManager.CurrencyAmount += 4;
        _stateManager.PointsAmount += 60;
        _stateManager.ExperienceAmount += 100;
    }

    private void ProcessQuizWinNormal()
    {
        _stateManager.CurrencyAmount += 3;
        _stateManager.PointsAmount += 40;
        _stateManager.ExperienceAmount += 80;
    }

    private void ProcessQuizWinBad()
    {
        _stateManager.CurrencyAmount += 2;
        _stateManager.PointsAmount += 20;
        _stateManager.ExperienceAmount += 50;
    }

    private void ProcessQuizStart()
    {
        currentQuestionIndex = 0;
        rightAnswers = 0;
    }

    private void SetAnswerBackPanelActive(bool answerIsRight, bool active)
    {
        if (answerIsRight)
        {
            rightAnwserPanelBack.SetActive(active);
        }
        else
        {
            wrongAnwserPanelBack.SetActive(active);
        }

        // tazyPanelBack.SetActive(active);
    }
}