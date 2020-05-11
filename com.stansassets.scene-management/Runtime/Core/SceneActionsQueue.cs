using System;
using System.Collections.Generic;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    public class SceneActionsQueue
    {
        public enum ActionType
        {
            Load,
            Unload,
            Deactivate,
        }

        struct ActionData
        {
            public string SceneName;
            public ActionType Type;
        }

        IScenePreloader m_Preloader;
        readonly ISceneLoadService m_SceneLoadService;
        readonly Stack<ActionData> m_ActionsStack = new Stack<ActionData>();
        readonly List<ISceneManager> m_SceneManagers = new List<ISceneManager>();

        public SceneActionsQueue(ISceneLoadService sceneLoadService)
        {
            m_SceneLoadService = sceneLoadService;
        }

        public void SetPreloader(IScenePreloader preloader)
        {
            m_Preloader = preloader;
        }

        public void AddAction(ActionType type, string sceneName)
        {
            var data = new ActionData
            {
                Type = type,
                SceneName = sceneName
            };

            m_ActionsStack.Push(data);
        }


        public void Start(Action onComplete = null)
        {
            m_SceneManagers.Clear();
            if (m_Preloader != null)
            {
                m_Preloader.FadeIn(() =>
                {
                    ExecuteActionsStack(() =>
                    {
                        m_Preloader.FadeOut(() =>
                        {
                            onComplete?.Invoke();
                        });
                    });
                });
            }
            else
            {
                ExecuteActionsStack(onComplete);
            }
        }

        public T GetLoadedSceneManager<T>() where T : ISceneManager
        {
            foreach (var sceneManager in m_SceneManagers)
            {
                if (sceneManager.GetType() == typeof(T))
                    return (T)sceneManager;
            }

            return default;
        }

        void ExecuteActionsStack(Action onComplete)
        {
            if (m_ActionsStack.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var actionData = m_ActionsStack.Pop();
            switch (actionData.Type)
            {
                case ActionType.Load:
                    m_SceneLoadService.Load(actionData.SceneName, sceneManager =>
                    {
                        if(sceneManager != null)
                            m_SceneManagers.Add(sceneManager);

                        ExecuteActionsStack(onComplete);
                    });
                    break;
                case ActionType.Deactivate:
                    m_SceneLoadService.Deactivate(actionData.SceneName, () =>
                    {
                        ExecuteActionsStack(onComplete);
                    });
                    break;
                case ActionType.Unload:
                    m_SceneLoadService.Unload(actionData.SceneName, () =>
                    {
                        ExecuteActionsStack(onComplete);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
