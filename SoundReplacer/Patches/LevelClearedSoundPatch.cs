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
            if (_lastClearedSoundSelected == _config.LevelClearedSound)
            {
                return _customLevelClearedSound;
            }
            _lastClearedSoundSelected = _config.LevelClearedSound;

            if (_customLevelClearedSound != _emptySound)
            {
                Object.Destroy(_customLevelClearedSound);
            }

            var levelClearedSound = SoundLoader.LoadAudioClip(_config.LevelClearedSound);
            return levelClearedSound != null ? levelClearedSound : _emptySound;
        }

        private AudioClip GetCustomLevelFailedSound()
        {
            if (_lastFailedSoundSelected == _config.LevelFailedSound)
            {
                return _customLevelFailedSound;
            }
            _lastFailedSoundSelected = _config.LevelFailedSound;

            if (_customLevelFailedSound != _emptySound)
            {
                Object.Destroy(_customLevelFailedSound);
            }

            var levelFailedSound = SoundLoader.LoadAudioClip(_config.LevelFailedSound);
            return levelFailedSound != null ? levelFailedSound : _emptySound;
        }

        [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.DidActivate))]
        [AffinityPrefix]
        public void PlayCustomLevelFinishedSound()
        {
            // This changes the sound that gets played when there's a new personal best
            // It may be preferable to instead play the custom sound separately
            _resultsViewController._levelClearedAudioClip = _config.LevelClearedSound switch
            {
                SoundLoader.NoSoundID => _emptySound,
                SoundLoader.DefaultSoundID => _originalLevelClearedSound,
                _ => _customLevelClearedSound = GetCustomLevelClearedSound()
            };

            if (_resultsViewController._levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed
                && _config.LevelFailedSound != SoundLoader.DefaultSoundID && _config.LevelFailedSound != SoundLoader.NoSoundID)
            {
                _customLevelFailedSound = GetCustomLevelFailedSound();
                _songPreviewPlayer.CrossfadeTo(_customLevelFailedSound, -4f, 0f, _customLevelFailedSound.length, null);
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
