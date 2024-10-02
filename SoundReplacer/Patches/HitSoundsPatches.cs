using SiraUtil.Affinity;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class HitSoundsPatches : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private readonly AudioClip[] _customBadCutSound = new AudioClip[1];
        private readonly AudioClip[] _customCutSound = new AudioClip[1];

        private AudioClip[]? _originalBadCutSounds;
        private AudioClip[]? _originalLongCutSounds;
        private AudioClip[]? _originalShortCutSounds;

        private string? _lastCutSoundSelected;
        private string? _lastBadCutSoundSelected;

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

        private AudioClip GetCustomCutSound()
        {
            if (_lastCutSoundSelected == Plugin.Config.GoodHitSound)
            {
                return _customCutSound[0];
            }
            _lastCutSoundSelected = Plugin.Config.GoodHitSound;
            var cutSound = SoundLoader.LoadAudioClip(Plugin.Config.GoodHitSound);
            return _customCutSound[0] = cutSound != null ? cutSound : _emptySound;
        }

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
                __instance._noteCutSoundEffectPrefab._badCutSoundEffectAudioClips = _customBadCutSound;
            }
            else if (Plugin.Config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                __instance._noteCutSoundEffectPrefab._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _customBadCutSound[0] = GetCustomBadCutSound();
                __instance._noteCutSoundEffectPrefab._badCutSoundEffectAudioClips = _customBadCutSound;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }

        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.Start))]
        [AffinityPrefix]
        public void ReplaceHitSound(NoteCutSoundEffectManager __instance)
        {
            _originalShortCutSounds ??= __instance._shortCutEffectsAudioClips;
            _originalLongCutSounds ??= __instance._longCutEffectsAudioClips;

            if (Plugin.Config.GoodHitSound == SoundLoader.NoSoundID)
            {
                _customCutSound[0] = _emptySound;
                __instance._shortCutEffectsAudioClips = _customCutSound;
                __instance._longCutEffectsAudioClips = _customCutSound;
            }
            else if (Plugin.Config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                __instance._shortCutEffectsAudioClips = _originalShortCutSounds;
                __instance._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                _customCutSound[0] = GetCustomCutSound();
                __instance._shortCutEffectsAudioClips = _customCutSound;
                __instance._longCutEffectsAudioClips = _customCutSound;
            }
        }

        public void Dispose()
        {
            if (_customBadCutSound[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSound[0]);
            }

            if (_customCutSound[0] != _emptySound)
            {
                Object.Destroy(_customCutSound[0]);
            }
        }
    }
}