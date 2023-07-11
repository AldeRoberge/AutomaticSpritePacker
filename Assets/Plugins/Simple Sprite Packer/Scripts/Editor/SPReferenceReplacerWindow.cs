using UnityEditor;
using UnityEngine;

namespace SimpleSpritePacker.Editor.Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    public class SPReferenceReplacerWindow : EditorWindow
    {
        public enum ReplaceMode : int
        {
            SourceWithAtlas,
            AtlasWithSource
        }

        public enum TargetMode : int
        {
            CurrentScene           = 0,
            ProjectOnly            = 1,
            CurrentSceneAndProject = 2,
            AllScenes              = 3,
            AllScenesAndProject    = 4,
        }

        private SPInstance  m_Instance;
        private TargetMode  m_TargetMode  = TargetMode.AllScenesAndProject;
        private ReplaceMode m_ReplaceMode = ReplaceMode.SourceWithAtlas;

        private static RectOffset padding                      = new(10, 10, 10, 10);
        public static  string     PrefsKey_TargetMode          = "SPRefReplacer_TargetMode";
        public static  string     PrefsKey_SpriteRenderersOnly = "SPRefReplacer_SpriteRenderersOnly";

        [MenuItem("Window/Simple Sprite Packer/Reference Replacer Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SPReferenceReplacerWindow));
        }

        protected void OnEnable()
        {
            titleContent = new GUIContent("SP Reference Replacer");

            if (EditorPrefs.HasKey(SPTools.Settings_SavedInstanceIDKey))
            {
                string instancePath = AssetDatabase.GetAssetPath(EditorPrefs.GetInt(SPTools.Settings_SavedInstanceIDKey, 0));

                if (!string.IsNullOrEmpty(instancePath))
                {
                    m_Instance = AssetDatabase.LoadAssetAtPath(instancePath, typeof(SPInstance)) as SPInstance;
                }
            }

            // Default prefs
            if (!EditorPrefs.HasKey(PrefsKey_TargetMode))
            {
                EditorPrefs.SetInt(PrefsKey_TargetMode, (int)m_TargetMode);
            }

            // Load target mode setting
            m_TargetMode = (TargetMode)EditorPrefs.GetInt(PrefsKey_TargetMode);
        }

        protected void OnGUI()
        {
            EditorGUIUtility.labelWidth = 100f;

            GUILayout.BeginVertical();
            GUILayout.Space((float)padding.top);
            GUILayout.BeginHorizontal();
            GUILayout.Space((float)padding.left);
            GUILayout.BeginVertical();

            GUI.changed = false;
            m_Instance = EditorGUILayout.ObjectField("Sprite Packer", m_Instance, typeof(SPInstance), false) as SPInstance;
            if (GUI.changed)
            {
                // Save the instance id
                EditorPrefs.SetInt(SPTools.Settings_SavedInstanceIDKey, (m_Instance == null) ? 0 : m_Instance.GetInstanceID());
            }

            GUILayout.Space(6f);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);

            EditorGUILayout.LabelField("Replace mode", GUILayout.Width(130f));
            m_ReplaceMode = (ReplaceMode)EditorGUILayout.EnumPopup(m_ReplaceMode);

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Replace references in", GUILayout.Width(130f));
            m_TargetMode = (TargetMode)EditorGUILayout.EnumPopup(m_TargetMode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(PrefsKey_TargetMode, (int)m_TargetMode);
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUI.changed = false;
            bool spriteRenderersOnly = GUILayout.Toggle(EditorPrefs.GetBool(PrefsKey_SpriteRenderersOnly), " Replace references in Sprite Renderers only ?");
            if (GUI.changed)
            {
                EditorPrefs.SetBool(PrefsKey_SpriteRenderersOnly, spriteRenderersOnly);
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.EndVertical();

            GUILayout.Space(6f);

            if (m_Instance == null)
            {
                EditorGUILayout.HelpBox("Please set the sprite packer instance reference in order to use this feature.", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Replace"))
                {
                    int replacedCount = 0;

                    switch (m_TargetMode)
                    {
                        case TargetMode.CurrentScene:
                        {
                            replacedCount += SPTools.ReplaceReferencesInScene(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            break;
                        }
                        case TargetMode.ProjectOnly:
                        {
                            replacedCount += SPTools.ReplaceReferencesInProject(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            break;
                        }
                        case TargetMode.CurrentSceneAndProject:
                        {
                            replacedCount += SPTools.ReplaceReferencesInProject(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            replacedCount += SPTools.ReplaceReferencesInScene(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            break;
                        }
                        case TargetMode.AllScenes:
                        {
                            replacedCount += SPTools.ReplaceReferencesInAllScenes(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly, false);
                            break;
                        }
                        case TargetMode.AllScenesAndProject:
                        {
                            replacedCount += SPTools.ReplaceReferencesInProject(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            replacedCount += SPTools.ReplaceReferencesInScene(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly);
                            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                            replacedCount += SPTools.ReplaceReferencesInAllScenes(m_Instance.copyOfSprites, m_ReplaceMode, spriteRenderersOnly, true);
                            break;
                        }
                    }

                    EditorUtility.DisplayDialog("Reference Replacer", $"Replaced references count: {replacedCount}", "Okay");
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space((float)padding.right);
            GUILayout.EndHorizontal();
            GUILayout.Space((float)padding.bottom);
            GUILayout.EndVertical();
        }
    }
}