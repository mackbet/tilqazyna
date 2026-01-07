using System;
using System.Collections;
using System.Threading.Tasks;
using Development.Managers.Bozok.HorseGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BozokQuizManager : MonoBehaviour
{
    public static event Action OnNextButtonTap;
    public static event Action<bool> OnEndQuiz;
    public static event Action<int> OnStarAppear;
    public static event Action OnBoneShootingOffLowerUI;
    public static event Action GameCompleted;

    [Header("Quiz info")]
    [SerializeField] public BozokQuizInfo quizInfo;

    [Header("Text quiz elements")]
    [SerializeField] private TextMeshProUGUI questionDescriptionText;
    [SerializeField] private TextMeshProUGUI questionTitleText;

    [Header("Question buttons game object")]
    [SerializeField] private Button questionButton1;
    [SerializeField] private Button questionButton2;
    [SerializeField] private Button questionButton3;

    [Header("Question buttons images")]
    [SerializeField] private Image questionButton1Image;
    [SerializeField] private Image questionButton2Image;
    [SerializeField] private Image questionButton3Image;

    [Header("Buttons text elements")]
    [SerializeField] private TextMeshProUGUI questionButton1Text;
    [SerializeField] private TextMeshProUGUI questionButton2Text;
    [SerializeField] private TextMeshProUGUI questionButton3Text;

    [Header("Buttons back sprites")]
    [SerializeField] private Sprite correctAnswerBack;
    [SerializeField] private Sprite wrongAnswerBack;
    [SerializeField] private Sprite regularAnwserBack;

    [Header("Question index and amount of right answers")]
    public int currentQuestionIndex;
    public int rightAnswers;
    private int currentInSequenceIndex;

    [Header("End game UI")]
    [SerializeField] private TextMeshProUGUI endLevelTitle;

    [Header("Info group text UI")]
    [SerializeField] private TextMeshProUGUI coins;
    [SerializeField] private TextMeshProUGUI points;

    [Header("Right Answer UI")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject _correctAnswerObject;

    private StateManager _stateManager;
    private SoundManager _soundManager;
    private bool _firstLineIsOver = false;

    private void Awake()
    {
        _stateManager = FindAnyObjectByType<StateManager>();
        _soundManager = FindAnyObjectByType<SoundManager>();
    }

    // public void Initialize(StateManager stateManager, SoundManager soundManager)
    // {
    //     _stateManager = stateManager;
    //     _soundManager = soundManager;
    // }

    public void StartNewQuiz()
    {
        nextButton.SetActive(false);

        if (_correctAnswerObject != null)
        {
            _correctAnswerObject.SetActive(false);
        }

        ProcessQuizStart();

        SetCurrentUI();
    }

    public async void OnAnswerClick(int buttonIndex)
    {
        DisableButtons();

        if (!quizInfo.questions[currentQuestionIndex].isSequence)
        {
            // If question is not sequence

            if (quizInfo.questions[currentQuestionIndex].answers[buttonIndex].correct)
            {
                // Question answered correctly
                RightAnswerChoosen(buttonIndex);

                OnBoneShootingOffLowerUI?.Invoke();
            }
            else
            {
                // Question answered incorrectly
                WrongAnswerChoosen(buttonIndex);

                await GameEnd(true);
            }
        }
        else
        {
            if (quizInfo.questions[currentQuestionIndex].answers[buttonIndex].correct && 
                quizInfo.questions[currentQuestionIndex].answers[buttonIndex].inSequenceOrder == currentInSequenceIndex)
            {
                // Question answered correctly
                ChangeButtonColor(buttonIndex: buttonIndex, newImage: correctAnswerBack);

                _soundManager.PlayRightAnswerSound();

                if (currentInSequenceIndex == 2)
                {
                    // All sequence answered
                    rightAnswers += 1;

                    currentQuestionIndex += 1;

                    nextButton.SetActive(true);

                    if (_correctAnswerObject != null)
                    {
                        _correctAnswerObject.SetActive(true);
                    }

                    OnBoneShootingOffLowerUI?.Invoke();
                }
                else
                {
                    // Continuing sequence
                    currentInSequenceIndex++;

                    // ResetBones?.Invoke();
                }
            }
            else
            {
                // Question answered incorrectly
                WrongAnswerChoosen(buttonIndex);

                await GameEnd(true);
            }
        }
    }

    public virtual void RightAnswerChoosen(int buttonIndex)
    {
        ChangeButtonColor(buttonIndex: buttonIndex, newImage: correctAnswerBack);

        _soundManager.PlayRightAnswerSound();
        if (quizInfo.questions[currentQuestionIndex].answerAudio != null)
        {
            _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].answerAudio);
        }

        rightAnswers += 1;

        currentQuestionIndex += 1;

        nextButton.SetActive(true);

        if (_correctAnswerObject != null)
        {
            _correctAnswerObject.SetActive(true);
        }
    }

    public virtual void RightAnswerChoosenBowGame(int buttonIndex)
    {
        ChangeButtonColor(buttonIndex: buttonIndex, newImage: correctAnswerBack);

        _soundManager.PlayRightAnswerSound();

        rightAnswers += 1;

        currentQuestionIndex += 1;

        nextButton.SetActive(true);
    }

    public virtual void WrongAnswerChoosen(int buttonIndex)
    {
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
        if (quizInfo.questions[currentQuestionIndex].wrongAnswerAudio != null)
        {
            _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].wrongAnswerAudio);
        }

        if (_stateManager.EnergyAmount > 0)
        {
            _stateManager.EnergyAmount -= 1;
        }
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
        }
    }

    public virtual void SetCurrentUI()
    {
        nextButton.SetActive(false);

        if (_correctAnswerObject != null)
        {
            _correctAnswerObject.SetActive(false);
        }

        questionTitleText.text = "Сұрақ " + (currentQuestionIndex + 1) + " / " + quizInfo.questions.Length;
        questionDescriptionText.text = quizInfo.questions[currentQuestionIndex].description;

        if (quizInfo.questions[currentQuestionIndex].wordByCharacter == "")
        {
            questionButton1Text.text = quizInfo.questions[currentQuestionIndex].answers[0].answer;
            questionButton2Text.text = quizInfo.questions[currentQuestionIndex].answers[1].answer;
            questionButton3Text.text = quizInfo.questions[currentQuestionIndex].answers[2].answer;
        }

        if (!_firstLineIsOver)
        {
            StartCoroutine(AudioCoroutine());   
        }
        else
        {
            if (quizInfo.questions[currentQuestionIndex].questionAudio != null)
            {
                _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].questionAudio);
            }
        }
        
        ActivateAllButtons();
    }

    private IEnumerator AudioCoroutine()
    {
        yield return new WaitWhile(() => _soundManager.IsVoicePlaying());
        _firstLineIsOver = true;
        Debug.Log("Done");
        if (quizInfo.questions[currentQuestionIndex].questionAudio != null)
        {
            _soundManager.PlayVoice(quizInfo.questions[currentQuestionIndex].questionAudio);
        }
    }

    public void DisableButtons()
    {
        questionButton1.enabled = false;
        questionButton2.enabled = false;
        questionButton3.enabled = false;
    }

    private void EnableButtons()
    {
        questionButton1.enabled = true;
        questionButton2.enabled = true;
        questionButton3.enabled = true;
    }

    private void ActivateAllButtons()
    {
        questionButton1.gameObject.SetActive(true);
        questionButton2.gameObject.SetActive(true);
        questionButton3.gameObject.SetActive(true);

        questionButton1Image.sprite = regularAnwserBack;
        questionButton2Image.sprite = regularAnwserBack;
        questionButton3Image.sprite = regularAnwserBack;

        EnableButtons();
    }

    public void NextButtonTapped()
    {
        if (rightAnswers < quizInfo.questions.Length)
        {
            // Game is not yet finished
            currentInSequenceIndex = 0;

            OnNextButtonTap?.Invoke();
        }
        else
        {
            // Game finished
            GameEndNoDelay();
        }
    }

    public void GameEndNoDelay()
    {
        if (rightAnswers <= 3)
        {
            coins.text = "2 тиын";
            points.text = "20 ұпай";

            OnStarAppear?.Invoke(1);
            ProcessQuizWinBad();
        }
        else if (rightAnswers <= 5 && rightAnswers > 3)
        {
            coins.text = "3 тиын";
            points.text = "40 ұпай";

            OnStarAppear?.Invoke(2);
            ProcessQuizWinNormal();
        }
        else
        {
            coins.text = "4 тиын";
            points.text = "60 ұпай";

            OnStarAppear?.Invoke(3);
            ProcessQuizWinBest();
        }

        _soundManager.PlayExpGainSound();

        endLevelTitle.text = "Деңгей" + rightAnswers + "/" + quizInfo.questions.Length;

        if (rightAnswers == quizInfo.questions.Length)
        {
            GameCompleted?.Invoke();
        }

        OnEndQuiz?.Invoke(rightAnswers == quizInfo.questions.Length);
    }

    public async Task GameEnd(bool wait)
    {
        if (wait)
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
        }

        if (rightAnswers <= 4)
        {
            coins.text = "2 тиын";
            points.text = "20 ұпай";

            OnStarAppear?.Invoke(1);
            ProcessQuizWinBad();
        }
        else if (rightAnswers <= 10 && rightAnswers > 4)
        {
            coins.text = "3 тиын";
            points.text = "40 ұпай";

            OnStarAppear?.Invoke(2);
            ProcessQuizWinNormal();
        }
        else
        {
            coins.text = "4 тиын";
            points.text = "60 ұпай";

            OnStarAppear?.Invoke(3);
            ProcessQuizWinBest();
        }

        _soundManager.PlayExpGainSound();

        endLevelTitle.text = "Деңгей" + rightAnswers + "/" + quizInfo.questions.Length;

        if (rightAnswers == quizInfo.questions.Length)
        {
            GameCompleted?.Invoke();
        }

        OnEndQuiz?.Invoke(rightAnswers == quizInfo.questions.Length);
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
        currentInSequenceIndex = 0;
    }

    private void OnEnable()
    {
        HorseGameManager.OnStartNewQuiz += StartNewQuiz;
        HorseGameManager.OnContinueQuiz += SetCurrentUI;
        BozokShootingGameManager.OnStartNewQuiz += StartNewQuiz;
        BozokShootingGameManager.OnContinueQuiz += SetCurrentUI;
        BozokBoneShootingGameManager.OnStartNewQuiz += StartNewQuiz;
        BozokBoneShootingGameManager.OnContinueQuiz += SetCurrentUI;
    }

    private void OnDisable()
    {
        HorseGameManager.OnStartNewQuiz -= StartNewQuiz;
        HorseGameManager.OnContinueQuiz -= SetCurrentUI;
        BozokShootingGameManager.OnStartNewQuiz -= StartNewQuiz;
        BozokShootingGameManager.OnContinueQuiz -= SetCurrentUI;
        BozokBoneShootingGameManager.OnStartNewQuiz -= StartNewQuiz;
        BozokBoneShootingGameManager.OnContinueQuiz -= SetCurrentUI;
    }
}
