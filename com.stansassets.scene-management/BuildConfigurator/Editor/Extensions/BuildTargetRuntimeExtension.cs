using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    public static class BuildTargetRuntimeExtension
    {
        public static BuildTargetRuntime ToBuildTargetRuntime(this BuildTarget target)
        {
            return (BuildTargetRuntime) target;
        } 
    }
}