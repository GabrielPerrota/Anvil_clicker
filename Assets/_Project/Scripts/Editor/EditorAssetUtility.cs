using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnvilClicker.Editor
{
    /// <summary>Helpers that keep the generators idempotent: create once, update in place afterwards.</summary>
    internal static class EditorAssetUtility
    {
        /// <summary>Creates every missing folder along <paramref name="assetPath"/> (e.g. "Assets/A/B").</summary>
        public static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent)) throw new ArgumentException($"Invalid folder path '{assetPath}'.");

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }

        /// <summary>Loads the asset at <paramref name="path"/>, creating it (and its folder) if missing.</summary>
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject => LoadOrCreate<T>(path, out _);

        public static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;

            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>Edits serialized (often private) fields through SerializedObject, then saves.</summary>
        public static void Edit(Object target, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(target);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        public static SerializedProperty Require(this SerializedObject serialized, string propertyPath)
        {
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
                throw new InvalidOperationException($"'{serialized.targetObject.GetType().Name}' has no serialized field '{propertyPath}'.");
            return property;
        }

        public static T LoadRequired<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException($"Missing {typeof(T).Name} at '{path}'. Run '{AnvilClickerPaths.MenuRoot}Setup/Run All'.");
            return asset;
        }
    }
}
