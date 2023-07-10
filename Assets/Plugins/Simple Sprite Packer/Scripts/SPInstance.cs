using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private List<SPSpriteInfo> m_Sprites        = new();
        [SerializeField] private List<SPAction>     m_PendingActions = new();

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
        public List<SPAction> pendingActions
        {
            get { return m_PendingActions; }
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
        /// Queues add sprite action.
        /// </summary>
        /// <param name="resource">Resource.</param>
        public void QueueAction_AddSprite(Object resource)
        {
            if (resource is Texture2D || resource is Sprite)
            {
                // Check if that sprite is already added to the queue
                if (m_PendingActions.Find(a => (a.actionType == SPAction.ActionType.Sprite_Add && a.resource == resource)) != null)
                    return;

                SPAction action = new SPAction
                {
                    actionType = SPAction.ActionType.Sprite_Add,
                    resource = resource
                };
                m_PendingActions.Add(action);
            }
        }

        /// <summary>
        /// Queues add sprites action.
        /// </summary>
        /// <param name="resources">Resources.</param>
        public void QueueAction_AddSprites(Object[] resources)
        {
            foreach (Object resource in resources)
            {
                QueueAction_AddSprite(resource);
            }
        }

        /// <summary>
        /// Queues remove sprite action.
        /// </summary>
        /// <param name="spriteInfo">Sprite info.</param>
        public void QueueAction_RemoveSprite(SPSpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return;

            if (!m_Sprites.Contains(spriteInfo))
                return;

            // Check if that sprite is already added to the queue
            if (m_PendingActions.Find(a => (a.actionType == SPAction.ActionType.Sprite_Remove && a.spriteInfo == spriteInfo)) != null)
                return;

            SPAction action = new SPAction
            {
                actionType = SPAction.ActionType.Sprite_Remove,
                spriteInfo = spriteInfo
            };
            m_PendingActions.Add(action);
        }

        /// <summary>
        /// Unqueues action.
        /// </summary>
        /// <param name="action">Action.</param>
        public void UnqueueAction(SPAction action)
        {
            if (m_PendingActions.Contains(action))
                m_PendingActions.Remove(action);
        }

        /// <summary>
        /// Gets the a list of add sprite actions.
        /// </summary>
        /// <returns>The add sprite actions.</returns>
        protected List<SPAction> GetAddSpriteActions()
        {
            var actions = new List<SPAction>();

            foreach (SPAction action in m_PendingActions)
            {
                if (action.actionType == SPAction.ActionType.Sprite_Add)
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        /// <summary>
        /// Gets a list of remove sprite actions.
        /// </summary>
        /// <returns>The remove sprite actions.</returns>
        protected List<SPAction> GetRemoveSpriteActions()
        {
            var actions = new List<SPAction>();

            foreach (SPAction action in m_PendingActions)
            {
                if (action.actionType == SPAction.ActionType.Sprite_Remove)
                {
                    actions.Add(action);
                }
            }

            return actions;
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
        /// Clears the current actions.
        /// </summary>
        public void ClearActions()
        {
            m_PendingActions.Clear();
        }

        /// <summary>
        /// Gets a sprite list with applied actions.
        /// </summary>
        /// <returns>The sprite list with applied actions.</returns>
        public List<SPSpriteInfo> GetSpriteListWithAppliedActions()
        {
            // Create temporary sprite info list
            // Add the current sprites
            var spriteInfoList = m_Sprites.ToList();

            // Apply the remove actions
            foreach (SPAction ra in GetRemoveSpriteActions())
            {
                if (spriteInfoList.Contains(ra.spriteInfo))
                    spriteInfoList.Remove(ra.spriteInfo);
            }

            // Apply the add actions
            foreach (SPAction asa in GetAddSpriteActions())
            {
                SPSpriteInfo si = new SPSpriteInfo
                {
                    source = asa.resource
                };
                spriteInfoList.Add(si);
            }

            // return the list
            return spriteInfoList;
        }
    }
}