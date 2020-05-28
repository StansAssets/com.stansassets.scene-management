using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    [InitializeOnLoad]
    static class StartLandingAction
    {
        static StartLandingAction()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void Execute()
        {
            if (SceneManagementSettings.Instance.LandingScene == null)
            {
                Debug.LogWarning("Please define the LandingScene scene");
                SceneManagementEditorMenu.OpenSettings();
                return;
            }

            if(!Application.isPlaying)
            {
                var userFinishedOperation =  EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                if(!userFinishedOperation)
                    return;

                var countLoaded = SceneManager.sceneCount;
                SceneManagementSettings.Instance.OpenScenesBeforeLandingStart = new List<SceneStateInfo>();
                for (var i = 0; i < countLoaded; i++)
                {
                    SceneManagementSettings.Instance.OpenScenesBeforeLandingStart.Add(new SceneStateInfo(SceneManager.GetSceneAt(i)));
                }
                //SceneManagementSettings.Instance.ActiveSceneIndex =

                var landingScenePath = AssetDatabase.GetAssetPath(SceneManagementSettings.Instance.LandingScene);
                EditorSceneManager.OpenScene(landingScenePath);
            }

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        // TODO would be nice to restore completely
        // - landing stat & position
        // - active scene
        // - active camera and scene view position

        static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode )
            {
                if (SceneManagementSettings.Instance.OpenScenesBeforeLandingStart != null
                    && SceneManagementSettings.Instance.OpenScenesBeforeLandingStart.Count > 0)
                {
                    foreach (var sceneStateInfo in SceneManagementSettings.Instance.OpenScenesBeforeLandingStart)
                    {
                        if(string.IsNullOrEmpty(sceneStateInfo.Path))
                            continue;

                        EditorSceneManager.OpenScene(sceneStateInfo.Path, sceneStateInfo.WasLoaded
                            ? OpenSceneMode.Additive
                            : OpenSceneMode.AdditiveWithoutLoading);
                    }

                    SceneManagementSettings.Instance.OpenScenesBeforeLandingStart = null;
                    EditorSceneManager.CloseScene(SceneManager.GetSceneAt(0), true);
                }
            }
        }
    }
}
