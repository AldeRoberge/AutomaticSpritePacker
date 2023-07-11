using UnityEngine;

namespace SimpleSpritePacker.Editor.Plugins.Simple_Sprite_Packer.Scripts.Editor
{
    [System.Serializable]
    public struct ASPSpriteImportData
    {
        public string          name;
        public Vector4         border;
        public SpriteAlignment alignment;
        public Vector2         pivot;
    }
}