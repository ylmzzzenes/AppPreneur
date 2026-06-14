# Phase A deploy — tek seferlik SSH sifre sorar, sonra sunucuda calistirir.
$ErrorActionPreference = "Stop"

if (-not (Get-Module -ListAvailable -Name Posh-SSH)) {
    Write-Host "Posh-SSH yukleniyor..."
    Install-Module Posh-SSH -Force -Scope CurrentUser -AllowClobber
}

Import-Module Posh-SSH

$hostName = "49.13.89.74"
$port = 22
$user = "root"

Write-Host ""
Write-Host "=== UniFlow Phase A Deploy ===" -ForegroundColor Cyan
Write-Host "Sunucu: ${user}@${hostName}:${port}"
Write-Host ""

$secure = Read-Host "SSH sifresini gir" -AsSecureString
$cred = New-Object System.Management.Automation.PSCredential($user, $secure)

Write-Host "Baglaniliyor..."
$session = New-SSHSession -ComputerName $hostName -Port $port -Credential $cred -AcceptKey -ErrorAction Stop

$cmd = @'
cd /opt/apppreneur && git pull origin main && bash deploy/phase-a/deploy.sh
'@

Write-Host "Deploy basliyor (3-5 dk surebilir)..."
$result = Invoke-SSHCommand -SessionId $session.SessionId -Command $cmd -TimeOut 900

Write-Host $result.Output
if ($result.Error) { Write-Host $result.Error -ForegroundColor Yellow }
Write-Host "Exit: $($result.ExitStatus)"

Remove-SSHSession -SessionId $session.SessionId | Out-Null

Write-Host ""
Write-Host "Dis test:" -ForegroundColor Cyan
curl.exe -s --connect-timeout 5 "http://${hostName}/health"
Write-Host ""
curl.exe -s -o NUL -w "web:%{http_code}`n" --connect-timeout 5 "http://${hostName}:3000/"

Write-Host ""
Read-Host "Kapatmak icin Enter"
