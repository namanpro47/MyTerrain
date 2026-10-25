"""Prepare a disposable Unity project from the current owned runtime source."""
from pathlib import Path
import shutil,json,hashlib
ROOT=Path(__file__).resolve().parents[3]
TARGET=ROOT/'Builds/NumericWebCheckProject'
TARGET.mkdir(parents=True,exist_ok=True)
files=[]
def copy(source,target):
 target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
 files.append({'source':str(source.relative_to(ROOT)),'sha256':hashlib.sha256(source.read_bytes()).hexdigest()})
for folder in ('Core','Contracts','Simulation'):
 for source in (ROOT/'Assets/Terra/Scripts'/folder).glob('*'):
  if source.suffix in ('.cs','.asmdef') or source.name=='link.xml':copy(source,TARGET/'Assets/Code'/folder/source.name)
for name in ('AnnualTownSession.cs','AnnualTownSession.Numeric.cs','AnnualNumericBrowserStore.cs','AnnualNumericJson.cs'):
 copy(ROOT/'Assets/Terra/Scripts/Gameplay'/name,TARGET/'Assets/Code/Gameplay'/name)
copy(ROOT/'Assets/Editor/AnnualNumericClientChecks.cs',TARGET/'Assets/Editor/AnnualNumericClientChecks.cs')
copy(ROOT/'Assets/Plugins/WebGL/TerraNumericStorage.jslib',TARGET/'Assets/Plugins/WebGL/TerraNumericStorage.jslib')
HERE=Path(__file__).resolve().parent
copy(HERE/'NumericWebCheck.cs',TARGET/'Assets/NumericWebCheck.cs')
copy(HERE/'NumericWebBuild.cs',TARGET/'Assets/Editor/NumericWebBuild.cs')
copy(HERE/'index.html',TARGET/'Assets/WebGLTemplates/NumericCheck/index.html')
copy(ROOT/'Builds/AnnualNumericClientVectors/definition.json',TARGET/'Assets/Resources/definition.json')
copy(ROOT/'ProjectSettings/ProjectVersion.txt',TARGET/'ProjectSettings/ProjectVersion.txt')
(TARGET/'Packages').mkdir(exist_ok=True)
if not (TARGET/'Packages/manifest.json').exists():(TARGET/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.jsonserialize':'1.0.0'}}))
(TARGET/'Assets/Plugins/WebGL/NumericCheckResult.jslib').write_text("""mergeInto(LibraryManager.library, {TerraNumericCheckResult:function(ptr){var text=UTF8ToString(ptr);var result=JSON.parse(text);document.getElementById('result').textContent=(result.passed?'PASS':'FAIL')+'\\n'+JSON.stringify(result,null,2);fetch('/evidence',{method:'POST',headers:{'Content-Type':'application/json'},body:text});}});""")
(TARGET/'source-manifest.json').write_text(json.dumps(files,indent=2)+'\n')
print(TARGET)
