using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StansAssets.SceneManagement.Build;
using UnityEditor;

namespace Tests
{
    public class BuildConfiguratorTests
    {
        IEnumerable<SceneAssetInfo> GetTestScenes()
        {
            var fodlerName = "Tests/Scenes";
            var result = new List<SceneAssetInfo>();

            for (var i = 1; i <= 4; i++)
            {
                result.Add(new SceneAssetInfo()
                {
                    Guid = GUID.Generate().ToString(),
                    Name = $"{fodlerName}/Test{i}"
                });
            }

            return result;
        }

        [Test]
        public void CompareScenesWithBuildSettingsExpectOutOfSync()
        {
            var editorBuildSettingsScenes = EditorBuildSettings
                .scenes.ToList();

            var defaultScenes = BuildConfigurationSettings.Instance.Configuration
                .DefaultScenes.ToArray();

            var platformsConfigurations = BuildConfigurationSettings.Instance.Configuration
                .Platforms.ToArray();

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes.Clear();
            BuildConfigurationSettings.Instance.Configuration.Platforms.Clear();

            var testScenes = GetTestScenes().ToArray();

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes
                .Add(testScenes[0]);

            BuildConfigurationSettings.Instance.Configuration.Platforms
                .Add(new PlatformsConfiguration()
                {
                    BuildTargets = new List<BuildTargetRuntime>()
                    {
                        BuildTargetRuntime.Editor
                    },
                    Scenes = new List<SceneAssetInfo>()
                    {
                        testScenes[1]
                    }
                });

            BuildConfigurationSettings.Instance.Configuration.Platforms
                .Add(new PlatformsConfiguration()
                {
                    BuildTargets = new List<BuildTargetRuntime>()
                    {
                        BuildTargetRuntime.StandaloneWindows64
                    },
                    Scenes = new List<SceneAssetInfo>()
                    {
                        testScenes[2]
                    }
                });

            var editorTestScene = testScenes[3];
            var editorTestScenes = new[]
            {
                new EditorBuildSettingsScene(editorTestScene.Guid, true)
            };

            EditorBuildSettings.scenes = editorTestScenes;
            
            var needScenesSyncExpectTrue = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes = defaultScenes.ToList();
            BuildConfigurationSettings.Instance.Configuration.Platforms = platformsConfigurations.ToList();
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

            Assert.IsTrue(needScenesSyncExpectTrue);
        }

        [Test]
        public void CompareScenesWithBuildSettingsExpectSyncedScenes()
        {
            var editorBuildSettingsScenes = EditorBuildSettings
                .scenes.ToList();

            var defaultScenes = BuildConfigurationSettings.Instance.Configuration
                .DefaultScenes.ToArray();

            var platformsConfigurations = BuildConfigurationSettings.Instance.Configuration
                .Platforms.ToArray();

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes.Clear();
            BuildConfigurationSettings.Instance.Configuration.Platforms.Clear();
            
            var testScenes = GetTestScenes().ToArray();
            var testScene1 = testScenes[0];
            var testScene2 = testScenes[1];
            var testScene3 = testScenes[2];
            
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(new GUID(testScene1.Guid), true),
                new EditorBuildSettingsScene(new GUID(testScene2.Guid), true),
                new EditorBuildSettingsScene(new GUID(testScene3.Guid), true)
            };

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes
                .Add(new SceneAssetInfo()
                {
                    Guid = testScene1.Guid,
                    Name = testScene1.Name
                });

            BuildConfigurationSettings.Instance.Configuration.Platforms
                .Add(new PlatformsConfiguration()
                {
                    BuildTargets = new List<BuildTargetRuntime>()
                    {
                        BuildTargetRuntime.Editor
                    },
                    Scenes = new List<SceneAssetInfo>()
                    {
                        new SceneAssetInfo()
                        {
                            Guid = testScene2.Guid,
                            Name = testScene2.Name
                        }
                    }
                });

            BuildConfigurationSettings.Instance.Configuration.Platforms
                .Add(new PlatformsConfiguration()
                {
                    BuildTargets = new List<BuildTargetRuntime>()
                    {
                        BuildTargetRuntime.StandaloneWindows64
                    },
                    Scenes = new List<SceneAssetInfo>()
                    {
                        new SceneAssetInfo()
                        {
                            Guid = testScene3.Guid,
                            Name = testScene3.Name
                        }
                    }
                });

            var needScenesSyncExpectFalse = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            BuildConfigurationSettings.Instance.Configuration.DefaultScenes = defaultScenes.ToList();
            BuildConfigurationSettings.Instance.Configuration.Platforms = platformsConfigurations.ToList();
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

            Assert.IsFalse(needScenesSyncExpectFalse);
        }
    }
}