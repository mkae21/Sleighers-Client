using UnityEngine;

namespace CharacterCustomization
{
    public class Variant
    {
        public readonly string Name;
        public readonly GameObject PreviewObject;
        public readonly Mesh Mesh;

        public Variant(Mesh mesh, GameObject previewObject)
        {
            Name = mesh.name;
            Mesh = mesh;
            PreviewObject = previewObject;
        }
    }
}