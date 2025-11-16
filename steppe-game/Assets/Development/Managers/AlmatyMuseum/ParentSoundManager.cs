using UnityEngine;
using UnityEngine.UI;

namespace Development.Managers.AlmatyMuseum
{
    public abstract class ParentSoundManager : MonoBehaviour
    {
        private AudioSource _audioSource;
        private StateManager _stateManager;
        private SoundManager _soundManager;
        
        //private Button _backButton;
        
        private void Awake()
        {
            //_backButton = GameObject.FindWithTag("GlobalBackButton").GetComponent<Button>();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _stateManager = FindAnyObjectByType<StateManager>();
            _soundManager = FindAnyObjectByType<SoundManager>();
        }
        
        protected void PlaySound(AudioClip clip)
        {
            //_audioSource.volume = Mathf.Clamp01(_stateManager.SoundVolume/10f);
            //_audioSource.PlayOneShot(clip);
        }

        protected void PlayMusic(AudioClip clip)
        {
            StopPlayingMainMusic();
            _soundManager.PlayMusic(clip);
        }

        protected void PlayVoice(AudioClip clip)
        {
            StopVoice();
            _soundManager.VolumeVoicesSounds = _stateManager.SoundVolume + 5;
            _soundManager.PlayVoiceLouder(clip);
        }

        private void StopVoice() => _soundManager.StopVoice();

        private void StopPlayingMainMusic() => _soundManager.StopMusic();

        private void ResumePlayingMainMusic()
        {
            _soundManager.StopMusic();
            _soundManager.PlayAlmatyMusic();
        }

        protected virtual void OnEnable()
        {
            // _backButton.onClick.AddListener(StopVoice);
            // _backButton.onClick.AddListener(ResumePlayingMainMusic);
        }

        protected virtual void OnDisable()
        {
            StopVoice();
            // _backButton.onClick.RemoveListener(StopVoice);
            // _backButton.onClick.RemoveListener(ResumePlayingMainMusic);
        }
    }
}