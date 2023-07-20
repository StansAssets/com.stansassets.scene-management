using System;
using System.Collections;
using System.Linq;
using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using StansAssets.Foundation.Editor.Extensions;
using UnityEditor;
using UnityEditorInternal;
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
    
    static class DrawingUtility
    {
        public static UIStyleConfig StyleConfig { get; } = new();

        public static ReorderableList CreateScenesReorderableList(IList elementsList, bool showBuildIndex,
            ReorderableList.ChangedCallbackDelegate changedCallbackDelegate, 
            ReorderableList.ReorderCallbackDelegate reorderCallbackDelegate,
            ReorderableList.RemoveCallbackDelegate removeCallbackDelegate
        )
        {
            var reorderableList = new ReorderableList(elementsList, typeof(SceneAssetInfo),
                true, true, true, false)
            {
                showDefaultBackground = false,
                footerHeight = 18f,
                elementHeightCallback = i => 22f,
            };

            // Draw element
            reorderableList.drawNoneElementCallback =
                rect => EditorGUI.LabelField(rect, "Add a scene", EditorStyles.miniLabel);
            
            reorderableList.drawElementCallback = (rect, index, active, focused) =>
                DrawSceneListItem(rect, index, reorderableList, showBuildIndex);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback = rect =>
                DrawListHeaderCallback(rect, "Scenes", showBuildIndex);
            
            reorderableList.drawFooterCallback = rect => DrawListFooterCallback(rect, reorderableList);

            // Actions
            reorderableList.onChangedCallback = changedCallbackDelegate;
            reorderableList.onReorderCallback = reorderCallbackDelegate;
            reorderableList.onRemoveCallback = removeCallbackDelegate;

            return reorderableList;
        }
        
        public static ReorderableList CreatePlatformsReorderableList(IList elementsList, bool showBuildIndex,
            ReorderableList.ChangedCallbackDelegate changedCallbackDelegate, 
            ReorderableList.RemoveCallbackDelegate removeCallbackDelegate)
        {
            var reorderableList = new ReorderableList(elementsList, typeof(BuildTargetRuntime),
                true, true, true, false)
            {
                showDefaultBackground = false,
                footerHeight = 18f,
                elementHeightCallback = i => 22f,
            };

            // Draw element
            reorderableList.drawNoneElementCallback = rect =>
                EditorGUI.LabelField(rect, "Add a build target", EditorStyles.miniLabel);
            
            reorderableList.drawElementCallback = (rect, index, active, focused) =>
                DrawBuildTargetListItem(rect, index, reorderableList);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback =
                rect => DrawListHeaderCallback(rect, "Build Targets", showBuildIndex);
            
            reorderableList.drawFooterCallback = rect => DrawListFooterCallback(rect, reorderableList);

            // Actions
            reorderableList.onChangedCallback = changedCallbackDelegate;
            reorderableList.onRemoveCallback = removeCallbackDelegate;

            return reorderableList;
        }
        
        public static void DrawEmptyScene()
        {
            GUILayout.Label("Add a scenes", EditorStyles.miniLabel);
        }
        
        static void DrawSceneListItem(Rect rect, int index, ReorderableList reorderableList, bool showBuildIndex)
        {
            var itemValue = reorderableList.list[index] as SceneAssetInfo ?? new SceneAssetInfo();

            rect.y += 1f;
            rect.height = 18f;

            GUI.BeginGroup(rect);
            {
                const float addressablesToggleWidth = 20.0f;
                const float removeButtonWidth = 24.0f;
                const float objectFieldRectWidth = 60.0f;
                const float removeButtonRectWidth = 24;

                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var positionRect = Rect.zero.WithSize(rect.size);
                var sceneIndexRect = showBuildIndex
                    ? positionRect.WithWidth(addressablesToggleWidth)
                    : positionRect.WithSize(Vector2.zero);
                var objectFieldRect = positionRect
                    .WithWidth(
                        Mathf.Clamp(
                            positionRect.width - sceneIndexRect.width - addressablesToggleWidth - removeButtonWidth,
                            objectFieldRectWidth, float.MaxValue)
                    )
                    .RightOf(sceneIndexRect);

                var addressableToggleRect = positionRect.WithWidth(addressablesToggleWidth).RightOf(objectFieldRect)
                    .ShiftHorizontally(4.0f);

                if (showBuildIndex)
                {
                    var sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue, EditorUserBuildSettings.activeBuildTarget);
                    GUI.Label(sceneIndexRect, sceneIndex.ToString());
                }

                var sceneAsset = itemValue.GetSceneAsset();
                var sceneSynced = BuildConfigurationSettings.Instance.Configuration
                    .CheckIntersectSceneWhBuildSettings(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

                GUI.color = LookForFieldColor(sceneAsset, sceneSynced, itemValue, reorderableList);

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                var newSceneAsset =
                    EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                    reorderableList.onChangedCallback?.Invoke(reorderableList);
                }

                GUI.color = Color.white;

                itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, StyleConfig.AddressableGuiContent);

                var removeButtonRect = positionRect.WithWidth(removeButtonWidth).RightOf(addressableToggleRect)
                    .ShiftHorizontally(-2.0f);
                removeButtonRect.width = removeButtonRectWidth;

                DrawRemoveButtonOfListElement(removeButtonRect, reorderableList, index);

                EditorGUI.indentLevel = indentLevel;
            }
            GUI.EndGroup();
        }
        
        public static void DrawListItemBackground(Rect rect, int index, bool isFocused, ReorderableList reorderableList)
        {
            if (!isFocused && reorderableList.count - 1 <= index)
            {
                return;
            }

            var startPos = new Vector2(rect.xMin, rect.yMax);
            var endPos = new Vector2(rect.xMax, rect.yMax);

            Handles.color = isFocused
                ? ReorderableListStyles.SelectionBackgroundColor
                : ReorderableListStyles.HorizontalLineColor;
            Handles.DrawLine(startPos, endPos);
            Handles.color = Color.white;
        }
        
        public static void DrawListHeaderCallback(Rect rect, string titleText, bool showBuildIndex = false)
        {
            var style = ReorderableListStyles.Title;

            rect.x -= 20f;
            rect.width += 25f;

            GUI.backgroundColor = showBuildIndex ? GUI.skin.settings.selectionColor : Color.white;
            EditorGUI.LabelField(rect, titleText, style);
            GUI.backgroundColor = Color.white;
        }
        
        static void DrawRemoveButtonOfListElement(Rect rect, ReorderableList reorderableList, int index)
        {
            var iconNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Remove_Normal);
            var iconActive = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Remove_Active);

            var removeButton = GUIHelper.IconButton(rect, true, iconNormal, iconActive, ReorderableListStyles.ItemButton);
            if (removeButton)
            {
                reorderableList.list.Remove(reorderableList.list[index]);
                reorderableList.onRemoveCallback?.Invoke(reorderableList);

                GUIUtility.ExitGUI();
            }
        }
        
        static Color LookForFieldColor(SceneAsset sceneAsset, bool scenesSynced, SceneAssetInfo itemValue,
            ReorderableList reorderableList)
        {
            var sceneDuplicate = BuildConfigurationSettings.Instance.Configuration
                .CheckSceneDuplicate(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

            if (!sceneDuplicate && itemValue.Guid != null && !string.IsNullOrEmpty(itemValue.Guid))
            {
                var scenes = new SceneAssetInfo[reorderableList.list.Count];
                reorderableList.list.CopyTo(scenes, 0);

                var itemPath = AssetDatabase.GUIDToAssetPath(itemValue.Guid);
                sceneDuplicate = scenes.Count(i =>
                    itemValue.Guid.Equals(i.Guid) || AssetDatabase.GUIDToAssetPath((string) i.Guid).Equals(itemPath)) > 1;
            }
            
            var sceneWithError = sceneAsset == null;
            var color = Color.white;
            
            if (sceneDuplicate)
            {
                color = StyleConfig.DuplicateColor;
            }
            else if (sceneWithError)
            {
                color = StyleConfig.ErrorColor;
            }
            else if (!scenesSynced)
            {
                color = StyleConfig.OutOfSyncColor;
            }
            
            return color;
        }
        
        static void DrawListFooterCallback(Rect rect, ReorderableList reorderableList)
        {
            var removeButtonRect = rect.RightOf(rect).ShiftHorizontally(-28.0f);
            removeButtonRect.width = 24f;
            removeButtonRect.height = 18f;

            var iconNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal);
            var iconActive = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Active);

            var removeButton = GUIHelper.IconButton(removeButtonRect, true, iconNormal, iconActive,
                ReorderableListStyles.ItemButton);
            if (removeButton)
            {
                var elementType = reorderableList.list.GetType().GetGenericArguments().Single();
                reorderableList.list.Add(Activator.CreateInstance(elementType));
            }
        }
        
        static void DrawBuildTargetListItem(Rect rect, int index, ReorderableList reorderableList)
        {
            var element = (BuildTargetRuntime)reorderableList.list[index];

            rect.y += 1f;
            rect.height = 18f;

            const float removeButtonWidth = 24.0f;

            var positionRect = new Rect(rect);
            positionRect.width -= removeButtonWidth;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var style = new GUIStyle(EditorStyles.popup)
            {
                fontStyle = element == (BuildTargetRuntime)EditorUserBuildSettings.activeBuildTarget ||
                    element == BuildTargetRuntime.Editor
                        ? FontStyle.Bold
                        : FontStyle.Normal
            };

            EditorGUI.BeginChangeCheck();
            
            GUI.color = LookForBuildTargetFieldColor(element);
            element = (BuildTargetRuntime)EditorGUI.EnumPopup(positionRect, element, style);
            GUI.color = Color.white;
            
            if (EditorGUI.EndChangeCheck())
            {
                reorderableList.list[index] = element;
                reorderableList.onChangedCallback?.Invoke(reorderableList);
            }

            EditorGUI.indentLevel = indentLevel;

            var removeButtonRect = rect.RightOf(positionRect).ShiftHorizontally(+2.0f);
            removeButtonRect.width = removeButtonWidth;

            DrawRemoveButtonOfListElement(removeButtonRect, reorderableList, index);
        }
        
        static Color LookForBuildTargetFieldColor(BuildTargetRuntime buildTargetRuntime)
        {
            if (buildTargetRuntime == BuildTargetRuntime.NoTarget)
            {
                return new Color(1f, 0.95f, 0.7f);
            }
            
            var hasDuplicates = BuildConfigurationSettings.Instance.Configuration
                .CheckBuildTargetDuplicate(buildTargetRuntime);

            return hasDuplicates ? StyleConfig.DuplicateColor : Color.white;
        }
    }
}
