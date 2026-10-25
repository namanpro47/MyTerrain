using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // A cancelled project leaves a cleared surface, never a fully restored meadow.
        public void ApplyClearedProject(Site site)
        {
            var holder=Apply(site,"cultivate-standard",0);
            if(holder!=null)foreach(Transform child in holder)child.gameObject.SetActive(false);
        }
    }
}
