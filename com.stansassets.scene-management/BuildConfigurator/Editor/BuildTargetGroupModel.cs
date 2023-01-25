using System;
using UnityEditor;

namespace Scenes.Editor
{
    [Serializable]
    public class BuildTargetGroupModel
    {
        public BuildTargetGroup group;
        public BuildTarget[] targets;
        public string iconName;
    
        public BuildTargetGroupModel(BuildTargetGroup group, BuildTarget[] targets, string iconName)
        {
            this.group = group;
            this.targets = targets;
            this.iconName = iconName;
        }
    }
}