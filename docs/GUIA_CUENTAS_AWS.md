# Guía de Configuración de Cuentas AWS - Free Tier

## 📋 Introducción

Esta guía te ayudará a configurar una cuenta AWS gratuita para realizar la prueba técnica de DevSecOps. AWS ofrece un **Free Tier** que incluye muchos servicios sin costo por 12 meses.

## 🆓 Servicios Incluidos en Free Tier (12 meses)

- **VPC, IAM**: Gratis (sin límite)
- **S3**: 5 GB almacenamiento
- **CloudWatch**: 5 GB logs, 10 métricas
- **CodeBuild**: 100 minutos/mes
- **CodePipeline**: Gratis primeros 30 días
- **EC2**: 750 horas/mes (t2.micro)

### ⚠️ Nota Importante

- **NAT Gateway**: NO está en Free Tier (~$0.045/hora)
- Esta solución usa subnets ISOLATED (sin NAT) para evitar costos

## 🚀 Pasos Esenciales (10 minutos)

### Paso 1: Crear Cuenta AWS (5 min)

1. Visita: **https://aws.amazon.com/free/**
2. Haz clic en **"Create a Free Account"**
3. Completa el registro:
   - Email, contraseña, nombre de cuenta
   - Información de contacto
4. **Agrega tarjeta** (solo verificación, no se cobra si usas Free Tier)
5. **Verifica email** y teléfono

### Paso 2: Crear Usuario IAM (3 min)

1. **AWS Console** → **IAM** → **Users** → **Add users**
2. **Nombre**: `devsecops-user`
3. **Tipo de acceso**: ✅ **Programmatic access**
4. **Permisos**: Adjuntar `AdministratorAccess` (solo para pruebas)
5. **Crear usuario**
6. **⚠️ GUARDAR credenciales** (solo se muestran una vez):
   - Access Key ID: `AKIA...`
   - Secret Access Key: `wJalr...`

### Paso 3: Configurar AWS CLI (2 min)

```bash
# Agregar AWS CLI al PATH (si no está)
export PATH="$PATH:/c/Program Files/Amazon/AWSCLIV2"

# Configurar
aws configure
# Ingresar: Access Key ID, Secret Access Key, Region (us-east-1), Output (json)

# Verificar
aws sts get-caller-identity
```

**✅ Listo para desplegar**

## 📖 Información Adicional (Opcional)

### Política Personalizada (Mejores Prácticas)
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ec2:*",
        "s3:*",
        "iam:*",
        "logs:*",
        "codebuild:*",
        "codepipeline:*",
        "cloudformation:*",
        "sts:GetCallerIdentity"
      ],
      "Resource": "*"
    }
  ]
}
```

### Paso 4: Guardar Credenciales

⚠️ **CRÍTICO**: Las credenciales solo se muestran una vez. Guárdalas de forma segura.

1. **Access Key ID**: Ej: `AKIAIOSFODNN7EXAMPLE`
2. **Secret Access Key**: Ej: `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`

**Guarda estas credenciales en**:
- Un gestor de contraseñas (1Password, LastPass, etc.)
- Un archivo seguro en tu máquina local
- ⚠️ **NUNCA** las subas a Git

## 💻 Paso a Paso: Configurar AWS CLI

### Instalación de AWS CLI

**Windows**:
```powershell
# Descargar desde: https://awscli.amazonaws.com/AWSCLIV2.msi
# O usar Chocolatey:
choco install awscli
```

**Linux/Mac**:
```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

### Configuración

```bash
aws configure
```

Ingresa:
1. **AWS Access Key ID**: [Tu Access Key ID]
2. **AWS Secret Access Key**: [Tu Secret Access Key]
3. **Default region name**: `us-east-1` (o la región más cercana)
4. **Default output format**: `json`

### Verificar Configuración

```bash
aws sts get-caller-identity
```

Deberías ver algo como:
```json
{
    "UserId": "AIDAXXXXXXXXXXXXXXXXX",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/devsecops-user"
}
```

## 🎯 Configurar Región

### Regiones Recomendadas para Free Tier

- **us-east-1** (N. Virginia) - Más servicios disponibles
- **us-west-2** (Oregon) - Buena disponibilidad
- **eu-west-1** (Ireland) - Si estás en Europa

### Cambiar Región

```bash
aws configure set region us-east-1
```

O edita `~/.aws/config`:
```ini
[default]
region = us-east-1
```

## 💰 Configurar Alertas de Presupuesto

### Paso 1: Crear Budget Alert

1. Ve a **AWS Billing** → **Budgets**
2. Haz clic en **"Create budget"**
3. Selecciona **"Cost budget"**
4. Configura:
   - **Budget amount**: $5 USD (o el límite que prefieras)
   - **Alert threshold**: 80% y 100%
   - **Email**: Tu correo

### Paso 2: Activar MFA (Opcional pero Recomendado)

1. Ve a **IAM** → **Users** → Tu usuario
2. Pestaña **"Security credentials"**
3. En **"Assigned MFA device"**, haz clic en **"Assign MFA device"**
4. Sigue las instrucciones para configurar un dispositivo MFA

## 🧪 Verificar que Todo Funciona

### Test 1: Listar Buckets S3

```bash
aws s3 ls
```

### Test 2: Crear Stack de Prueba

```bash
cdk init app --language csharp
cdk bootstrap
cdk synth
```

### Test 3: Verificar Límites de Cuenta

```bash
aws service-quotas list-service-quotas --service-code ec2
```

## 📊 Monitoreo de Costos

### CloudWatch Billing Dashboard

1. Ve a **AWS Billing** → **Cost Management**
2. Revisa **"Cost Explorer"** para ver gastos diarios
3. Configura **"Cost Anomaly Detection"** para alertas automáticas

### Comandos Útiles

```bash
# Ver costos estimados
aws ce get-cost-and-usage \
  --time-period Start=2024-01-01,End=2024-01-31 \
  --granularity MONTHLY \
  --metrics BlendedCost

# Listar servicios utilizados
aws ce get-dimension-values \
  --time-period Start=2024-01-01,End=2024-01-31 \
  --dimension SERVICE
```

## ⚠️ Mejores Prácticas para Free Tier

1. **Usa NAT Instance en lugar de NAT Gateway**
   - NAT Gateway cuesta ~$32/mes
   - NAT Instance (t2.micro) está en Free Tier

2. **Elimina recursos que no uses**
   - Detén instancias EC2 cuando no las uses
   - Elimina buckets S3 vacíos
   - Limpia snapshots antiguos

3. **Usa CloudFormation/CDK para limpieza fácil**
   ```bash
   cdk destroy --all
   ```

4. **Monitorea costos diariamente**
   - Revisa el dashboard de billing
   - Configura alertas

5. **Usa tags para identificar recursos**
   ```bash
   aws ec2 create-tags --resources i-1234567890abcdef0 --tags Key=Project,Value=DevSecOps-Prueba
   ```

## 🗑️ Limpieza de Recursos

### Script de Limpieza

```bash
#!/bin/bash
# cleanup-aws.sh

echo "Limpiando recursos AWS..."

# Eliminar instancias EC2
aws ec2 describe-instances --query 'Reservations[*].Instances[*].[InstanceId]' --output text | xargs -I {} aws ec2 terminate-instances --instance-ids {}

# Eliminar buckets S3
aws s3 ls | awk '{print $3}' | xargs -I {} aws s3 rb s3://{} --force

# Eliminar stacks CloudFormation
aws cloudformation list-stacks --stack-status-filter CREATE_COMPLETE UPDATE_COMPLETE --query 'StackSummaries[*].StackName' --output text | xargs -I {} aws cloudformation delete-stack --stack-name {}

echo "Limpieza completada"
```

## 📞 Soporte y Recursos

- **Documentación AWS Free Tier**: https://aws.amazon.com/free/
- **Calculadora de Costos**: https://calculator.aws/
- **Foros de Soporte**: https://forums.aws.amazon.com/
- **AWS Support**: Basic (gratis) incluye acceso a documentación y foros

## ✅ Checklist de Configuración

- [ ] Cuenta AWS creada y verificada
- [ ] Tarjeta de crédito agregada (solo para verificación)
- [ ] Usuario IAM creado con permisos adecuados
- [ ] Access Key ID y Secret Access Key guardados de forma segura
- [ ] AWS CLI instalado y configurado
- [ ] Región configurada (us-east-1 recomendado)
- [ ] Alertas de presupuesto configuradas
- [ ] MFA activado (opcional pero recomendado)
- [ ] CDK instalado y bootstrap ejecutado
- [ ] Test de conexión exitoso (`aws sts get-caller-identity`)

## 🎉 ¡Listo!

Ahora estás listo para comenzar a trabajar con AWS. Puedes proceder con la instalación y despliegue del proyecto DevSecOps.

