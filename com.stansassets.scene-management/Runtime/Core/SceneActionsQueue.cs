using System;
using System.Collections;
using System.Collections.Generic;
using StansAssets.Foundation.Async;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    public class SceneActionsQueue
    {
        IScenePreloader m_Preloader;
        IAsyncOperation m_CurrentAsyncOperation;
        bool m_IsRunning;
        event Action OnComplete;
        event Action<float> OnProgress;

        readonly ISceneLoadService m_SceneLoadService;
        readonly Queue<SceneAction> m_ActionsQueue = new Queue<SceneAction>();

        public Dictionary<string, ISceneManager> AvailableSceneManagers { get; } = new Dictionary<string, ISceneManager>();
        public Dictionary<string, Scene> LoadedScenes { get; } = new Dictionary<string, Scene>();


        public IEnumerable<SceneAction> ScheduledActions => m_ActionsQueue;

        public SceneActionsQueue(ISceneLoadService sceneLoadService)
        {
            m_SceneLoadService = sceneLoadService;
        }

        /// <summary>
        /// Sets preloader implementation.
        /// The `1f` will be artificially sent to <see cref="IScenePreloader.OnProgress"/> once scene load completed.
        /// </summary>
        /// <param name="preloader">Preloader implementation</param>
        public void SetPreloader(IScenePreloader preloader)
        {
            m_Preloader = preloader;
        }

        public void AddAction(SceneActionType type, string sceneName)
        {
            var data = new SceneAction
            {
                Type = type,
                SceneName = sceneName
            };

            m_ActionsQueue.Enqueue(data);
        }

        public void Start(Action<float> onProgress = null, Action onComplete = null)
        {
            AvailableSceneManagers.Clear();
            LoadedScenes.Clear();
            OnComplete = onComplete;
            OnProgress = onProgress;
            if (m_Preloader != null)
            {
                m_Preloader.FadeIn(() =>
                {
                    StartActionsStack(() =>
                    {
                        onProgress?.Invoke(1f);
                        m_Preloader.OnProgress(1f);
                        m_Preloader.FadeOut(Complete);
                    });
                });
            }
            else
            {
                StartActionsStack(Complete);
            }
        }

        void Complete()
        {
            OnProgress?.Invoke(1f);
            OnComplete?.Invoke();

            OnComplete = null;
            OnProgress = null;
        }

        public T GetLoadedSceneManager<T>() where T : ISceneManager
        {
            foreach (var kvp in AvailableSceneManagers)
            {
                var sceneManager = kvp.Value;
                var sceneManagerType = sceneManager.GetType();
                if (sceneManagerType == typeof(T) || typeof(T).IsAssignableFrom(sceneManagerType))
                    return (T)sceneManager;
            }

            return default;
        }

        public Scene GetLoadedScene(string sceneName)
        {
            return LoadedScenes[sceneName];
        }

        public IEnumerator OnStackProgress()
        {
            while (m_IsRunning)
            {
                if (m_CurrentAsyncOperation != null)
                {
                    m_Preloader?.OnProgress(m_CurrentAsyncOperation.Progress);
                    OnProgress?.Invoke(m_CurrentAsyncOperation.Progress);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        void StartActionsStack(Action onComplete)
        {
            m_IsRunning = true;
            CoroutineUtility.Start(OnStackProgress());
            ExecuteActionsStack(onComplete);
        }

        void ExecuteActionsStack(Action onComplete)
        {
            if (m_ActionsQueue.Count == 0)
            {
                m_IsRunning = false;
                CoroutineUtility.Stop(OnStackProgress());
                onComplete?.Invoke();
                return;
            }

            var actionData = m_ActionsQueue.Dequeue();
            switch (actionData.Type)
            {
                case SceneActionType.Load:
                    m_SceneLoadService.Load<ISceneManager>(actionData.SceneName, (scene, sceneManager) =>
                    {
                        if (sceneManager != null)
                            AvailableSceneManagers[actionData.SceneName] = sceneManager;

                        LoadedScenes.Add(actionData.SceneName, scene);
                        ExecuteActionsStack(onComplete);
                    });

                    m_CurrentAsyncOperation = AdditiveScenesLoader.GetSceneAsyncOperation(actionData.SceneName);
                    break;
                case SceneActionType.Deactivate:
                    m_SceneLoadService.Deactivate<ISceneManager>(actionData.SceneName, (sceneManager) =>
                    {
                        if (sceneManager != null)
                            AvailableSceneManagers[actionData.SceneName] = sceneManager;

                        ExecuteActionsStack(onComplete);
                    });
                    break;
                case SceneActionType.Unload:
                    m_SceneLoadService.Unload(actionData.SceneName, () =>
                    {
                        AvailableSceneManagers.Remove(actionData.SceneName);
                        LoadedScenes.Remove(actionData.SceneName);
                        ExecuteActionsStack(onComplete);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
