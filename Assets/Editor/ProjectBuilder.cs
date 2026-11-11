using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefenseLab;

public static class ProjectBuilder
{
    public static void BuildStandalone()
    {
        EnsureStandardMaterial();
        string requestedDirectory = Environment.GetEnvironmentVariable("TOWER_DEFENSE_BUILD_DIR");
        string outputDirectory = string.IsNullOrEmpty(requestedDirectory)
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Linux"))
            : Path.GetFullPath(requestedDirectory);
        Directory.CreateDirectory(outputDirectory);
        string outputPath = Path.Combine(outputDirectory, "TowerDefenseLab.x86_64");
        BuildReport report = BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/Main.unity" },
            outputPath,
            BuildTarget.StandaloneLinux64,
            BuildOptions.None);
        Debug.Log($"TOWER_DEFENSE_BUILD_RESULT:{report.summary.result}:{report.summary.totalSize}");
    }

    private static void EnsureStandardMaterial()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        const string path = "Assets/Resources/StandardBase.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

        Shader shader = Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("Unity Standard shader is unavailable.");
        AssetDatabase.CreateAsset(new Material(shader), path);
        AssetDatabase.SaveAssets();
    }

    public static void BuildProject()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject app = new GameObject("TowerDefenseApp");
        app.AddComponent<TowerDefenseApp>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("TOWER_DEFENSE_SCENE_CREATED");
    }
}
