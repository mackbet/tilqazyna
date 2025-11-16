using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public struct Phrase
{
    public LocalizedString LocalizedString => localizedString;
    [SerializeField] private LocalizedString localizedString;
    public LocalizedAudioClip LocalizedAudio => localizedAudio;
    [SerializeField] private LocalizedAudioClip localizedAudio;
}