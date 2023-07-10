using UnityEngine;
using System;

namespace SimpleSpritePacker
{
    [Serializable]
    public class SPSpriteInfo : IComparable<SPSpriteInfo>
    {
        /// <summary>
        /// The source texture or sprite.
        /// </summary>
        public UnityEngine.Object source;

        /// <summary>
        /// The target sprite (the one in the atlas).
        /// </summary>
        public Sprite targetSprite;

        /// <summary>
        /// Gets the name of the sprite.
        /// </summary>
        /// <value>The name.</value>
        public string name
        {
            get
            {
                if (targetSprite != null)
                {
                    return targetSprite.name;
                }
                else if (source != null)
                {
                    return source.name;
                }

                // Default
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the sprite size used for comparison.
        /// </summary>
        /// <value>The size for comparison.</value>
        public Vector2 sizeForComparison
        {
            get
            {
                if (source != null)
                {
                    if (source is Texture2D)
                    {
                        return new Vector2((source as Texture2D).width, (source as Texture2D).height);
                    }
                    else if (source is Sprite)
                    {
                        return (source as Sprite).rect.size;
                    }
                }
                else if (targetSprite != null)
                {
                    return targetSprite.rect.size;
                }

                // Default
                return Vector2.zero;
            }
        }

        public int CompareTo(SPSpriteInfo other)
        {
            return name.CompareTo(other.name);
        }
    }
}