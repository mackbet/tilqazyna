using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<SoundManager>();
            return instance;
        }
    }
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _effectsAudioSource;
    [SerializeField] private AudioSource _voiceAudioSource;

    [Header("Sound Effects Cooking")]
    [SerializeField] private AudioClip _soundButton;
    [SerializeField] private AudioClip _soundWrongAttempt;

    [Header("Sound Effects Five Weapons")]
    [SerializeField] private AudioClip _spearWinSound;
    [SerializeField] private AudioClip _swordWinSound;
    [SerializeField] private AudioClip _bowWinSound;
    [SerializeField] private AudioClip _mazeWinSound;
    [SerializeField] private AudioClip _axeWinSound;
    [SerializeField] private AudioClip _weaponPartSound;

    [Header("Sound Effects Other")]
    [SerializeField] private AudioClip _enterCitySound;
    [SerializeField] private AudioClip _logoSound;
    [SerializeField] private AudioClip _tipAppearSound;
    [SerializeField] private AudioClip _defaultButtonSound;
    [SerializeField] private AudioClip _unavailableButtonSound;
    [SerializeField] private AudioClip _energyRefillButtonSound;

    [Header("Sound Effects Quiz")]
    [SerializeField] private AudioClip _wrongAnswerSound;
    [SerializeField] private AudioClip _rightAnswerSound;
    [SerializeField] private AudioClip _removeAnswerSound;
    [SerializeField] private AudioClip _tickingSound;
    [SerializeField] private AudioClip _expGainSound;

    [SerializeField] private AudioClip _soundButton02;
    [SerializeField] private AudioClip _soundEgg;
    [SerializeField] private AudioClip _soundGrabItem;
    [SerializeField] private AudioClip _soundLiquid;
    [SerializeField] private AudioClip _soundMagicTransition;
    [SerializeField] private AudioClip _soundNotification;
    [SerializeField] private AudioClip _soundSand;
    [SerializeField] private AudioClip _soundKnife;
    [SerializeField] private AudioClip _soundKurtCooked;
    [SerializeField] private AudioClip _soundBoilPot;
    [SerializeField] private AudioClip _soundBoilCauldron;
    [SerializeField] private AudioClip _soundRollingPin;
    [SerializeField] private AudioClip _soundOnion;
    [SerializeField] private AudioClip _soundOrderCompleted;
    [SerializeField] private AudioClip _soundReciepe;
    [SerializeField] private AudioClip _soundDoughCutted;
    [SerializeField] private AudioClip _soundMeatCutted;
    [SerializeField] private AudioClip _soundMeatPut;
    [SerializeField] private AudioClip _soundBoilPotMilk;
    [SerializeField] private AudioClip _soundPutInWater;

    [Header("Sound Effects Game Start")]
    [SerializeField]
    private AudioClip _playerCreation;

    [Space, SerializeField] private AudioClip _tip_1;
    [SerializeField] private AudioClip _tip_2;
    [SerializeField] private AudioClip _tip_3;
    [SerializeField] private AudioClip _tip_4;

    [Header("Weapons audio")]
    [Space, SerializeField] private AudioClip spearAudio;
    [SerializeField] private AudioClip axeAudio;
    [SerializeField] private AudioClip swordAudio;
    [SerializeField] private AudioClip bowAudio;
    [SerializeField] private AudioClip mazeAudio;
    [SerializeField] private AudioClip findMissingPartAudio;

    [Header("Sharp audio")]
    [Space, SerializeField] private AudioClip oldManStartHint;
    [SerializeField] private AudioClip oldManRightAnswer;
    [SerializeField] private AudioClip boyEndPhrase;
    [SerializeField] private AudioClip oldManEndPhrase;
    [SerializeField] private AudioClip blueTableHint;
    [SerializeField] private AudioClip akshaAudio;
    [SerializeField] private AudioClip bailykAudio;
    [SerializeField] private AudioClip tamakAudio;
    [SerializeField] private AudioClip kysmetAudio;
    [SerializeField] private AudioClip izAudio;
    [SerializeField] private AudioClip zhyzAudio;
    [SerializeField] private AudioClip girlEndPhrase;

    [Header("Bozok intro audio")]
    [Space, SerializeField] private AudioClip tazyStartTip;
    [SerializeField] private AudioClip toEnterTip;
    [SerializeField] private AudioClip sunTip;
    [SerializeField] private AudioClip horsemanTip;
    [SerializeField] private AudioClip huntTip;
    [SerializeField] private AudioClip natureTip;
    [SerializeField] private AudioClip freedomTip;

    [Header("Bozok audio")]
    [Space, SerializeField] private AudioClip tazyChooseGame;
    [SerializeField] private AudioClip tazyBall;
    [SerializeField] private AudioClip tazyBone;
    [SerializeField] private AudioClip tazyBow;
    [SerializeField] private AudioClip tazyHorse;

    [Header("Music")][SerializeField] private AudioClip _musicIntro;
    [SerializeField] private AudioClip _musicCooking;
    [SerializeField] private AudioClip _musicAlmaty;
    [SerializeField] private AudioClip _musicAstana;
    [SerializeField] private AudioClip _musicExpo;
    [SerializeField] private AudioClip _musicAqorda;
    [SerializeField] private AudioClip _musicBaiterek;
    [SerializeField] private AudioClip _musicBeibitishik;
    [SerializeField] private AudioClip _musicKekTobe;
    [SerializeField] private AudioClip _musicMedey;
    [SerializeField] private AudioClip _musicKonak;
    [SerializeField] private AudioClip _musicHanChatyr;
    [SerializeField] private AudioClip _musicSpear;
    [SerializeField] private AudioClip _musicAxe;
    [SerializeField] private AudioClip _musicBow;
    [SerializeField] private AudioClip _musicSword;
    [SerializeField] private AudioClip _musicMaze;

    private float musicMultiplier = 1;

    private StateManager _stateManager;
    public float VolumeVoicesSounds { get; set; } = 1;

    public void Initialize(StateManager stateManager, CookingViewManager cookingViewManager,
        DragDropManager dragDropManager, HUDManager hudManager)
    {
        _stateManager = stateManager;
        _stateManager.Language = Language.Kazakh;

        StateManager.StateChanged += UpdateVolume;
        dragDropManager.OnIngredientGrabbed += () => PlaySound(_soundGrabItem);
        cookingViewManager.OnIngredientAdded += PlayIngredientSound;
        HUDManager.OnButtonClick += PlayButtonSound;
        UpdateVolume();
    }

    public void PlayButtonSound()
    {
        PlaySound(_soundButton);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            if (_effectsAudioSource.isPlaying)
            {
                _effectsAudioSource.Stop();
            }

            _effectsAudioSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundLouder(AudioClip clip, float volumeScale = 2)
    {
        if (clip != null)
        {
            if (_effectsAudioSource.isPlaying)
            {
                _effectsAudioSource.Stop();
            }

            _effectsAudioSource.PlayOneShot(clip, volumeScale: volumeScale);
        }
    }

    public bool IsVoicePlaying() => _voiceAudioSource.isPlaying;

    public void PlayVoice(AudioClip clip)
    {
        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
            {
                _voiceAudioSource.Stop();
            }

            _voiceAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayVoiceLouder(AudioClip clip, float volumeScale = 2)
    {
        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
            {
                _voiceAudioSource.Stop();
            }

            _voiceAudioSource.PlayOneShot(clip, volumeScale: volumeScale);
        }
    }

    public void StopVoice()
    {
        if (_voiceAudioSource.isPlaying)
        {
            _voiceAudioSource.Stop();
        }
    }

    public void StopEffects()
    {
        if (_effectsAudioSource.isPlaying)
        {
            _effectsAudioSource.Stop();
        }
    }

    public AudioSource PlayMusic(AudioClip musicClip, float volume = 1)
    {
        if (_musicAudioSource.isPlaying && musicClip != null && musicClip != _musicAudioSource.clip)
        {
            _musicAudioSource.Stop();
        }
        musicMultiplier = volume;
        _musicAudioSource.clip = musicClip;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();

        return _musicAudioSource;
    }

    public void StopMusic() => _musicAudioSource.Stop();

    public AudioSource GetMusicAudioSource() { return _musicAudioSource; }

    private void PlayIngredientSound(IngredientType ingredient)
    {
        switch (ingredient)
        {
            case IngredientType.Egg:
                PlaySound(_soundEgg);
                break;
            case IngredientType.Salt or IngredientType.Sugar or IngredientType.Flour or IngredientType.Yest:
                PlaySound(_soundSand);
                break;
            case IngredientType.Milk or IngredientType.Water or IngredientType.Oil or IngredientType.Kumis:
                PlaySound(_soundLiquid);
                break;
            case IngredientType.KurutBoiled:
                PlaySound(_soundKurtCooked);
                break;
        }
    }

    private void UpdateVolume()
    {
        _musicAudioSource.volume = Mathf.Clamp(_stateManager.MusicVolume / 10f * musicMultiplier, 0f, 1f);
        _effectsAudioSource.volume = Mathf.Clamp(_stateManager.SoundVolume / 10f, 0f, 1f);
        //_voiceAudioSource.volume = _effectsAudioSource.volume;

        if (_stateManager.Language == Language.Russian)
        {
            _voiceAudioSource.volume = 0;
        }
        else
        {
            if (_musicAudioSource.volume == 0)
            {
                _voiceAudioSource.volume = 0;
            }
            else
            {
                _voiceAudioSource.volume = 1;
            }
        }

        VolumeVoicesSounds = _voiceAudioSource.volume;
    }

    public void OnSceneChanged(GameScene scene)
    {
        PlaySound(_soundMagicTransition);

        if (_effectsAudioSource.isPlaying)
        {
            _effectsAudioSource.Stop();
        }

        if (scene == GameScene.ChooseCharacter)
        {
            PlayVoice(_playerCreation);
            PlayMusic(_musicIntro);
        }
        else if (scene == GameScene.ChooseCity)
        {
            //PlayVoice(_cityChoose_1);
            PlayMusic(_musicIntro);

            // if (_cityChoose_1 != null)
            // {
            //     PlaySoundWithDelay(_cityChoose_2, _cityChoose_1.length);
            // }
        }
        else if (scene == GameScene.Cooking)
        {
            PlayMusic(_musicCooking);
        }
        else if (scene == GameScene.Astana)
        {
            PlayAstanaMusic();
        }
        else if (scene == GameScene.Almaty)
        {
            PlayAlmatyMusic();
        }
    }

    public void PlayAstanaMusic() => PlayMusic(_musicAstana);
    public void PlayAlmatyMusic() => PlayMusic(_musicAlmaty);

    private void PlaySoundWithDelay(AudioClip clip, float delay)
    {
        if (clip != null)
        {
            StartCoroutine(PlaySoundCoroutine(clip, delay));
        }
    }

    private IEnumerator PlaySoundCoroutine(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_voiceAudioSource.isPlaying)
        {
            _voiceAudioSource.Stop();
        }

        _voiceAudioSource.PlayOneShot(clip);
    }

    public void OnTutorialSound(int tutorialCount)
    {
        switch (tutorialCount)
        {
            case 1:
                PlayVoice(_tip_1);
                break;
            case 2:
                PlayVoice(_tip_2);
                break;
            case 3:
                PlayVoice(_tip_3);
                break;
            case 4:
                PlayVoice(_tip_4);
                break;
        }
    }

    public void PlayFiveWeaponsInstructionsAudio()
    {
        PlayVoiceLouder(findMissingPartAudio, 3);
    }

    public void PlayFiveWeaponsAudio(int weaponIndex)
    {
        switch (weaponIndex)
        {
            case 1:
                PlayVoiceLouder(spearAudio, volumeScale: 3f);
                break;
            case 2:
                PlayVoiceLouder(swordAudio, volumeScale: 3f);
                break;
            case 3:
                PlayVoiceLouder(axeAudio, volumeScale: 3f);
                break;
            case 4:
                PlayVoiceLouder(bowAudio, volumeScale: 3f);
                break;
            case 5:
                PlayVoiceLouder(mazeAudio, volumeScale: 3f);
                break;
        }
    }

    public void PlaySharpAnswerAudio(int wordIndex)
    {
        switch (wordIndex)
        {
            case 1:
                PlayVoiceLouder(akshaAudio);
                break;
            case 2:
                PlayVoiceLouder(bailykAudio);
                break;
            case 3:
                PlayVoiceLouder(tamakAudio);
                break;
            case 4:
                PlayVoiceLouder(kysmetAudio);
                break;
            case 5:
                PlayVoiceLouder(izAudio);
                break;
            case 6:
                PlayVoiceLouder(zhyzAudio);
                break;
        }
    }

    public void PlaySharpOldManStartHintAudio()
    {
        PlayVoiceLouder(oldManStartHint, volumeScale: 6);
    }

    public void PlaySharpOldManRightAnswerSound()
    {
        PlaySoundLouder(oldManRightAnswer, volumeScale: 4);
    }

    public void PlaySharpOldManEndPhraseAudio()
    {
        PlayVoiceLouder(oldManEndPhrase);
    }

    public void PlaySharpBoyEndPhraseAudio()
    {
        PlayVoiceLouder(boyEndPhrase);
    }

    public void PlaySharpGirlEndPhraseAudio()
    {
        PlayVoiceLouder(girlEndPhrase);
    }

    public void PlayBlueTableHintAudio()
    {
        PlayVoiceLouder(blueTableHint);
    }

    public void PlayTazyStartTipAudio()
    {
        PlayVoiceLouder(tazyStartTip);
    }

    public void PlayToEnterTipAudio()
    {
        PlayVoiceLouder(toEnterTip);
    }

    public void PlayBozokIntroCardAudio(int cardIndex)
    {
        switch (cardIndex)
        {
            case 1:
                PlayVoiceLouder(sunTip);
                break;
            case 2:
                PlayVoiceLouder(horsemanTip);
                break;
            case 3:
                PlayVoiceLouder(huntTip);
                break;
            case 4:
                PlayVoiceLouder(natureTip);
                break;
            case 5:
                PlayVoiceLouder(freedomTip);
                break;
        }
    }

    public void PlayTazyChooseGameAudio()
    {
        PlayVoiceLouder(tazyChooseGame);
    }

    public void PlayBozokGameAudio(int gameIndex)
    {
        switch (gameIndex)
        {
            case 1:
                PlayVoiceLouder(tazyBall);
                break;
            case 2:
                PlayVoiceLouder(tazyBone);
                break;
            case 3:
                PlayVoiceLouder(tazyBow);
                break;
            case 4:
                PlayVoiceLouder(tazyHorse);
                break;
        }
    }

    public void PlayMusicSpear()
    {
        PlayMusic(_musicSpear);
    }

    public void PlaySoundSpearWin()
    {
        PlaySound(_spearWinSound);
    }

    public void PlayMusicAxe()
    {
        PlayMusic(_musicAxe);
    }

    public void PlaySoundAxeWin()
    {
        PlaySound(_axeWinSound);
    }

    public void PlayMusicSword()
    {
        PlayMusic(_musicSword);
    }

    public void PlaySoundSwordWin()
    {
        PlaySound(_swordWinSound);
    }

    public void PlayMusicBow()
    {
        PlayMusic(_musicBow);
    }

    public void PlaySoundBowWin()
    {
        PlaySound(_bowWinSound);
    }

    public void PlayMusicMaze()
    {
        PlayMusic(_musicMaze);
    }

    public void PlaySoundMazeWin()
    {
        PlaySound(_mazeWinSound);
    }

    public void PlaySoundWeaponPart()
    {
        PlaySound(_weaponPartSound);
    }

    public void PlayMusicQuizBayterek()
    {
        PlayMusic(_musicBaiterek);
    }

    public void PlayMusicQuizShatyr()
    {
        PlayMusic(_musicHanChatyr);
    }

    public void PlayMusicQuizExpo()
    {
        PlayMusic(_musicExpo);
    }

    public void PlayMusicQuizAkorda()
    {
        PlayMusic(_musicAqorda);
    }

    public void PlayMusicQuizKekTobe()
    {
        PlayMusic(_musicKekTobe);
    }

    public void PlayMusicQuizMedey()
    {
        PlayMusic(_musicMedey);
    }

    public void PlayMusicQuizKonak()
    {
        PlayMusic(_musicKonak);
    }

    public void PlayMusicQuizBeybitishlik()
    {
        PlayMusic(_musicBeibitishik);
    }

    public void PlayEnterCitySound()
    {
        PlaySound(_enterCitySound);
    }

    public void PlayLogoSound()
    {
        PlaySound(_logoSound);
    }

    public void PlayTipAppearSound()
    {
        PlaySound(_tipAppearSound);
    }

    public void PlayDefaultButtonSound()
    {
        PlaySound(_defaultButtonSound);
    }

    public void PlayUnavailableButtonSound()
    {
        PlaySound(_unavailableButtonSound);
    }

    public void PlayEnergyRefillButtonSound()
    {
        PlaySound(_energyRefillButtonSound);
    }

    public void PlayWrongAnswerSound()
    {
        PlaySound(_wrongAnswerSound);
    }

    public void PlayRightAnswerSound()
    {
        PlaySound(_rightAnswerSound);
    }

    public void PlayRemoveAnswerSound()
    {
        PlaySound(_removeAnswerSound);
    }

    public void PlayTickingSound()
    {
        PlaySound(_tickingSound);
    }

    public void PlayExpGainSound()
    {
        PlaySound(_expGainSound);
    }

    // public float GetCityChooseOneSoundLength()
    // {
    //     return _cityChoose_1.length;
    // }

    // public void PlayChooseCitySecondPartSound()
    // {
    //     PlayVoice(_cityChoose_2);
    // }

    public void PlayRevealSound()
    {
        PlaySound(_soundMagicTransition);
    }

    public void PlayKnifeSound()
    {
        PlaySound(_soundKnife);
    }

    public void PlayBoilPotSound()
    {
        PlaySound(_soundBoilPotMilk);
    }

    public void PlayBoilCauldronSound()
    {
        PlaySound(_soundBoilCauldron);
    }

    public void PlayCookPotSound()
    {
        PlaySound(_soundBoilPot);
    }

    public void PlayPutInMeatSound()
    {
        PlaySound(_soundMeatPut);
    }

    public void PlayMeatCutSound()
    {
        PlaySound(_soundMeatCutted);
    }

    public void PlayDoughCutSound()
    {
        PlaySound(_soundDoughCutted);
    }

    public void PlayRollingPinSound()
    {
        PlaySound(_soundRollingPin);
    }

    public void PlaySoundOnion()
    {
        PlaySound(_soundOnion);
    }

    public void PlayOrderCompletedSound()
    {
        PlaySound(_soundOrderCompleted);
    }

    public void PlayPutInWaterSound()
    {
        PlaySound(_soundPutInWater);
    }

    public void PlayReciepeSound()
    {
        PlaySound(_soundReciepe);
    }

    public void PlayWrongAttemptSound()
    {
        PlaySound(_soundWrongAttempt);
    }
}