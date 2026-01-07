using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Quiz Question simple")]
public class QuizQuestionSimple : ScriptableObject
{
    [field: SerializeField] public QuizField Question { get; private set; }
    [Header("Answers")]
    [field: SerializeField] public QuizField[] Answers { get; private set; }
    [field: SerializeField] public int IndexOfCorrect { get; private set; }
}

[Serializable]
public class QuizField
{
    public string text;
    public AudioClip audioClip;
}