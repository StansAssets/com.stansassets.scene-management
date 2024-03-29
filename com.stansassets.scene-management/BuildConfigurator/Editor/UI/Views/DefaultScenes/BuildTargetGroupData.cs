﻿using System.Collections.Generic;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    class BuildTargetGroupData
    {
        public readonly BuildTargetGroupModel[] ValidPlatforms;
        BuildTargetGroupModel[] m_Groups;

        public BuildTargetGroupData()
        {
            GetBuildTargetGroups();
            ValidPlatforms = GetInstalledPlatforms();
        }

        BuildTargetGroupModel[] GetInstalledPlatforms()
        {
            var buildTargetGroupModels = new List<BuildTargetGroupModel>();
            for (var i = 0; i < m_Groups.Length; i++)
            {
                BuildTargetGroupModel buildTargetGroup = m_Groups[i];
                for (var j = 0; j < buildTargetGroup.BuildTargets.Length; j++)
                {
                    BuildTarget target = buildTargetGroup.BuildTargets[j];
                    if (BuildPipeline.IsBuildTargetSupported(buildTargetGroup.BuildTargetGroup, target))
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
#if !UNITY_2022_2_OR_NEWER
                new BuildTargetGroupModel(
                    BuildTargetGroup.Lumin,
                    new BuildTarget[] { BuildTarget.Lumin }, "BuildSettings.Lumin.small"),
#endif
                new BuildTargetGroupModel(
                    BuildTargetGroup.Stadia,
                    new BuildTarget[] { BuildTarget.Stadia }, "BuildSettings.Stadia.small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.PS5,
                    new BuildTarget[] { BuildTarget.PS5 }, "BuildSettings.PS5.Small"),
                new BuildTargetGroupModel(
                    BuildTargetGroup.Unknown,
                    new BuildTarget[] { BuildTarget.NoTarget }, "BuildSettings.StandaloneGLESEmu.Small"),
            };
        }
    }
}
