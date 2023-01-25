using System;
using UnityEditor;

namespace Scenes.Editor
{
    [Serializable]
    public class BuildTargetGroupModel
    {
        public BuildTargetGroup BuildTargetGroup;
        public BuildTarget[] BuildTargets;
        public string IconName;
    
        public BuildTargetGroupModel(BuildTargetGroup buildTargetGroup, BuildTarget[] buildTargets, string iconName)
        {
            BuildTargetGroup = buildTargetGroup;
            BuildTargets = buildTargets;
            IconName = iconName;
        }
    }
}