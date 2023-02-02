using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildScenesPreprocessor
    {
        public const string ScenesAddressablesGroupName = "Scenes";
        public const string ScenesDependenciesAddressablesGroupName = "Scenes Dependencies";

        public delegate void BuildAddressablesDelegate();

        static BuildAddressablesDelegate s_BuildAddressablesImpl = AddressableAssetSettings.BuildPlayerContent;

        static readonly List<Action<BuildPlayerOptions>> s_BuildHandlers = new List<Action<BuildPlayerOptions>>();

        public static void RegisterBuildPlayerHandler(Action<BuildPlayerOptions> handler)
        {
            s_BuildHandlers.Add(handler);
        }

        static BuildScenesPreprocessor()
        {
            AnalyzeSystem.RegisterNewRule<FindScenesDuplicateDependencies>();

#if !BUILD_SYSTEM_ENABLED
            BuildPlayerWindow.RegisterBuildPlayerHandler(options =>
            {
                SetupBuildOptions(ref options);
                EditorApplication.delayCall += () =>
                {
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                };
            });
#endif
        }

        public static void SetupBuildOptions(ref BuildPlayerOptions options)
        {
            foreach (var handler in s_BuildHandlers)
            {
                handler.Invoke(options);
            }

            PrebuildCleanup();
            SetupAddressableScenes(options.target);
            options.scenes = FilterScenesByPath(options.target, options.scenes);

            Debug.Log("Built scenes:\n" + string.Join(", \n", options.scenes));
        }

        /// <summary>
        /// Use this method to override Addressables build process. By default it's AddressableAssetSettings.BuildPlayerContent().
        /// </summary>
        public static void SetBuildAddressablesOverride(BuildAddressablesDelegate buildAddressablesDelegate)
        {
            s_BuildAddressablesImpl = buildAddressablesDelegate;
        }

        static string[] FilterScenesByPath(BuildTarget target, string[] buildScenes)
        {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration == false)
            {
                return buildScenes;
            }

            var configuration = BuildConfigurationSettings.Instance.Configuration;
            var sceneAssets = configuration.BuildScenesCollection(new BuildScenesParams(target, true, false));
            var scenes = sceneAssets.Select(s => AssetDatabase.GUIDToAssetPath(s.Guid)).ToArray();
            return scenes;
        }

        internal static void SetupAddressableScenes(BuildTarget target) {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration == false)
            {
                return;
            }

            InitializeAddressablesSettings();
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

                s_BuildAddressablesImpl.Invoke();
            }
            else
            {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
            }
            BuildConfigurationSettings.Instance.Configuration.InitializeBuildData(target);
            EditorUtility.SetDirty(BuildConfigurationSettings.Instance);
            AssetDatabase.SaveAssets();
        }

        static void InitializeAddressablesSettings() {
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

        static void PrebuildCleanup()
        {
            RemoveMissingGroupReferences();
            if (BuildConfigurationSettings.Instance.Configuration.ClearAllAddressablesCache)
            {
                ClearAllAddressablesCache();
            }
        }

        static void RemoveMissingGroupReferences()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            var missingAddressableAssetGroups = groups.Where(g => g == null).ToList();
            for (var i = 0; i < missingAddressableAssetGroups.Count; i++)
            {
                settings.RemoveGroup(missingAddressableAssetGroups[i]);
            }
        }

        static void ClearAllAddressablesCache()
        {
            AddressableAssetSettings.CleanPlayerContent();
            BuildCache.PurgeCache(true);
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
