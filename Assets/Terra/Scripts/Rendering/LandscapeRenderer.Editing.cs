using Terra.Contracts;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        string movePreviewSource;
        // Preview both sides of an atomic move without changing canonical state.
        public void PreviewMoveSource(Site site)
        {
            movePreviewSource=site.id;
            if(proposals.TryGetValue(site.id,out var prior))prior.gameObject.SetActive(false);
            baselines[site.id].gameObject.SetActive(true);
        }
        public void PreviewOriginal(Site site)
        {
            ClearEditPreview();PreviewMoveSource(site);
        }
        public void ClearEditPreview()
        {
            ClearGhost();
            if(movePreviewSource==null)return;
            if(proposals.TryGetValue(movePreviewSource,out var prior))prior.gameObject.SetActive(true);
            baselines[movePreviewSource].gameObject.SetActive(!proposals.ContainsKey(movePreviewSource));
            movePreviewSource=null;
        }
    }
}
