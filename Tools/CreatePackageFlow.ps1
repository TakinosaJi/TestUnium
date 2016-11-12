param (
	[parameter(Mandatory=$false, HelpMessage="Specify path to target solution file.")]
	[string] $SolutionFilePath,
	[parameter(Mandatory=$true, HelpMessage="Specify path to target project file.")]
	[ValidateNotNullOrEmpty()]
	[string] $ProjectFilePath,
	[parameter(Mandatory=$true, HelpMessage="Specify destination path for nuspec file.")]
	[ValidateNotNullOrEmpty()]
	[string] $NuspecDestinationPath,
	[parameter(Mandatory=$true, HelpMessage="Specify source path for nuspec file.")]
	[ValidateNotNullOrEmpty()]
	[string] $NuspecSourcePath,
	[parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
	[string] $BumpVersionFilePath = "..\.bumpversion",
	[parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
	[string] $VersionFilePath = "..\version.txt",
    [switch] $RebuildSolution
)
$env:PsModulePath += ";$PsScriptRoot\Modules"

Import-Module Invoke-MsBuild -Verbose
Import-Module Transform-Nuspec -Verbose
Import-Module Bump-Version -Verbose

try
{
    $Version = Get-Content -Path $VersionFilePath
    if(!($Version -match "^(\d+\.)?(\d+\.)?(\*|\d+)$"))
    {
        throw [System.Exception] "Invalid version number. Check your version.txt file." 
    }

    $BuildConfigurations = @("Release 4.6.1", "Release 4.6", "Release 4.5.2", "Release 4.5.1", "Release 4.5") 
    if($RebuildSolution)
    {   
        foreach ($conf in $BuildConfigurations)
        {
            Write-Host "Rebuilding project in $conf configuration."
            Build-Solution -SolutionFilePath $SolutionFilePath -BuildConfiguration $conf
        }
    }

    Transform-Nuspec -Version $Version -SourcePath $NuspecSourcePath -DestinationPath $NuspecDestinationPath
    if(Test-Path "Package"){
        Get-ChildItem -Path "Package" -Force -Recurse |
          Sort-Object -Property FullName -Descending |
            Remove-Item -Recurse -Force
    }
    New-Item -Path "Package" -Type directory
    
    & nuget pack $ProjectFilePath -Symbols -Verbose -outputdirectory Package -Prop Configuration=`"$($BuildConfigurations[0])`"

    Bump-Version -FilePath $BumpVersionFilePath -Version $Version
}
catch [Exception]
{
    Write-Host $_.Exception.Message
    throw $_
}


