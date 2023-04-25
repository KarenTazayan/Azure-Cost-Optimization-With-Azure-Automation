# Connect using a Managed Service Identity
try {
    (Connect-AzAccount -Identity).context
}
catch{
    Write-Output "There is no system-assigned managed identity."; 
    exit
}

# Input parameters
$resourceGroupName = "rg-shoppingapp2-t1"
$appServicePlanName = "plan-shoppingapp2-ui-t1"
$scaleUpTime = [TimeSpan]::ParseExact("08:00", "hh\:mm", $null)

# HH 24 hour format.
$utcNow = Get-Date -AsUTC -Format "HH:mm"
$currentTime = [TimeSpan]::ParseExact($utcNow, "hh\:mm", $null)
Write-Output "Current UTC Time: $($currentTime)"

# Default size of the App Service Plan
$tier = 'PremiumV3'
$workerSize = 'Small' # Small, Medium, Large, etc.
$numberOfWorkers = 1 # The number of running instances

# Scale up the App Service Plan
if($currentTime -ge $scaleUpTime)
{
    $workerSize = 'Medium'
    $numberOfWorkers = 2
}

# Apply the changes
Set-AzAppServicePlan -ResourceGroupName $resourceGroupName -Name $appServicePlanName `
    -Tier $tier -WorkerSize $workerSize -NumberofWorkers $numberOfWorkers