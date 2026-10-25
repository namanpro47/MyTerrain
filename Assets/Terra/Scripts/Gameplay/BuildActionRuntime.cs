using System;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    public static class BuildActionRuntime
    {
        public static BuildActionRules Expand(BuildActionCatalog catalog, ref WorldData world, ref Scenario scenario)
        {
            if(catalog==null)throw new InvalidOperationException("Build action art catalog is missing. Run the build-action setup before exporting.");
            // Keep canonical source data and the separate annual demo unchanged.
            world=JsonUtility.FromJson<WorldData>(JsonUtility.ToJson(world));
            scenario=JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(scenario));
            var rules=new BuildActionRules(world,catalog.entries.Select(e=>{
                string id=e.actionId=="cultivation"?"cultivate-standard":e.actionId=="living-margin"?"cultivate-margin":e.actionId=="ground-cover-restoration"?"restore-grass":e.actionId=="vegetated-swale"?"water-swale":e.actionId;
                string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
                int cost=category=="Residential"?30:category=="Utilities"?36:category=="Community"?22:category=="Nature"?14:26;
                return new BuildActionRule{id=id,artId=e.actionId,title=e.title,category=category,cost=cost,width=e.siteSizeMetres.x,depth=e.siteSizeMetres.y,landscape=e.landscapeSample,description=e.dimensionNotes};
            }).ToArray());
            rules.ExpandOffers(world,scenario);
            return rules;
        }
    }
}
