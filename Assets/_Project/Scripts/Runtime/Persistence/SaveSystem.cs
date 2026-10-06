using System;
using System.IO;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Reads and writes the save file. Writes go to a temporary file first and the previous save is kept
    /// as a backup, so a crash mid-write never destroys progress.
    /// </summary>
    public sealed class SaveSystem
    {
        public const string FileName = "anvil_save.json";

        readonly SaveSerializer _serializer;
        readonly IClock _clock;

        public SaveSystem(string directory, SaveSerializer serializer, IClock clock)
        {
            if (string.IsNullOrEmpty(directory)) throw new ArgumentException("Save directory is required.", nameof(directory));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Directory = directory;
            SavePath = Path.Combine(directory, FileName);
        }

        public static string DefaultDirectory => Application.persistentDataPath;

        public string Directory { get; }

        public string SavePath { get; }

        string BackupPath => SavePath + ".bak";
        string TempPath => SavePath + ".tmp";
        string CorruptPath => SavePath + ".corrupt";

        public bool HasSave => File.Exists(SavePath) || File.Exists(BackupPath);

        public void Save(GameState state)
        {
            var json = _serializer.Serialize(state, _clock.UtcNow, Application.version);

            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(TempPath, json);

            if (File.Exists(SavePath))
            {
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                File.Move(SavePath, BackupPath);
            }
            File.Move(TempPath, SavePath);
        }

        /// <summary>Loads the save, falling back to the backup. A corrupt main file is set aside, never deleted.</summary>
        public bool TryLoad(out SaveEnvelope envelope)
        {
            if (TryRead(SavePath, out envelope, out var error)) return true;

            if (error != null)
            {
                Debug.LogWarning($"[Save] '{SavePath}' could not be loaded ({error.Message}). Keeping it as '{CorruptPath}'.");
                if (File.Exists(CorruptPath)) File.Delete(CorruptPath);
                File.Move(SavePath, CorruptPath);
            }

            if (TryRead(BackupPath, out envelope, out var backupError))
            {
                Debug.LogWarning("[Save] Loaded the backup save.");
                return true;
            }

            if (backupError != null) Debug.LogWarning($"[Save] Backup could not be loaded either ({backupError.Message}). Starting a new game.");
            return false;
        }

        public void Delete()
        {
            foreach (var path in new[] { SavePath, BackupPath, TempPath, CorruptPath })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        bool TryRead(string path, out SaveEnvelope envelope, out Exception error)
        {
            envelope = null;
            error = null;
            if (!File.Exists(path)) return false;

            try
            {
                envelope = _serializer.Deserialize(File.ReadAllText(path));
                return true;
            }
            catch (Exception exception) when (exception is SaveFormatException || exception is IOException)
            {
                error = exception;
                return false;
            }
        }
    }
}
