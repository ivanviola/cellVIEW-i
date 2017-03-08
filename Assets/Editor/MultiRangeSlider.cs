// Decompiled with JetBrains decompiler
// Type: UnityEditor.MultiRangeSlider
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 50E5777D-9A1E-4323-A533-F67E7D3B76B6
// Assembly location: D:\Mathieu\Git\cellVIEW_color\trunk\Library\UnityAssemblies\UnityEditor.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


    public static class MultiRangeSlider
    {
        private static readonly Color[] kCascadeColors = new Color[4]
        {
            new Color(0.5f, 0.5f, 0.6f, 1f),
            new Color(0.5f, 0.6f, 0.5f, 1f),
            new Color(0.6f, 0.6f, 0.5f, 1f),
            new Color(0.6f, 0.5f, 0.5f, 1f)
        };

        private static readonly GUIStyle s_CascadeSliderBG = (GUIStyle)"LODSliderRange";
        private static readonly GUIStyle s_TextCenteredStyle = new GUIStyle(EditorStyles.whiteMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        private static readonly int s_CascadeSliderId = "s_CascadeSliderId".GetHashCode();
        private static DrawCameraMode s_OldSceneDrawMode = DrawCameraMode.Textured;
        private const int kSliderbarTopMargin = 2;
        private const int kSliderbarHeight = 24;
        private const int kSliderbarBottomMargin = 2;
        private const int kPartitionHandleWidth = 2;
        private const int kPartitionHandleExtraHitAreaWidth = 2;
        private static MultiRangeSlider.DragCache s_DragCache;
        private static SceneView s_RestoreSceneView;
        private static bool s_OldSceneLightingMode;

        public static void HandleCascadeSliderGUI(ref float[] normalizedCascadePartitions, int maxDistance, UnityEngine.Object target)
        {
            if (s_DragCache != null)
            {
                EditorUtility.SetDirty(target);
            }

        GUILayout.Label("Level Ranges");
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, MultiRangeSlider.s_CascadeSliderBG, new GUILayoutOption[2]
            {
        GUILayout.Height(28f),
        GUILayout.ExpandWidth(true)
            });
            GUI.Box(rect, GUIContent.none);
            float x = rect.x;
            float y = rect.y + 2f;
            float num1 = rect.width - (float)(normalizedCascadePartitions.Length * 2);
            Color color = GUI.color;
            Color backgroundColor = GUI.backgroundColor;
            int index1 = -1;
            float[] numArray = new float[normalizedCascadePartitions.Length + 1];
            Array.Copy((Array)normalizedCascadePartitions, (Array)numArray, normalizedCascadePartitions.Length);
            numArray[numArray.Length - 1] = 1f - Enumerable.Sum((IEnumerable<float>)normalizedCascadePartitions);
            int controlId = GUIUtility.GetControlID(MultiRangeSlider.s_CascadeSliderId, FocusType.Passive);
            Event current = Event.current;
            int activePartition = -1;

            int distAcc = 0;

            for (int index2 = 0; index2 < numArray.Length; ++index2)
            {
                float num2 = numArray[index2];
                index1 = (index1 + 1) % MultiRangeSlider.kCascadeColors.Length;
                GUI.backgroundColor = MultiRangeSlider.kCascadeColors[index1];
                float width = num1 * num2;
                Rect position1 = new Rect(x, y, width, 24f);
                GUI.Box(position1, GUIContent.none, MultiRangeSlider.s_CascadeSliderBG);
                float num3 = x + width;
                GUI.color = Color.white;
                Rect position2 = position1;

                string str = string.Format("{0}\n{1:F1}%", (object)index2, (object)(float)((double)num2 * 100.0));

                if (maxDistance != 0)
                {
                    distAcc += (int) ((double) num2*maxDistance);
                    str = string.Format("{0}\n{1}", (object)index2, (object)distAcc);
                }

                GUI.Label(position2, str, MultiRangeSlider.s_TextCenteredStyle);
                //GUI.Label(position2, GUIContent.Temp(str, str), MultiRangeSlider.s_TextCenteredStyle);
                
                if (index2 != numArray.Length - 1)
                {
                    GUI.backgroundColor = Color.black;
                    Rect position3 = position1;
                    position3.x = num3;
                    position3.width = 2f;
                    GUI.Box(position3, GUIContent.none, MultiRangeSlider.s_CascadeSliderBG);
                    Rect position4 = position3;
                    position4.xMin -= 2f;
                    position4.xMax += 2f;
                    if (position4.Contains(current.mousePosition))
                        activePartition = index2;
                    if (MultiRangeSlider.s_DragCache == null)
                        EditorGUIUtility.AddCursorRect(position4, MouseCursor.ResizeHorizontal, controlId);
                    x = num3 + 2f;
                }
                else
                    break;
            }
            GUI.color = color;
            GUI.backgroundColor = backgroundColor;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (activePartition < 0)
                        break;
                    MultiRangeSlider.s_DragCache = new MultiRangeSlider.DragCache(activePartition, normalizedCascadePartitions[activePartition], current.mousePosition);
                    if (GUIUtility.hotControl == 0)
                        GUIUtility.hotControl = controlId;
                    current.Use();
                    if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView == (UnityEngine.Object)null))
                        break;
                    MultiRangeSlider.s_RestoreSceneView = SceneView.lastActiveSceneView;
                    if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView != (UnityEngine.Object)null))
                        break;
                    MultiRangeSlider.s_OldSceneDrawMode = MultiRangeSlider.s_RestoreSceneView.renderMode;
                    MultiRangeSlider.s_OldSceneLightingMode = MultiRangeSlider.s_RestoreSceneView.m_SceneLighting;
                    MultiRangeSlider.s_RestoreSceneView.renderMode = DrawCameraMode.ShadowCascades;
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    MultiRangeSlider.s_DragCache = (MultiRangeSlider.DragCache)null;
                    if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView != (UnityEngine.Object)null))
                        break;
                    MultiRangeSlider.s_RestoreSceneView.renderMode = MultiRangeSlider.s_OldSceneDrawMode;
                    MultiRangeSlider.s_RestoreSceneView.m_SceneLighting = MultiRangeSlider.s_OldSceneLightingMode;
                    MultiRangeSlider.s_RestoreSceneView = (SceneView)null;
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    float num4 = (current.mousePosition - MultiRangeSlider.s_DragCache.m_LastCachedMousePosition).x / num1;
                    if ((double)numArray[MultiRangeSlider.s_DragCache.m_ActivePartition] + (double)num4 > 0.0 && (double)numArray[MultiRangeSlider.s_DragCache.m_ActivePartition + 1] - (double)num4 > 0.0)
                    {
                        MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize += num4;
                        normalizedCascadePartitions[MultiRangeSlider.s_DragCache.m_ActivePartition] = MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize;
                        if (MultiRangeSlider.s_DragCache.m_ActivePartition < normalizedCascadePartitions.Length - 1)
                            normalizedCascadePartitions[MultiRangeSlider.s_DragCache.m_ActivePartition + 1] -= num4;
                    }
                    MultiRangeSlider.s_DragCache.m_LastCachedMousePosition = current.mousePosition;
                    current.Use();
                    break;
            }
        }

        private class DragCache
        {
            public int m_ActivePartition;
            public float m_NormalizedPartitionSize;
            public Vector2 m_LastCachedMousePosition;

            public DragCache(int activePartition, float normalizedPartitionSize, Vector2 currentMousePos)
            {
                this.m_ActivePartition = activePartition;
                this.m_NormalizedPartitionSize = normalizedPartitionSize;
                this.m_LastCachedMousePosition = currentMousePos;
            }
        }
    }