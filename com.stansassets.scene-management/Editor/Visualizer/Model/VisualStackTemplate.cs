using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class VisualStackTemplate
    {
        public string Title;
        public VisualStackItemStatus Status = VisualStackItemStatus.Disabled;
    }

    public enum VisualStackItemStatus
    {
        Active,
        Disabled
    }
}