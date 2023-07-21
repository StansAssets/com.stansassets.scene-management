﻿using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    static class TestBuildRunner
    {
        [MenuItem("Stan's Assets/Print Build Scenes")]
        static void Build()
        {
            var configuration = BuildConfigurationSettings.Instance.Configuration;

            configuration.UpdateSceneNames();
            var allScenes = configuration.BuildScenesCollection(new BuildScenesParams(EditorUserBuildSettings.activeBuildTarget, false, false));
            Debug.Log($"Build Scenes Collection [{allScenes.Count()}]:\n{string.Join("\n", allScenes.Select(s => $"{s.Name}, Addressable: {s.Addressable}"))}");
        } 
    }
}
