#if !BUILD_SYSTEM_ENABLED

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace StansAssets.SceneManagement.Build
{
    class RegisterBuildPlayerHandler : IPreprocessBuildWithReport
    {
        public int callbackOrder => -101;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildScenesPreprocessor.PrebuildCleanup();
        }
    }
}

#endif