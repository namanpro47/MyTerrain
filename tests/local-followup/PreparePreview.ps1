param([string]$UnityRoot = 'C:/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$preview=Join-Path $PSScriptRoot '.preview'
foreach($dir in @('Assets/Editor','Packages','ProjectSettings')) { New-Item -ItemType Directory -Force (Join-Path $preview $dir) | Out-Null }
foreach($file in @('Assets/Terra/Scripts/UI/LocalFollowup.cs','Assets/Terra/Scripts/UI/HudGlyph.cs','Assets/Terra/Scripts/Core/LocalActionPlan.cs')) { Copy-Item -LiteralPath (Join-Path $taskRoot $file) -Destination (Join-Path $preview 'Assets') -Force }
Copy-Item -LiteralPath (Join-Path $taskRoot 'Assets/TextMesh Pro') -Destination (Join-Path $preview 'Assets') -Recurse -Force
Copy-Item -LiteralPath (Join-Path $taskRoot 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $preview 'ProjectSettings') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'RenderProbe.cs.txt') -Destination (Join-Path $preview 'Assets/Editor/RenderProbe.cs') -Force
'{"dependencies":{"com.unity.ugui":"2.6.0","com.unity.inputsystem":"1.20.0"}}' | Set-Content (Join-Path $preview 'Packages/manifest.json')
'namespace Terra.Audio { public sealed class TerraAudio { static readonly TerraAudio audio=new TerraAudio(); public static TerraAudio Ensure()=>audio; public void Click(){} } } namespace Terra.Gameplay { public sealed class WorldAtlas { public static WorldAtlas Instance=>null; public void Pointer(int kind,UnityEngine.Vector2 position){} } }' | Set-Content (Join-Path $preview 'Assets/AudioStub.cs')
Write-Output $preview
# Run only through terra-work.py with an acquired editor + unity-heavy reservation.
