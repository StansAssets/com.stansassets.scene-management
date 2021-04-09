using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class StackVisualModel
    {
        public string Title;
        public StackVisualItemStatus Status = StackVisualItemStatus.InActive;
    }

    public enum StackVisualItemStatus
    {
        Active,
        InActive
    }
}