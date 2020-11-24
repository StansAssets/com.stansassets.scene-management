using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    Debug.Log("Scenes list:\n" + string.Join(", \n", options.scenes));
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                };
            });

            BuildConfigurationSettings.Instance.Configuration.InitializeBuildData(EditorUserBuildSettings.activeBuildTarget);
        }

        public static void SetupBuildOptions(ref BuildPlayerOptions options)
        {
            foreach (var handler in s_BuildHandlers)
            {
                handler.Invoke(options);
            }

            SetupAddressableScenes(options.target);
            options.scenes = FilterScenesByPath(options.target, options.scenes);
        }

        [MenuItem("Window/Asset Management/Setup Addressable Scenes")]
        public static void SetupAddressableScenes()
        {
            SetupAddressableScenes(EditorUserBuildSettings.activeBuildTarget);
        }

        static string[] FilterScenesByPath(BuildTarget target, string[] buildScenes)
        {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration == false)
            {
                return buildScenes;
            }

            var configuration = BuildConfigurationSettings.Instance.Configuration;
            var sceneAssets = configuration.GetSceneAssetsToIncludeInBuild(target.ToBuildTargetRuntime());
            var scenes = sceneAssets.Select(s => AssetDatabase.GUIDToAssetPath(s.Guid)).ToArray();
            return scenes;
        }

        static void SetupAddressableScenes(BuildTarget target)
        {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration == false)
            {
                return;
            }

            var configuration = BuildConfigurationSettings.Instance.Configuration;

            var scenesPathesToAdd = configuration.GetAddressableSceneAssets(target.ToBuildTargetRuntime())
                                           .Select(addressableScene => addressableScene.GetSceneAsset())
                                           .Where(sa => sa != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sa)));
            if (scenesPathesToAdd.Any())
            {
                InitializeAddressablesSettings();
                AddressableAssetGroup group = null;
                try
                {
                    group = AddressablesUtility.GetOrCreateGroup(ScenesAddressablesGroupName);
                    AddAddressableScenesIntoGroup(scenesPathesToAdd, group);
                    if (group.entries.Count > 0)
                    {
                        FindAndFixDublicateDependencies();
                        AddressableAssetSettings.BuildPlayerContent();
                    }
                    else
                    {
                        AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
                    }
                    BuildConfigurationSettings.Instance.Configuration.InitializeBuildData(target);
                    EditorUtility.SetDirty(BuildConfigurationSettings.Instance);
                    AssetDatabase.SaveAssets();
                }
                catch (ArgumentException e)
                {
                    Debug.LogError(e.Message);
                    AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    throw;
                }
            }
        }

        static void FindAndFixDublicateDependencies()
        {
            var rule = AnalyzeSystemHelper.FindRule<FindScenesDuplicateDependencies>();
            var results = AnalyzeSystemHelper.RefreshRule(rule);

            bool fixNeeded = false;
            foreach (var result in results)
            {
                if (result.severity == MessageType.Error || result.severity == MessageType.Warning)
                {
                    fixNeeded = true;
                    break;
                }
            }

            if (fixNeeded)
            {
                AnalyzeSystemHelper.FixIssues(rule);
            }

            AnalyzeSystemHelper.ClearAnalysis(rule);
        }

        static void InitializeAddressablesSettings()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                                                                                                 AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                                                                                                 true,
                                                                                                 true);
            }
        }
        
        static void AddAddressableScenesIntoGroup(IEnumerable<SceneAsset> scenes, AddressableAssetGroup group)
        {
            foreach (var scene in scenes)
            {
                if (scene == null)
                {
                    throw new ArgumentException("Scenes list contains null items");
                }
                var sceneAssetPath = AssetDatabase.GetAssetPath(scene);
                if (string.IsNullOrEmpty(sceneAssetPath))
                    throw new ArgumentException("Can not find path for scene" + scene.name);

                var guid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
                var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, true);
                if (entry == null)
                {
                    throw new ArgumentException("Can not create AddressableAssetEntry for scene " + scene.name);
                }
                entry.address = scene.name;
            }
        }
    }

    public static class AddressablesUtility
    {
        public static AddressableAssetGroup GetOrCreateGroup(string name)
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup((g) => g.name == name);
            if (group != null)
            {
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