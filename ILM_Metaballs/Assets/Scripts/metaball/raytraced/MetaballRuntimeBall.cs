using UnityEngine;

namespace Metaball
{
    internal class MetaballRuntimeBall : MonoBehaviour
    {
        // TODO: bundle into descriptor
        [SerializeField]
        private Geometry.MetaballDescriptorBall descriptor;
        [SerializeField]
        private MetaballProperties properties;
        [SerializeField]
        private MetaballMaterialIndex material;

        [SerializeField]
        private MetaballBuffer buffer;

        void OnEnable()
        {
            // bu
        }
    }
}