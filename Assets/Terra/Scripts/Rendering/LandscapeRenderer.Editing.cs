using Terra.Contracts;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Reuse the preview lifecycle so cancel, undo and commit restore visibility atomically.
        public void PreviewBaseline(Site site)
        {
            ClearGhost();
            if(site==null)return;
            ghostSiteId=site.id;ghostTreatment="";surfaceDirty=true;
            if(baselineVegetation.TryGetValue(site.id,out var vegetation))vegetation.Exclude(null);
            if(proposals.TryGetValue(site.id,out var proposal))proposal.gameObject.SetActive(false);
            baselines[site.id].gameObject.SetActive(true);
        }
    }
}
