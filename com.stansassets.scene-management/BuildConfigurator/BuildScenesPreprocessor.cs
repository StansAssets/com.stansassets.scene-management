using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildScenesPreprocessor
    {
        static readonly List<Action<BuildPlayerOptions>> s_BuildHandlers = new List<Action<BuildPlayerOptions>>();

        public static void RegisterBuildPlayerHandler(Action<BuildPlayerOptions> handler)
        {
            s_BuildHandlers.Add(handler);
        }

        static BuildScenesPreprocessor()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler((options) =>
            {
                SetupBuildOptions(ref options);
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("Scenes list " + string.Join(", \n", options.scenes));
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                };
            });
        }

        public static void SetupBuildOptions(ref BuildPlayerOptions options)
        {
            foreach (var handler in s_BuildHandlers)
            {
                handler.Invoke(options);
            }

            options.scenes = FilterScenesByPath(EditorUserBuildSettings.activeBuildTarget, options.scenes);
        }

        static string[] FilterScenesByPath(BuildTarget target, string[] buildScenes)
        {
            var configuration = BuildConfigurationSettings.Instance.Configuration;
            if (configuration.IsEmpty)
            {
                return buildScenes;
            }

            List<string> scenes = new List<string>();

            if (configuration.DefaultScenes.Count == 0)
            {
                scenes.AddRange(buildScenes);
                ProcessPlatforms(ref scenes, target, configuration.Platforms);
            }
            else
            {
                if (configuration.DefaultScenesFirst)
                {
                    ProcessPlatforms(ref scenes, target, configuration.Platforms);
                    InsertScenes(ref scenes, configuration.DefaultScenes);
                }
                else
                {
                    InsertScenes(ref scenes, configuration.DefaultScenes);
                    ProcessPlatforms(ref scenes, target, configuration.Platforms);
                }
            }

            return scenes.ToArray();
        }

        static void ProcessPlatforms(ref List<string> scenes, BuildTarget target, List<PlatformsConfiguration> platforms)
        {
            foreach (var platformsConfiguration in platforms)
            {
                if (platformsConfiguration.BuildTargets.Contains(target))
                {
                    InsertScenes(ref scenes, platformsConfiguration.Scenes);
                }
                else
                {
                    RemoveScenes(ref scenes, platformsConfiguration.Scenes);
                }
            }
        }

        static void InsertScenes(ref List<string> scenes, List<SceneAsset> sceneAssets)
        {
            for (var index = 0; index < sceneAssets.Count; index++)
            {
                var sceneAssetPath = AssetDatabase.GetAssetPath(sceneAssets[index]);
                if (string.IsNullOrEmpty(sceneAssetPath))
                    continue;

                if (scenes.Contains(sceneAssetPath))
                {
                    scenes.Remove(sceneAssetPath);
                }

                scenes.Insert(index, sceneAssetPath);
            }
        }

        static void RemoveScenes(ref List<string> scenes, List<SceneAsset> sceneAssets)
        {
            foreach (var sceneAsset in sceneAssets)
            {
                var sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
                if (scenes.Contains(sceneAssetPath))
                {
                    scenes.Remove(sceneAssetPath);
                }
            }
        }
    }
}
