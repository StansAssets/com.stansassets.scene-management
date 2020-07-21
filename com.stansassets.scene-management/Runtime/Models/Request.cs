using System;

namespace StansAssets.SceneManagement
{
    class Request : IProgressReporter
    {
        public event Action Done;
        public event Action<float> ProgressChange;

        public float Progress { get; protected set; }

        public Request() { }

        public virtual bool IsDone => Progress == 1f;

        public virtual void UpdateProgress(float v)
        {
            if (IsDone)
                return;

            SetProgress(v);

            if (IsDone)
                InvokeDone();
        }

        public virtual void SetDone()
        {
            UpdateProgress(1f);
        }

        protected void SetProgress(float p)
        {
            p = Math.Min(1f, Math.Max(0f, p));
            Progress = p;
            ProgressChange?.Invoke(Progress);
        }

        protected void InvokeDone()
        {
            Done?.Invoke();
        }
    }
}
