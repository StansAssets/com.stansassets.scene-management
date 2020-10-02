using System;
using System.Collections.Generic;
using StansAssets.SceneManagement.Build;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    /// <summary>
    /// Provides methods to perform additive scenes loading.
    /// </summary>
    public static class AdditiveScenesLoader
    {
        /// <summary>
        /// Event is fired when scene was unloaded.
        /// </summary>
        public static event Action<Scene> SceneUnloaded = delegate { };

        /// <summary>
        /// Event is fired when new scene was loaded.
        /// </summary>
        public static event Action<Scene, LoadSceneMode> SceneLoaded = delegate { };

        static readonly List<Scene> s_AdditiveScenes = new List<Scene>();
        static readonly List<SceneInstance> s_AdditiveScenesInstances = new List<SceneInstance>();
        static readonly Dictionary<string, IAsyncOperation> s_LoadSceneOperations = new Dictionary<string, IAsyncOperation>();
        static readonly Dictionary<string, List<Action<Scene>>> s_LoadSceneRequests = new Dictionary<string, List<Action<Scene>>>();
        static readonly Dictionary<string, List<Action>> s_UnloadSceneCallbacks = new Dictionary<string, List<Action>>();

        static AdditiveScenesLoader()
        {
             SceneManager.sceneLoaded += AdditiveSceneLoaded;
             SceneManager.sceneUnloaded += SceneUnloadComplete;
        }

        /// <summary>
        /// Load Scene Additively by it's build index.
        /// </summary>
        /// <param name="sceneBuildIndex">Build index of he scene to be loaded.</param>
        /// <param name="loadCompleted">Load Completed callback.</param>
        /// <returns></returns>
        public static IAsyncOperation LoadAdditively(int sceneBuildIndex, Action<Scene> loadCompleted = null)
        {
            return LoadAdditively(string.Empty, sceneBuildIndex, loadCompleted);
        }

        /// <summary>
        /// Load Scene Additively by it's name.
        /// <param name="sceneName">Name of the scene to be loaded.</param>
        /// <param name="loadCompleted">Load Completed callback.</param>
        /// </summary>
        public static IAsyncOperation LoadAdditively(string sceneName, Action<Scene> loadCompleted = null)
        {
            if (!Application.isEditor && IsSceneAddressable(sceneName))
            {
                return LoadAddressableAdditively(sceneName, loadCompleted);
            }
            else {
                return LoadAdditively(sceneName, -1, loadCompleted);
            }
        }

        static IAsyncOperation LoadAdditively(string sceneName, int buildIndex,  Action<Scene> loadCompleted = null)
        {
            if (buildIndex != -1)
            {
                sceneName = buildIndex.ToString();
            }

            if (TryGetLoadedScene(sceneName, out var loadedScene))
            {
                loadCompleted?.Invoke(loadedScene);
                return s_LoadSceneOperations[sceneName];
            }
            if (!s_LoadSceneRequests.ContainsKey(sceneName))
            {
                var callbacks = new List<Action<Scene>>();
                if (loadCompleted != null)
                    callbacks.Add(loadCompleted);

                var loadAsyncOperation = buildIndex != -1
                    ? SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive)
                    : SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                var asyncWrapper = new AsyncOperationWrapper(loadAsyncOperation);

                s_LoadSceneRequests.Add(sceneName, callbacks);
                s_LoadSceneOperations.Add(sceneName, asyncWrapper);
                return asyncWrapper;
            }

            if (loadCompleted != null) {
                var callbacks = s_LoadSceneRequests[sceneName] ?? new List<Action<Scene>>();
                callbacks.Add(loadCompleted);
                s_LoadSceneRequests[sceneName] = callbacks;
            }

            return s_LoadSceneOperations[sceneName];
        }

        static IAsyncOperation LoadAddressableAdditively(string sceneName, Action<Scene> loadCompleted = null)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively call: {sceneName}");
            if (TryGetLoadedScene(sceneName, out var loadedScene))
            {
                AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively already loaded: {sceneName}");
                loadCompleted?.Invoke(loadedScene);
                return s_LoadSceneOperations[sceneName];
            }
            if (!s_LoadSceneRequests.ContainsKey(sceneName))
            {
                AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively start loading: {sceneName}");
                var callbacks = new List<Action<Scene>>();
                if (loadCompleted != null)
                    callbacks.Add(loadCompleted);


                var loadAsyncOperation = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                loadAsyncOperation.Completed += AdditiveAddressableSceneLoaded;
                var asyncWrapper = new AsyncOperationHandleWrapper<SceneInstance>(loadAsyncOperation);

                s_LoadSceneRequests.Add(sceneName, callbacks);
                s_LoadSceneOperations.Add(sceneName, asyncWrapper);
                return asyncWrapper;
            }

            if (loadCompleted != null) {
                var callbacks = s_LoadSceneRequests[sceneName] ?? new List<Action<Scene>>();
                callbacks.Add(loadCompleted);
                s_LoadSceneRequests[sceneName] = callbacks;
            }
            AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively already loading: {sceneName}");

            return s_LoadSceneOperations[sceneName];
        }

        /// <summary>
        /// Use GetSceneAsyncOperation to retrieve info about scene load progress.
        /// </summary>
        /// <param name="sceneName">Name of the scene.</param>
        /// <returns>Additive scene <see cref="AsyncOperation"/> or `null` if scene load was never requested. </returns>
        public static IAsyncOperation GetSceneAsyncOperation(string sceneName)
        {
            return s_LoadSceneOperations.ContainsKey(sceneName)
                ? s_LoadSceneOperations[sceneName]
                : null;
        }

        /// <summary>
        /// Use GetSceneAsyncOperation to retrieve info about scene load progress.
        /// </summary>
        /// <param name="sceneBuildIndex">Build index of he scene.</param>
        /// <returns>Additive scene <see cref="AsyncOperation"/> or `null` if scene load was never requested. </returns>
        public static IAsyncOperation GetSceneAsyncOperation(int sceneBuildIndex)
        {
            return GetSceneAsyncOperation(sceneBuildIndex.ToString());
        }

        /// <summary>
        /// Unload scene.
        /// <param name="scene">The scene to be loaded.</param>
        /// <param name="unloadCompleted">Unload Completed callback.</param>
        /// </summary>
        public static void Unload(Scene scene, Action unloadCompleted = null)
        {
            Unload(scene.name, unloadCompleted);
        }

        /// <summary>
        /// Destroys all GameObjects associated with the given Scene and removes the Scene from the SceneManager.
        /// <param name="sceneName">Name of the scene to be loaded.</param>
        /// <param name="unloadCompleted">Unload Completed callback.</param>
        /// </summary>
        public static void Unload(string sceneName, Action unloadCompleted = null)
        {
            if (!Application.isEditor && IsSceneAddressable(sceneName))
            {
                UnloadAddressable(sceneName, unloadCompleted);
            }
            else {
                Unload(sceneName, -1, unloadCompleted);
            }
        }

        /// <summary>
        /// Destroys all GameObjects associated with the given Scene and removes the Scene from the SceneManager.
        /// <param name="sceneBuildIndex">Build index of he scene.</param>
        /// <param name="unloadCompleted">Unload Completed callback.</param>
        /// </summary>
        public static void Unload(int sceneBuildIndex, Action unloadCompleted = null)
        {
            Unload(string.Empty, sceneBuildIndex, unloadCompleted);
        }

        static void Unload(string sceneName, int buildIndex, Action unloadCompleted = null)
        {
            if (!s_UnloadSceneCallbacks.ContainsKey(sceneName))
            {
                var callbacks = new List<Action>();
                if (unloadCompleted != null)
                    callbacks.Add(unloadCompleted);

                s_UnloadSceneCallbacks.Add(sceneName, callbacks);

                for (var i = 0; i < s_AdditiveScenes.Count; i++)
                {
                    if (s_AdditiveScenes[i].name == sceneName)
                    {
                        s_AdditiveScenes.Remove(s_AdditiveScenes[i]);
                        break;
                    }
                }

                if (buildIndex != -1)
                {
                    SceneManager.UnloadSceneAsync(buildIndex);
                }
                else
                {
                    SceneManager.UnloadSceneAsync(sceneName);
                }
            }
            else
            {
                var callbacks = s_UnloadSceneCallbacks[sceneName];
                if (unloadCompleted != null) {
                    if (callbacks == null)
                        callbacks = new List<Action>();
                    callbacks.Add(unloadCompleted);
                }
            }
        }

        static void UnloadAddressable(string sceneName, Action unloadCompleted = null)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable call: {sceneName}");
            if (!s_UnloadSceneCallbacks.ContainsKey(sceneName))
            {
                AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable unload start: {sceneName}");
                var callbacks = new List<Action>();
                if (unloadCompleted != null)
                    callbacks.Add(unloadCompleted);

                s_UnloadSceneCallbacks.Add(sceneName, callbacks);

                SceneInstance sceneInstance = default;
                bool sceneFound = false;
                for (var i = 0; i < s_AdditiveScenesInstances.Count; i++)
                {
                    AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable searching Scene: {s_AdditiveScenesInstances[i].Scene.name}");
                    if (s_AdditiveScenesInstances[i].Scene.name == sceneName)
                    {
                        AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable Scene found: {s_AdditiveScenesInstances[i].Scene.name}");
                        sceneFound = true;
                        sceneInstance = s_AdditiveScenesInstances[i];
                        s_AdditiveScenesInstances.Remove(sceneInstance);
                        break;
                    }
                }
                AddressablesLogger.Log("[ADDRESSABLES] UnloadAddressable Addressables.UnloadSceneAsync Scene: " + (sceneInstance.Scene.name ?? "NULL"));

                if (sceneFound)
                {
                    var addressableSceneUnloader = new AddressableSceneUnloader(sceneInstance);
                    addressableSceneUnloader.Unload(AddressableSceneUnloaded);
                }
                else
                {
                    if (s_LoadSceneOperations.ContainsKey(sceneName))
                    {
                        AddressablesLogger.LogWarning($"You are trying to unload {sceneName} scene, but it's loading is not complete yet!");
                    }
                    else
                    {
                        AddressablesLogger.LogWarning($"You are trying to unload {sceneName} scene, but it wasn't loaded!");
                    }
                }
            }
            else
            {
                AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable unload already started: {sceneName}");
                var callbacks = s_UnloadSceneCallbacks[sceneName];
                if (unloadCompleted != null) {
                    if (callbacks == null)
                        callbacks = new List<Action>();
                    callbacks.Add(unloadCompleted);
                }
            }
        }

        /// <summary>
        /// Method to find currently loaded scene by it's name.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="scene">Found scene. If methods returns `false` scene value will be set to `default`.</param>
        /// <returns>Returns `true` if scene was found among currently loaded scenes, `false` otherwise.</returns>
        public static bool TryGetLoadedScene(string sceneName, out Scene scene)
        {
            foreach (var additivelyLoadedScene in s_AdditiveScenes)
            {
                if (additivelyLoadedScene.name == sceneName)
                {
                    scene = additivelyLoadedScene;
                    return true;
                }
            }

            foreach (var additivelyLoadedScene in s_AdditiveScenesInstances)
            {
                if (additivelyLoadedScene.Scene.name == sceneName)
                {
                    scene = additivelyLoadedScene.Scene;
                    return true;
                }
            }

            scene = default;
            return false;
        }

        static void AdditiveSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            s_AdditiveScenes.Add(scene);
            SceneLoaded.Invoke(scene, mode);

            if (s_LoadSceneRequests.TryGetValue(scene.name, out var callbacks))
            {
                foreach (var callback in callbacks)
                    callback(scene);
                s_LoadSceneRequests.Remove(scene.name);
            }
        }

        static void SceneUnloadComplete(Scene scene)
        {
            SceneUnloaded.Invoke(scene);
            if (s_UnloadSceneCallbacks.TryGetValue(scene.name, out var callbacks))
            {
                foreach (var callback in callbacks)
                    callback();

                s_UnloadSceneCallbacks.Remove(scene.name);
            }

            s_LoadSceneOperations.Remove(scene.name);
        }

        static void AdditiveAddressableSceneLoaded(AsyncOperationHandle<SceneInstance> asyncOperation)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] AdditiveAddressableSceneLoaded Status: {asyncOperation.Status}, Scene: "  + (asyncOperation.Result.Scene.name ?? "NULL"));
            var scene = asyncOperation.Result.Scene;
            s_AdditiveScenes.Add(scene);
            s_AdditiveScenesInstances.Add(asyncOperation.Result);
            SceneLoaded.Invoke(scene, LoadSceneMode.Additive);

            if (s_LoadSceneRequests.TryGetValue(scene.name, out var callbacks))
            {
                foreach (var callback in callbacks)
                    callback(scene);
                s_LoadSceneRequests.Remove(scene.name);
            }
        }

        static void AddressableSceneUnloaded(AddressableSceneUnloaderResult result)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] AddressableSceneUnloaded Status: {result.AsyncOperationHandle.Status}, Scene: {result.SceneName}");
            SceneUnloaded.Invoke(result.Scene);

            if (s_UnloadSceneCallbacks.TryGetValue(result.SceneName, out var callbacks))
            {
                foreach (var callback in callbacks)
                    callback();

                s_UnloadSceneCallbacks.Remove(result.SceneName);
            }

            s_LoadSceneOperations.Remove(result.SceneName);
        }

        static bool IsSceneAddressable(string sceneName)
        {
            return BuildConfigurationSettings.Instance.Configuration.IsSceneAddressable(sceneName);
        }
    }

    static class AddressablesLogger
    {
        public static bool Verbose = false;

        public static void Log(string msg)
        {
            if (Verbose)
            {
                Debug.Log(msg);
            }
        }

        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }
    }
}