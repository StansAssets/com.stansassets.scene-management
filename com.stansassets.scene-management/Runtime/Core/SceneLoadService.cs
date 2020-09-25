using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    public class SceneLoadService : ISceneLoadService
    {
        public void Load(IScenePreloader preloader, string sceneName, Action<ISceneManager> onComplete)
        {
            preloader.FadeIn(() =>
            {
                Load(sceneName, sceneManager =>
                {
                    preloader.FadeOut(() =>
                    {
                        onComplete?.Invoke(null);
                    });
                });
            });
        }

        public void Load(string sceneName, Action<ISceneManager> onComplete)
        {
            AdditiveScenesLoader.LoadAdditively(sceneName, scene =>
            {
                var sceneManager = FindMonoTypeOnSceneRoot<ISceneManager>(scene);
                var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                if (sceneDelegate != null)
                {
                    sceneDelegate.OnSceneLoaded();
                    sceneDelegate.ActivateScene(() =>
                    {
                        onComplete?.Invoke(sceneManager);
                    });
                }
                else
                {
                    onComplete?.Invoke(sceneManager);
                }
            });
        }

        public void Deactivate(string sceneName, Action<ISceneManager> onComplete)
        {
            if (AdditiveScenesLoader.TryGetLoadedScene(sceneName, out var scene))
            {
                var sceneManager = FindMonoTypeOnSceneRoot<ISceneManager>(scene);
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
                var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                sceneDelegate?.OnSceneUnload();
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
