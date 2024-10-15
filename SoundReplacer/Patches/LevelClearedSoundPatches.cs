using SiraUtil.Affinity;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class LevelClearedSoundPatches : IAffinity, IDisposable
    {
        private readonly ResultsViewController _resultsViewController;
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip _originalLevelClearedSound;

        private AudioClip _customLevelClearedSound;
        private string? _lastClearedSoundSelected;

        private AudioClip _customLevelFailedSound;
        private string? _lastFailedSoundSelected;

        private LevelClearedSoundPatches(ResultsViewController resultsViewController, SongPreviewPlayer songPreviewPlayer)
        {
            _resultsViewController = resultsViewController;
            _songPreviewPlayer = songPreviewPlayer;
            _originalLevelClearedSound = resultsViewController._levelClearedAudioClip;
            _customLevelClearedSound = _emptySound;
            _customLevelFailedSound = _emptySound;
        }

        private AudioClip GetCustomLevelClearedSound()
        {
            if (_lastClearedSoundSelected == Plugin.Config.SuccessSound)
            {
                return _customLevelClearedSound;
            }
            _lastClearedSoundSelected = Plugin.Config.SuccessSound;

            if (_customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelClearedSound);
            }

            var levelClearedSound = SoundLoader.LoadAudioClip(Plugin.Config.SuccessSound);
            return levelClearedSound != null ? levelClearedSound : _emptySound;
        }

        private AudioClip GetCustomLevelFailedSound()
        {
            if (_lastFailedSoundSelected == Plugin.Config.FailSound)
            {
                return _customLevelFailedSound;
            }
            _lastFailedSoundSelected = Plugin.Config.FailSound;

            if (_customLevelFailedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }

            var levelFailedSound = SoundLoader.LoadAudioClip(Plugin.Config.FailSound);
            return levelFailedSound != null ? levelFailedSound : _emptySound;
        }

        [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.DidActivate))]
        [AffinityPrefix]
        public void PlayCustomLevelFinishedSound()
        {
            // This changes the sound that gets played when there's a new personal best
            // It may be preferable to instead play the custom sound separately
            _resultsViewController._levelClearedAudioClip = Plugin.Config.SuccessSound switch
            {
                SoundLoader.NoSoundID => _emptySound,
                SoundLoader.DefaultSoundID => _originalLevelClearedSound,
                _ => _customLevelClearedSound = GetCustomLevelClearedSound()
            };

            if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed 
                && Plugin.Config.FailSound != SoundLoader.DefaultSoundID)
            {
                var failSound = Plugin.Config.FailSound switch
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
