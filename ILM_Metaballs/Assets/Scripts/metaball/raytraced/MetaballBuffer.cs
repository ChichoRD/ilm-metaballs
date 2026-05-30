using UnityEngine;

namespace Metaball
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Serializable]
    internal struct MetaballMaterial
    {
        public Vector3 diffuse;
        // FIXME: sizeof(MetaballMaterial) is (20) off by one float from (16), probably turned into 32 bytes due to padding
        public float specular;
        public float fresnel;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Serializable]
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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal struct MetaballMaterialIndex
    {
        public uint value;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal struct Metaball
    {
        public MetaballType type;
        public MetaballAny any;
        public MetaballMaterialIndex material;
        public MetaballProperties properties;
    }


    [CreateAssetMenu(fileName = "MetaballBuffer", menuName = "Scriptable Objects/MetaballBuffer")]
    internal class MetaballBuffer : ScriptableObject
    {
        public struct MetaballIndex
        {
            public ulong value;
        }
        [System.Flags]
        public enum BufferFlags : ushort
        {
            None = 0,
            Dirty = 1 << 0,
        }

        [SerializeField]
        private MetaballMaterial[] defaultMaterials = new MetaballMaterial[]
        {
            new MetaballMaterial() { diffuse = new Vector3(1, 0, 0), specular = 0.5f, fresnel = 0.5f },
            new MetaballMaterial() { diffuse = new Vector3(0, 1, 0), specular = 0.5f, fresnel = 0.5f },
            new MetaballMaterial() { diffuse = new Vector3(0, 0, 1), specular = 0.5f, fresnel = 0.5f },
        };
        [HideInInspector]
        public BufferFlags flags;
        [HideInInspector]
        public InplaceList<Metaball> metaballs;
        [HideInInspector]
        public InplaceList<MetaballMaterial> materials;

        public void Init()
        {
            metaballs = InplaceList<Metaball>.Create();
            materials = InplaceList<MetaballMaterial>.Create();
        }
        public void Deinit()
        {
            flags = BufferFlags.None;
            InplaceList<Metaball>.Destroy(ref metaballs);
            InplaceList<MetaballMaterial>.Destroy(ref materials);
        }
        public void Reinit()
        {
            Deinit();
            Init();
        }

        private void OnEnable()
        {
            Reinit();
            InsertMaterialRange(defaultMaterials);
        }
        private void OnDisable()
        {
            Deinit();
        }

        public MetaballIndex InsertMetaball(Metaball metaball)
        {
            flags |= BufferFlags.Dirty;
            MetaballIndex index = new MetaballIndex() { value = metaballs.Count };
            metaballs.Add(metaball);
            return index;
        }
        public MetaballMaterialIndex InsertMaterial(MetaballMaterial material)
        {
            flags |= BufferFlags.Dirty;
            MetaballMaterialIndex index = new MetaballMaterialIndex() { value = (uint)materials.Count };
            materials.Add(material);
            return index;
        }
        public MetaballMaterialIndex InsertMaterialRange(System.ReadOnlySpan<MetaballMaterial> materials)
        {
            flags |= BufferFlags.Dirty;
            MetaballMaterialIndex index = new MetaballMaterialIndex() { value = (uint)this.materials.Count };
            this.materials.AddRange(materials);
            return index;
        }

        public void RemoveMetaball(MetaballIndex index)
        {
            flags |= BufferFlags.Dirty;
            metaballs.SwapLastPop(index.value);
        }
        // XXX: invalidates metaballs referencing this material
        // double indirection buffer?: https://www.aortiz.me/2018/12/21/CG.html#light-culling-methods
        // public void RemoveMaterial(MaterialIndex index)
        // {
        //     flags |= BufferFlags.Dirty;
        //     materials.SwapLastPop(index.value);
        // }

        public ref readonly Metaball GetMetaball(MetaballIndex index)
        {
            return ref metaballs.Get(index.value);
        }
        public ref readonly MetaballMaterial GetMaterial(MetaballMaterialIndex index)
        {
            return ref materials.Get(index.value);
        }
        public ref Metaball GetMetaballMut(MetaballIndex index)
        {
            flags |= BufferFlags.Dirty;
            return ref metaballs.GetMut(index.value);
        }
        public ref MetaballMaterial GetMaterialMut(MetaballMaterialIndex index)
        {
            flags |= BufferFlags.Dirty;
            return ref materials.GetMut(index.value);
        }
    }
}