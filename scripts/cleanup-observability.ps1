# Script para limpiar el stack ObservabilityStack si está en estado fallido
# Uso: .\scripts\cleanup-observability.ps1

Write-Host "Limpiando stack ObservabilityStack..." -ForegroundColor Yellow

# Configurar variables de entorno
$env:CDK_DEFAULT_ACCOUNT = "696795625614"
$env:CDK_DEFAULT_REGION = "us-east-1"

# Configurar credenciales
$credPath = "$env:USERPROFILE\.aws\credentials"
$content = Get-Content $credPath -Raw
if ($content -match 'aws_access_key_id\s*=\s*([^\r\n]+)') {
    $env:AWS_ACCESS_KEY_ID = $matches[1].Trim()
}
if ($content -match 'aws_secret_access_key\s*=\s*([^\r\n]+)') {
    $env:AWS_SECRET_ACCESS_KEY = $matches[1].Trim()
}

# Intentar destruir el stack
Write-Host "Intentando destruir ObservabilityStack..." -ForegroundColor Cyan
$tempScript = Join-Path $env:TEMP "cdk-destroy-$(Get-Random).js"
$destroyScript = @"
process.env.AWS_ACCESS_KEY_ID = '$env:AWS_ACCESS_KEY_ID';
process.env.AWS_SECRET_ACCESS_KEY = '$env:AWS_SECRET_ACCESS_KEY';
process.env.AWS_DEFAULT_REGION = 'us-east-1';
process.env.CDK_DEFAULT_ACCOUNT = '696795625614';
process.env.CDK_DEFAULT_REGION = 'us-east-1';

const { execSync } = require('child_process');
try {
    execSync('npx aws-cdk destroy ObservabilityStack --force', {
        stdio: 'inherit',
        env: process.env
    });
    process.exit(0);
} catch (error) {
    console.log('Stack no existe o ya fue eliminado');
    process.exit(0);
}
"@

$destroyScript | Out-File -FilePath $tempScript -Encoding UTF8

try {
    node $tempScript
} finally {
    Remove-Item $tempScript -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Limpieza completada. Ahora puedes desplegar nuevamente:" -ForegroundColor Green
Write-Host "  .\scripts\deploy.ps1" -ForegroundColor Cyan

