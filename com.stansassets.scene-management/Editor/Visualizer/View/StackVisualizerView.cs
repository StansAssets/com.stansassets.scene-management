﻿using System.Collections.Generic;
using StansAssets.Plugins.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    public class StackVisualizerView : BaseTab, IStackVisualizerView
    {
        readonly Label m_Header;
        readonly VisualElement m_Container;
        readonly ProgressBar m_ProgressBar;

        public new VisualElement Root => base.Root;

        public StackVisualizerView() : base($"{SceneManagementPackage.StackVisualizerPath}/StackVisualizerView")
        {
            Root.AddToClassList(StackVisualizerViewUss.RootClass);

            m_Header = new Label();
            m_Header.AddToClassList(StackVisualizerViewUss.StackTitleClass);
            m_Container = new VisualElement();
            m_Container.AddToClassList(StackVisualizerViewUss.ContainerClass);
            m_ProgressBar = new ProgressBar();
            m_ProgressBar.AddToClassList(StackVisualizerViewUss.ProgressBarClass);
            Root.Add(m_Header);
            Root.Add(m_Container);
            Root.Add(m_ProgressBar);

            m_ProgressBar.style.display = DisplayStyle.None;
        }

        public void ShowView(bool show)
        {
            Root.style.display = (show)? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetStackName(string stackName)
        {
            m_Header.text = stackName;
        }

        public void SetStackChange(IEnumerable<VisualStackTemplate> oldStackTemplates, IEnumerable<VisualStackTemplate> newStackTemplates)
        {
            m_ProgressBar.style.display = DisplayStyle.Flex;
            m_ProgressBar.value = 0;
            m_Container.Clear();

            m_Container.Add(CreateStackElements(oldStackTemplates));

            var labelArrow = new Label {text = "→"};
            labelArrow.AddToClassList(StackVisualizerViewUss.ArrowClass);
            m_Container.Add(labelArrow);

            m_Container.Add(CreateStackElements(newStackTemplates));
        }

        public void SetStack(IEnumerable<VisualStackTemplate> stackTemplates)
        {
            m_ProgressBar.style.display = DisplayStyle.None;
            m_ProgressBar.value = 0;
            m_Container.Clear();

            m_Container.Add(CreateStackElements(stackTemplates));
        }

        public void UpdateProgress(float progress, string title)
        {
            m_ProgressBar.value = progress;
            m_ProgressBar.title = title;
        }

        VisualElement CreateStackElements(IEnumerable<VisualStackTemplate> stackTemplates)
        {
            var stackRoot = new VisualElement();
            stackRoot.AddToClassList(StackVisualizerViewUss.StackСontentClass);

            foreach (var template in stackTemplates)
            {
                stackRoot.Add(CreateElement(template));
            }

            return stackRoot;
        }

        VisualElement CreateElement(VisualStackTemplate template)
        {
            var label = new Label {text = template.Title, tooltip = template.FullTitle };
            label.AddToClassList(StackVisualizerViewUss.StackItemClass);

            if (template.Status == VisualStackItemStatus.Active)
            {
                label.AddToClassList(StackVisualizerViewUss.StackItemActiveClass);
            }

            return label;
        }
    }
}
