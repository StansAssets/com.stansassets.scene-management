using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using StansAssets.Foundation.Editor.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    delegate void FillContextMenuDelegate(GenericMenu menu);
    
    struct SceneFieldStatus : IEquatable<SceneFieldStatus>
    {
        public Color Color;
        public string Tooltip;

        public bool Equals(SceneFieldStatus other)
        {
            return Color.Equals(other.Color) && Tooltip == other.Tooltip;
        }

        public override bool Equals(object obj)
        {
            return obj is SceneFieldStatus other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Color, Tooltip);
        }
    }

    static class DrawingUtility
    {
        public static UIStyleConfig StyleConfig { get; } = new();
        public static bool ShowBuildIndex { get; set; }

        static GUIContent s_TempGUIContent = new GUIContent();

        public static ReorderableList CreateScenesReorderableList(IList elementsList,
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
                DrawSceneListItem(rect, index, reorderableList, ShowBuildIndex);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback = rect =>
                DrawListHeaderCallback(rect, "Scenes", ShowBuildIndex);
            
            reorderableList.drawFooterCallback = rect => DrawListFooterCallback(rect, reorderableList);

            // Actions
            reorderableList.onChangedCallback = changedCallbackDelegate;
            reorderableList.onReorderCallback = reorderCallbackDelegate;
            reorderableList.onRemoveCallback = removeCallbackDelegate;

            return reorderableList;
        }
        
        public static ReorderableList CreatePlatformsReorderableList(IList elementsList,
            ReorderableList.ChangedCallbackDelegate changedCallbackDelegate, 
            ReorderableList.RemoveCallbackDelegate removeCallbackDelegate,
            FillContextMenuDelegate dropdownClickDelegate)
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
                DrawBuildTargetListItem(rect, index, reorderableList, dropdownClickDelegate);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback =
                rect => DrawListHeaderCallback(rect, "Build Targets", ShowBuildIndex);
            
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
                    if (sceneIndex >= 0)
                    {
                        GUI.Label(sceneIndexRect, sceneIndex.ToString());
                    }
                }

                var sceneAsset = itemValue.GetSceneAsset();
                var sceneSynced = BuildConfigurationSettings.Instance.Configuration
                    .CheckIntersectSceneWhBuildSettings(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

                var prevColor = GUI.color;
                var fieldStatus = GetFieldStatus(sceneAsset, sceneSynced, itemValue, reorderableList);
                GUI.color = fieldStatus.Color;

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                var newSceneAsset =
                    EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                    reorderableList.onChangedCallback?.Invoke(reorderableList);
                }
                
                // Scene ObjectField tooltip
                if (!string.IsNullOrEmpty(fieldStatus.Tooltip))
                {
                    s_TempGUIContent.tooltip = fieldStatus.Tooltip;
                    GUI.Label(objectFieldRect, s_TempGUIContent);
                    s_TempGUIContent.tooltip = null;
                }

                GUI.color = prevColor;

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
        
        static SceneFieldStatus GetFieldStatus(SceneAsset sceneAsset, bool scenesSynced, SceneAssetInfo itemValue,
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
            var status = new SceneFieldStatus()
            {
                Color = Color.white
            };
            
            if (sceneDuplicate)
            {
                status.Color = StyleConfig.DuplicateColor;
                status.Tooltip = "This scene duplicated in configuration, it should be fixed manually.";
            }
            else if (sceneWithError)
            {
                status.Color = StyleConfig.ErrorColor;
                status.Tooltip = "Please assign the scene or remove this item.";
            }
            else if (!scenesSynced)
            {
                status.Color = StyleConfig.OutOfSyncColor;
                status.Tooltip = "Scenes in configuration and Build Settings are our of sync. Please click button above to sync.";
            }

            return status;
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
        
        static void DrawBuildTargetListItem(Rect rect, int index, ReorderableList reorderableList,
            FillContextMenuDelegate dropdownClickDelegate)
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
            
            var prevColor = GUI.color;
            GUI.color = GetBuildTargetFieldColor(element);
            if (GUI.Button(positionRect, element.ToString(), style))
            {
                // var allTargets = Enum.GetValues(typeof(BuildTargetRuntime)).Cast<BuildTargetRuntime>();
                // var selectedTargets = reorderableList.list.Cast<BuildTargetRuntime>();
                var menu = new GenericMenu();

                dropdownClickDelegate.Invoke(menu);
                menu.DropDown(positionRect);
                
                /*foreach (var target in allTargets)
                {
                    if (target == element)
                    {
                        menu.AddItem(new GUIContent(target.ToString()), true, () => { });
                    }
                    else
                    {
                        if(selectedTargets.Contains())
                        menu.AddItem(new GUIContent(target.ToString()), false, () =>
                        {
                            element = target;
                        });
                    }
                }
                

                foreach (BuildTargetRuntime buildTarget in reorderableList.list)
                {
                    
                }*/
            }
            
            // element = (BuildTargetRuntime)EditorGUI.EnumPopup(positionRect, element, style);
            GUI.color = prevColor;
            
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
        
        static Color GetBuildTargetFieldColor(BuildTargetRuntime buildTargetRuntime)
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
