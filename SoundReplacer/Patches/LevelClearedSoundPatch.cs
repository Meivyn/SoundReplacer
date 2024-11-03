using System;
using SiraUtil.Affinity;
using UnityEngine;

namespace SoundReplacer.Patches
{
    internal class LevelClearedSoundPatch : IAffinity, IDisposable
    {
        private readonly ResultsViewController _resultsViewController;
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip _originalLevelClearedSound;
        private AudioClip? _levelClearedSound;
        private AudioClip? _levelFailedSound;

        private LevelClearedSoundPatch(ResultsViewController resultsViewController, SongPreviewPlayer songPreviewPlayer, SoundLoader soundLoader, PluginConfig config)
        {
            _resultsViewController = resultsViewController;
            _songPreviewPlayer = songPreviewPlayer;
            _soundLoader = soundLoader;
            _config = config;
            _originalLevelClearedSound = resultsViewController._levelClearedAudioClip;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.LevelCleared);
            _soundLoader.Unload(SoundType.LevelFailed);
        }

        [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.DidActivate))]
        [AffinityPrefix]
        // TODO: I'd rather avoid the bool Prefix but I don't think there's another way...
        private bool PlayLevelClearedSound(bool firstActivation, bool addedToHierarchy)
        {
            if (firstActivation)
            {
                _resultsViewController.buttonBinder.AddBinding(_resultsViewController._restartButton, _resultsViewController.RestartButtonPressed);
                _resultsViewController.buttonBinder.AddBinding(_resultsViewController._continueButton, _resultsViewController.ContinueButtonPressed);
            }

            if (addedToHierarchy)
            {
                _resultsViewController.SetDataToUI();

                if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared
                    && _resultsViewController._newHighScore)
                {
                    _resultsViewController._startFireworksAfterDelayCoroutine = _resultsViewController.StartCoroutine(_resultsViewController.StartFireworksAfterDelay(1.95f));
                    if (_config.LevelClearedSound != SoundLoader.NoSoundID)
                    {
                        // This changes the sound that gets played when there's a new personal best.
                        // It may be preferable to instead play the custom sound separately.
                        _resultsViewController._levelClearedAudioClip = _config.LevelClearedSound switch
                        {
                            SoundLoader.DefaultSoundID => _originalLevelClearedSound,
                            _ => _levelClearedSound = _soundLoader.Load(_levelClearedSound, SoundType.LevelCleared)
                        };
                        _songPreviewPlayer.CrossfadeTo(_resultsViewController._levelClearedAudioClip, -4f, 0f, _resultsViewController._levelClearedAudioClip.length, null);
                    }
                }
                else if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed
                    && _config.LevelFailedSound != SoundLoader.DefaultSoundID && _config.LevelFailedSound != SoundLoader.NoSoundID)
                {
                    _levelFailedSound = _soundLoader.Load(_levelFailedSound, SoundType.LevelFailed);
                    _songPreviewPlayer.CrossfadeTo(_levelFailedSound, -4f, 0f, _levelFailedSound.length, null);
                }

                if (_resultsViewController._menuDestinationRequest != null)
                {
                    _resultsViewController.ProcessMenuDestinationRequest(_resultsViewController._menuDestinationRequest);
                }
            }

            return false;
        }
    }
}
