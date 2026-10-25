using Terra.Contracts;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        // A new scenario changes rules/seed without creating another HUD hierarchy.
        public void SetAnnualRules(AnnualTownDefinition definition){annualRules=definition;}
    }
}
