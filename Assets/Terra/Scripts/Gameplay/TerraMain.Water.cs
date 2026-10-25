using System.Linq;
using UnityEngine.Rendering.Universal;

namespace Terra.Gameplay
{
    public sealed partial class TerraMain
    {
        void ConfigureWaterRendering()
        {
            // Depth and scene color belong to this camera, not the shared quality asset.
            // Locations without a mapped stream do not incur these additional passes.
            if(!world.features.Any(feature=>feature.kind=="water"))return;
            var data=view.GetComponent<UniversalAdditionalCameraData>();
            if(data==null)data=view.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.requiresDepthTexture=true;
            data.requiresColorTexture=true;
        }
    }
}
