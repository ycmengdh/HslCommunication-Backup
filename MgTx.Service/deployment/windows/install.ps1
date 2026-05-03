#!/usr/bin/env pwsh

@"
MgTx Service 部署脚本 - 猛攻通讯
================================

此脚本用于在 Windows 系统上部署 MgTx 服务
需要先安装 .NET 10 Runtime

"@

param(
    [string]$InstallPath = "C:\MgTx",
    [int]$Port = 8080,
    [switch]$AutoStart
)

$ErrorActionPreference = "Stop"

Write-Host "开始部署 MgTx Service..." -ForegroundColor Cyan

if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Host "创建安装目录: $InstallPath" -ForegroundColor Green
}

Write-Host "复制服务文件..." -ForegroundColor Yellow
Copy-Item -Path ".\publish\*" -Destination $InstallPath -Recurse -Force

$envFile = Join-Path $InstallPath ".env"
@"
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:${Port}
DOTNET_RUNNING_IN_CONTAINER=false
"@ | Set-Content -Path $envFile -Encoding UTF8

Write-Host "创建 Windows 服务..." -ForegroundColor Yellow

$serviceName = "MgTxService"
$exePath = Join-Path $InstallPath "MgTx.Service.exe"
$workingDir = $InstallPath

if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "停止现有服务..." -ForegroundColor Yellow
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $serviceName
}

$binaryPath = "`"$env:SystemRoot\system32\svchost.exe`" -k netsvcs -p"
sc.exe create $serviceName binPath= $binaryPath DisplayName= "MgTx 猛攻通讯服务" start= auto

sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/120000/restart/300000

$envContent = @"
SERVICE_NAME=$serviceName
INSTALL_PATH=$InstallPath
PORT=$Port
"@

$envContent | Add-Content -Path $envFile -Encoding UTF8

Write-Host "配置防火墙..." -ForegroundColor Yellow
netsh advfirewall firewall add rule name="$serviceName HTTP" dir=in action=allow protocol=TCP localport=$Port | Out-Null

if ($AutoStart) {
    Write-Host "启动服务..." -ForegroundColor Yellow
    Start-Service -Name $serviceName
}

Write-Host ""
Write-Host "部署完成!" -ForegroundColor Green
Write-Host "服务名称: $serviceName" -ForegroundColor White
Write-Host "安装路径: $InstallPath" -ForegroundColor White
Write-Host "访问地址: http://localhost:$Port/api/v1/health" -ForegroundColor White
Write-Host ""
Write-Host "使用以下命令管理服务:" -ForegroundColor Cyan
Write-Host "  启动: Start-Service -Name $serviceName" -ForegroundColor Gray
Write-Host "  停止: Stop-Service -Name $serviceName" -ForegroundColor Gray
Write-Host "  状态: Get-Service -Name $serviceName" -ForegroundColor Gray
Write-Host "  日志: Get-EventLog -LogName Application -Source $serviceName" -ForegroundColor Gray
