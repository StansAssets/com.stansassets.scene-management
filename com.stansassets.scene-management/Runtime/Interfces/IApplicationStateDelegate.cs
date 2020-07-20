namespace StansAssets.SceneManagement
{
    public interface IApplicationStateDelegate
    {
        void OnApplicationStateWillChanged(StackChangeEvent eventArg);
        void ApplicationStateChangeProgressChanged(float progress, StackChangeEvent eventArg);
        void ApplicationStateChanged(StackChangeEvent eventArg);
    }
}
