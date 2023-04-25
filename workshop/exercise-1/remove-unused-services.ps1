# Connect using a Managed Service Identity
try {
    (Connect-AzAccount -Identity).context
}
catch{
    Write-Output "There is no system-assigned user identity. Aborting."; 
    exit
}

# Get all resources with tag ExpirationTime
$query = 'resources | project id, name, tags | where tags contains "ExpirationTime"'

# HH 24 hour format.
$utcNow = Get-Date -AsUTC -Format "HH:mm"
$currentTime = [TimeSpan]::ParseExact($utcNow, "hh\:mm", $null)

do
{
    $resourcesToEliminate= Search-AzGraph -Query $query
    foreach ($resource in $resourcesToEliminate) {
        $resourceExpirationTime = [TimeSpan]::ParseExact($resource.tags.ExpirationTime, "hh\:mm", $null)
        if ($currentTime -ge $resourceExpirationTime) {
            Write-Output "$($resource.name) : $($resource.id)"
            Remove-AzResource -ResourceId $resource.id -Force
        }
    }
}
while ($resourcesToEliminate.count -gt 0)