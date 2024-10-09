using SiraUtil.Affinity;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class LevelClearedSoundPatches : IAffinity, IDisposable
    {
        private readonly ResultsViewController _resultsViewController;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip _originalLevelClearedSound;

        private AudioClip _customLevelClearedSound;
        private string? _lastClearedSoundSelected;

        private AudioClip _customLevelFailedSound;
        private string? _lastFailedSoundSelected;

        private LevelClearedSoundPatches(ResultsViewController resultsViewController)
        {
            _resultsViewController = resultsViewController;
            _originalLevelClearedSound = resultsViewController._levelClearedAudioClip;
            _customLevelClearedSound = _emptySound;
            _customLevelFailedSound = _emptySound;
        }

        private AudioClip GetCustomLevelClearedAudio()
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
            return _customLevelClearedSound = levelClearedSound != null ? levelClearedSound : _emptySound;
        }

        private AudioClip GetCustomLevelFailedAudio()
        {
            if (_lastFailedSoundSelected == Plugin.Config.FailSound)
            {
                return _customLevelClearedSound;
            }
            _lastFailedSoundSelected = Plugin.Config.FailSound;

            if (_customLevelFailedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }

            var levelFailedSound = SoundLoader.LoadAudioClip(Plugin.Config.FailSound);
            return _customLevelFailedSound = levelFailedSound != null ? levelFailedSound : _emptySound;
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
                _ => GetCustomLevelClearedAudio()
            };

            if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed)
            {
                var failSound = Plugin.Config.SuccessSound switch
                {
                    SoundLoader.NoSoundID or SoundLoader.DefaultSoundID => _emptySound,
                    _ => GetCustomLevelFailedAudio()
                };
                var audioPlayer = _resultsViewController._songPreviewPlayer;
                audioPlayer.CrossfadeTo(failSound, -4f, -1f, failSound.length, null);
            }
        }

        public void Dispose()
        {
            if (_customLevelClearedSound != null && _customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelClearedSound);
            }

            if (_customLevelFailedSound != null && _customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }
        }
    }
}
