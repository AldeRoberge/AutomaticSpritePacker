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

        private static Color gray     = new(0.3f, 0.3f, 0.3f, 1f);
        private static Color darkGray = new(0.3f, 0.3f, 0.3f, 1f);

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

            // Draw the texture preview
            if (m_SPInstance.texture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(100));

                EditorGUI.DrawPreviewTexture(rect, m_SPInstance.texture, null, ScaleMode.ScaleToFit);
            }

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
            
            if (m_SPInstance.includedFolders.Count > 0)
            {
                DrawRebuildAtlasButton();
                EditorGUILayout.Space();
            }

            DropAreaGUI();
            EditorGUILayout.Space();

            DrawIncludedFolders();
            EditorGUILayout.Space();
        }

        private void DrawRebuildAtlasButton()
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

        private void DrawIncludedFolders()
        {
            if (m_SPInstance == null)
                return;

            EditorGUILayout.LabelField($"{m_SPInstance.includedFolders.Count} folder(s) with {m_SPInstance.sprites.Count} sprite(s).", EditorStyles.boldLabel);

            float labelWidth = 90f;

            if (m_SPInstance.includedFolders.Count == 0)
            {
                return;
            }

            EditorGUILayout.BeginVertical();

            var toRemoveFolder = new List<string>();

            // Draw the actions
            foreach (string folder in m_SPInstance.includedFolders)
            {
                GUI.color = gray;
                EditorGUILayout.BeginHorizontal(boxStyle);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("Folder", GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(folder);

                // Remove folder button
                if (GUILayout.Button("X", GUILayout.Width(20f)))
                {
                    toRemoveFolder.Add(folder);
                }

                EditorGUILayout.EndHorizontal();
            }

            foreach (string a in toRemoveFolder)
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
            GUI.color = darkGray;

            //Set box text color to white

            GUI.enabled = true;

            DrawMessage("＋ Drop Folders Here", drop_area);

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
                        foreach (string folder in folders)
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

        private void DrawMessage(string message, Rect rect = default)
        {
            GUI.enabled = true;

            if (rect == default)
                rect = GUILayoutUtility.GetRect(0.0f, 25.0f, GUILayout.ExpandWidth(true));

            boxStyle.alignment = TextAnchor.MiddleCenter;
            boxStyle.normal.textColor = Color.white;

            GUI.color = Color.white;

            // Set text color
            GUI.backgroundColor = darkGray;

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