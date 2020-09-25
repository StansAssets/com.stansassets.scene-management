using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildScenesPreprocessor
    {
        public const string ScenesAddressablesGroupName = "Scenes";
        public const string ScenesDependenciesAddressablesGroupName = "Scenes Dependencies";

        static readonly List<Action<BuildPlayerOptions>> s_BuildHandlers = new List<Action<BuildPlayerOptions>>();

        public static void RegisterBuildPlayerHandler(Action<BuildPlayerOptions> handler)
        {
            s_BuildHandlers.Add(handler);
        }

        static BuildScenesPreprocessor()
        {
            AnalyzeSystem.RegisterNewRule<FindScenesDuplicateDependencies>();
            BuildPlayerWindow.RegisterBuildPlayerHandler(options =>
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

            SetupAddressableScenes(EditorUserBuildSettings.activeBuildTarget);
            options.scenes = FilterScenesByPath(EditorUserBuildSettings.activeBuildTarget, options.scenes);
        }

        [MenuItem("Window/Asset Management/Setup Addressable Scenes")]
        public static void SetupAddressableScenes() {
            SetupAddressableScenes(EditorUserBuildSettings.activeBuildTarget);
        }

        static string[] FilterScenesByPath(BuildTarget target, string[] buildScenes)
        {
            var configuration = BuildConfigurationSettings.Instance.Configuration;
            if (configuration.IsEmpty)
            {
                return buildScenes;
            }

            List<string> scenes = new List<string>();

            var defaultNonAddrScenes = configuration.GetNonAddressableDefaultScenes();
            if (defaultNonAddrScenes.Count == 0)
            {
                scenes.AddRange(buildScenes);
                ProcessPlatforms(ref scenes, target, configuration.Platforms);
            }
            else
            {
                if (configuration.DefaultScenesFirst)
                {
                    ProcessPlatforms(ref scenes, target, configuration.Platforms);
                    InsertScenes(ref scenes, defaultNonAddrScenes);
                }
                else
                {
                    InsertScenes(ref scenes, defaultNonAddrScenes);
                    ProcessPlatforms(ref scenes, target, configuration.Platforms);
                }
            }

            return scenes.ToArray();
        }

        static void ProcessPlatforms(ref List<string> scenes, BuildTarget target, List<PlatformsConfiguration> platforms)
        {
            foreach (var platformsConfiguration in platforms)
            {
                var editorBuildTargets = platformsConfiguration.GetBuildTargetsEditor();
                if (editorBuildTargets.Contains(target))
                {
                    InsertScenes(ref scenes, platformsConfiguration.GetNonAddressableScenes());
                }
                else
                {
                    RemoveScenes(ref scenes, platformsConfiguration.GetNonAddressableScenes());
                }
                // Remove any addressable scene from a build
                RemoveScenes(ref scenes, platformsConfiguration.GetAddressableScenes());
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

        static void SetupAddressableScenes(BuildTarget target) {
            InitializeAddressablesAPI();
            // TODO: Don't create a group until we checked that even 1 scene is Addressable
            var group = AddressablesUtility.GetOrCreateGroup(ScenesAddressablesGroupName);
            var configuration = BuildConfigurationSettings.Instance.Configuration;

            AddAddressableScenesIntoGroup(configuration.GetAddressableDefaultScenes(), group);

            foreach (var platformsConfiguration in configuration.Platforms)
            {
                var editorBuildTargets = platformsConfiguration.GetBuildTargetsEditor();
                if (editorBuildTargets.Contains(target))
                {
                    AddAddressableScenesIntoGroup(platformsConfiguration.GetAddressableScenes(), group);
                    break;
                }
            }

            if (group.entries.Count > 0) {
                var rule = AnalyzeSystemHelper.FindRule<FindScenesDuplicateDependencies>();
                var results = AnalyzeSystemHelper.RefreshRule(rule);

                bool fixNeeded = false;
                foreach (var result in results) {
                    if (result.severity == MessageType.Error || result.severity == MessageType.Warning) {
                        fixNeeded = true;
                        break;
                    }
                }

                if (fixNeeded) {
                    AnalyzeSystemHelper.FixIssues(rule);
                }
                AnalyzeSystemHelper.ClearAnalysis(rule);
                AddressableAssetSettings.BuildPlayerContent();
            }
            else
            {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
            }
            BuildConfigurationSettings.Instance.Configuration.InitializeBuildData((BuildTargetRuntime)(int)target);
            EditorUtility.SetDirty(BuildConfigurationSettings.Instance);
            AssetDatabase.SaveAssets();
        }

        static void InitializeAddressablesAPI() {
            if (AddressableAssetSettingsDefaultObject.Settings == null) {
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                                                                                                 AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                                                                                                 true,
                                                                                                 true);
            }
        }

        static void AddAddressableScenesIntoGroup(List<SceneAsset> scenes, AddressableAssetGroup group)
        {
            foreach (var scene in scenes)
            {
                var sceneAssetPath = AssetDatabase.GetAssetPath(scene);
                if (string.IsNullOrEmpty(sceneAssetPath))
                    continue;

                var guid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
                var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, true);
                entry.address = scene.name;
            }
        }
    }

    public static class AddressablesUtility
    {
        public static AddressableAssetGroup GetOrCreateGroup(string name)
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup((g) => g.name == name);
            if (group != null) {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
            }

            group = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(name, false, false, true, new List<AddressableAssetGroupSchema>());
            group.ClearSchemas(true, true);
            group.AddSchema<ContentUpdateGroupSchema>();
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.UseAssetBundleCache = false;
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
            group.AddSchema(schema);

            return group;
        }
    }
}
