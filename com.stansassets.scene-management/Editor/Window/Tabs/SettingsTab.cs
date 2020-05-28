using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class SettingsTab : BaseTab
    {
        public SettingsTab()
            : base($"{SceneManagementPackage.WindowTabsPath}/SettingsTab.uxml")
        {
            var landingSceneField = Root.Q<ObjectField>("landing-scene");
            landingSceneField.objectType = typeof(SceneAsset);
            landingSceneField.SetValueWithoutNotify(SceneManagementSettings.Instance.LandingScene);

            landingSceneField.RegisterValueChangedCallback((e) =>
            {
                SceneManagementSettings.Instance.LandingScene = (SceneAsset)e.newValue;
                SceneManagementSettings.Save();
            });
        }
    }
}
