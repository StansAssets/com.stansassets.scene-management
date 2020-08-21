using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 using UnityEngine.AddressableAssets;

namespace StansAssets.SceneManagement
{
    public class SceneLoadService : ISceneLoadService
    {
        private Dictionary<string, Scene?> loadedScenes = new Dictionary<string, Scene?>();
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

        public void LoadFromAddresables(string sceneName, Action<ISceneManager> onComplete)
        {
            if (!loadedScenes.ContainsKey(sceneName))
            {
                loadedScenes[sceneName]=null;
                Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).Completed += (sceneRequest) =>{
                    if (sceneRequest.Status == AsyncOperationStatus.Succeeded)
                    {
                        var scene = sceneRequest.Result.Scene;
                        var sceneManager = FindMonoTypeOnSceneRoot<ISceneManager>(scene);
                        var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                        loadedScenes[sceneName] = scene;
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
                    }
                    else
                    {
                        loadedScenes.Remove(sceneName);
                    }
                };
            }
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

        public void Deactivate(string sceneName, Action onComplete)
        {
            if (AdditiveScenesLoader.TryGetLoadedScene(sceneName, out var scene))
            {
                var sceneDelegate = FindMonoTypeOnSceneRoot<ISceneDelegate>(scene);
                sceneDelegate?.DeactivateScene(() =>
                {
                    onComplete?.Invoke();
                });
            }
            else
            {
                //TODO error and optional param with scene action stack
                onComplete?.Invoke();
            }
        }

        public void UnloadFromAddresables(string sceneName, Action onComplete)
        {
            //loadedScenes[sceneName] may be null while scene is loading.
            if (loadedScenes.ContainsKey(sceneName) && loadedScenes[sceneName]!=null)
            {
                var scene = loadedScenes[sceneName].Value;
                loadedScenes.Remove(sceneName);
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
