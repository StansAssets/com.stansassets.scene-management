namespace StansAssets.SceneManagement
{
    public interface IApplicationState
    {
        void Activate();
        void Pause();
        void Deactivate();
    }
}
