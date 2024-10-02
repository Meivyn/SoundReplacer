using SiraUtil.Affinity;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class BadCutSoundPatch : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customBadCutSound = new AudioClip[1];

        private AudioClip[]? _originalBadCutSounds;

        private string? _lastBadCutSoundSelected;

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        public void ReplaceSoundEffectPrefabSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            if (Plugin.Config.GoodHitSound == SoundLoader.NoSoundID)
            {
                _customBadCutSound[0] = _emptySound;
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSound;
            }
            else if (Plugin.Config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                noteCutSoundEffect._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _customBadCutSound[0] = GetCustomBadCutSound();
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSound;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }

        private AudioClip GetCustomBadCutSound()
        {
            if (_lastBadCutSoundSelected == Plugin.Config.BadHitSound)
            {
                return _customBadCutSound[0];
            }
            _lastBadCutSoundSelected = Plugin.Config.BadHitSound;

            var badCutSound = SoundLoader.LoadAudioClip(Plugin.Config.BadHitSound);
            return _customBadCutSound[0] = badCutSound != null ? badCutSound : _emptySound;
        }

        public void Dispose()
        {
            if (_customBadCutSound[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSound[0]);
            }
        }
    }
}