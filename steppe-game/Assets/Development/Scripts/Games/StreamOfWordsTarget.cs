using UnityEngine;

public class StreamOfWordsTarget : CustomImageButton
{
    public Phrase Phrase => phrase;
    [SerializeField] private Phrase phrase;
}
