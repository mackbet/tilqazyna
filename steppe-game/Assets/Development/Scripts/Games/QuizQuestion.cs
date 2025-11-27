using UnityEngine;

[CreateAssetMenu(menuName = "SO/Quiz Question")]
public class QuizQuestion : ScriptableObject
{
    [field: SerializeField] public Phrase Question { get; private set; }
    [Header("Answers")]
    [field: SerializeField] public Phrase[] Answers { get; private set; }
    [field: SerializeField] public int IndexOfCorrect { get; private set; }
}
