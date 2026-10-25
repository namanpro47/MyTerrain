using UnityEngine;

namespace Terra.Rendering
{
    // Broad, low plants need detail based on their footprint rather than tree height.
    public sealed class NatureLodProfile : MonoBehaviour
    {
        public float nearPixels=20;
        public float farPixels=8;
        public float cullPixels=.6f;
        public bool includeWidth=true;
    }
}
