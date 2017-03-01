param(
    [string]$packageVersion = $null,
    [string]$configuration = "Release"
)

. ".\common.ps1"

$solutionName = "AssemblyChecker"
$sourceUrl = "https://github.com/neutmute/AssemblyChecker"

function init {
    # Initialization
    $global:rootFolder = Split-Path -parent $script:MyInvocation.MyCommand.Path
    $global:rootFolder = Join-Path $rootFolder .
    $global:packagesFolder = Join-Path $rootFolder packages
    $global:outputFolder = Join-Path $rootFolder _artifacts
    $global:msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

    
    # Read App
    if(!(Test-Path Env:\PackageVersion )){
        $env:PackageVersion = $env:APPVEYOR_BUILD_VERSION
    }
    
    # Default when no env vars
    if(!(Test-Path Env:\PackageVersion )){
        $env:PackageVersion = "1.0.0.0"
    }
    
    
    _WriteOut -ForegroundColor $ColorScheme.Banner "-= $solutionName Build =-"
    _WriteConfig "rootFolder" $rootFolder
    _WriteConfig "version" $env:PackageVersion
}

function restorePackages{
    _WriteOut -ForegroundColor $ColorScheme.Banner "nuget, gitlink restore"
    
    New-Item -Force -ItemType directory -Path $packagesFolder
    _DownloadNuget $packagesFolder
    nuget restore
    nuget install gitlink -SolutionDir "$rootFolder" -ExcludeVersion
}

function nugetPack{
    _WriteOut -ForegroundColor $ColorScheme.Banner "Nuget pack"
    
    New-Item -Force -ItemType directory -Path $outputFolder

    if(!(Test-Path Env:\nuget )){
        $env:nuget = nuget
    }
    if(!(Test-Path Env:\PackageVersion )){
        $env:PackageVersion = "1.0.0.0"
    }
    
    $packableProjects = @("Redback")

   $packableProjects | foreach {
       nuget pack "$rootFolder\src\$_\$_.csproj" -o $outputFolder -IncludeReferencedProjects -p Configuration=$configuration -Version $env:PackageVersion
   }    
}

function nugetPublish{

    if(Test-Path Env:\nugetapikey ){
        _WriteOut -ForegroundColor $ColorScheme.Banner "Nuget publish..."
        &nuget push $outputFolder\* -ApiKey "$env:nugetapikey" -source https://www.nuget.org
    }
    else{
        _WriteOut -ForegroundColor Yellow "nugetapikey environment variable not detected. Skipping nuget publish"
    }
}

function buildSolution{

    _WriteOut -ForegroundColor $ColorScheme.Banner "Build Solution"
    & $msbuild "$rootFolder\$solutionName.sln" /p:Configuration=$configuration /verbosity:minimal

    &"$rootFolder\packages\gitlink\lib\net45\GitLink.exe" $rootFolder -u $sourceUrl
}

function executeTests{

    Write-Host "Execute Tests"

    $testResultformat = ""
    $nunitConsole = "$rootFolder\packages\NUnit.ConsoleRunner.3.6.0\tools\nunit3-console.exe"

    if(Test-Path Env:\APPVEYOR){
        $testResultformat = ";format=AppVeyor"
        $nunitConsole = "nunit3-console"
    }
	    
    & $nunitConsole .\src\Redback.Tests\bin\Release\Redback.Tests.dll `
                --result=$outputFolder\redback.tests.xml$testResultformat

	        
	checkExitCode
}

init

restorePackages

buildSolution

#executeTests

#nugetPack

#nugetPublish

Write-Host "Build $env:PackageVersion complete"