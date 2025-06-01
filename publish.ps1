$projectFilePath = "Syndiesis/Syndiesis.csproj" 
$project = [xml](Get-Content $projectFilePath)

$versionNode = Select-Xml -Xml $project -XPath "//PropertyGroup/Version"
$version = $versionNode.Node.InnerText

if ($version -eq $null)
{
	throw "The project's version could not be determined. Aborting publish."
	exit 80085
}

$artifactRoot = ./artifacts/${version}

dotnet publish Syndiesis/ -c Release -r win-x64 -o ${artifactRoot}/win-x64
dotnet publish Syndiesis/ -c Release -r win-arm64 -o ${artifactRoot}/win-arm64
dotnet publish Syndiesis/ -c Release -r linux-x64 -o ${artifactRoot}/linux-x64
dotnet publish Syndiesis/ -c Release -r linux-arm64 -o ${artifactRoot}/linux-arm64
dotnet publish Syndiesis/ -c Release -r osx-x64 -o ${artifactRoot}/osx-x64
dotnet publish Syndiesis/ -c Release -r osx-arm64 -o ${artifactRoot}/osx-arm64

Compress-Archive -Path ${artifactRoot}/win-x64 -DestinationPath ${artifactRoot}/win-x64.zip
Compress-Archive -Path ${artifactRoot}/win-arm64 -DestinationPath ${artifactRoot}/win-arm64.zip
Compress-Archive -Path ${artifactRoot}/linux-x64 -DestinationPath ${artifactRoot}/linux-x64.zip
Compress-Archive -Path ${artifactRoot}/linux-arm64 -DestinationPath ${artifactRoot}/linux-arm64.zip
Compress-Archive -Path ${artifactRoot}/osx-x64 -DestinationPath ${artifactRoot}/osx-x64.zip
Compress-Archive -Path ${artifactRoot}/osx-arm64 -DestinationPath ${artifactRoot}/osx-arm64.zip
