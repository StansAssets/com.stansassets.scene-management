using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    interface IStackVisualizerView
    {
        VisualElement Root { get; }

        void ShowView(bool show);
        void SetStackName(string stackName);
        void SetStackChange(IEnumerable<VisualStackTemplate> oldStack, IEnumerable<VisualStackTemplate> newStack);
        void SetStack(IEnumerable<VisualStackTemplate> stack);
        void UpdateProgress(float progress, string title);
    }
}
