using System;
using System.Linq;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class ClickSoundPatch : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private AudioClip[]? _originalClickSounds;
        private readonly AudioClip[] _customClickSounds = new AudioClip[1];
        private string? _lastClickSelected;

        private ClickSoundPatch()
        {
            _customClickSounds[0] = _emptySound;
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            if (Plugin.Config.ClickSound == SoundLoader.NoSoundID)
            {
                _customClickSounds[0] = _emptySound;
                __instance._clickSounds = _customClickSounds;
            }
            else if (Plugin.Config.ClickSound == SoundLoader.DefaultSoundID)
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
            if (_lastClickSelected == Plugin.Config.ClickSound)
            {
                return _customClickSounds[0];
            }
            _lastClickSelected = Plugin.Config.ClickSound;

            if (_customClickSounds[0] != _emptySound)
            {
                Object.Destroy(_customClickSounds[0]);
            }

            var clickSound = SoundLoader.LoadAudioClip(Plugin.Config.ClickSound);
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