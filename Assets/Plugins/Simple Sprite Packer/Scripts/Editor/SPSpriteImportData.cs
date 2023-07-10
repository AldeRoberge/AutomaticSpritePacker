using UnityEngine;

namespace Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    [System.Serializable]
    public struct SPSpriteImportData
    {
        public string          name;
        public Vector4         border;
        public SpriteAlignment alignment;
        public Vector2         pivot;
    }
}