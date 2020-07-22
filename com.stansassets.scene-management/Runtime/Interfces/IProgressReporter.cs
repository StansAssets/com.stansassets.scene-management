namespace StansAssets.SceneManagement
{
    public interface IProgressReporter
    {
        void UpdateProgress(float v);
        void SetDone();
    }
}
