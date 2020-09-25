using System;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    class ProgressListenerRequest : IProgressReporter
    {
        public event Action OnComplete;
        public event Action OnProgressChange;

        public float Progress { get; protected set; }

        protected bool IsDone => Progress >= 1f;

        public void UpdateProgress(float v)
        {
            if (IsDone)
                return;

            SetProgress(v);

            if (IsDone)
                InvokeDone();
        }

        public void SetDone()
        {
            UpdateProgress(1f);
        }

        protected void SetProgress(float p)
        {
            p = Mathf.Clamp(p, 0f, 1f);
            Progress = p;
            OnProgressChange?.Invoke();
        }

        protected void InvokeDone()
        {
            OnComplete?.Invoke();
        }
    }
}
