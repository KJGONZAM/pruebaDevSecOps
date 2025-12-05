# Script de despliegue para CDK desde PowerShell
# Uso: .\scripts\deploy.ps1

Write-Host "Iniciando despliegue de stacks CDK..." -ForegroundColor Cyan

# Configurar PATH si es necesario
$nodePath = "$env:USERPROFILE\AppData\Local\nvm\v18.20.4"
if (Test-Path $nodePath) {
    $env:Path = "$nodePath;$env:Path"
    Write-Host "Node.js agregado al PATH" -ForegroundColor Gray
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
    $env:AWS_DEFAULT_REGION = "us-east-1"
    
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

# Verificar Node.js
Write-Host ""
Write-Host "Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "Node.js detectado: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "Error: Node.js no esta instalado o no esta en el PATH" -ForegroundColor Red
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

# Verificar si se necesita bootstrap
Write-Host ""
Write-Host "Verificando si se necesita bootstrap..." -ForegroundColor Yellow
try {
    $bootstrapCheck = aws cloudformation describe-stacks --stack-name CDKToolkit --region us-east-1 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Bootstrap no encontrado. Ejecutando bootstrap..." -ForegroundColor Yellow
        Write-Host "Ejecuta primero: .\scripts\bootstrap.ps1" -ForegroundColor Cyan
        exit 1
    } else {
        Write-Host "Bootstrap ya esta configurado" -ForegroundColor Green
    }
} catch {
    Write-Host "Bootstrap no encontrado. Ejecuta primero: .\scripts\bootstrap.ps1" -ForegroundColor Yellow
    exit 1
}

# Sintetizar (opcional, para verificar)
Write-Host ""
Write-Host "Sintetizando stacks..." -ForegroundColor Yellow
$tempNodeScript = Join-Path $env:TEMP "cdk-synth-$(Get-Random).js"
$nodeScriptContent = @"
process.env.AWS_ACCESS_KEY_ID = '$env:AWS_ACCESS_KEY_ID';
process.env.AWS_SECRET_ACCESS_KEY = '$env:AWS_SECRET_ACCESS_KEY';
process.env.AWS_DEFAULT_REGION = 'us-east-1';
process.env.CDK_DEFAULT_ACCOUNT = '696795625614';
process.env.CDK_DEFAULT_REGION = 'us-east-1';
process.env.AWS_SHARED_CREDENTIALS_FILE = '$env:AWS_SHARED_CREDENTIALS_FILE';
if ('$env:AWS_CONFIG_FILE') {
    process.env.AWS_CONFIG_FILE = '$env:AWS_CONFIG_FILE';
}

const { execSync } = require('child_process');
try {
    execSync('npx aws-cdk synth', {
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
    $synthExitCode = $LASTEXITCODE
} finally {
    Remove-Item $tempNodeScript -ErrorAction SilentlyContinue
}

if ($synthExitCode -ne 0) {
    Write-Host "Error al sintetizar" -ForegroundColor Red
    exit 1
}
Write-Host "Sintesis completada" -ForegroundColor Green

# Desplegar
Write-Host ""
Write-Host "Desplegando todos los stacks..." -ForegroundColor Yellow
Write-Host "Esto puede tardar varios minutos..." -ForegroundColor Gray
Write-Host ""

$tempDeployScript = Join-Path $env:TEMP "cdk-deploy-$(Get-Random).js"
$deployScriptContent = @"
process.env.AWS_ACCESS_KEY_ID = '$env:AWS_ACCESS_KEY_ID';
process.env.AWS_SECRET_ACCESS_KEY = '$env:AWS_SECRET_ACCESS_KEY';
process.env.AWS_DEFAULT_REGION = 'us-east-1';
process.env.CDK_DEFAULT_ACCOUNT = '696795625614';
process.env.CDK_DEFAULT_REGION = 'us-east-1';
process.env.AWS_SHARED_CREDENTIALS_FILE = '$env:AWS_SHARED_CREDENTIALS_FILE';
if ('$env:AWS_CONFIG_FILE') {
    process.env.AWS_CONFIG_FILE = '$env:AWS_CONFIG_FILE';
}

const { execSync } = require('child_process');
try {
    execSync('npx aws-cdk deploy --all --require-approval never', {
        stdio: 'inherit',
        env: process.env
    });
    process.exit(0);
} catch (error) {
    process.exit(1);
}
"@

$deployScriptContent | Out-File -FilePath $tempDeployScript -Encoding UTF8

try {
    node $tempDeployScript
    $deployExitCode = $LASTEXITCODE
} finally {
    Remove-Item $tempDeployScript -ErrorAction SilentlyContinue
}

if ($deployExitCode -ne 0) {
    Write-Host ""
    Write-Host "Error al desplegar" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Despliegue completado exitosamente!" -ForegroundColor Green
Write-Host ""
Write-Host "Para ver los recursos desplegados:" -ForegroundColor Cyan
Write-Host "- VPC: https://console.aws.amazon.com/vpc/" -ForegroundColor Gray
Write-Host "- Pipeline: https://console.aws.amazon.com/codesuite/codepipeline/" -ForegroundColor Gray
Write-Host "- Logs: https://console.aws.amazon.com/cloudwatch/" -ForegroundColor Gray
Write-Host "- Dashboards: https://console.aws.amazon.com/cloudwatch/" -ForegroundColor Gray

