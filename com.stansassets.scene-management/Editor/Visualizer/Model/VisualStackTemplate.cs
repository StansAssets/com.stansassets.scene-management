namespace StansAssets.SceneManagement
{
    public class VisualStackTemplate
    {
        public string Title;
        public VisualStackItemStatus Status;
    }

    public enum VisualStackItemStatus
    {
        Undefined,
        Active,
        Disabled
    }
}