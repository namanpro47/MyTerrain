# Stylized Nature Pack v1.1

Creator: Elijah Cobden (ElijahCobden).
Source: https://elijahcobden.itch.io/stylized-nature-pack
Downloaded: October 24, 2026, name-your-price free option.
License listed by the creator: Creative Commons Attribution 4.0 International.
License: https://creativecommons.org/licenses/by/4.0/

The source archive contains no separate license text. The creator page is the license source. Preserve this attribution and identify modifications if any of these assets ship in Terra. No endorsement is implied.

Status: evaluated asset family; selected terrain textures now used in the unaccepted Phase 2A study. Imported with supplied materials, shader graphs, textures and demo. No visual acceptance. Demo camera changes are temporary. Original archive and file inventory are retained outside Assets in Builds/ArtResearch/ElijahCobdenNature.

Study modifications: GrassWheat material instancing enabled; source wheat texture uses coverage-preserving mipmaps and trilinear filtering; an identity wrapper preserves source root scale and removes its stored position offset. This wheat candidate failed visual evaluation at the elevated gameplay scale and is superseded by a geometric candidate. Source demo is not a new game level.

Study 13 terrain use: StylisedGrassDiffuse.png and StylisedDirtDiffused.png copied unchanged to Assets/Terra/Resources/ArtistGrassGround.png and ArtistDirtGround.png. Repeating mipmapped import; shader uses luminance to modulate Terra’s ground and bank palette. Authored pixel content is unchanged.

Study 14: WheatPatch geometry is combined into nine-plant clusters with illustrative scale, its unchanged GrassWheat texture, and an adapted instanced matte cutout shader. The prior sparse single-patch trial and the original shader were compared; this dense field experiment is not visual acceptance.

Study15 replaces the nine-copy wheat cluster with three source plants in a narrow row (36 vertices), using the same unchanged artist texture. Row spacing and illustrative access lanes are Terra modifications. Alpha-to-coverage is implemented in TerraCropCards.shader; this remains an unaccepted study.
