using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Player builds. The Unity CLI only builds desktop players on its own; WebGL needs a build method.
    /// Batch: unity run . -- -executeMethod AnvilClicker.Editor.BuildTools.BuildWebGLBatch -buildOutput Builds/WebGL
    /// </summary>
    public static class BuildTools
    {
        const string OutputArgument = "-buildOutput";
        const string WebGLCompressionArgument = "-webglCompression";
        const string DefaultWebGLOutput = "Builds/WebGL";

        [MenuItem(AnvilClickerPaths.MenuRoot + "Build/WebGL", priority = 200)]
        public static void BuildWebGL() => Build(BuildTarget.WebGL, DefaultWebGLOutput);

        /// <summary>Entry point for -executeMethod. Exits with code 1 on any failure.</summary>
        public static void BuildWebGLBatch()
        {
            try
            {
                Build(BuildTarget.WebGL, ReadOutputArgument() ?? DefaultWebGLOutput);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        static void Build(BuildTarget target, string outputPath)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No scene in the build settings. Run Tools/Anvil Clicker/Setup/Run All.");

            if (target == BuildTarget.WebGL) ApplyWebGLCompression();

            Directory.CreateDirectory(outputPath);
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[Anvil Clicker] {target} build {summary.result}: {summary.totalSize / (1024 * 1024)} MB in {summary.totalTime:mm\\:ss}, {summary.totalErrors} error(s) -> {outputPath}");

            if (summary.result != BuildResult.Succeeded) throw new InvalidOperationException($"{target} build failed ({summary.result}).");
        }

        /// <summary>
        /// Local test builds are uncompressed so any static file server can host them. Pass
        /// "-webglCompression brotli" (or gzip) for a build to publish.
        /// </summary>
        static void ApplyWebGLCompression()
        {
            var value = ReadArgument(WebGLCompressionArgument) ?? "disabled";
            PlayerSettings.WebGL.compressionFormat = value.ToLowerInvariant() switch
            {
                "brotli" => WebGLCompressionFormat.Brotli,
                "gzip" => WebGLCompressionFormat.Gzip,
                _ => WebGLCompressionFormat.Disabled
            };
            PlayerSettings.WebGL.decompressionFallback = false;
        }

        static string ReadOutputArgument() => ReadArgument(OutputArgument);

        static string ReadArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            }

            return null;
        }
    }
}
