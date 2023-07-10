using System.Collections.Generic;
using System.Linq;
using SimpleSpritePacker;
using UnityEditor;
using UnityEngine;

namespace Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    [CustomEditor(typeof(SPInstance))]
    public class SPInstanceEditor : UnityEditor.Editor
    {
        private SPInstance     m_SPInstance;
        private SPAtlasBuilder m_AtlasBuilder;

        private static Color green                   = new(0.345f, 0.625f, 0.370f, 1f);
        private static Color red                     = new(0.779f, 0.430f, 0.430f, 1f);
        private static Color spriteBoxNormalColor    = new(0.897f, 0.897f, 0.897f, 1f);
        private static Color spriteBoxHighlightColor = new(0.798f, 0.926f, 0.978f, 1f);

        private Vector2 scrollViewOffset           = Vector2.zero;
        private int     m_SelectedSpriteInstanceID = 0;

        private GUIStyle boxStyle;
        private GUIStyle paddingStyle;

        protected void OnEnable()
        {
            m_SPInstance = target as SPInstance;
            m_AtlasBuilder = new SPAtlasBuilder(m_SPInstance);

            SPTools.PrepareDefaultEditorPrefs();

            boxStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).box);
            paddingStyle = new GUIStyle
            {
                padding = new RectOffset(3, 3, 3, 3)
            };
        }

        protected void OnDisable()
        {
            m_AtlasBuilder = null;
            m_SPInstance = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Texture"), new GUIContent("Atlas Texture"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Padding"), new GUIContent("Packing Padding"));
            MaxSizePopup(serializedObject.FindProperty("m_MaxSize"), "Packing Max Size");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PackingMethod"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultPivot"));

            if ((SpriteAlignment)serializedObject.FindProperty("m_DefaultPivot").enumValueIndex != SpriteAlignment.Custom)
                GUI.enabled = false;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultCustomPivot"), new GUIContent("Default Cus. Pivot"));

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            DrawPendingActions();
            EditorGUILayout.Space();

            DropAreaGUI();
            EditorGUILayout.Space();

            if (m_SPInstance.includedFolders.Count > 0)
            {
                DrawActionButtons();
                EditorGUILayout.Space();
            }
        }

        private void DrawActionButtons()
        {
            // Get a rect for the buttons
            Rect controlRect = EditorGUILayout.GetControlRect();

            if (GUI.Button(controlRect, "Rebuild Atlas", EditorStyles.miniButton))
            {
                m_AtlasBuilder.RebuildAtlas();
            }
        }

        private void MaxSizePopup(SerializedProperty property, string label)
        {
            string[] names = { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
            int[] sizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };

            GUI.changed = false;
            int size = EditorGUILayout.IntPopup(label, property.intValue, names, sizes);

            if (GUI.changed)
            {
                property.intValue = size;
            }
        }


        private Color c;

        private void DrawPendingActions()
        {
            if (m_SPInstance == null)
                return;

            EditorGUILayout.LabelField($"{m_SPInstance.includedFolders.Count} folder(s) with {m_SPInstance.sprites.Count} sprite(s).", EditorStyles.boldLabel);

            float labelWidth = 90f;

            if (m_SPInstance.includedFolders.Count == 0)
            {
                DrawMessage("There are no included folders.");
                return;
            }

            EditorGUILayout.BeginVertical();

            var toRemoveList = new List<SPFolder>();

            // Draw the actions
            foreach (SPFolder action in m_SPInstance.includedFolders)
            {
                GUI.color = green;
                EditorGUILayout.BeginHorizontal(boxStyle);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("Folder", GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(action.FolderPath);

                // Remove folder button
                if (GUILayout.Button("X", GUILayout.Width(20f)))
                {
                    toRemoveList.Add(action);
                }

                EditorGUILayout.EndHorizontal();
            }

            // Unqueue actions in the list
            foreach (SPFolder a in toRemoveList)
            {
                m_SPInstance.RemoveFolder(a);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DropAreaGUI()
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            boxStyle.alignment = TextAnchor.MiddleCenter;
            GUI.color = green;
            GUI.Box(drop_area, "＋ Drop Folders Here", boxStyle);
            GUI.color = Color.white;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                {
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        var folders = SPTools.FilterFoldersForAtlasImport(DragAndDrop.objectReferences);

                        // Ensure folder does not already exist
                        foreach (SPFolder folder in folders)
                        {
                            if (m_SPInstance.includedFolders.Contains(folder))
                                continue;

                            m_SPInstance.includedFolders.Add(folder);
                        }
                    }

                    break;
                }
            }
        }

        private void DrawMessage(string message)
        {
            Rect rect = GUILayoutUtility.GetRect(0.0f, 25.0f, GUILayout.ExpandWidth(true));
            boxStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Box(rect, message, boxStyle);
        }

        private bool IsSelected(int id)
        {
            return (m_SelectedSpriteInstanceID == id);
        }

        private void SetSelected(int id)
        {
            m_SelectedSpriteInstanceID = id;
            Repaint();
        }

        private static string GetSavePath()
        {
            string path = "Assets";
            Object obj = Selection.activeObject;

            if (obj != null)
            {
                path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

                if (path.Length > 0)
                {
                    if (!System.IO.Directory.Exists(path))
                    {
                        string[] pathParts = path.Split("/"[0]);
                        pathParts[pathParts.Length - 1] = "";
                        path = string.Join("/", pathParts);
                    }
                }
            }

            return EditorUtility.SaveFilePanelInProject("Sprite Packer", "Sprite Packer", "asset", "Create a new sprite packer instance.", path);
        }

        [MenuItem("Assets/Create/Sprite Packer")]
        public static void CreateInstance()
        {
            string assetPath = GetSavePath();

            if (string.IsNullOrEmpty(assetPath))
                return;

            // Create the sprite packer instance
            SPInstance asset = ScriptableObject.CreateInstance<SPInstance>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.Refresh();

            // Save the instance id in the editor prefs
            EditorPrefs.SetInt(SPTools.Settings_SavedInstanceIDKey, asset.GetInstanceID());

            // Get a name for the texture
            string texturePath = assetPath.Replace(".asset", ".png");

            // Create blank texture
            if (SPTools.CreateBlankTexture(texturePath, true))
            {
                // Set the texture reff in the sprite packer instance
                asset.texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
            }

            // Focus on the new sprite packer
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}