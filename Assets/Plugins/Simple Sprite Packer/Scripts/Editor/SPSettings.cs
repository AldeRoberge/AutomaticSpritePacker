﻿using SimpleSpritePacker;
using UnityEditor;
using UnityEngine;

namespace Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    public class SPSettings : EditorWindow
    {
        private static RectOffset padding = new(10, 10, 10, 10);

        protected void OnEnable()
        {
            titleContent = new GUIContent("SP Settings");
        }

        protected void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space((float)padding.top);
            GUILayout.BeginHorizontal();
            GUILayout.Space((float)padding.left);
            GUILayout.BeginVertical();

            GUILayout.Label("General", EditorStyles.boldLabel);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUI.changed = false;
            bool drwe = GUILayout.Toggle(EditorPrefs.GetBool(SPTools.Settings_DisableReadWriteEnabled), " Disable Read/Write Enabled of the source textures after packing ?");
            if (GUI.changed)
            {
                EditorPrefs.SetBool(SPTools.Settings_DisableReadWriteEnabled, drwe);
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUI.changed = false;
            bool amsos = GUILayout.Toggle(EditorPrefs.GetBool(SPTools.Settings_AllowMultiSpritesOneSource), " Allow multiple sprites from the same source ?");
            if (GUI.changed)
            {
                EditorPrefs.SetBool(SPTools.Settings_AllowMultiSpritesOneSource, amsos);
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.EndVertical();

            GUILayout.Label("Layout", EditorStyles.boldLabel);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUI.changed = false;
            bool ust = GUILayout.Toggle(EditorPrefs.GetBool(SPTools.Settings_UseSpriteThumbsKey), " Use sprite thumbs ?");
            if (GUI.changed)
            {
                EditorPrefs.SetBool(SPTools.Settings_UseSpriteThumbsKey, ust);
                InvokeRepaint();
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Thumbs Max Height: " + EditorPrefs.GetFloat(SPTools.Settings_ThumbsHeightKey).ToString(), GUILayout.Width(150f));
            GUI.changed = false;
            float th = GUILayout.HorizontalSlider(EditorPrefs.GetFloat(SPTools.Settings_ThumbsHeightKey), 20f, 200f, GUILayout.ExpandWidth(true));
            if (GUI.changed)
            {
                EditorPrefs.SetFloat(SPTools.Settings_ThumbsHeightKey, Mathf.Round(th));
                InvokeRepaint();
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUI.changed = false;
            bool ssv = GUILayout.Toggle(EditorPrefs.GetBool(SPTools.Settings_UseScrollViewKey), " Use scroll view for sprites ?");
            if (GUI.changed)
            {
                EditorPrefs.SetBool(SPTools.Settings_UseScrollViewKey, ssv);
                InvokeRepaint();
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Scroll View Height: " + EditorPrefs.GetFloat(SPTools.Settings_ScrollViewHeightKey).ToString(), GUILayout.Width(150f));
            GUI.changed = false;
            float svs = GUILayout.HorizontalSlider(EditorPrefs.GetFloat(SPTools.Settings_ScrollViewHeightKey), 40f, 500f, GUILayout.ExpandWidth(true));
            if (GUI.changed)
            {
                EditorPrefs.SetFloat(SPTools.Settings_ScrollViewHeightKey, Mathf.Round(svs));
                InvokeRepaint();
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.Space((float)padding.right);
            GUILayout.EndHorizontal();
            GUILayout.Space((float)padding.bottom);
            GUILayout.EndVertical();
        }

        private void InvokeRepaint()
        {
            // Only repaint if the selected object is a sprite packer
            if (Selection.activeObject is SPInstance)
            {
                // Repaint by setting it dirty
                EditorUtility.SetDirty(Selection.activeObject);
            }
        }

        [MenuItem("Window/Simple Sprite Packer/Settings")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SPSettings));
        }
    }
}