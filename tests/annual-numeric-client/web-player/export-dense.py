"""Export a real, validated dense fixture without editing the frozen projection suite."""
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Builds/NumericDenseVectors';OUT.mkdir(exist_ok=True)
source=(ROOT/'tests/annual-report-projection/Dense/Program.cs').read_text()
source=source.replace('foreach(var location in LocationProfile.All)','foreach(var location in LocationProfile.All.Where(item=>item.PackId=="suusamyr-v1"))')
source=source.replace('int bytes=System.Text.Encoding.UTF8.GetByteCount(store.Json);','''File.WriteAllText("Builds/AnnualNumericClientVectors/dense-definition.json",JsonSerializer.Serialize(definition,options));
 File.WriteAllText("Builds/AnnualNumericClientVectors/dense-save.json",store.Json);
 int bytes=System.Text.Encoding.UTF8.GetByteCount(store.Json);''')
(OUT/'Program.cs').write_text(source)
(OUT/'DenseVectors.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup><Compile Include="../../Assets/Terra/Scripts/Core/*.cs"/><Compile Include="../../Assets/Terra/Scripts/Contracts/*.cs"/><Compile Include="../../Assets/Terra/Scripts/Simulation/*.cs"/></ItemGroup></Project>''')
print(OUT/'DenseVectors.csproj')
