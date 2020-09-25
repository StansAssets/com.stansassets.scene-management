using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    sealed class FindScenesDuplicateDependencies : BundleRuleBaseRevealed
    {
        private struct CheckDupeResult
        {
            public AddressableAssetGroup Group;
            public string DuplicatedFile;
            public string AssetPath;
            public GUID DuplicatedGroupGuid;
        }

        public override bool CanFix => true;

        public override string ruleName => "Find Scenes Duplicate Dependencies";

        [NonSerialized] private readonly Dictionary<string, Dictionary<string, List<string>>> m_allIssues = new Dictionary<string, Dictionary<string, List<string>>>();
        [SerializeField] private HashSet<GUID> m_implicitAssets;

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            ClearAnalysis();
            return CheckForDuplicateDependencies(settings);
        }

        List<AnalyzeResult> CheckForDuplicateDependencies(AddressableAssetSettings settings) {
            var results = new List<AnalyzeResult>();
            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return results;
            }

            CalculateInputDefinitions(settings);

            if (m_allBundleInputDefs.Count > 0)
            {
                var context = GetBuildContext(settings);
                ReturnCode exitCode = RefreshBuild(context);
                if (exitCode < ReturnCode.Success)
                {
                    Debug.LogError("Analyze build failed. " + exitCode);
                    results.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
                    return results;
                }

                var implicitGuids = GetImplicitGuidToFilesMap();
                var checkDupeResults = CalculateDuplicates(implicitGuids, context);
                BuildImplicitDuplicatedAssetsSet(checkDupeResults);

                results = (from issueGroup in m_allIssues
                    from bundle in issueGroup.Value
                    from item in bundle.Value
                    select new AnalyzeResult
                    {
                        resultName = ruleName + kDelimiter +
                            issueGroup.Key + kDelimiter +
                            ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                            item,
                        severity = MessageType.Warning
                    }).ToList();
            }

            if (results.Count == 0)
                results.Add(noErrors);

            return results;
        }

        private IEnumerable<CheckDupeResult> CalculateDuplicates(Dictionary<GUID, List<string>> implicitGuids, AddressableAssetsBuildContext aaContext)
        {
            //Get all guids that have more than one bundle referencing them
            IEnumerable<KeyValuePair<GUID, List<string>>> validGuids =
                from dupeGuid in implicitGuids
                where dupeGuid.Value.Distinct().Count() > 1
                where IsValidPath(AssetDatabase.GUIDToAssetPath(dupeGuid.Key.ToString()))
                select dupeGuid;

            return
                from guidToFile in validGuids
                from file in guidToFile.Value

                //Get the files that belong to those guids
                let fileToBundle = m_extractData.WriteData.FileToBundle[file]

                    //Get the bundles that belong to those files
                    let bundleToGroup = aaContext.bundleToAssetGroup[fileToBundle]

                    //Get the asset groups that belong to those bundles
                    let selectedGroup = aaContext.Settings.FindGroup(findGroup => findGroup != null && findGroup.Guid == bundleToGroup)

                    select new CheckDupeResult
                {
                    Group = selectedGroup,
                    DuplicatedFile = file,
                    AssetPath = AssetDatabase.GUIDToAssetPath(guidToFile.Key.ToString()),
                    DuplicatedGroupGuid = guidToFile.Key
                };
        }

        private void BuildImplicitDuplicatedAssetsSet(IEnumerable<CheckDupeResult> checkDupeResults)
        {
            m_implicitAssets = new HashSet<GUID>();

            foreach (var checkDupeResult in checkDupeResults)
            {
                if (!m_allIssues.TryGetValue(checkDupeResult.Group.Name, out var groupData))
                {
                    groupData = new Dictionary<string, List<string>>();
                    m_allIssues.Add(checkDupeResult.Group.Name, groupData);
                }

                if (!groupData.TryGetValue(m_extractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], out var assets))
                {
                    assets = new List<string>();
                    groupData.Add(m_extractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], assets);
                }

                assets.Add(checkDupeResult.AssetPath);

                m_implicitAssets.Add(checkDupeResult.DuplicatedGroupGuid);
            }
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            if (m_implicitAssets == null)
                CheckForDuplicateDependencies(settings);

            if (m_implicitAssets.Count == 0)
                return;

            var group = AddressablesUtility.GetOrCreateGroup(BuildScenesPreprocessor.ScenesDependenciesAddressablesGroupName);
            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;

            foreach (var asset in m_implicitAssets) {
                var guidString = asset.ToString();
                var entry = settings.CreateOrMoveEntry(guidString, group, false, false);
                entry.address = $"{Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guidString))}_{guidString}";
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }

        public override void ClearAnalysis()
        {
            m_allIssues.Clear();
            m_implicitAssets = null;
            base.ClearAnalysis();
        }
    }
}
