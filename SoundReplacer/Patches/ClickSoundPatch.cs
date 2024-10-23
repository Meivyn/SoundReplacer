using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class ClickSoundPatch : IAffinity, IDisposable
    {
        private readonly PluginConfig _config;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private AudioClip[]? _originalClickSounds;
        private readonly AudioClip[] _customClickSounds = new AudioClip[1];
        private string? _lastClickSelected;

        private ClickSoundPatch(PluginConfig config)
        {
            _config = config;
            _customClickSounds[0] = _emptySound;
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            if (_config.ClickSound == SoundLoader.NoSoundID)
            {
                _customClickSounds[0] = _emptySound;
                __instance._clickSounds = _customClickSounds;
            }
            else if (_config.ClickSound == SoundLoader.DefaultSoundID)
            {
                __instance._clickSounds = _originalClickSounds;
            }
            else
            {
                _customClickSounds[0] = GetCustomClickSound();
                __instance._clickSounds = _customClickSounds;
            }
        }

        private AudioClip GetCustomClickSound()
        {
            if (_lastClickSelected == _config.ClickSound)
            {
                return _customClickSounds[0];
            }
            _lastClickSelected = _config.ClickSound;

            if (_customClickSounds[0] != _emptySound)
            {
                Object.Destroy(_customClickSounds[0]);
            }

            var clickSound = SoundLoader.LoadAudioClip(_config.ClickSound);
            return clickSound != null ? clickSound : _emptySound;
        }

        public void Dispose()
        {
            if (_customClickSounds[0] != _emptySound)
            {
                Object.Destroy(_customClickSounds[0]);
            }
        }
    }
}