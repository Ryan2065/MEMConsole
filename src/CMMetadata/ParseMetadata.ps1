$Settings = Import-PowerShellDataFile "$PSScriptRoot\ParseMetadata.psd1"

$CMVersionFolders = Get-ChildItem -Path $PSScriptRoot -Directory

foreach($cmVersionFolder in $CMVersionFolders){
    foreach($file in $settings.SupportedMetadataFiles){
        if(-not ( Test-Path "$($cmVersionFolder.FullName)\$($file)" )){ continue }
        $metadata = [xml]( Get-Content "$($cmVersionFolder.FullName)\$($file)" -Raw )
        $metadata.Edmx
    }
}
