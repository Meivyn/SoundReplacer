using System;
using System.IO;
using System.Linq;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace SoundReplacer
{
    internal static class SoundLoader
    {
        public const string NoSoundID = "None";
        public const string DefaultSoundID = "Default";
        public static readonly string[] DefaultSoundList = { NoSoundID, DefaultSoundID };

        public static string[] SoundList = DefaultSoundList;
        private static AudioClip? _emptyAudioClip;

        public static void PopulateSoundList()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, nameof(SoundReplacer)));
                directoryInfo.Create();
                SoundList = SoundList
                    .Concat(directoryInfo
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Where(f => f.Extension is ".ogg" or ".mp3" or ".wav")
                        .Select(f => f.Name))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Could not create sound list. {ex}");
            }
        }

        private static string GetFullPath(string name)
        {
            return Path.Combine(UnityGame.UserDataPath, nameof(SoundReplacer), name);
        }

        private static AudioType GetAudioTypeFromPath(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension switch
            {
                _ when extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase) => AudioType.OGGVORBIS,
                _ when extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) => AudioType.MPEG,
                _ when extension.Equals(".wav", StringComparison.OrdinalIgnoreCase) => AudioType.WAV,
                _ => AudioType.UNKNOWN
            };
        }

        private static void SetConfigToDefault(string configName)
        {
            var currentConfig = Plugin.Config;
            foreach (var fieldInfo in currentConfig.GetType().GetFields())
            {
                if ((string)fieldInfo.GetValue(currentConfig) == configName)
                {
                    fieldInfo.SetValue(currentConfig, DefaultSoundID);
                    break;
                }
            }
        }

        public static AudioClip? LoadAudioClip(string name)
        {
            var fullPath = GetFullPath(name);
            var request = UnityWebRequestMultimedia.GetAudioClip(FileHelpers.GetEscapedURLForFilePath(fullPath), GetAudioTypeFromPath(fullPath));
            var task = request.SendWebRequest();

            // while I would normally kill people for this
            // we are loading a local file, so it should be
            // basically instant success or error
            while (!task.isDone) { }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Plugin.Log.Error($"Failed to load file {fullPath} with error {request.error}");
                SetConfigToDefault(name);

                return null;
            }

            return DownloadHandlerAudioClip.GetContent(request);
        }

        public static AudioClip GetEmptyAudioClip()
        {
            if (_emptyAudioClip != null)
            {
                return _emptyAudioClip;
            }

            // Duration of 1 second as NoteCutSoundEffect could disable itself before the note is cut otherwise.
            return _emptyAudioClip = AudioClip.Create("Empty", 44100, 1, 44100, false);
        }
    }
}
