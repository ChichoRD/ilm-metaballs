using UnityEngine;

namespace Metaball.Geometry
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Serializable]
    internal struct MetaballDescriptorBall
    {
        public Vector3 position;
        public float radius;
    }
}