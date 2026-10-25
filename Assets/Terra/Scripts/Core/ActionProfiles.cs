using System.Linq;

namespace Terra.Core
{
    // The team's source ratings are comparative design intent, never additive meter points.
    public sealed class ActionProfile
    {
        public string id;
        public long upfrontCents, annualReceiptsCents;
        public double economic, social, environmental;
        public ActionProfile(string id,long dollars,long receipts,double economic,double social,double environmental)
        {this.id=id;upfrontCents=dollars*100;annualReceiptsCents=receipts*100;this.economic=economic;this.social=social;this.environmental=environmental;}
    }
    public static class ActionProfiles
    {
        public static readonly ActionProfile[] Source = {
            new ActionProfile("housing-low",1000000,20000,3,2.5,1.5),
            new ActionProfile("farmland-traditional",100000,0,3,3,2),
            new ActionProfile("farmland-regenerative",160000,0,4,4,4.5),
            new ActionProfile("public-park",50000,0,1,4,5),
            new ActionProfile("housing-high",5000000,150000,4,4,2),
            new ActionProfile("warehouse",2000000,0,2.5,2,2),
            new ActionProfile("data-center",3000000,0,4,2,1.5),
            new ActionProfile("downtown-center",4000000,0,4.5,4,2),
            new ActionProfile("power-nonrenewable",5000000,0,4.5,1,1),
            new ActionProfile("power-renewable",3000000,0,5,4.5,5),
            new ActionProfile("stewardship-agreement",150000,0,3,4.5,5),
            new ActionProfile("water-processing",2500000,0,3,5,4.5)
        };
        public static ActionProfile ForAction(string id)
        {
            switch(id)
            {
                case "low-density-housing":id="housing-low";break;
                case "high-density-housing":id="housing-high";break;
                case "cultivate-standard":id="farmland-traditional";break;
                case "conservation-rotation":id="farmland-regenerative";break;
                case "agricultural-storage":id="warehouse";break;
                case "village-center":id="downtown-center";break;
                case "non-renewable-power":id="power-nonrenewable";break;
                case "solar":case "wind":id="power-renewable";break;
                case "wastewater-upgrade":id="water-processing";break;
            }
            return Source.FirstOrDefault(p=>p.id==id);
        }
    }
}
