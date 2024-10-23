using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class LevelClearedSoundPatch : IAffinity, IDisposable
    {
        private readonly ResultsViewController _resultsViewController;
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly PluginConfig _config;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip _originalLevelClearedSound;

        private AudioClip _customLevelClearedSound;
        private string? _lastClearedSoundSelected;

        private AudioClip _customLevelFailedSound;
        private string? _lastFailedSoundSelected;

        private LevelClearedSoundPatch(ResultsViewController resultsViewController, SongPreviewPlayer songPreviewPlayer, PluginConfig config)
        {
            _resultsViewController = resultsViewController;
            _songPreviewPlayer = songPreviewPlayer;
            _config = config;
            _originalLevelClearedSound = resultsViewController._levelClearedAudioClip;
            _customLevelClearedSound = _emptySound;
            _customLevelFailedSound = _emptySound;
        }

        private AudioClip GetCustomLevelClearedSound()
        {
            if (_lastClearedSoundSelected == _config.SuccessSound)
            {
                return _customLevelClearedSound;
            }
            _lastClearedSoundSelected = _config.SuccessSound;

            if (_customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelClearedSound);
            }

            var levelClearedSound = SoundLoader.LoadAudioClip(_config.SuccessSound);
            return levelClearedSound != null ? levelClearedSound : _emptySound;
        }

        private AudioClip GetCustomLevelFailedSound()
        {
            if (_lastFailedSoundSelected == _config.FailSound)
            {
                return _customLevelFailedSound;
            }
            _lastFailedSoundSelected = _config.FailSound;

            if (_customLevelFailedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }

            var levelFailedSound = SoundLoader.LoadAudioClip(_config.FailSound);
            return levelFailedSound != null ? levelFailedSound : _emptySound;
        }

        [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.DidActivate))]
        [AffinityPrefix]
        public void PlayCustomLevelFinishedSound()
        {
            // This changes the sound that gets played when there's a new personal best
            // It may be preferable to instead play the custom sound separately
            _resultsViewController._levelClearedAudioClip = _config.SuccessSound switch
            {
                SoundLoader.NoSoundID => _emptySound,
                SoundLoader.DefaultSoundID => _originalLevelClearedSound,
                _ => _customLevelClearedSound = GetCustomLevelClearedSound()
            };

            if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed
                && _config.FailSound != SoundLoader.DefaultSoundID)
            {
                var failSound = _config.FailSound switch
                {
                    SoundLoader.NoSoundID => _emptySound,
                    _ => _customLevelFailedSound = GetCustomLevelFailedSound()
                };
                _songPreviewPlayer.CrossfadeTo(failSound, -4f, 0f, failSound.length, null);
            }
        }

        public void Dispose()
        {
            if (_customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelClearedSound);
            }

            if (_customLevelFailedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }
        }
    }
}
