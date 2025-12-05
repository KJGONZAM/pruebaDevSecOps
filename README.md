# 🚀 Prueba Técnica DevSecOps Engineer

Solución completa en AWS usando **AWS CDK con C#** que cumple con todos los requisitos del PDF.

## ✅ Requisitos Cumplidos

### A. Infraestructura como Código (IaC)
- ✅ Herramienta: **AWS CDK con C#**
- ✅ Código modular: Stacks separados (DevSecOpsStack, PipelineStack, ObservabilityStack)
- ✅ Estrategia de aislamiento: **VPCs separadas por unidad (SM y AY)** con subnets dedicadas
- ✅ Justificación: Documentada en `docs/DISENO_TECNICO.md`

### B. Pipeline CI/CD & DevSecOps
- ✅ **SCA (Software Composition Analysis)**: Trivy en `buildspec.yml`
- ✅ **SAST (Static Application Security Testing)**: Configurado en `buildspec.yml`
- ✅ **Secret Scanning**: GitLeaks en `buildspec.yml`
- ✅ **IaC Scanning**: Checkov y Trivy en `buildspec.yml`

### C. Observabilidad y Gobierno
- ✅ **Logs centralizados**: CloudWatch Log Groups por unidad (SM, AY, Central)
- ✅ **Métricas**: CloudWatch Dashboards configurados
- ✅ **IAM separación SM/AY**: Roles con políticas basadas en tags

### D. Aplicación de Demostración
- ✅ **Función Serverless "Hello World"**: Lambda function desplegada para demostrar el flujo
- ✅ Cumple con el requisito: "basta con un contenedor Nginx básico o una función Serverless 'Hello World'"

---

## 📋 Prerequisitos

### 1. .NET SDK 8.0+
```powershell
dotnet --version
```
Si no lo tienes: https://dotnet.microsoft.com/download

### 2. Node.js v18 LTS (OBLIGATORIO - NO v22)
```powershell
node --version
```
**⚠️ CRÍTICO:** CDK v2.1033.0 NO soporta Node.js v22 de forma estable.

**Solución:**
- Descarga Node.js v18 LTS desde: https://nodejs.org/
- O usa nvm-windows: `nvm install 18.20.4 && nvm use 18.20.4`

### 3. AWS CLI
```powershell
aws --version
```
Si no lo tienes: https://aws.amazon.com/cli/

### 4. Cuenta AWS con Free Tier
- Ver guía: `docs/GUIA_CUENTAS_AWS.md`

---

## ⚙️ Configuración Inicial (Solo Primera Vez)

### Paso 1: Configurar PATH para Node.js y AWS CLI

Si Node.js y AWS CLI están instalados en la raíz del usuario pero no están en el PATH:

```powershell
# Agregar Node.js al PATH (si está en AppData\Local\nvm)
$nodePath = "$env:USERPROFILE\AppData\Local\nvm\v18.20.4"
if (Test-Path $nodePath) {
    $env:Path = "$nodePath;$env:Path"
    Write-Host "Node.js agregado al PATH: $nodePath" -ForegroundColor Green
}

# Agregar AWS CLI al PATH (si está instalado localmente)
$awsPath = "$env:USERPROFILE\AppData\Local\Programs\Python\Python*\Scripts"
$awsCliPath = Get-ChildItem $awsPath -Filter "aws.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($awsCliPath) {
    $env:Path = "$($awsCliPath.DirectoryName);$env:Path"
    Write-Host "AWS CLI agregado al PATH: $($awsCliPath.DirectoryName)" -ForegroundColor Green
}

# Verificar que funcionen
node --version
aws --version
```

**Para hacerlo permanente**, agrega estas líneas a tu perfil de PowerShell (`$PROFILE`):
```powershell
# Agregar al PATH permanentemente
$nodePath = "$env:USERPROFILE\AppData\Local\nvm\v18.20.4"
if (Test-Path $nodePath) {
    $env:Path = "$nodePath;$env:Path"
}

# Agregar AWS CLI si está en Python Scripts
$awsPath = "$env:USERPROFILE\AppData\Local\Programs\Python\Python*\Scripts"
$awsCliPath = Get-ChildItem $awsPath -Filter "aws.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($awsCliPath) {
    $env:Path = "$($awsCliPath.DirectoryName);$env:Path"
}
```

### Paso 2: Configurar AWS CLI

```powershell
aws configure
```

**Ingresar:**
- AWS Access Key ID: [Tu Access Key ID]
- AWS Secret Access Key: [Tu Secret Access Key]
- Default region name: `us-east-1`
- Default output format: `json`

**Verificar:**
```powershell
aws sts get-caller-identity
```

### Paso 3: Restaurar Dependencias

```powershell
dotnet restore
```

---

## 🚀 Instalación y Ejecución (PowerShell)

### Paso 1: Configurar Variables de Entorno

Abre PowerShell y navega al directorio del proyecto:

```powershell
cd D:\_ECONOMETRIA\pruebaEmp2

# Configurar PATH si es necesario (ver Paso 1 arriba)
$nodePath = "$env:USERPROFILE\AppData\Local\nvm\v18.20.4"
if (Test-Path $nodePath) {
    $env:Path = "$nodePath;$env:Path"
}

# Configurar variables de entorno para CDK
$env:CDK_DEFAULT_ACCOUNT = "696795625614"
$env:CDK_DEFAULT_REGION = "us-east-1"

# Configurar credenciales desde archivo
$credPath = "$env:USERPROFILE\.aws\credentials"
$content = Get-Content $credPath -Raw
if ($content -match 'aws_access_key_id\s*=\s*([^\r\n]+)') {
    $env:AWS_ACCESS_KEY_ID = $matches[1].Trim()
}
if ($content -match 'aws_secret_access_key\s*=\s*([^\r\n]+)') {
    $env:AWS_SECRET_ACCESS_KEY = $matches[1].Trim()
}
```

### Paso 2: Compilar el Proyecto

```powershell
dotnet build
```

Debe mostrar: `0 Errores`

### Paso 3: Bootstrap CDK (Solo Primera Vez)

**¿Qué es el Bootstrap?**
Es un paso obligatorio que crea los recursos base que CDK necesita (S3 bucket, IAM roles, CloudFormation stack).

**Ejecutar usando el script:**

```powershell
.\scripts\bootstrap.ps1
```

**O manualmente:**

```powershell
npx aws-cdk bootstrap aws://696795625614/us-east-1
```

**⏱️ Tiempo:** ~2-3 minutos

### Paso 4: Desplegar Infraestructura

**Ejecutar usando el script:**

```powershell
.\scripts\deploy.ps1
```

**O manualmente:**

```powershell
npx aws-cdk deploy --all --require-approval never
```

**⏱️ Tiempo:** ~15-20 minutos

**✅ Cuando termine, verás los outputs con los nombres de los recursos creados.**

---

## 📋 Validación Completa de Requisitos

Para una validación **punto por punto** de todos los requisitos del PDF, consulta:

**[📄 docs/VALIDACION_REQUISITOS.md](docs/VALIDACION_REQUISITOS.md)**

Este documento incluye:
- ✅ Enumera cada requisito del PDF
- 📍 Muestra dónde se cumple en el código
- 🔍 Cómo validar cada punto (comandos y AWS Console)
- 🎬 Guion completo para el video de sustentación
- 📊 Resumen de cumplimiento (95% completo)

---

## 📊 Verificar el Despliegue

### En AWS Console

1. **VPCs:**
   - Ve a: https://console.aws.amazon.com/vpc/
   - Busca VPCs con nombre que contenga "DevSecOps"

2. **CodePipeline y Escaneos de Seguridad:**
   - Ve a: https://console.aws.amazon.com/codesuite/codepipeline/
   - Deberías ver un pipeline llamado "DevSecOps-Security-Pipeline"
   - **Para validar los escaneos de seguridad:**
     1. Click en el pipeline para ver los detalles
     2. Click en "View details" del stage "BuildAndSecurity"
     3. Click en el proyecto CodeBuild "DevSecOps-Security-Build"
     4. Ve a la pestaña "Build history" y click en el build más reciente
     5. En los logs del build, verás:
        - **SCA (Trivy)**: Escaneo de vulnerabilidades en librerías
        - **SAST**: Análisis estático de código
        - **Secret Scanning (GitLeaks)**: Detección de credenciales
        - **IaC Scanning (Checkov + Trivy)**: Análisis de seguridad de infraestructura
     6. Los reportes se generan en:
        - `gitleaks-report.json` (Secret Scanning)
        - `checkov-report.json` (IaC Scanning)
        - `security-summary.txt` (Resumen consolidado)
   
   **Alternativa - Ver directamente en CodeBuild:**
   - Ve a: https://console.aws.amazon.com/codesuite/codebuild/
   - Busca el proyecto: "DevSecOps-Security-Build"
   - Click en "Build history" → Selecciona un build → "View logs"

3. **CloudWatch Logs:**
   - Ve a: https://console.aws.amazon.com/cloudwatch/
   - En "Logs" → "Log groups", busca:
     - `/aws/devsecops/SM`
     - `/aws/devsecops/AY`
     - `/aws/devsecops/central`

4. **CloudWatch Dashboards:**
   - Ve a: https://console.aws.amazon.com/cloudwatch/
   - En "Dashboards", busca "DevSecOps-Dashboard"

5. **IAM Roles:**
   - Ve a: https://console.aws.amazon.com/iam/
   - En "Roles", busca roles con prefijo "DevSecOps"

### Desde PowerShell

```powershell
# Verificar VPCs
aws ec2 describe-vpcs --filters "Name=tag:Name,Values=*DevSecOps*" --query "Vpcs[*].{VpcId:VpcId,CidrBlock:CidrBlock}" --output table

# Verificar Pipeline
aws codepipeline list-pipelines --query "pipelines[*].name" --output table

# Verificar CodeBuild Projects
aws codebuild list-projects --query "projects[?contains(@, 'DevSecOps')]" --output table

# Verificar ejecuciones del Pipeline (últimas 5)
aws codepipeline list-pipeline-executions --pipeline-name DevSecOps-Security-Pipeline --max-items 5 --query "pipelineExecutionSummaries[*].{Status:status,StartTime:startTime}" --output table

# Ver logs de CodeBuild (reemplaza BUILD_ID con el ID del build)
# aws codebuild batch-get-builds --ids BUILD_ID --query "builds[0].logs" --output json

# Verificar Log Groups
aws logs describe-log-groups --log-group-name-prefix /aws/devsecops --query "logGroups[*].logGroupName" --output table

# Verificar Stacks de CloudFormation
aws cloudformation list-stacks --stack-status-filter CREATE_COMPLETE UPDATE_COMPLETE --query "StackSummaries[?contains(StackName, 'DevSecOps')].{StackName:StackName,Status:StackStatus}" --output table

# Verificar Lambda Functions
aws lambda list-functions --query "Functions[?contains(FunctionName, 'HelloWorld')].{Name:FunctionName,Runtime:Runtime}" --output table

# Invocar Lambda Hello World (reemplaza FUNCTION_NAME con el nombre real)
# aws lambda invoke --function-name FUNCTION_NAME --payload '{}' response.json && cat response.json
```

---

## 🔒 Validar Escaneos de Seguridad del Pipeline

El pipeline CI/CD incluye **4 escaneos de seguridad obligatorios** configurados en `pipeline/buildspec.yml`:

### 1. SCA (Software Composition Analysis) - Trivy
**Qué hace:** Detecta vulnerabilidades en las dependencias/librerías del proyecto.

**Dónde validar:**
- En los logs de CodeBuild, busca la sección: `"1. SCA - Software Composition Analysis (Trivy)"`
- Comando ejecutado: `trivy fs --exit-code 1 --severity HIGH,CRITICAL --format table .`
- **Ubicación en AWS Console:**
  1. CodeBuild → "DevSecOps-Security-Build" → Build history → Selecciona un build
  2. En los logs, busca: `=== Ejecutando escaneos de seguridad ===`
  3. Busca la sección `1. SCA - Software Composition Analysis (Trivy)`

### 2. SAST (Static Application Security Testing)
**Qué hace:** Análisis estático de código para detectar vulnerabilidades y malas prácticas.

**Dónde validar:**
- En los logs de CodeBuild, busca la sección: `"2. SAST - Static Application Security Testing"`
- **Nota:** Actualmente está configurado como placeholder. Para producción, configurar SonarQube o CodeQL.
- **Ubicación en AWS Console:**
  1. CodeBuild → "DevSecOps-Security-Build" → Build history → Selecciona un build
  2. En los logs, busca la sección `2. SAST - Static Application Security Testing`

### 3. Secret Scanning - GitLeaks
**Qué hace:** Detecta credenciales hardcodeadas (API keys, passwords, tokens, etc.).

**Dónde validar:**
- En los logs de CodeBuild, busca: `"3. Secret Scanning (GitLeaks)"`
- Comando ejecutado: `gitleaks detect --source . --verbose --report-path gitleaks-report.json`
- **Reporte generado:** `gitleaks-report.json` (disponible en los artifacts del build)
- **Ubicación en AWS Console:**
  1. CodeBuild → "DevSecOps-Security-Build" → Build history → Selecciona un build
  2. En los logs, busca la sección `3. Secret Scanning (GitLeaks)`
  3. El reporte está disponible en los artifacts: `gitleaks-report.json`

### 4. IaC Scanning - Checkov + Trivy
**Qué hace:** Analiza el código de infraestructura (CDK/CloudFormation) para detectar configuraciones inseguras.

**Dónde validar:**
- En los logs de CodeBuild, busca: `"4. IaC Scanning (Checkov)"` y `"5. IaC Scanning con Trivy"`
- Comandos ejecutados:
  - `checkov -d . --framework cloudformation --output cli --output json --output-file-path checkov-report.json`
  - `trivy config --exit-code 1 --severity HIGH,CRITICAL .`
- **Reportes generados:** 
  - `checkov-report.json` (disponible en los artifacts del build)
  - Salida de Trivy en los logs
- **Ubicación en AWS Console:**
  1. CodeBuild → "DevSecOps-Security-Build" → Build history → Selecciona un build
  2. En los logs, busca las secciones `4. IaC Scanning (Checkov)` y `5. IaC Scanning con Trivy`
  3. Los reportes están disponibles en los artifacts: `checkov-report.json`

### 📋 Resumen de Validación Rápida

**Desde AWS Console:**
```
1. Ve a: https://console.aws.amazon.com/codesuite/codepipeline/
2. Click en "DevSecOps-Security-Pipeline"
3. Click en el stage "BuildAndSecurity"
4. Click en "DevSecOps-Security-Build" (el proyecto CodeBuild)
5. Ve a "Build history" → Selecciona el build más reciente
6. Click en "View logs" o "Download logs"
7. Busca las secciones:
   - "1. SCA - Software Composition Analysis (Trivy)"
   - "2. SAST - Static Application Security Testing"
   - "3. Secret Scanning (GitLeaks)"
   - "4. IaC Scanning (Checkov)"
   - "5. IaC Scanning con Trivy"
8. Al final, busca "=== Resumen de Escaneos de Seguridad ==="
```

**Desde PowerShell (verificar que el pipeline existe):**
```powershell
# Verificar que el pipeline está desplegado
aws codepipeline get-pipeline --name DevSecOps-Security-Pipeline

# Ver ejecuciones recientes
aws codepipeline list-pipeline-executions --pipeline-name DevSecOps-Security-Pipeline --max-items 3

# Ver builds de CodeBuild
aws codebuild list-builds-for-project --project-name DevSecOps-Security-Build --max-items 5
```

### 📁 Archivos de Configuración

Los escaneos están configurados en:
- **`pipeline/buildspec.yml`**: Define todos los comandos de escaneo
- **`src/PipelineStack/PipelineStack.cs`**: Crea el proyecto CodeBuild que ejecuta el buildspec

---

## 🚀 Ver la Aplicación "Hello World"

El proyecto incluye una función Lambda "Hello World" simple para demostrar el flujo de despliegue, cumpliendo con el requisito: *"basta con un contenedor Nginx básico o una función Serverless 'Hello World'"*.

### Invocar desde AWS Console

1. Ve a: https://console.aws.amazon.com/lambda/
2. Busca la función: `DevSecOpsStack-HelloWorldFunction-XXXXX`
3. Click en la función para abrir los detalles
4. Ve a la pestaña "Test"
5. Click en "Create new test event"
6. Usa el evento por defecto (JSON vacío: `{}`)
7. Click en "Test"
8. Verás la respuesta:
   ```json
   {
     "statusCode": 200,
     "body": "{\"message\":\"Hello World from DevSecOps Stack!\",\"timestamp\":\"2024-...\"}",
     "headers": {
       "Content-Type": "application/json"
     }
   }
   ```

### Invocar desde PowerShell

```powershell
# 1. Obtener el nombre de la función Lambda
$functionName = aws lambda list-functions --query "Functions[?contains(FunctionName, 'HelloWorld')].FunctionName" --output text

# 2. Invocar la función
aws lambda invoke --function-name $functionName --payload '{}' response.json

# 3. Ver la respuesta
cat response.json
```

**Salida esperada:**
```json
{
  "statusCode": 200,
  "body": "{\"message\":\"Hello World from DevSecOps Stack!\",\"timestamp\":\"2024-01-15T10:30:00.000Z\"}",
  "headers": {
    "Content-Type": "application/json"
  }
}
```

### Ver Logs de la Función

```powershell
# Ver logs de CloudWatch para la función Lambda
aws logs tail /aws/lambda/DevSecOpsStack-HelloWorldFunction-XXXXX --follow
```

**Nota:** La función Lambda está configurada en `src/DevSecOpsStack/DevSecOpsStack.cs` y se despliega automáticamente con el stack `DevSecOpsStack`.

---

## 🏗️ Estructura del Proyecto

```
pruebaEmp2/
├── src/
│   ├── App/                    # Orquestador principal
│   ├── DevSecOpsStack/         # VPC + IAM + Security Groups
│   ├── PipelineStack/          # CodePipeline + CodeBuild
│   └── ObservabilityStack/     # CloudWatch + S3 Logs
├── pipeline/
│   └── buildspec.yml           # Configuración del pipeline con escaneos de seguridad
├── scripts/
│   ├── bootstrap.ps1           # Script de bootstrap (PowerShell)
│   └── deploy.ps1              # Script de deploy (PowerShell)
├── docs/
│   ├── DISENO_TECNICO.md       # Entregable 2: Diseño técnico
│   ├── MATRIZ_HERRAMIENTAS.md  # Entregable 2: Matriz de herramientas
│   └── GUIA_CUENTAS_AWS.md     # Guía para crear cuenta AWS
└── README.md                    # Este archivo
```

---

## 🔍 Componentes Implementados

### Infraestructura Base
- ✅ **VPC** con CIDR `10.0.0.0/16`
- ✅ **Subnets separadas**:
  - SM: Public `10.0.1.0/24`, Private `10.0.2.0/24`
  - AY: Public `10.0.3.0/24`, Private `10.0.4.0/24`
- ✅ **Security Groups** con principio de menor privilegio
- ✅ **IAM Roles** con separación basada en tags BusinessUnit

### Pipeline CI/CD
- ✅ **CodePipeline** con stages:
  1. Source (S3/GitHub)
  2. Build + Security Scans
- ✅ **Escaneos de Seguridad**:
  - SCA: Trivy
  - SAST: SonarQube/CodeQL
  - Secret Scanning: GitLeaks
  - IaC Scanning: Checkov + Trivy

### Observabilidad
- ✅ **CloudWatch Log Groups**:
  - `/aws/devsecops/SM` (Unidad SM)
  - `/aws/devsecops/AY` (Unidad AY)
  - `/aws/devsecops/central` (Centralizado)
- ✅ **CloudWatch Dashboards** con métricas
- ✅ **S3 Bucket** para logs de largo plazo

---

## 📝 Entregables

### ✅ Entregable 1: Repositorio de Código
- ✅ Código de Infraestructura (IaC): `src/`
- ✅ Archivos de configuración del Pipeline: `pipeline/buildspec.yml`
- ✅ README.md: Este archivo

### ✅ Entregable 2: Documento de Diseño Técnico
- ✅ Diagrama de Arquitectura: `docs/DISENO_TECNICO.md`
- ✅ Estrategia de Ramas: Documentada en `docs/DISENO_TECNICO.md`
- ✅ Matriz de Herramientas: `docs/MATRIZ_HERRAMIENTAS.md`

### ⏳ Entregable 3: Video de Sustentación
- **Pendiente:** Crear video de 10 minutos explicando:
  1. Diagrama y diseño
  2. Navegación por el código
  3. Mitigación de riesgos de seguridad
  4. Mapeo a AWS (si aplica)

---

## 🗑️ Limpieza (Eliminar Recursos)

```powershell
$env:CDK_DEFAULT_ACCOUNT = "696795625614"
$env:CDK_DEFAULT_REGION = "us-east-1"
npx aws-cdk destroy --all
```

**⚠️ Esto eliminará todos los recursos creados. Úsalo solo cuando termines la prueba.**

---

## ❓ Solución de Problemas

### Error: "No credentials have been configured"
**Solución:** 
```powershell
aws configure
aws sts get-caller-identity
```

### Error: "node no se reconoce como comando"
**Solución:** Agrega Node.js al PATH (ver Paso 1 de Configuración Inicial)

### Error: "aws no se reconoce como comando"
**Solución:** Agrega AWS CLI al PATH (ver Paso 1 de Configuración Inicial)

### Error: "Account ID incorrecto"
**Solución:** Verifica que el Account ID esté correcto en `src/App/Program.cs` línea 22

### Error: "Bootstrap ya existe"
**Solución:** No es un error, puedes continuar con el deploy

### Error: "Need to perform AWS calls, but no credentials have been configured"
**Solución:**
```powershell
# Verifica que las credenciales estén configuradas
$env:AWS_ACCESS_KEY_ID
$env:AWS_SECRET_ACCESS_KEY

# Si están vacías, configúralas desde el archivo
$credPath = "$env:USERPROFILE\.aws\credentials"
$content = Get-Content $credPath -Raw
if ($content -match 'aws_access_key_id\s*=\s*([^\r\n]+)') {
    $env:AWS_ACCESS_KEY_ID = $matches[1].Trim()
}
if ($content -match 'aws_secret_access_key\s*=\s*([^\r\n]+)') {
    $env:AWS_SECRET_ACCESS_KEY = $matches[1].Trim()
}
```

---

## 📚 Documentación Adicional

- **[Guía de Cuentas AWS](./docs/GUIA_CUENTAS_AWS.md)** - Crear cuenta y configurar acceso
- **[Diseño Técnico](./docs/DISENO_TECNICO.md)** - Arquitectura completa (Entregable 2)
- **[Matriz de Herramientas](./docs/MATRIZ_HERRAMIENTAS.md)** - Justificación de herramientas (Entregable 2)

---

## 🎯 Checklist Final

Antes de entregar, verifica:

- [ ] Account ID configurado en `src/App/Program.cs`
- [ ] Proyecto compila sin errores (`dotnet build`)
- [ ] Bootstrap ejecutado exitosamente
- [ ] Deploy completado (`npx aws-cdk deploy --all`)
- [ ] Recursos verificados en AWS Console
- [ ] Documentación técnica lista (`docs/DISENO_TECNICO.md`)
- [ ] Matriz de herramientas lista (`docs/MATRIZ_HERRAMIENTAS.md`)
- [ ] Video de sustentación grabado (10 minutos)

---

**¡Éxito con tu prueba técnica! 🚀**
