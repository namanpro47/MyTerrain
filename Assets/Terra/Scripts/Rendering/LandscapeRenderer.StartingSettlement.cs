using System;
using System.Linq;
using Terra.Core;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        public void BuildStartingSettlement(UniformParcelPack pack)
        {
            foreach(var building in pack.initialBuildings)
            {
                var site=world.sites.Single(s=>s.id==building.siteId);
                var parent=baselines[site.id];
                parent.name=building.title+" · inherited baseline";
                if(!BuildCatalogAction(site,building.actionId,parent,false))
                    throw new InvalidOperationException("Starting community model does not fit: "+building.title);
            }
            // These objects belong to the baseline. Annual proposals/undo/comparison never
            // destroy them, charge for them or repeat the pre-existing service capacities.
        }
    }
}
