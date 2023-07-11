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

                // Default
                return source != null ? source.name :
                    string.Empty;
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
                    switch (source)
                    {
                        case Texture2D texture2D:
                            return new Vector2(texture2D.width, texture2D.height);
                        case Sprite sprite:
                            return sprite.rect.size;
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
            return string.Compare(name, other.name, StringComparison.Ordinal);
        }
    }
}