using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class GUIStylesEditorWindow : EditorWindow
    {
        PropertyInfo[] m_PropertyInfos;
        Vector2 m_ScrollPosition;

        [MenuItem("Stan's Assets/Styles")]
        static void ShowWindow()
        {
            GetWindow<GUIStylesEditorWindow>().Show(true);
        }
        
        void OnGUI()
        {
            UpdateFieldInfos();

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            for (int i = 0; i < m_PropertyInfos.Length; i++)
            {
                var property = m_PropertyInfos[i];
                var style = property.GetValue(null) as GUIStyle;
                if (GUILayout.Button(style.name, style))
                {
                    GUIUtility.systemCopyBuffer = style.name;
                }
            }
            GUILayout.EndScrollView();
        }

        void UpdateFieldInfos()
        {
            m_PropertyInfos ??= typeof(FullEditorStyles).GetProperties(BindingFlags.Public | BindingFlags.Static);
        }
    }
    
    class FullEditorStyles
    {
        public static GUIStyle[] Styles = new[]
        {
            new GUIStyle("CN Box"),
            new GUIStyle("CN EntryInfo"),
            new GUIStyle("CN EntryWarn")
        };
        
        public static GUIStyle CNBox => new GUIStyle("CN Box");
        public static GUIStyle CNEntryInfo => new GUIStyle("CN EntryInfo");
        public static GUIStyle CNEntryWarn => new GUIStyle("CN EntryWarn");
        public static GUIStyle CNEntryError => new GUIStyle("CN EntryError");
        public static GUIStyle CNEntryBackEven => new GUIStyle("CN EntryBackEven");
        public static GUIStyle CNEntryBackodd => new GUIStyle("CN EntryBackodd");
        public static GUIStyle CNMessage => new GUIStyle("CN Message");
        public static GUIStyle CNStatusError => new GUIStyle("CN StatusError");
        public static GUIStyle CNStatusWarn => new GUIStyle("CN StatusWarn");
        public static GUIStyle CNStatusInfo => new GUIStyle("CN StatusInfo");
        public static GUIStyle CNCountBadge => new GUIStyle("CN CountBadge");
        public static GUIStyle Box => new GUIStyle("Box");
        public static GUIStyle LogStyle => new GUIStyle("LogStyle");
        public static GUIStyle WarningStyle => new GUIStyle("WarningStyle");
        public static GUIStyle ErrorStyle => new GUIStyle("ErrorStyle");
        public static GUIStyle EvenBackground => new GUIStyle("EvenBackground");
        public static GUIStyle OddBackground => new GUIStyle("OddBackground");
        public static GUIStyle MessageStyle => new GUIStyle("MessageStyle");
        public static GUIStyle StatusError => new GUIStyle("StatusError");
        public static GUIStyle StatusWarn => new GUIStyle("StatusWarn");
        public static GUIStyle StatusLog => new GUIStyle("StatusLog");
        public static GUIStyle CountBadge => new GUIStyle("CountBadge");
        public static GUIStyle InBigTitle => new GUIStyle("In BigTitle");
        public static GUIStyle MiniLabel => new GUIStyle("miniLabel");
        public static GUIStyle LargeLabel => new GUIStyle("LargeLabel");
        public static GUIStyle BoldLabel => new GUIStyle("BoldLabel");
        public static GUIStyle MiniBoldLabel => new GUIStyle("MiniBoldLabel");
        public static GUIStyle WordWrappedLabel => new GUIStyle("WordWrappedLabel");
        public static GUIStyle WordWrappedMiniLabel => new GUIStyle("WordWrappedMiniLabel");
        public static GUIStyle WhiteLabel => new GUIStyle("WhiteLabel");
        public static GUIStyle WhiteMiniLabel => new GUIStyle("WhiteMiniLabel");
        public static GUIStyle WhiteLargeLabel => new GUIStyle("WhiteLargeLabel");
        public static GUIStyle WhiteBoldLabel => new GUIStyle("WhiteBoldLabel");
        public static GUIStyle MiniTextField => new GUIStyle("MiniTextField");
        public static GUIStyle Radio => new GUIStyle("Radio");
        public static GUIStyle MiniButton => new GUIStyle("miniButton");
        public static GUIStyle MiniButtonLeft => new GUIStyle("miniButtonLeft");
        public static GUIStyle MiniButtonMid => new GUIStyle("miniButtonMid");
        public static GUIStyle MiniButtonRight => new GUIStyle("miniButtonRight");
        public static GUIStyle Toolbar => new GUIStyle("toolbar");
        public static GUIStyle Toolbarbutton => new GUIStyle("toolbarbutton");
        public static GUIStyle ToolbarPopup => new GUIStyle("toolbarPopup");
        public static GUIStyle ToolbarDropDown => new GUIStyle("toolbarDropDown");
        public static GUIStyle ToolbarTextField => new GUIStyle("toolbarTextField");
        public static GUIStyle ToolbarSeachTextField => new GUIStyle("ToolbarSeachTextField");
        public static GUIStyle ToolbarSeachTextFieldPopup => new GUIStyle("ToolbarSeachTextFieldPopup");
        public static GUIStyle ToolbarSeachCancelButton => new GUIStyle("ToolbarSeachCancelButton");
        public static GUIStyle ToolbarSeachCancelButtonEmpty => new GUIStyle("ToolbarSeachCancelButtonEmpty");
        public static GUIStyle SearchTextField => new GUIStyle("SearchTextField");
        public static GUIStyle SearchCancelButton => new GUIStyle("SearchCancelButton");
        public static GUIStyle SearchCancelButtonEmpty => new GUIStyle("SearchCancelButtonEmpty");
        public static GUIStyle HelpBox => new GUIStyle("HelpBox");
        public static GUIStyle AssetLabel => new GUIStyle("AssetLabel");
        public static GUIStyle AssetLabelPartial => new GUIStyle("AssetLabel Partial");
        public static GUIStyle AssetLabelIcon => new GUIStyle("AssetLabel Icon");
        public static GUIStyle SelectionRect => new GUIStyle("selectionRect");
        public static GUIStyle MinMaxHorizontalSliderThumb => new GUIStyle("MinMaxHorizontalSliderThumb");
        public static GUIStyle DropDownButton => new GUIStyle("DropDownButton");
        public static GUIStyle Label => new GUIStyle("Label");
        public static GUIStyle ProgressBarBack => new GUIStyle("ProgressBarBack");
        public static GUIStyle ProgressBarBar => new GUIStyle("ProgressBarBar");
        public static GUIStyle ProgressBarText => new GUIStyle("ProgressBarText");
        public static GUIStyle FoldoutPreDrop => new GUIStyle("FoldoutPreDrop");
        public static GUIStyle INTitle => new GUIStyle("IN Title");
        public static GUIStyle INTitleText => new GUIStyle("IN TitleText");
        public static GUIStyle BoldToggle => new GUIStyle("BoldToggle");
        public static GUIStyle Tooltip => new GUIStyle("Tooltip");
        public static GUIStyle NotificationText => new GUIStyle("NotificationText");
        public static GUIStyle NotificationBackground => new GUIStyle("NotificationBackground");
        public static GUIStyle MiniPopup => new GUIStyle("MiniPopup");
        public static GUIStyle TextField => new GUIStyle("textField");
        public static GUIStyle ControlLabel => new GUIStyle("ControlLabel");
        public static GUIStyle ObjectField => new GUIStyle("ObjectField");
        public static GUIStyle ObjectFieldThumb => new GUIStyle("ObjectFieldThumb");
        public static GUIStyle ObjectFieldMiniThumb => new GUIStyle("ObjectFieldMiniThumb");
        public static GUIStyle Toggle => new GUIStyle("Toggle");
        public static GUIStyle ToggleMixed => new GUIStyle("ToggleMixed");
        public static GUIStyle ColorField => new GUIStyle("ColorField");
        public static GUIStyle Foldout => new GUIStyle("Foldout");
        public static GUIStyle TextFieldDropDown => new GUIStyle("TextFieldDropDown");
        public static GUIStyle TextFieldDropDownText => new GUIStyle("TextFieldDropDownText");
        public static GUIStyle PRLabel => new GUIStyle("PR Label");
        public static GUIStyle ProjectBrowserGridLabel => new GUIStyle("ProjectBrowserGridLabel");
        public static GUIStyle ObjectPickerResultsGrid => new GUIStyle("ObjectPickerResultsGrid");
        public static GUIStyle ObjectPickerBackground => new GUIStyle("ObjectPickerBackground");
        public static GUIStyle ObjectPickerPreviewBackground => new GUIStyle("ObjectPickerPreviewBackground");
        public static GUIStyle ProjectBrowserHeaderBgMiddle => new GUIStyle("ProjectBrowserHeaderBgMiddle");
        public static GUIStyle ProjectBrowserHeaderBgTop => new GUIStyle("ProjectBrowserHeaderBgTop");
        public static GUIStyle ObjectPickerToolbar => new GUIStyle("ObjectPickerToolbar");
        public static GUIStyle PRTextField => new GUIStyle("PR TextField");
        public static GUIStyle PRPing => new GUIStyle("PR Ping");
        public static GUIStyle ProjectBrowserIconDropShadow => new GUIStyle("ProjectBrowserIconDropShadow");
        public static GUIStyle ProjectBrowserTextureIconDropShadow => new GUIStyle("ProjectBrowserTextureIconDropShadow");
        public static GUIStyle ProjectBrowserIconAreaBg => new GUIStyle("ProjectBrowserIconAreaBg");
        public static GUIStyle ProjectBrowserPreviewBg => new GUIStyle("ProjectBrowserPreviewBg");
        public static GUIStyle ProjectBrowserSubAssetBg => new GUIStyle("ProjectBrowserSubAssetBg");
        public static GUIStyle ProjectBrowserSubAssetBgOpenEnded => new GUIStyle("ProjectBrowserSubAssetBgOpenEnded");
        public static GUIStyle ProjectBrowserSubAssetBgCloseEnded => new GUIStyle("ProjectBrowserSubAssetBgCloseEnded");
        public static GUIStyle ProjectBrowserSubAssetBgMiddle => new GUIStyle("ProjectBrowserSubAssetBgMiddle");
        public static GUIStyle ProjectBrowserSubAssetBgDivider => new GUIStyle("ProjectBrowserSubAssetBgDivider");
        public static GUIStyle ProjectBrowserSubAssetExpandBtn => new GUIStyle("ProjectBrowserSubAssetExpandBtn");
    }
}
