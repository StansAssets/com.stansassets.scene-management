#if UNITY_2019_4_OR_NEWER
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

            if (!Application.isPlaying)
            {
                var userFinishedOperation = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                if (!userFinishedOperation)
                    return;

                SceneManagementSettings.Instance.OpenScenesBeforeLandingStart = new List<SceneStateInfo>();
                var loadedBeforeScenes = GetScenesList();
                foreach (var scene in loadedBeforeScenes)
                {
                    SceneManagementSettings.Instance.OpenScenesBeforeLandingStart.Add(new SceneStateInfo(scene));
                }

                SceneManagementSettings.Instance.LastActiveSceneIndex = loadedBeforeScenes.IndexOf(SceneManager.GetActiveScene());

                var lastView = SceneView.lastActiveSceneView;
                if (lastView == null)
                {
                    SceneManagementSettings.Instance.LastSceneView = new SceneViewInfo();
                }
                else
                {
                    SceneManagementSettings.Instance.LastSceneView = new SceneViewInfo
                    (
                        lastView.camera.transform.position,
                        lastView.pivot,
                        lastView.rotation,
                        lastView.size,
                        lastView.in2DMode,
                        lastView.orthographic
                    );
                }

                var landingScenePath = AssetDatabase.GetAssetPath(SceneManagementSettings.Instance.LandingScene);
                EditorSceneManager.OpenScene(landingScenePath);
            }

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        static List<Scene> GetScenesList()
        {
            List<Scene> scenes = new List<Scene>();
            for (int i = 0, count = SceneManager.sceneCount; i < count; i++)
            {
                scenes.Add(SceneManager.GetSceneAt(i));
            }

            return scenes;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (SceneManagementSettings.Instance.OpenScenesBeforeLandingStart != null
                    && SceneManagementSettings.Instance.OpenScenesBeforeLandingStart.Count > 0)
                {
                    int startIndex = 0;
                    var landingScenePath = AssetDatabase.GetAssetPath(SceneManagementSettings.Instance.LandingScene);
                    var firstSceneInfo = SceneManagementSettings.Instance.OpenScenesBeforeLandingStart[0];

                    // The 1st scene in hierarchy should be landing scene, and we need to unload it (to re-open saved scenes).
                    // But we can't unload active scene, so we switch active scene to 2nd, and then unload 1st.
                    // (Except the case when 1st saved scene is landing itself)
                    if (firstSceneInfo.Path != landingScenePath)
                    {
                        // Forced open scene additive, so it can be set as active
                        EditorSceneManager.OpenScene(firstSceneInfo.Path, OpenSceneMode.Additive);
                        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
                        EditorSceneManager.CloseScene(SceneManager.GetSceneAt(0), true);
                        startIndex = 1;
                    }

                    // Next, we load all scenes in order
                    for (int i = startIndex; i < SceneManagementSettings.Instance.OpenScenesBeforeLandingStart.Count; i++)
                    {
                        OpenScene(SceneManagementSettings.Instance.OpenScenesBeforeLandingStart[i]);
                    }

                    SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManagementSettings.Instance.LastActiveSceneIndex));

                    // If first scene isn't actually loaded, we unload it
                    if (!firstSceneInfo.WasLoaded)
                        EditorSceneManager.CloseScene(SceneManager.GetSceneAt(0), false);

                    var info = SceneManagementSettings.Instance.LastSceneView;
                    if (info != null && ! info.IsDefault)
                    {
                        SceneView.lastActiveSceneView.in2DMode = info.Is2D;
                        SceneView.lastActiveSceneView.LookAt(info.Pivot, info.Rotation, info.Size, info.IsOrtho);
                    }

                    SceneManagementSettings.Instance.OpenScenesBeforeLandingStart = null;
                }
            }
        }

        static void OpenScene(SceneStateInfo sceneStateInfo)
        {
            if (string.IsNullOrEmpty(sceneStateInfo.Path))
                return;

            EditorSceneManager.OpenScene(sceneStateInfo.Path, sceneStateInfo.WasLoaded
                ? OpenSceneMode.Additive
                : OpenSceneMode.AdditiveWithoutLoading);
        }
    }
}
#endif