using SiraUtil.Affinity;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class BadCutSoundPatch : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customBadCutSounds = new AudioClip[1];

        private AudioClip[]? _originalBadCutSounds;

        private string? _lastBadCutSoundSelected;

        private BadCutSoundPatch()
        {
            _customBadCutSounds[0] = _emptySound;
        }

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        public void ReplaceSoundEffectPrefabSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            if (Plugin.Config.BadHitSound == SoundLoader.NoSoundID)
            {
                _customBadCutSounds[0] = _emptySound;
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSounds;
            }
            else if (Plugin.Config.BadHitSound == SoundLoader.DefaultSoundID)
            {
                noteCutSoundEffect._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _customBadCutSounds[0] = GetCustomBadCutSound();
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSounds;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }

        private AudioClip GetCustomBadCutSound()
        {
            if (_lastBadCutSoundSelected == Plugin.Config.BadHitSound)
            {
                return _customBadCutSounds[0];
            }
            _lastBadCutSoundSelected = Plugin.Config.BadHitSound;

            if (_customBadCutSounds[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSounds[0]);
            }

            var badCutSound = SoundLoader.LoadAudioClip(Plugin.Config.BadHitSound);
            return badCutSound != null ? badCutSound : _emptySound;
        }

        public void Dispose()
        {
            if (_customBadCutSounds[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSounds[0]);
            }
        }
    }
}