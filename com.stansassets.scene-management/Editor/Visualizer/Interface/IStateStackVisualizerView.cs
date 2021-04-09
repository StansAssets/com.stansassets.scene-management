using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    interface IStateStackVisualizerView
    {
        VisualElement Root { get; }

        void ShowView(bool show);
        void SetStackName(string stackName);
        void SetTwoStack(IEnumerable<StackVisualModel> oldStack, IEnumerable<StackVisualModel> newStack);
        void SetStack(IEnumerable<StackVisualModel> stack);
        void UpdateProgress(float progress, string title);
    }
}
