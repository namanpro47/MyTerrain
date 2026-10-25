using Terra.Contracts;

namespace Terra.Core
{
    public sealed partial class AnnualTown
    {
        internal bool HasNumericSimulation => definition.simulation != null;

        // The live engine has already validated its definition and accepted history.
        // A read-only forecast need not rehash that definition or replay all past years.
        // Share immutable rules/reports privately; detach every value the next step writes.
        internal AnnualTown CopyForForecast() => new AnnualTown(this);
        AnnualTown(AnnualTown source)
        {
            var d=source.definition;
            definition=new AnnualTownDefinition {
                schemaVersion=d.schemaVersion,priceYear=d.priceYear,years=d.years,
                id=d.id,version=d.version,worldPackId=d.worldPackId,currency=d.currency,assumptions=d.assumptions,
                startingCashCents=d.startingCashCents,appropriationsCents=d.appropriationsCents,
                sites=d.sites,actions=d.actions,rubric=d.rubric,economy=d.economy,playerRules=d.playerRules,simulation=d.simulation,
                // NeutralPlanningDriver changes the forecast's current driver only.
                drivers=TownCopies.Copy(d.drivers)
            };
            sites=source.sites;actions=source.actions;
            RunId=source.RunId;DefinitionHash=source.DefinitionHash;
            Phase=source.Phase;Year=source.Year;Revision=source.Revision;
            CashCents=source.CashCents;ArrearsCents=source.ArrearsCents;planningYears=source.planningYears;
            queue=TownCopies.Copy(source.queue);assets=TownCopies.Copy(source.assets);
            pending=TownCopies.Copy(source.pending);numericState=TownCopies.Copy(source.numericState);
            history.AddRange(source.history);
        }
    }
}
