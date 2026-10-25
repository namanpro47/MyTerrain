Unity-written precision regression supplied by the community import chat. The geographic map is test data only and is not a playable map in this release.

save-roundtrip-definition.json.gz: sha256 a117f5cd2300eca9bd98071dcd3a5f8bbbd233bfb9f715eae462de6a9779933a
save-roundtrip-original.json.gz: sha256 a4a6b6b18216951ad6a533a96b02bb83b95f609060c75802b31d04fa365f623e

Original failure: history[0].outcomes.sites[1].vegetatedAreaM2 parses through JsonUtility from 12899.999713897705 to 12899.999713897703 (one ULP). Expected exact bits: 4668317464170332160. Exact Json.NET read preserves that bit pattern and all committed report hashes.
