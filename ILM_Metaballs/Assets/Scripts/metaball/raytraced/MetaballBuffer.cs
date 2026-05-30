using UnityEngine;

namespace Metaball
{
    internal struct MetaballMaterial
    {
        public Vector3 diffuse;
        // FIXME: sizeof(MetaballMaterial) is (20) off by one float from (16), probably turned into 32 bytes due to padding
        public float specular;
        public float fresnel;
    }
    internal struct MetaballProperties
    {
        public float strength;
    }

    internal enum MetaballType
    {
        Ball,
        // Cube,
        // [...]
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    internal struct MetaballAny
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Geometry.MetaballDescriptorBall ball;
        // [System.Runtime.InteropServices.FieldOffset(0)]
        // public Geometry.MetaballDescriptorCube cube;
        // [...]
    }

    internal struct Metaball
    {
        public MetaballType type;
        public MetaballAny any;
        public uint material;
        public MetaballProperties properties;
    }

    [CreateAssetMenu(fileName = "MetaballBuffer", menuName = "Scriptable Objects/MetaballBuffer")]
    public class MetaballBuffer : ScriptableObject
    {
        // public Array
    }
}