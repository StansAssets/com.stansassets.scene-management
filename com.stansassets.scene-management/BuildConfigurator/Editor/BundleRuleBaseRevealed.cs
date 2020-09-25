using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace StansAssets.SceneManagement.Build
{
    class BundleRuleBaseRevealed : AnalyzeRule
    {
        [NonSerialized] private List<GUID> m_addressableAssets = new List<GUID>();
        [NonSerialized] private readonly Dictionary<string, List<GUID>> m_resourcesToDependencies = new Dictionary<string, List<GUID>>();
        [NonSerialized] private readonly List<ContentCatalogDataEntry> m_locations = new List<ContentCatalogDataEntry>();
        [NonSerialized] protected readonly List<AssetBundleBuild> m_allBundleInputDefs = new List<AssetBundleBuild>();
        [NonSerialized] private readonly Dictionary<string, string> m_bundleToAssetGroup = new Dictionary<string, string>();
        [NonSerialized] private readonly List<AddressableAssetEntry> m_assetEntries = new List<AddressableAssetEntry>();
        [NonSerialized] protected ExtractDataTask m_extractData = new ExtractDataTask();

        private IList<IBuildTask> RuntimeDataBuildTasks(string builtinShaderBundleName)
        {
            IList<IBuildTask> buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle(builtinShaderBundleName));

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new UpdateBundleObjectLayout());

            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());

            //buildTasks.Add(new GenerateLocationListsTask());

            return buildTasks;
        }

        protected AddressableAssetsBuildContext GetBuildContext(AddressableAssetSettings settings)
        {
            ResourceManagerRuntimeData runtimeData = new ResourceManagerRuntimeData {
                LogResourceManagerExceptions = settings.buildSettings.LogResourceManagerExceptions
            };

            var aaContext = new AddressableAssetsBuildContext
            {
                Settings = settings,
                runtimeData = runtimeData,
                bundleToAssetGroup = m_bundleToAssetGroup,
                locations = m_locations,
                providerTypes = new HashSet<Type>(),
                assetEntries = m_assetEntries
            };
            return aaContext;
        }

        protected bool IsValidPath(string path) {
            var utilityType = Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetUtility,Unity.Addressables.Editor.dll");
            if (utilityType != null) {
                var methodInfo = utilityType.GetMethod("IsPathValidForEntry", BindingFlags.NonPublic | BindingFlags.Static);
                return methodInfo != null && (bool)methodInfo.Invoke(null, new object[] { path })
                                          && !path.ToLower().Contains("/resources/")
                                          && !path.ToLower().StartsWith("resources/");
            }

            return false;
        }

        protected ReturnCode RefreshBuild(AddressableAssetsBuildContext buildContext)
        {
            var settings = buildContext.Settings;
            var context = new AddressablesDataBuilderInput(settings);

            var buildTarget = context.Target;
            var buildTargetGroup = context.TargetGroup;
            var buildParams = new AddressableAssetsBundleBuildParameters(settings, m_bundleToAssetGroup, buildTarget,
                buildTargetGroup, settings.buildSettings.bundleBuildPath);
            var builtinShaderBundleName =
                settings.DefaultGroup.Name.ToLower().Replace(" ", "").Replace('\\', '/').Replace("//", "/") +
                "_unitybuiltinshaders.bundle";
            var buildTasks = RuntimeDataBuildTasks(builtinShaderBundleName);
            buildTasks.Add(m_extractData);

            IBundleBuildResults buildResults;
            var exitCode = ContentPipeline.BuildAssetBundles(buildParams, new BundleBuildContent(m_allBundleInputDefs),
                out buildResults, buildTasks, buildContext);

            return exitCode;
        }

        private List<GUID> GetAllBundleDependencies()
        {
            var explicitGuids = m_extractData.WriteData.AssetToFiles.Keys;
            var implicitGuids = GetImplicitGuidToFilesMap().Keys;
            var allBundleGuids = explicitGuids.Union(implicitGuids);

            return allBundleGuids.ToList();
        }

        private void IntersectResourcesDependenciesWithBundleDependencies(List<GUID> bundleDependencyGuids)
        {
            foreach (var key in m_resourcesToDependencies.Keys)
            {
                var bundleDependencies = bundleDependencyGuids.Intersect(m_resourcesToDependencies[key]).ToList();

                m_resourcesToDependencies[key].Clear();
                m_resourcesToDependencies[key].AddRange(bundleDependencies);
            }
        }

        private void BuiltInResourcesToDependenciesMap(string[] resourcePaths)
        {
            foreach (string path in resourcePaths)
            {
                string[] dependencies = AssetDatabase.GetDependencies(path);

                if (!m_resourcesToDependencies.ContainsKey(path))
                    m_resourcesToDependencies.Add(path, new List<GUID>());

                m_resourcesToDependencies[path].AddRange(from dependency in dependencies
                    select new GUID(AssetDatabase.AssetPathToGUID(dependency)));
            }
        }

        private void ConvertBundleNamesToGroupNames(AddressableAssetsBuildContext buildContext)
        {
            Dictionary<string, string> bundleNamesToUpdate = new Dictionary<string, string>();

            foreach (var assetGroup in buildContext.Settings.groups)
            {
                if (assetGroup == null)
                    continue;

                if (buildContext.assetGroupToBundles.TryGetValue(assetGroup, out var bundles))
                {
                    foreach (string bundle in bundles)
                    {
                        var keys = m_extractData.WriteData.FileToBundle.Keys.Where(key => m_extractData.WriteData.FileToBundle[key] == bundle);
                        foreach (string key in keys)
                            bundleNamesToUpdate.Add(key, assetGroup.Name);
                    }
                }
            }

            foreach (string key in bundleNamesToUpdate.Keys)
            {
                var bundle = m_extractData.WriteData.FileToBundle[key];
                var inputDef = m_allBundleInputDefs.FirstOrDefault(b => b.assetBundleName == bundle);
                int index = m_allBundleInputDefs.IndexOf(inputDef);
                if (index >= 0)
                {
                    inputDef.assetBundleName = ConvertBundleName(inputDef.assetBundleName, bundleNamesToUpdate[key]);
                    m_allBundleInputDefs[index] = inputDef;
                    m_extractData.WriteData.FileToBundle[key] = inputDef.assetBundleName;
                }
            }
        }

        internal void CalculateInputDefinitions(AddressableAssetSettings settings) {
            var group = settings.FindGroup(g => g.Name.Equals(BuildScenesPreprocessor.ScenesAddressablesGroupName));
            if (group == null)
                return;

            if (group.HasSchema<BundledAssetGroupSchema>()) {
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                List<AssetBundleBuild> bundleInputDefinitions = new List<AssetBundleBuild>();

                var modeType = Type.GetType("UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptPackedMode,Unity.Addressables.Editor.dll");
                if (modeType != null) {
                    var methodInfo = modeType.GetMethod("PrepGroupBundlePacking", BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodInfo != null) {
                        var entries = (IEnumerable<AddressableAssetEntry>) methodInfo.Invoke(null,
                            new object[] { group, bundleInputDefinitions, schema.BundleMode });
                        m_assetEntries.AddRange(entries);

                        for (int i = 0; i < bundleInputDefinitions.Count; i++) {
                            if (m_bundleToAssetGroup.ContainsKey(bundleInputDefinitions[i].assetBundleName))
                                bundleInputDefinitions[i] = CreateUniqueBundle(bundleInputDefinitions[i]);

                            m_bundleToAssetGroup.Add(bundleInputDefinitions[i].assetBundleName, schema.Group.Guid);
                        }

                        m_allBundleInputDefs.AddRange(bundleInputDefinitions);
                    }
                }
            }
        }

        private AssetBundleBuild CreateUniqueBundle(AssetBundleBuild bid)
        {
            int count = 1;
            var newName = bid.assetBundleName;
            while (m_bundleToAssetGroup.ContainsKey(newName) && count < 1000)
                newName = bid.assetBundleName.Replace(".bundle", $"{count++}.bundle");
            return new AssetBundleBuild
            {
                assetBundleName = newName,
                addressableNames = bid.addressableNames,
                assetBundleVariant = bid.assetBundleVariant,
                assetNames = bid.assetNames
            };
        }

        internal List<GUID> GetImplicitGuidsForBundle(string fileName)
        {
            List<GUID> guids = (from id in m_extractData.WriteData.FileToObjects[fileName]
                where !m_extractData.WriteData.AssetToFiles.Keys.Contains(id.guid)
                select id.guid).ToList();
            return guids;
        }

        internal Dictionary<GUID, List<string>> GetImplicitGuidToFilesMap()
        {
            Dictionary<GUID, List<string>> implicitGuids = new Dictionary<GUID, List<string>>();
            IEnumerable<KeyValuePair<ObjectIdentifier, string>> validImplicitGuids =
                from fileToObject in m_extractData.WriteData.FileToObjects
                from objectId in fileToObject.Value
                where !m_extractData.WriteData.AssetToFiles.Keys.Contains(objectId.guid)
                select new KeyValuePair<ObjectIdentifier, string>(objectId, fileToObject.Key);

            //Build our Dictionary from our list of valid implicit guids (guids not already in explicit guids)
            foreach (var objectIdToFile in validImplicitGuids)
            {
                if (!implicitGuids.ContainsKey(objectIdToFile.Key.guid))
                    implicitGuids.Add(objectIdToFile.Key.guid, new List<string>());
                implicitGuids[objectIdToFile.Key.guid].Add(objectIdToFile.Value);
            }

            return implicitGuids;
        }

        internal List<AnalyzeResult> CalculateBuiltInResourceDependenciesToBundleDependencies(AddressableAssetSettings settings, string[] builtInResourcesPaths)
        {
            List<AnalyzeResult> results = new List<AnalyzeResult>();

            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return results;
            }

            m_addressableAssets = (from aaGroup in settings.groups
                where aaGroup != null
                from entry in aaGroup.entries
                select new GUID(entry.guid)).ToList();


            BuiltInResourcesToDependenciesMap(builtInResourcesPaths);
            CalculateInputDefinitions(settings);

            var context = GetBuildContext(settings);
            ReturnCode exitCode = RefreshBuild(context);
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError("Analyze build failed. " + exitCode);
                results.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
                return results;
            }

            IntersectResourcesDependenciesWithBundleDependencies(GetAllBundleDependencies());

            ConvertBundleNamesToGroupNames(context);

            results = (from resource in m_resourcesToDependencies.Keys
                from dependency in m_resourcesToDependencies[resource]

                let assetPath = AssetDatabase.GUIDToAssetPath(dependency.ToString())
                    let files = m_extractData.WriteData.FileToObjects.Keys

                    from file in files
                    where m_extractData.WriteData.FileToObjects[file].Any(oid => oid.guid == dependency)
                    where m_extractData.WriteData.FileToBundle.ContainsKey(file)
                    let bundle = m_extractData.WriteData.FileToBundle[file]

                    select new AnalyzeResult
                {
                    resultName =
                        resource + kDelimiter +
                        bundle + kDelimiter +
                        assetPath,
                    severity = MessageType.Warning
                }).ToList();

            if (results.Count == 0)
                results.Add(new AnalyzeResult { resultName = ruleName + " - No issues found." });

            return results;
        }

        protected string ConvertBundleName(string bundleName, string groupName)
        {
            string[] bundleNameSegments = bundleName.Split('_');
            bundleNameSegments[0] = groupName.Replace(" ", "").ToLower();
            return string.Join("_", bundleNameSegments);
        }

        public override void ClearAnalysis()
        {
            m_locations.Clear();
            m_addressableAssets.Clear();
            m_allBundleInputDefs.Clear();
            m_bundleToAssetGroup.Clear();
            m_resourcesToDependencies.Clear();
            m_extractData = new ExtractDataTask();

            base.ClearAnalysis();
        }
    }
}
