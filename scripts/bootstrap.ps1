# Script de bootstrap para CDK desde PowerShell
# Uso: .\scripts\bootstrap.ps1

Write-Host "Iniciando bootstrap de CDK desde PowerShell..." -ForegroundColor Cyan

# Asegurarse de usar Node.js v18
Write-Host ""
Write-Host "Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "Node.js detectado: $nodeVersion" -ForegroundColor Green
    
    if ($nodeVersion -notmatch 'v18\.') {
        Write-Host "Advertencia: Se recomienda Node.js v18 LTS" -ForegroundColor Yellow
        Write-Host "Si tienes nvm-windows, ejecuta: nvm use 18.20.4" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error: Node.js no esta instalado o no esta en el PATH" -ForegroundColor Red
    exit 1
}

# Configurar variables de entorno
Write-Host ""
Write-Host "Configurando variables de entorno..." -ForegroundColor Yellow
$env:CDK_DEFAULT_ACCOUNT = "696795625614"
$env:CDK_DEFAULT_REGION = "us-east-1"

# Configurar credenciales explícitamente para CDK
Write-Host "Configurando credenciales desde archivo..." -ForegroundColor Yellow
$credPath = "$env:USERPROFILE\.aws\credentials"
$configPath = "$env:USERPROFILE\.aws\config"

if (Test-Path $credPath) {
    $content = Get-Content $credPath -Raw
    if ($content -match 'aws_access_key_id\s*=\s*([^\r\n]+)') {
        $env:AWS_ACCESS_KEY_ID = $matches[1].Trim()
        $keyPartial = $env:AWS_ACCESS_KEY_ID.Substring(0, 8) + "..." + $env:AWS_ACCESS_KEY_ID.Substring($env:AWS_ACCESS_KEY_ID.Length - 4)
        Write-Host "Access Key ID configurado: $keyPartial" -ForegroundColor Green
    }
    if ($content -match 'aws_secret_access_key\s*=\s*([^\r\n]+)') {
        $env:AWS_SECRET_ACCESS_KEY = $matches[1].Trim()
        Write-Host "Secret Access Key configurado" -ForegroundColor Green
    }
    
    # Configurar rutas de archivos de credenciales
    $env:AWS_SHARED_CREDENTIALS_FILE = $credPath
    if (Test-Path $configPath) {
        $env:AWS_CONFIG_FILE = $configPath
    }
    $env:AWS_PROFILE = "default"
    
    # Asegurar que las variables estén en el entorno del proceso
    [Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY_ID", $env:AWS_ACCESS_KEY_ID, "Process")
    [Environment]::SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", $env:AWS_SECRET_ACCESS_KEY, "Process")
    [Environment]::SetEnvironmentVariable("AWS_SHARED_CREDENTIALS_FILE", $credPath, "Process")
    [Environment]::SetEnvironmentVariable("AWS_DEFAULT_REGION", "us-east-1", "Process")
    
    if (-not $env:AWS_ACCESS_KEY_ID -or -not $env:AWS_SECRET_ACCESS_KEY) {
        Write-Host "Error: No se pudieron leer las credenciales del archivo" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Error: No se encontro archivo de credenciales en $credPath" -ForegroundColor Red
    exit 1
}

# Compilar el proyecto
Write-Host ""
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}
Write-Host "Proyecto compilado correctamente" -ForegroundColor Green

# Ejecutar bootstrap
Write-Host ""
Write-Host "Ejecutando bootstrap..." -ForegroundColor Yellow
Write-Host "Account: 696795625614 | Region: us-east-1" -ForegroundColor Gray
Write-Host ""

# Verificar que las variables estén configuradas antes de ejecutar
Write-Host "Verificando variables de entorno..." -ForegroundColor Yellow
Write-Host "AWS_ACCESS_KEY_ID: $($env:AWS_ACCESS_KEY_ID.Substring(0, 8))..." -ForegroundColor Gray
Write-Host "AWS_SECRET_ACCESS_KEY: $($env:AWS_SECRET_ACCESS_KEY.Substring(0, 4))..." -ForegroundColor Gray
Write-Host "AWS_SHARED_CREDENTIALS_FILE: $env:AWS_SHARED_CREDENTIALS_FILE" -ForegroundColor Gray
Write-Host ""

# Crear un script Node.js temporal que configure las credenciales y ejecute CDK
$tempNodeScript = Join-Path $env:TEMP "cdk-bootstrap-$(Get-Random).js"
$nodeScriptContent = @"
// Script para ejecutar CDK bootstrap con credenciales configuradas
process.env.AWS_ACCESS_KEY_ID = '$env:AWS_ACCESS_KEY_ID';
process.env.AWS_SECRET_ACCESS_KEY = '$env:AWS_SECRET_ACCESS_KEY';
process.env.AWS_DEFAULT_REGION = 'us-east-1';
process.env.CDK_DEFAULT_ACCOUNT = '696795625614';
process.env.CDK_DEFAULT_REGION = 'us-east-1';
process.env.AWS_SHARED_CREDENTIALS_FILE = '$env:AWS_SHARED_CREDENTIALS_FILE';
if ('$env:AWS_CONFIG_FILE') {
    process.env.AWS_CONFIG_FILE = '$env:AWS_CONFIG_FILE';
}
process.env.AWS_PROFILE = 'default';

const { execSync } = require('child_process');
try {
    console.log('Ejecutando: npx aws-cdk bootstrap aws://696795625614/us-east-1');
    execSync('npx aws-cdk bootstrap aws://696795625614/us-east-1', {
        stdio: 'inherit',
        env: process.env
    });
    process.exit(0);
} catch (error) {
    process.exit(1);
}
"@

$nodeScriptContent | Out-File -FilePath $tempNodeScript -Encoding UTF8

try {
    node $tempNodeScript
    $exitCode = $LASTEXITCODE
} finally {
    Remove-Item $tempNodeScript -ErrorAction SilentlyContinue
}

if ($exitCode -ne 0) {
    Write-Host ""
    Write-Host "Error al ejecutar bootstrap" -ForegroundColor Red
    Write-Host ""
    Write-Host "Soluciones posibles:" -ForegroundColor Yellow
    Write-Host "1. Verifica que las credenciales sean correctas" -ForegroundColor Gray
    Write-Host "2. Verifica que tengas permisos para crear recursos en AWS" -ForegroundColor Gray
    Write-Host "3. Intenta ejecutar manualmente:" -ForegroundColor Gray
    Write-Host "   npx aws-cdk bootstrap aws://696795625614/us-east-1" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "Bootstrap completado exitosamente!" -ForegroundColor Green

