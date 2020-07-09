namespace StansAssets.SceneManagement
{
    public interface IApplicationState
    {
        void ChangeState(StackChangeEvent evt, IProgressReporter reporter);
    }
}
