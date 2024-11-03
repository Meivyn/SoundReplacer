using System;
using UnityEngine;
using Zenject;

namespace SoundReplacer.Patches
{
    internal class CutSoundPatch : IInitializable, IDisposable
    {
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip[] _cutSounds = new AudioClip[1];
        private readonly AudioClip[] _originalLongCutSounds;
        private readonly AudioClip[] _originalShortCutSounds;

        private CutSoundPatch(NoteCutSoundEffectManager noteCutSoundEffectManager, SoundLoader soundLoader, PluginConfig config)
        {
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _soundLoader = soundLoader;
            _config = config;
            _originalShortCutSounds = noteCutSoundEffectManager._shortCutEffectsAudioClips;
            _originalLongCutSounds = noteCutSoundEffectManager._longCutEffectsAudioClips;
        }

        public void Initialize()
        {
            if (_config.CutSound == SoundLoader.NoSoundID)
            {
                _cutSounds[0] = SoundLoader.Empty;
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _cutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _cutSounds;
            }
            else if (_config.CutSound == SoundLoader.DefaultSoundID)
            {
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _originalShortCutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                _cutSounds[0] = _soundLoader.Load(_cutSounds[0], SoundType.Cut);
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _cutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _cutSounds;
            }
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.Cut);
        }
    }
}
