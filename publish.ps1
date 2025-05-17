$projectFilePath = "Syndiesis/Syndiesis.csproj" 
$project = [xml](Get-Content $projectFilePath)

$versionNode = Select-Xml -Xml $project -XPath "//PropertyGroup/Version"
$version = $versionNode.Node.InnerText

if ($version -eq $null)
{
	throw "The project's version could not be determined. Aborting publish."
	exit 80085
}

dotnet publish Syndiesis/ -c Release -r win-x64 -o ./artifacts/${version}/win-x64
dotnet publish Syndiesis/ -c Release -r win-arm64 -o ./artifacts/${version}/win-arm64
dotnet publish Syndiesis/ -c Release -r linux-x64 -o ./artifacts/${version}/linux-x64
dotnet publish Syndiesis/ -c Release -r linux-arm64 -o ./artifacts/${version}/linux-arm64
dotnet publish Syndiesis/ -c Release -r osx-x64 -o ./artifacts/${version}/osx-x64
dotnet publish Syndiesis/ -c Release -r osx-arm64 -o ./artifacts/${version}/osx-arm64
