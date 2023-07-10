using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleSpritePacker
{
    public class SPInstance : ScriptableObject
    {
        public enum PackingMethod
        {
            MaxRects,
            Unity
        }

        [SerializeField] private Texture2D       m_Texture;
        [SerializeField] private int             m_Padding            = 1;
        [SerializeField] private int             m_MaxSize            = 4096;
        [SerializeField] private PackingMethod   m_PackingMethod      = PackingMethod.MaxRects;
        [SerializeField] private SpriteAlignment m_DefaultPivot       = SpriteAlignment.Center;
        [SerializeField] private Vector2         m_DefaultCustomPivot = new(0.5f, 0.5f);

        [SerializeField]                                            private List<SPSpriteInfo> m_Sprites         = new();
        [FormerlySerializedAs("mIncludedFolders")] [SerializeField] private List<SPFolder>     m_IncludedFolders = new();

        /// <summary>
        /// Gets or sets the atlas texture.
        /// </summary>
        /// <value>The texture.</value>
        public Texture2D texture
        {
            get { return m_Texture; }
            set
            {
                m_Texture = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the packing padding.
        /// </summary>
        /// <value>The padding.</value>
        public int padding
        {
            get { return m_Padding; }
            set
            {
                m_Padding = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the max packing size.
        /// </summary>
        /// <value>The size of the max.</value>
        public int maxSize
        {
            get { return m_MaxSize; }
            set
            {
                m_MaxSize = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the packing method.
        /// </summary>
        /// <value>The packing method.</value>
        public PackingMethod packingMethod
        {
            get { return m_PackingMethod; }
            set
            {
                m_PackingMethod = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the default pivot.
        /// </summary>
        /// <value>The default pivot.</value>
        public SpriteAlignment defaultPivot
        {
            get { return m_DefaultPivot; }
            set
            {
                m_DefaultPivot = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the default custom pivot.
        /// </summary>
        /// <value>The default custom pivot.</value>
        public Vector2 defaultCustomPivot
        {
            get { return m_DefaultCustomPivot; }
            set
            {
                m_DefaultCustomPivot = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets the list of sprites.
        /// </summary>
        /// <value>The sprites.</value>
        public List<SPSpriteInfo> sprites
        {
            get { return m_Sprites; }
        }

        /// <summary>
        /// Gets a copy of the list of sprites.
        /// </summary>
        /// <value>The copy of sprites.</value>
        public List<SPSpriteInfo> copyOfSprites
        {
            get
            {
                var list = new List<SPSpriteInfo>();
                foreach (SPSpriteInfo i in m_Sprites)
                    list.Add(i);
                return list;
            }
        }

        /// <summary>
        /// Gets the list of pending actions.
        /// </summary>
        /// <value>The pending actions.</value>
        public List<SPFolder> includedFolders
        {
            get { return m_IncludedFolders; }
        }

        /// <summary>
        /// Changes the sprite source.
        /// </summary>
        /// <param name="spriteInfo">Sprite info.</param>
        /// <param name="newSource">New source.</param>
        public void ChangeSpriteSource(SPSpriteInfo spriteInfo, Object newSource)
        {
            // Validate the new source
            if (newSource == null)
            {
                spriteInfo.source = null;
            }
            else if (newSource is Texture2D || newSource is Sprite)
            {
                spriteInfo.source = newSource;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Unqueues action.
        /// </summary>
        /// <param name="folder">Action.</param>
        public void RemoveFolder(SPFolder folder)
        {
            if (m_IncludedFolders.Contains(folder))
                m_IncludedFolders.Remove(folder);
        }


        /// <summary>
        /// Clears the current sprites collection data.
        /// </summary>
        public void ClearSprites()
        {
            m_Sprites.Clear();
        }

        /// <summary>
        /// Adds sprite to the sprite collection.
        /// </summary>
        /// <param name="spriteInfo">Sprite info.</param>
        public void AddSprite(SPSpriteInfo spriteInfo)
        {
            if (spriteInfo != null)
                m_Sprites.Add(spriteInfo);
        }


        /// <summary>
        /// Gets a sprite list with applied actions.
        /// </summary>
        /// <returns>The sprite list with applied actions.</returns>
        public List<SPSpriteInfo> GetSpriteListWithAppliedActions()
        {
            // Create temporary sprite info list
            // Add the current sprites
            var spriteInfoList = sprites.ToList();

            // Apply the add actions
            foreach (SPFolder asa in includedFolders)
            {
                var assets = GetDirectoryAssets(asa.FolderPath);

                foreach (Object asset in assets)
                {
                    // Ensure is not a duplicate
                    if (spriteInfoList.Any(si => si.source == asset))
                        continue;

                    // Ensure is Sprite or Texture2D
                    SPSpriteInfo si = new SPSpriteInfo
                    {
                        source = asset
                    };

                    spriteInfoList.Add(si);
                }
            }

            // return the list
            return spriteInfoList;
        }

        /// <summary>
        /// Gets the assets in the specified directory.
        /// </summary>
        /// <returns>The directory assets.</returns>
        /// <param name="path">Path.</param>
        public static Object[] GetDirectoryAssets(string path)
        {
            var assets = new List<Object>();

            // Get the file paths of all the files in the specified directory
            string[] assetPaths = Directory.GetFiles(path);

            // Enumerate through the list of files loading the assets they represent
            foreach (string assetPath in assetPaths)
            {
                // Check if it's a meta file
                if (assetPath.Contains(".meta"))
                    continue;

                Object objAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

                if (objAsset != null)
                    assets.Add(objAsset);
            }

            // Return the array of objects
            return assets.ToArray();
        }
    }
}