using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Validated baseline DTOs belong to the loader and are never written by edits.
    public sealed class WorldStateStore
    {
        readonly WorldData world;
        readonly Scenario scenario;
        Intervention[] current = Array.Empty<Intervention>();
        readonly Stack<Intervention[]> history = new Stack<Intervention[]>();
        readonly Stack<Intervention[]> future = new Stack<Intervention[]>();
        public int Revision { get; private set; }
        public bool CanUndo => history.Count > 0;
        public bool CanRedo => future.Count > 0;
        public bool HasProposals => current.Length > 0;
        public WorldResult Result => Calculate(current);
        public string TreatmentAt(string siteId) => current.FirstOrDefault(x => x.siteId == siteId)?.treatmentId;
        public WorldStateStore(WorldData world, Scenario scenario) { this.world = world; this.scenario = scenario; }

        public Evaluation Preview(EditCommand command)
        {
            if (command == null) return Invalid("Choose an editing action.");
            if (command.expectedRevision != Revision) return Invalid("The landscape changed. Preview again.");
            if (command.operation == "reset")
                return HasProposals ? Evaluate(Array.Empty<Intervention>(), "Ready to reset all proposals") : Invalid("The landscape is already at its baseline.");
            if (command.operation != "add" && command.operation != "replace" && command.operation != "remove" && command.operation != "move")
                return Invalid("Choose a supported editing action.");
            var site = world.sites.FirstOrDefault(x => x.id == command.siteId);
            if (site == null) return Invalid("Choose a management site.");
            if (site.protectedSite) return Invalid(site.protectionReason);
            var existing = current.FirstOrDefault(x => x.siteId == site.id);
            if (command.operation == "add" && existing != null) return Invalid("This site already has a proposal.");
            if (command.operation != "add" && existing == null) return Invalid("There is no proposal to " + command.operation + ".");
            var next = current.Where(x => x.siteId != site.id).ToList();
            if (command.operation == "remove") return Evaluate(next.ToArray(), "Ready to restore the original land");

            var destination = site;
            // Moving preserves the treatment, but uses the destination's area, price and capacity.
            string treatment = command.operation == "move" ? existing.treatmentId : command.treatmentId;
            if (command.operation == "move")
            {
                destination = world.sites.FirstOrDefault(x => x.id == command.destinationSiteId);
                if (destination == null) return Invalid("Choose a destination for this proposal.");
                if (destination.id == site.id) return Invalid("Choose a different destination.");
                if (destination.protectedSite) return Invalid(destination.protectionReason);
                if (TreatmentAt(destination.id) != null) return Invalid("The destination already has a proposal. Remove it first or choose another site.");
            }
            if (!destination.allowedTreatments.Contains(treatment) || !destination.offers.Any(x => x.treatmentId == treatment) || !scenario.treatments.Any(x => x.id == treatment))
                return Invalid("This treatment is not available at this site.");
            if (command.operation == "replace" && existing.treatmentId == treatment) return Invalid("This proposal is already placed.");
            next.Add(new Intervention { siteId = destination.id, treatmentId = treatment });
            return Evaluate(next.ToArray(), command.operation == "move" ? "Ready to move" : "Ready to place");
        }
        Evaluation Evaluate(Intervention[] next, string reason)
        {
            var result = Calculate(next);
            if (result.remainingCredits < 0) return Invalid("Not enough credits. Remove or undo a proposal to try this one.");
            return new Evaluation { valid = true, reason = reason, result = result, interventions = Clone(next) };
        }
        public Evaluation Commit(EditCommand command)
        {
            var evaluation = Preview(command);
            if (!evaluation.valid) return evaluation;
            history.Push(Clone(current));
            current = Clone(evaluation.interventions);
            future.Clear();
            Revision++;
            evaluation.result = Calculate(current);
            return evaluation;
        }
        public bool Undo()
        {
            if (!CanUndo) return false;
            future.Push(Clone(current)); current = history.Pop(); Revision++; return true;
        }
        public bool Redo()
        {
            if (!CanRedo) return false;
            history.Push(Clone(current)); current = future.Pop(); Revision++; return true;
        }
        static Intervention[] Clone(Intervention[] values) => values.Select(x => new Intervention { siteId = x.siteId, treatmentId = x.treatmentId }).ToArray();
        Evaluation Invalid(string reason) => new Evaluation { valid = false, reason = reason, result = Calculate(current), interventions = Clone(current) };
        WorldResult Calculate(Intervention[] proposals)
        {
            var areas = new Dictionary<string, float>(); var results = new List<SiteResult>(); int spent = 0; float total = 0, baseline = 0;
            foreach (var site in world.sites)
            {
                var edit = proposals.FirstOrDefault(x => x.siteId == site.id); var cover = site.baselineCover; float production = site.baselineProduction;
                if (edit != null)
                {
                    var offer = site.offers.First(x => x.treatmentId == edit.treatmentId); var treatment = scenario.treatments.First(x => x.id == edit.treatmentId);
                    spent += offer.cost; production = offer.production; cover = treatment.cover;
                }
                baseline += site.baselineProduction; total += production;
                foreach (var fraction in cover) { if (!areas.ContainsKey(fraction.coverId)) areas[fraction.coverId] = 0; areas[fraction.coverId] += site.analysisAreaM2 * fraction.fraction; }
                results.Add(new SiteResult { siteId = site.id, production = production, productionDelta = production - site.baselineProduction });
            }
            return new WorldResult { revision = Revision, spentCredits = spent, remainingCredits = scenario.budget - spent, production = total, baselineProduction = baseline, netProduction = total - baseline,
                runoffAvailable = false, coverAreas = areas.OrderBy(x => x.Key).Select(x => new CoverArea { coverId = x.Key, areaM2 = x.Value }).ToArray(), sites = results.ToArray(),
                explanations = new[] { new ExplanationFact { code = "illustrative-production", value = total, unit = "game-units", sourceId = "scenario-assumptions-v1" } } };
        }
    }
}
