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
            var sourceWorld=world; var sourceScenario=scenario;
            var rules=new BuildActionRules(world,catalog.entries.Select(e=>{
                string id=e.actionId=="cultivation"?"cultivate-standard":e.actionId=="living-margin"?"cultivate-margin":e.actionId=="ground-cover-restoration"?"restore-grass":e.actionId=="vegetated-swale"?"water-swale":e.actionId;
                string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
                int cost=category=="Residential"?30:category=="Utilities"?36:category=="Community"?22:category=="Nature"?14:26;
                return new BuildActionRule{id=id,artId=e.actionId,title=e.title,category=category,cost=cost,width=e.siteSizeMetres.x,depth=e.siteSizeMetres.y,landscape=e.landscapeSample,description=e.dimensionNotes};
            }).ToArray());
            foreach(var rule in rules.actions)
            {
                var legacy=sourceScenario.treatments.FirstOrDefault(t=>t.id==rule.id);
                if(legacy!=null){legacy.title=rule.title;continue;}
                bool farming=rule.category=="Farming",green=rule.landscape||rule.id=="public-park";
                // These are disclosed visual/accounting assumptions. No new jobs, water benefit,
                // energy output or annual economic effects are invented by choosing a model.
                var treatment=new Treatment{id=rule.id,title=rule.title,family=rule.category.ToLowerInvariant(),description=rule.description,visualRecipeId=rule.artId,modelParameterId="unmodeled-build-action",productiveFraction=farming?1:0,
                    cover=new[]{new CoverFraction{coverId=farming?"cultivated":green?"grass":"sealed",fraction=1}}};
                sourceScenario.treatments=sourceScenario.treatments.Concat(new[]{treatment}).ToArray();
            }
            foreach(var site in sourceWorld.sites.Where(s=>!s.protectedSite))
            {
                var extras=rules.actions.Where(a=>!site.allowedTreatments.Contains(a.id)&&rules.Rejection(site,a.id)==null).ToArray();
                site.allowedTreatments=site.allowedTreatments.Concat(extras.Select(a=>a.id)).ToArray();
                site.offers=site.offers.Concat(extras.Select(a=>new SiteOffer{treatmentId=a.id,cost=a.cost,production=a.category=="Farming"?site.baselineProduction:0,retentionCapacityM3=0})).ToArray();
            }
            return rules;
        }
    }
}
