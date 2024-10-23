using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class MenuMusicPatches : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly PluginConfig _config;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private readonly AudioClip _originalMenuMusic;
        private readonly AudioClip _originalLobbyMusic;

        private AudioClip _customMenuMusic;
        private string? _lastMusicSelected;

        private MenuMusicPatches(SongPreviewPlayer songPreviewPlayer, GameServerLobbyFlowCoordinator lobbyFlowCoordinator, PluginConfig config)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _config = config;
            _originalMenuMusic = songPreviewPlayer.defaultAudioClip;
            _originalLobbyMusic = lobbyFlowCoordinator._ambienceAudioClip;
            _customMenuMusic = _emptySound;
        }

        private AudioClip GetCustomMenuMusic()
        {
            if (_lastMusicSelected == _config.MenuMusic)
            {
                return _customMenuMusic;
            }
            _lastMusicSelected = _config.MenuMusic;

            if (_customMenuMusic != _emptySound)
            {
                Object.Destroy(_customMenuMusic);
            }

            var menuMusic = SoundLoader.LoadAudioClip(_config.MenuMusic);
            return menuMusic != null ? menuMusic : _emptySound;
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.Start))]
        [AffinityPrefix]
        public void ReplaceMenuMusic()
        {
            // Replace the default menu music on start
            _songPreviewPlayer._defaultAudioClip = _config.MenuMusic switch
            {
                SoundLoader.NoSoundID => _emptySound,
                SoundLoader.DefaultSoundID => _originalMenuMusic,
                _ => _customMenuMusic = GetCustomMenuMusic()
            };
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.CrossfadeToNewDefault))]
        [AffinityPrefix]
        public bool PreventExternalDefaultMenuMusic(AudioClip audioClip)
        {
            // If there is no custom sound in use, use the new default
            if (_songPreviewPlayer.defaultAudioClip != _customMenuMusic && _songPreviewPlayer.defaultAudioClip != _emptySound)
            {
                return true;
            }

            // If the new default is the default menu music, cancel the method
            return audioClip != _originalMenuMusic && audioClip != _originalLobbyMusic;
        }

        public void Dispose()
        {
            if (_customMenuMusic != _emptySound)
            {
                Object.Destroy(_customMenuMusic);
            }
        }
    }
}