using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class UIStyleConfig
    {
        public Color DuplicateColor { get; } = new Color(1f, 0.78f, 1f);
        public Color ErrorColor = new Color(1f, 0.8f, 0.0f);
        public Color OutOfSyncColor= new Color(0.93f, 0.39f, 0.32f);

        public GUIStyle FrameBox {
            get
            {
                if (m_FrameBox == null)
                {
                    m_FrameBox = "FrameBox";
                    m_FrameBox.padding = new RectOffset(-2, -2, -2, -2);
                }
                return m_FrameBox;
            }
        }
        public GUIStyle ToolbarButton {
            get
            {
                if (m_ToolbarButton == null)
                {
                    m_ToolbarButton = GUI.skin.button;
                    m_ToolbarButton.margin = new RectOffset(0, 0, 0, 0);
                    m_ToolbarButton.border = new RectOffset(1, 1, 1, 0);
                }
                return m_ToolbarButton;
            }
        }
        public GUIContent AddressableGuiContent => m_AddressableGuiContent ??= new GUIContent("", "Mark scene Addressable?\nIf true - scene will be added as Addressable asset into \"Scenes\" group, otherwise - scene will be added into build settings.");
        
        GUIContent m_AddressableGuiContent;
        GUIStyle m_FrameBox;
        GUIStyle m_ToolbarButton;
    }
}
