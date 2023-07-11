using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleSpritePacker.Editor.Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    [CustomEditor(typeof(ASPInstance))]
    public class ASPInstanceEditor : UnityEditor.Editor
    {
        private ASPInstance     _mAspInstance;
        private ASPAtlasBuilder m_AtlasBuilder;

        private static Color darkGray = new(0.4f, 0.4f, 0.4f, 1f);

        private Vector2 scrollViewOffset           = Vector2.zero;
        private int     m_SelectedSpriteInstanceID = 0;

        private GUIStyle boxStyle;
        private GUIStyle paddingStyle;

        protected void OnEnable()
        {
            _mAspInstance = target as ASPInstance;
            m_AtlasBuilder = new ASPAtlasBuilder(_mAspInstance);

            ASPTools.PrepareDefaultEditorPrefs();

            boxStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).box);
            paddingStyle = new GUIStyle
            {
                padding = new RectOffset(3, 3, 3, 3)
            };
        }

        protected void OnDisable()
        {
            m_AtlasBuilder = null;
            _mAspInstance = null;
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

            if (_mAspInstance.includedRootFolders.Count > 0)
            {
                DrawRebuildAtlasButton();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space(12f, true);

            DropAreaGUI();
            EditorGUILayout.Space();

            DrawIncludedFolders();
            EditorGUILayout.Space();

            if (_mAspInstance.includedRootFolders.Count > 0)
            {
                // Get control rect
                Rect controlRect = EditorGUILayout.GetControlRect();

                GUI.color = Color.red;

                var folderS = _mAspInstance.includedRootFolders.Count > 1 ? "folders" : "folder";
                var spriteS = _mAspInstance.sprites.Count > 1 ? "sprites" : "sprite";

                if (GUI.Button(controlRect, $"× Remove all {_mAspInstance.sprites.Count} {spriteS} from {_mAspInstance.includedRootFolders.Count} {folderS}"))
                {
                    // Remove all sprites
                    _mAspInstance.sprites.Clear();
                    _mAspInstance.includedRootFolders.Clear();

                    // Rebuild atlas
                    m_AtlasBuilder.RebuildAtlas();
                }
            }
        }

        private void DrawRebuildAtlasButton()
        {
            // Get a rect for the buttons
            Rect controlRect = EditorGUILayout.GetControlRect();

            controlRect.height = 30f;

            GUI.color = Color.green;

            if (GUI.Button(controlRect, "↻ Rebuild Atlas"))
            {
                m_AtlasBuilder.RebuildAtlas();
            }
        }

        private void MaxSizePopup(SerializedProperty property, string label)
        {
            string[] names = { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192", "16384" };
            int[] sizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

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
            if (_mAspInstance == null)
                return;

            float labelWidth = 90f;

            if (_mAspInstance.includedRootFolders.Count == 0)
            {
                return;
            }

            EditorGUILayout.BeginVertical();

            var toRemoveFolder = new List<string>();

            // Draw the actions
            foreach (string folder in _mAspInstance.includedRootFolders)
            {
                GUI.color = darkGray;
                EditorGUILayout.BeginHorizontal(boxStyle);
                GUI.color = Color.white;


                // Draw Unity 'Folder' icon
                var icon = EditorGUIUtility.IconContent("Folder Icon");
                var iconRect = GUILayoutUtility.GetRect(icon, GUIStyle.none, GUILayout.Width(20f), GUILayout.Height(20f));
                GUI.DrawTexture(iconRect, icon.image);

                EditorGUILayout.LabelField("Folder", GUILayout.Width(50));
                EditorGUILayout.LabelField(folder);

                // Add 'Open' in explorer button
                if (GUILayout.Button("Open", GUILayout.Width(40f)))
                {
                    EditorUtility.RevealInFinder(folder);
                }

                // Add 'Ping' button
                if (GUILayout.Button("Ping", GUILayout.Width(40f)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(folder));
                }

                // Remove folder button
                if (GUILayout.Button("X", GUILayout.Width(20f)))
                {
                    toRemoveFolder.Add(folder);
                }

                EditorGUILayout.EndHorizontal();
            }

            foreach (string a in toRemoveFolder)
            {
                _mAspInstance.RemoveFolder(a);
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

            DrawMessage("＋ Drag and Drop Root Folders Here", drop_area);

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

                        var folders = ASPTools.FilterFoldersForAtlasImport(DragAndDrop.objectReferences);

                        // Ensure folder does not already exist
                        foreach (string folder in folders)
                        {
                            if (_mAspInstance.includedRootFolders.Contains(folder))
                                continue;

                            _mAspInstance.includedRootFolders.Add(folder);
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

        [MenuItem("Assets/Create/Automatic Sprite Packer")]
        public static void CreateInstance()
        {
            string assetPath = GetSavePath();

            if (string.IsNullOrEmpty(assetPath))
                return;

            // Create the sprite packer instance
            ASPInstance asset = ScriptableObject.CreateInstance<ASPInstance>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.Refresh();

            // Save the instance id in the editor prefs
            EditorPrefs.SetInt(ASPTools.Settings_SavedInstanceIDKey, asset.GetInstanceID());

            // Get a name for the texture
            string texturePath = assetPath.Replace(".asset", ".png");

            // Create blank texture
            if (ASPTools.CreateBlankTexture(texturePath, true))
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