using System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class CutSoundPatch : IInitializable, IDisposable
    {
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly PluginConfig _config;

        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customCutSound = new AudioClip[1];

        private readonly AudioClip[] _originalLongCutSounds;
        private readonly AudioClip[] _originalShortCutSounds;

        private string? _lastCutSoundSelected;

        private CutSoundPatch(NoteCutSoundEffectManager noteCutSoundEffectManager, PluginConfig config)
        {
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _config = config;
            _originalShortCutSounds = noteCutSoundEffectManager._shortCutEffectsAudioClips;
            _originalLongCutSounds = noteCutSoundEffectManager._longCutEffectsAudioClips;
            _customCutSound[0] = _emptySound;
        }

        public void Initialize()
        {
            if (_config.GoodHitSound == SoundLoader.NoSoundID)
            {
                _customCutSound[0] = _emptySound;
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _customCutSound;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _customCutSound;
            }
            else if (_config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _originalShortCutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                _customCutSound[0] = GetCustomCutSound();
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _customCutSound;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _customCutSound;
            }
        }

        private AudioClip GetCustomCutSound()
        {
            if (_lastCutSoundSelected == _config.GoodHitSound)
            {
                return _customCutSound[0];
            }
            _lastCutSoundSelected = _config.GoodHitSound;

            var cutSound = SoundLoader.LoadAudioClip(_config.GoodHitSound);
            return cutSound != null ? cutSound : _emptySound;
        }

        public void Dispose()
        {
            if (_customCutSound[0] != null && _customCutSound[0] != _emptySound)
            {
                Object.Destroy(_customCutSound[0]);
            }
        }
    }
}
