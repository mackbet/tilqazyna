using System;
using System.Collections;
using Development.Managers.Bozok;
using Development.Managers.Bozok.BoneGame;
using TMPro;
using UnityEngine;

public class BozokBoneShootingGameManager : MonoBehaviour
{
    [SerializeField] private GameObject _startBoneObject;
    [SerializeField] private GameObject _tutorialIconObject;
    [SerializeField] private GameObject _tazyObject;
    [SerializeField] private TextMeshProUGUI _tazyText;
    [SerializeField] private GameObject _startBoneBackground;

    [Header("Bones objects")]
    [SerializeField] private Bone _boneObject;
    [SerializeField] private BoneNonInteractable _boneAnswer1Object;
    [SerializeField] private BoneNonInteractable _boneAnswer2Object;
    [SerializeField] private BoneNonInteractable _boneAnswer3Object;

    [Header("Bones objects koy")]
    [SerializeField] private BoneNonInteractable _boneAnswerKoy1Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKoy2Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKoy3Object;

    [Header("Bones objects Kirpi")]
    [SerializeField] private BoneNonInteractable _boneAnswerKirpi1Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKirpi2Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKirpi3Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKirpi4Object;
    [SerializeField] private BoneNonInteractable _boneAnswerKirpi5Object;

    public static event Action OnStartNewQuiz;
    public static event Action OnContinueQuiz;

    private int currentQuestion = 1;

    [SerializeField] private GameObject[] _quizObjects;

    private void ResetAllBones()
    {
        _boneObject.ResetObject();
        _boneAnswer1Object.ResetObject();
        _boneAnswer2Object.ResetObject();
        _boneAnswer3Object.ResetObject();
        _boneAnswerKoy1Object.ResetObject();
        _boneAnswerKoy2Object.ResetObject();
        _boneAnswerKoy3Object.ResetObject();
        _boneAnswerKirpi1Object.ResetObject();
        _boneAnswerKirpi2Object.ResetObject();
        _boneAnswerKirpi3Object.ResetObject();
        _boneAnswerKirpi4Object.ResetObject();
        _boneAnswerKirpi5Object.ResetObject();
    }

    public void StartNewGame()
    {
        // ResetAllBones();
        currentQuestion = 1;
        _boneObject.ResetObject();
        OnStartNewQuiz?.Invoke();
        StartCoroutine(EnableTutorialForThreeSecond(true));
    }

    private void ContinueTap()
    {
        // ResetAllBones();
        currentQuestion += 1;
        _boneObject.ResetObject();
        // _startBoneObject.SetActive(true);
        _startBoneBackground.SetActive(true);
        OnContinueQuiz?.Invoke();
        StartCoroutine(EnableTutorialForThreeSecond(false));
    }

    private void DisableLowerUI()
    {
        // _startBoneObject.SetActive(false);
        _startBoneBackground.SetActive(false);
        _tutorialIconObject.SetActive(false);
        _tazyObject.SetActive(false);
    }

    private void OnEnable()
    {
        BozokBoneQuizManager.OnNextButtonTap += ContinueTap;
        BozokBoneQuizManager.OnBoneShootingOffLowerUI += DisableLowerUI;
    }

    private void OnDisable()
    {
        BozokBoneQuizManager.OnNextButtonTap -= ContinueTap;
        BozokBoneQuizManager.OnBoneShootingOffLowerUI -= DisableLowerUI;
    }

    private IEnumerator EnableTutorialForThreeSecond(bool tutorialButtonEnabled)
    {
        if (tutorialButtonEnabled)
        {
            _tutorialIconObject.SetActive(true);
        }

        _tazyObject.SetActive(true);

        // if (currentQuestion != 5)
        // {
        //     if (currentQuestion == 3)
        //     {
        _tazyText.text = "Асықты тартып тұрып, нысананы дәл көздеп атып жібер!";
        //     }
        //     else
        //     {
        // _tazyText.text = "Асық ойнайсың ба? Сөзді дұрыс тапсаң – жеңгенің.";
        //     }
        // }
        // else
        // {
        //     _tazyText.text = "Кел, балақай, асық ойнайық! Дұрыс тапсаң, ұпай қосылады.";
        // }

        yield return new WaitForSeconds(3f);

        _tutorialIconObject.SetActive(false);

        // if (currentQuestion != 5)
        // {
        //     _tazyText.text = "Адырнаны қаттырақ тартып, нысананы жақсырақ көздесең, дөп тигізуің де оңайырақ болады";
        // }
    }
}
