using System;
using System.Linq;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    struct AutoSyncParams : IEquatable<AutoSyncParams>
    {
        public bool Synced;
        public bool NeedScenesSync;

        public bool Equals(AutoSyncParams other)
        {
            return Synced == other.Synced && NeedScenesSync == other.NeedScenesSync;
        }

        public override bool Equals(object obj)
        {
            return obj is AutoSyncParams other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Synced, NeedScenesSync);
        }
    }

    class BuildConfigurationContext
    {
        public AutoSyncParams AutoSyncParams;

        public void SyncScenes()
        {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration)
            {
                BuildConfigurationSettings.Instance.Configuration.ClearMissingScenes();
            
                BuildConfigurationSettings.Instance.Configuration
                    .SetupEditorSettings(EditorUserBuildSettings.activeBuildTarget, true);
            
                AutoSyncParams.Synced = true;
            }
        }
        
        public void CheckNTryAutoSync(bool ignoreCollectionsSize = false)
        {
            var hasAnyScene = BuildConfigurationSettingsValidator.HasAnyScene();
            if (!hasAnyScene)
            {
                AutoSyncParams.Synced = false;
                return;
            }

            var hasBuildTargetDuplicates = BuildConfigurationSettingsValidator.HasBuildTargetsDuplicates();
            if (hasBuildTargetDuplicates)
            {
                return;
            }
            
            var hasDuplicates = BuildConfigurationSettingsValidator.HasScenesDuplicates();
            if (hasDuplicates)
            {
                return;    
            }
            
            var hasMissingScenes = BuildConfigurationSettingsValidator.HasMissingScenes();
            if (hasMissingScenes)
            {
                return;
            }

            if (!ignoreCollectionsSize)
            {
                var scenesCollections = BuildConfigurationSettingsValidator.GetScenesCollections();
                if (scenesCollections.buildScenes.Count() > scenesCollections.confScenes.Count())
                {
                    AutoSyncParams.Synced = false;
                    return;
                }
            }
            
            AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            if (AutoSyncParams.Synced && AutoSyncParams.NeedScenesSync)
            {
                SyncScenes();
                AutoSyncParams.Synced = true;
            }
            else if (!AutoSyncParams.Synced && !AutoSyncParams.NeedScenesSync)
            {
                AutoSyncParams.Synced = true;
            }
        }
    }
}
