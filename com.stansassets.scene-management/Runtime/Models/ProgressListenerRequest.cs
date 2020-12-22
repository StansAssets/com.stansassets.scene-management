using System;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    class ProgressListenerRequest : IProgressReporter
    {
        public event Action OnComplete;
        public event Action OnProgressChange;

        bool m_IsDone;

        public float Progress { get; protected set; }


        public void UpdateProgress(float v)
        {
            if (m_IsDone)
                return;

            SetProgress(v);
        }

        public void SetDone()
        {
            UpdateProgress(1f);
            m_IsDone = true;
            OnComplete?.Invoke();
        }

        private void SetProgress(float p)
        {
            p = Mathf.Clamp(p, 0f, 1f);
            Progress = p;
            OnProgressChange?.Invoke();
        }
    }
}
