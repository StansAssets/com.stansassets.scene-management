using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    public class SceneLoadService : ISceneLoadService
    {
        public void Load<T>(IScenePreloader preloader, string sceneName, Action<Scene, T> onComplete) where T : ISceneManager
        {
            preloader.FadeIn(() =>
            {
                Load<T>(sceneName, (scene, sceneManager) =>
                {
                    preloader.FadeOut(() =>
                    {
                        onComplete?.Invoke(scene, sceneManager);
                    });
                });
            });
        }

        public void Load<T>(string sceneName, Action<Scene, T> onComplete) where T : ISceneManager
        {
            AdditiveScenesLoader.LoadAdditively(sceneName, scene =>
            {
                var sceneManager = FindMonoTypeOnSceneRoot<T>(scene);
                var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                if (sceneDelegate != null)
                {
                    sceneDelegate.ActivateScene(() =>
                    {
                        onComplete?.Invoke(scene, sceneManager);
                    });
                }
                else
                {
                    onComplete?.Invoke(scene, sceneManager);
                }
            });
        }

        public void Deactivate<T>(string sceneName, Action<T> onComplete) where T : ISceneManager
        {
            if (AdditiveScenesLoader.TryGetLoadedScene(sceneName, out var scene))
            {
                var sceneManager = FindMonoTypeOnSceneRoot<T>(scene);
                var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                sceneDelegate?.DeactivateScene(() =>
                {
                    onComplete?.Invoke(sceneManager);
                });
            }
            else
            {
               throw new InvalidOperationException($"{nameof(SceneLoadService)} can not deactivate {sceneName} scene, because it wasn't loaded. ");
            }
        }

        public void Unload(string sceneName, Action onComplete)
        {
            if (AdditiveScenesLoader.TryGetLoadedScene(sceneName, out var scene))
            {
                AdditiveScenesLoader.Unload(scene, onComplete);
            }
            else
            {
                // TODO error and optional param with scene action stack
                onComplete?.Invoke();
            }
        }

        T FindMonoTypeOnSceneRoot<T>(Scene scene)
        {
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                foreach (var mono in gameObject.GetComponents<MonoBehaviour>())
                {
                    if (mono is T sceneDelegate)
                        return sceneDelegate;
                }
            }

            return default;
        }
    }
}
