using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StansAssets.SceneManagement.Build;
using UnityEditor;

namespace StansAssets.SceneManagement.Tests
{
    public class BuildConfiguratorTests
    {
        IEnumerable<SceneAssetInfo> GetTestScenes()
        {
            var result = new List<SceneAssetInfo>();

            for (var i = 1; i <= 4; i++)
            {
                var sceneGuid = AssetDatabase.FindAssets($"SceneManagementTestScene{i}").First();
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                
                result.Add(new SceneAssetInfo()
                {
                    Guid = sceneGuid,
                    Name = scenePath
                });
            }

            return result;
        }

        struct SettingsBackup
        {
            List<EditorBuildSettingsScene> m_EditorBuildSettingsScenes;
            SceneAssetInfo[] m_DefaultScenes;
            PlatformsConfiguration[] m_PlatformsConfigurations;

            public void BackUp()
            {
                m_EditorBuildSettingsScenes = EditorBuildSettings
                    .scenes.ToList();

                m_DefaultScenes = BuildConfigurationSettings.Instance.Configuration
                    .DefaultScenes.ToArray();

                m_PlatformsConfigurations = BuildConfigurationSettings.Instance.Configuration
                    .Platforms.ToArray();
            }

            public void RestoreSettings()
            {
                BuildConfigurationSettings.Instance.Configuration.DefaultScenes = m_DefaultScenes.ToList();
                BuildConfigurationSettings.Instance.Configuration.Platforms = m_PlatformsConfigurations.ToList();
                EditorBuildSettings.scenes = m_EditorBuildSettingsScenes.ToArray();
            }
        }

        [Test]
        public void CompareScenesWithBuildSettingsExpectOutOfSync()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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

            try
            {
                var needScenesSyncExpectTrue = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

                Assert.IsTrue(needScenesSyncExpectTrue);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }

        [Test]
        public void CompareScenesWithBuildSettingsExpectSyncedScenes()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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

            try
            {
                var needScenesSyncExpectFalse = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();
                Assert.IsFalse(needScenesSyncExpectFalse);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }

        [Test]
        public void CheckMissingScenesExpectMissing()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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
                    Guid = "",
                    Name = ""
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

            try
            {
                var hasMissingScenesExpectTrue = BuildConfigurationSettingsValidator.HasMissingScenes();
                Assert.IsTrue(hasMissingScenesExpectTrue);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }

        [Test]
        public void CheckMissingScenesExpectNoMissing()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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

            try
            {
                var hasMissingScenesExpectFalse = BuildConfigurationSettingsValidator.HasMissingScenes();
                Assert.IsFalse(hasMissingScenesExpectFalse);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }

        [Test]
        public void CheckRepetitiveScenesExpectRepetitive()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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
                    Guid = testScene2.Guid,
                    Name = testScene2.Name
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

            try
            {
                var hasDuplicatesExpectTrue = BuildConfigurationSettingsValidator.HasScenesDuplicates();
                Assert.IsTrue(hasDuplicatesExpectTrue);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }

        [Test]
        public void CheckRepetitiveScenesExpectNoRepetitive()
        {
            var backup = new SettingsBackup();
            backup.BackUp();

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

            try
            {
                var hasDuplicatesExpectFalse = BuildConfigurationSettingsValidator.HasScenesDuplicates();
                Assert.IsFalse(hasDuplicatesExpectFalse);
            }
            finally
            {
                backup.RestoreSettings();
            }
        }
    }
}