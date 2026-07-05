using UnityEditor;
using UnityEngine;

namespace Golgehalka.EditorTools
{
    /// Komut satırından Android APK derlemesi:
    /// Unity -batchmode -quit -projectPath <proje> -buildTarget Android
    ///       -executeMethod Golgehalka.EditorTools.BuildScript.PerformAndroidBuild
    public static class BuildScript
    {
        public static void PerformAndroidBuild()
        {
            var scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Prototype.unity" };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/ShadowRing.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("BUILD SONUCU: " + report.summary.result +
                      " | boyut: " + (report.summary.totalSize / (1024 * 1024)) + " MB" +
                      " | hata: " + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
