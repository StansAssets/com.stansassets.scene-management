using System.Collections.Generic;
using UnityEditor;

namespace BuildConfigurator.Runtime
{
    public class BuildTargetGroupData
    {
        public BuildTargetGroupModel[] ValidPlatforms;
        BuildTargetGroupModel[] m_Groups;

        public BuildTargetGroupData()
        {
            GetBuildTargetGroups();
            ValidPlatforms = GetInstalledPlatforms();
        }

        private BuildTargetGroupModel[] GetInstalledPlatforms()
        {
            var buildTargetGroupModels = new List<BuildTargetGroupModel>();
            for (var i = 0; i < m_Groups.Length; i++)
            {
                BuildTargetGroupModel buildTargetGroup = m_Groups[i];
                for (var j = 0; j < buildTargetGroup.targets.Length; j++)
                {
                    BuildTarget target = buildTargetGroup.targets[j];
                    if (BuildPipeline.IsBuildTargetSupported(buildTargetGroup.group, target))
                    {
                        buildTargetGroupModels.Add(buildTargetGroup);
                        break;
                    }
                }
            }

            return buildTargetGroupModels.ToArray();
        }

        void GetBuildTargetGroups()
        {
            m_Groups = new BuildTargetGroupModel[]
            {
                new BuildTargetGroupModel(
                    BuildTargetGroup.Standalone,
                    new BuildTarget[]
                    {
                        BuildTarget.StandaloneWindows,
                        BuildTarget.StandaloneWindows64,
                        BuildTarget.StandaloneOSX,
                        BuildTarget.StandaloneLinux64,
                    }, "BuildSettings.Standalone.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.iOS,
                    new BuildTarget[] { BuildTarget.iOS }, "d_BuildSettings.iPhone.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.tvOS,
                    new BuildTarget[] { BuildTarget.tvOS }, "BuildSettings.tvOS.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Android,
                    new BuildTarget[] { BuildTarget.Android }, "BuildSettings.Android.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.WebGL,
                    new BuildTarget[] { BuildTarget.WebGL }, "BuildSettings.WebGL.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.PS4,
                    new BuildTarget[] { BuildTarget.PS4 }, "BuildSettings.PS4.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.XboxOne,
                    new BuildTarget[] { BuildTarget.XboxOne }, "BuildSettings.XboxOne.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Switch,
                    new BuildTarget[] { BuildTarget.Switch }, "BuildSettings.Switch.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Lumin,
                    new BuildTarget[] { BuildTarget.Lumin }, "BuildSettings.Lumin.small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Stadia,
                    new BuildTarget[] { BuildTarget.Stadia }, "BuildSettings.Stadia.small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Unknown,
                    new BuildTarget[] { BuildTarget.NoTarget }, "BuildSettings.StandaloneGLESEmu.Small"),
            };
        }
    }
}