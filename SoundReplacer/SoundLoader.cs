using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace SoundReplacer
{
    internal class SoundLoader : IDisposable
    {
        public const string NoSoundID = "None";
        public const string DefaultSoundID = "Default";
        public static readonly string[] DefaultSounds = { NoSoundID, DefaultSoundID };
        // Duration of 1 second as NoteCutSoundEffect could disable itself before the note is cut otherwise.
        public static readonly AudioClip Empty = AudioClip.Create("Empty", 44100, 1, 44100, false);

        private readonly PluginConfig _config;
        private readonly Dictionary<string, AudioClip> _cache = new();

        private SoundLoader(PluginConfig config)
        {
            _config = config;
        }

        private static AudioType GetAudioTypeFromPath(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.OGGVORBIS;
            }
            if (extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.MPEG;
            }
            if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.WAV;
            }

            return AudioType.UNKNOWN;
        }

        private static AudioClip? LoadAudioClip(string fileName)
        {
            var filePath = Directory.EnumerateFiles(Path.Combine(UnityGame.UserDataPath, nameof(SoundReplacer)), fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (filePath is null)
            {
                Plugin.Log.Error($"Could not find sound {fileName}.");
                return null;
            }

            var request = UnityWebRequestMultimedia.GetAudioClip(FileHelpers.GetEscapedURLForFilePath(filePath), GetAudioTypeFromPath(filePath));
            var task = request.SendWebRequest();

            // while I would normally kill people for this
            // we are loading a local file, so it should be
            // basically instant success or error
            while (!task.isDone) { }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Plugin.Log.Error($"Failed to load file {filePath} with error {request.error}.");
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(request);
        }

        private string GetSoundFileName(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Cut => _config.CutSound,
                SoundType.BadCut => _config.BadCutSound,
                SoundType.Menu => _config.MenuMusic,
                SoundType.Click => _config.ClickSound,
                SoundType.LevelCleared => _config.LevelClearedSound,
                SoundType.LevelFailed => _config.LevelFailedSound,
                _ => throw new ArgumentOutOfRangeException(nameof(soundType))
            };
        }

        public AudioClip Load(AudioClip? currentSound, SoundType soundType)
        {
            var fileName = GetSoundFileName(soundType);
            if (_cache.TryGetValue(fileName, out var cachedSound) && cachedSound == currentSound)
            {
                return currentSound;
            }

            Object.Destroy(cachedSound);

            var customSound = LoadAudioClip(fileName);
            if (customSound == null)
            {
                return Empty;
            }

            return _cache[fileName] = customSound;
        }

        public void Unload(SoundType soundType)
        {
            if (_cache.TryGetValue(GetSoundFileName(soundType), out var cachedSound))
            {
                Object.Destroy(cachedSound);
            }
        }

        public void Dispose()
        {
            foreach (var audioClip in _cache.Values)
            {
                Object.Destroy(audioClip);
            }
        }
    }
}
