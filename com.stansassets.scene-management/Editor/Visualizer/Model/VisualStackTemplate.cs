namespace StansAssets.SceneManagement
{
    public class VisualStackTemplate
    {
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public VisualStackItemStatus Status { get; set; }
    }

    public enum VisualStackItemStatus
    {
        Undefined,
        Active,
        Disabled
    }
}