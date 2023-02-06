using System;
using System.Collections.Generic;
using StansAssets.SceneManagement.Build;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    /// <summary>
    /// Scene loading operation status.
    /// </summary>
    public enum OperationStatus
    {
        Unknown,
        Fail,
        Success
    }

    /// <summary>
    /// Scene load operation arguments.
    /// </summary>
    public struct SceneLoadOperationArgs
    {
        /// <summary>
        /// Loaded scene instance.
        /// </summary>
        public Scene Scene;

        /// <summary>
        /// Scene loading operation status.
        /// </summary>
        public OperationStatus Status;
    }

    /// <summary>
    /// Provides methods to perform additive scenes loading.
    /// </summary>
    public static class AdditiveScenesLoader
    {
        /// <summary>
        /// Event is fired when scene was unloaded.
        /// The reason why it has <see cref="SceneInfo"/> type and not <see cref="Scene"/> is because when working with addressable
        /// ofter we have scene with no name and no metadata since those scene are not included in the build.
        ///
        /// Let us know if you would like to have more metadata inside.
        /// </summary>
        public static event Action<SceneInfo> SceneUnloaded = delegate { };

        /// <summary>
        /// Event is fired when new scene was loaded.
        /// </summary>
        public static event Action<Scene, LoadSceneMode> SceneLoaded = delegate { };

        static readonly List<Scene> s_AdditiveScenes = new List<Scene>();
        static readonly List<SceneInstance> s_AdditiveScenesInstances = new List<SceneInstance>();
        static readonly Dictionary<string, IAsyncOperation> s_LoadSceneOperations = new Dictionary<string, IAsyncOperation>();
        static readonly Dictionary<string, List<Action<SceneLoadOperationArgs>>> s_LoadSceneRequests = new Dictionary<string, List<Action<SceneLoadOperationArgs>>>();
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
        public static IAsyncOperation LoadAdditively(int sceneBuildIndex, Action<SceneLoadOperationArgs> loadCompleted = null)
        {
            return LoadAdditively(string.Empty, sceneBuildIndex, loadCompleted);
        }

        /// <summary>
        /// Load Scene Additively by it's name.
        /// Method will bypass scene configuration availability check
        /// <param name="sceneName">Name of the scene to be loaded.</param>
        /// <param name="loadCompleted">Load Completed callback.</param>
        /// </summary>
        public static IAsyncOperation LoadAdditively(string sceneName, Action<SceneLoadOperationArgs> loadCompleted = null)
        {
            if (ValidateScene(sceneName) == false)
            {
                throw new ArgumentException($"Build Configuration doesn't contain scene: {sceneName}." +
                    $"\nTo load a scene please add it to platform specific collection or Default scenes.");
            }

            if (IsSceneAddressable(sceneName))
            {
                return LoadAddressableAdditively(sceneName, loadCompleted);
            }

            return LoadAdditively(sceneName, -1, loadCompleted);
        }

        static IAsyncOperation LoadAdditively(string sceneName, int buildIndex, Action<SceneLoadOperationArgs> loadCompleted = null)
        {
            if (buildIndex != -1)
            {
                // Our loaded / load requests scenes cache is using string key
                // so we need to make sceneName from index just to make sure our cache will work.
                sceneName = buildIndex.ToString();
            }

            if (TryGetLoadedScene(sceneName, out var loadedScene)) {
                loadCompleted?.Invoke(new SceneLoadOperationArgs {
                    Scene = loadedScene,
                    Status = OperationStatus.Success
                });
                return s_LoadSceneOperations[sceneName];
            }

            if (!s_LoadSceneRequests.ContainsKey(sceneName))
            {
                var callbacks = new List<Action<SceneLoadOperationArgs>>();
                if (loadCompleted != null)
                    callbacks.Add(loadCompleted);

                var loadAsyncOperation = buildIndex != -1
                    ? SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive)
                    : SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                var asyncWrapper = new AsyncOperationWrapper(sceneName, loadAsyncOperation);

                s_LoadSceneRequests.Add(sceneName, callbacks);
                s_LoadSceneOperations.Add(sceneName, asyncWrapper);
                return asyncWrapper;
            }

            if (loadCompleted != null)
            {
                var callbacks = s_LoadSceneRequests[sceneName] ?? new List<Action<SceneLoadOperationArgs>>();
                callbacks.Add(loadCompleted);
                s_LoadSceneRequests[sceneName] = callbacks;
            }

            return s_LoadSceneOperations[sceneName];
        }

        static IAsyncOperation LoadAddressableAdditively(string sceneName, Action<SceneLoadOperationArgs> loadCompleted = null)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively call: {sceneName}");
            if (TryGetLoadedScene(sceneName, out var loadedScene))
            {
                AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively already loaded: {sceneName}");
                loadCompleted?.Invoke(new SceneLoadOperationArgs {
                    Scene = loadedScene,
                    Status = OperationStatus.Success
                });
                return s_LoadSceneOperations[sceneName];
            }

            if (!s_LoadSceneRequests.ContainsKey(sceneName))
            {
                AddressablesLogger.Log($"[ADDRESSABLES] LoadAddressableAdditively start loading: {sceneName}");
                var callbacks = new List<Action<SceneLoadOperationArgs>>();
                if (loadCompleted != null)
                    callbacks.Add(loadCompleted);

                var loadAsyncOperation = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                var asyncWrapper = new AsyncOperationHandleWrapper(sceneName, loadAsyncOperation);
                asyncWrapper.OnComplete += AdditiveAddressableSceneLoaded;

                s_LoadSceneRequests.Add(sceneName, callbacks);
                s_LoadSceneOperations.Add(sceneName, asyncWrapper);
                return asyncWrapper;
            }

            if (loadCompleted != null)
            {
                var callbacks = s_LoadSceneRequests[sceneName] ?? new List<Action<SceneLoadOperationArgs>>();
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
            if (IsSceneAddressable(sceneName))
            {
                UnloadAddressable(sceneName, unloadCompleted);
            }
            else
            {
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
                if (unloadCompleted != null)
                {
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
                        s_AdditiveScenes.Remove(sceneInstance.Scene);
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
                    AddressablesLogger.LogWarning(s_LoadSceneOperations.ContainsKey(sceneName) ? $"You are trying to unload {sceneName} scene, but it's loading is not complete yet!" : $"You are trying to unload {sceneName} scene, but it wasn't loaded!");
                }
            }
            else
            {
                AddressablesLogger.Log($"[ADDRESSABLES] UnloadAddressable unload already started: {sceneName}");
                var callbacks = s_UnloadSceneCallbacks[sceneName];
                if (unloadCompleted != null)
                {
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
            // Skip addressable scenes
            if (IsSceneAddressable(scene.name))
            {
                return;
            }

            ProcessSceneLoadSuccess(scene);
        }

        static void AdditiveAddressableSceneLoaded(IAsyncOperation asyncOperation)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] AdditiveAddressableSceneLoaded Status: {asyncOperation.Status}," +
                                   "Scene: " + (asyncOperation.Status == OperationStatus.Unknown ? asyncOperation.SceneName : "NULL"));

            if (asyncOperation.Status == OperationStatus.Success) {
                s_AdditiveScenesInstances.Add(asyncOperation.SceneInstance);
                ProcessSceneLoadSuccess(asyncOperation.SceneInstance.Scene);
            }
            else {
                ProcessSceneLoadFail(asyncOperation.SceneName);
            }
        }

        static void ProcessSceneLoadFail(string sceneName)
        {
            DispatchSceneLoadRequests(sceneName, new SceneLoadOperationArgs {
                Status = OperationStatus.Fail
            });
        }

        static void ProcessSceneLoadSuccess(Scene scene)
        {
            s_AdditiveScenes.Add(scene);
            DispatchSceneLoadRequests(scene.name, new SceneLoadOperationArgs {
                Scene = scene,
                Status = OperationStatus.Success
            });

            SceneLoaded.Invoke(scene, LoadSceneMode.Additive);
        }

        static void DispatchSceneLoadRequests(string sceneName, SceneLoadOperationArgs args) {
            if (s_LoadSceneRequests.TryGetValue(sceneName, out var callbacks))
            {
                s_LoadSceneRequests.Remove(sceneName);
                foreach (var callback in callbacks)
                    callback(args);
            }
        }

        static void SceneUnloadComplete(Scene scene)
        {
            // Skip addressable scenes
            if (IsSceneAddressable(scene.name))
            {
                return;
            }

            ProcessSceneUnLoad(scene.name);
        }

        static void AddressableSceneUnloaded(AddressableSceneUnloaderResult result)
        {
            AddressablesLogger.Log($"[ADDRESSABLES] AddressableSceneUnloaded Status: {result.AsyncOperationHandle.Status}, Scene: {result.SceneName}");
            ProcessSceneUnLoad(result.SceneName);
        }

        static void ProcessSceneUnLoad(string sceneName)
        {
            s_LoadSceneOperations.Remove(sceneName);
            if (s_UnloadSceneCallbacks.TryGetValue(sceneName, out var callbacks))
            {
                s_UnloadSceneCallbacks.Remove(sceneName);
                foreach (var callback in callbacks)
                    callback();
            }

            SceneUnloaded.Invoke(new SceneInfo(sceneName));
        }

        static bool IsSceneAddressable(string sceneName) {
            var conf = BuildConfigurationSettings.Instance.Configuration;

            if (Application.isEditor && !conf.UseAddressablesInEditor) {
                return false;
            }

            return BuildConfigurationSettings.Instance.Configuration.IsSceneAddressable(sceneName);
        }

        static bool ValidateScene(string sceneName)
        {
            return BuildConfigurationSettings.Instance.Configuration.HasScene(sceneName);
        }
    }

    static class AddressablesLogger
    {
        public static readonly bool Verbose = true;

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
