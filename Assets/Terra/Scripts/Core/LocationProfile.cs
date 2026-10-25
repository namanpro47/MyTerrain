namespace Terra.Core
{
    // The atlas is a bounded authored catalog, not a geocoder or live world generator.
    public sealed class LocationProfile
    {
        public readonly string PackId, Name, Region;
        public bool IsWisconsin => PackId == "meadowbrook-v1";
        public string Subtitle => Region + (IsWisconsin ? " · real geography" : " · real terrain / authored cover");
        public string AnnualSaveKey => IsWisconsin ? "terra.meadowbrook.annual-full.v3.1" : "terra." + PackId + ".annual-full.v3.1";
        public string ChapterSaveKey => IsWisconsin ? "terra.meadowbrook.annual-full.chapters.v3.1" : "terra." + PackId + ".annual-full.chapters.v3.1";
        LocationProfile(string id,string name,string region){PackId=id;Name=name;Region=region;}
        public static readonly LocationProfile[] All = {
            new LocationProfile("meadowbrook-v1","Meadowbrook Valley","Wisconsin"),
            new LocationProfile("point-reyes-v1","Point Reyes","California"),
            new LocationProfile("suusamyr-v1","Suusamyr Valley","Kyrgyzstan")
        };
        public static LocationProfile At(int index) => All[index >= 0 && index < All.Length ? index : 0];
        public static LocationProfile Find(string id){foreach(var place in All)if(place.PackId==id)return place;return All[0];}
    }
}
