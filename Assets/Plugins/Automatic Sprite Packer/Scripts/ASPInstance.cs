using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleSpritePacker
{
    public class ASPInstance : ScriptableObject
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

        [SerializeField] private List<ASPSpriteInfo> m_Sprites = new();

        [SerializeField] private int          m_SpriteCount         = 0;
        [SerializeField] private List<string> m_IncludedRootFolders = new();

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
        public List<ASPSpriteInfo> sprites
        {
            get { return m_Sprites; }
        }

        /// <summary>
        /// Gets a copy of the list of sprites.
        /// </summary>
        /// <value>The copy of sprites.</value>
        public List<ASPSpriteInfo> copyOfSprites
        {
            get
            {
                var list = new List<ASPSpriteInfo>();
                foreach (ASPSpriteInfo i in m_Sprites)
                    list.Add(i);
                return list;
            }
        }

        /// <summary>
        /// Gets the list of pending actions.
        /// </summary>
        /// <value>The pending actions.</value>
        public List<string> includedRootFolders
        {
            get { return m_IncludedRootFolders; }
        }

        /// <summary>
        /// Changes the sprite source.
        /// </summary>
        /// <param name="spriteInfo">Sprite info.</param>
        /// <param name="newSource">New source.</param>
        public void ChangeSpriteSource(ASPSpriteInfo spriteInfo, Object newSource)
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
        public void RemoveFolder(string folder)
        {
            if (m_IncludedRootFolders.Contains(folder))
                m_IncludedRootFolders.Remove(folder);
        }

        /// <summary>
        /// Adds sprite to the sprite collection.
        /// </summary>
        /// <param name="spriteInfo">Sprite info.</param>
        public void AddSprite(ASPSpriteInfo spriteInfo)
        {
            if (spriteInfo != null)
                m_Sprites.Add(spriteInfo);
        }
    }
}