using System;

namespace Terra.Core
{
    // Inherited community assets visualize baseline capacities, never new-project rewards.
    [Serializable] public sealed class StartingBuilding
    {
        public string siteId, actionId, title;
    }
}
