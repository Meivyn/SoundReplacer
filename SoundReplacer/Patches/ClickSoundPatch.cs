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
                __instance._clickSounds = GetCustomClickSounds();
            }
        }

        private AudioClip[] GetCustomClickSounds()
        {
            if (_lastClickSelected == Plugin.Config.ClickSound)
            {
                return _customClickSounds;
            }
            _lastClickSelected = Plugin.Config.ClickSound;

            if (_customClickSounds[0] != _emptySound)
            {
                Object.Destroy(_customClickSounds[0]);
            }

            var clickSound = SoundLoader.LoadAudioClip(Plugin.Config.ClickSound);
            _customClickSounds[0] = clickSound != null ? clickSound : _emptySound;
            return _customClickSounds;
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