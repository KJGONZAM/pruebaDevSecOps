# Documento de Diseño Técnico - DevSecOps

## 📋 Información del Proyecto

- **Proyecto**: Prueba Técnica DevSecOps Engineer
- **Herramienta IaC**: AWS CDK con C#
- **Plataforma Cloud**: AWS
- **Fecha**: 2024

## 🏗️ Arquitectura

### Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                         AWS Cloud                                │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    VPC (10.0.0.0/16)                     │  │
│  │                                                           │  │
│  │  ┌──────────────────┐      ┌──────────────────┐        │  │
│  │  │  Unidad SM       │      │  Unidad AY       │        │  │
│  │  │                  │      │                  │        │  │
│  │  │ ┌──────────────┐ │      │ ┌──────────────┐ │        │  │
│  │  │ │ Public Subnet│ │      │ │ Public Subnet│ │        │  │
│  │  │ │ 10.0.1.0/24  │ │      │ │ 10.0.3.0/24  │ │        │  │
│  │  │ └──────────────┘ │      │ └──────────────┘ │        │  │
│  │  │                  │      │                  │        │  │
│  │  │ ┌──────────────┐ │      │ ┌──────────────┐ │        │  │
│  │  │ │Private Subnet│ │      │ │Private Subnet│ │        │  │
│  │  │ │ 10.0.2.0/24  │ │      │ │ 10.0.4.0/24  │ │        │  │
│  │  │ └──────────────┘ │      │ └──────────────┘ │        │  │
│  │  │                  │      │                  │        │  │
│  │  │ Security Group   │      │ Security Group   │        │  │
│  │  │ SM-SG            │      │ AY-SG            │        │  │
│  │  └──────────────────┘      └──────────────────┘        │  │
│  │                                                           │  │
│  │  ┌──────────────────────────────────────────────────┐   │  │
│  │  │              NAT Gateway/Instance                │   │  │
│  │  └──────────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              CodePipeline + CodeBuild                    │  │
│  │                                                           │  │
│  │  Source → Build → Security Scans → Deploy               │  │
│  │    │        │         │              │                   │  │
│  │    │        │    ┌─────┴─────┐        │                   │  │
│  │    │        │    │ SCA       │        │                   │  │
│  │    │        │    │ SAST      │        │                   │  │
│  │    │        │    │ Secrets   │        │                   │  │
│  │    │        │    │ IaC Scan  │        │                   │  │
│  │    └────────┴────┴───────────┴────────┘                   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │         Observabilidad Centralizada                     │  │
│  │                                                           │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │  │
│  │  │ CloudWatch   │  │  S3 Logs     │  │  Dashboards  │  │  │
│  │  │   Logs       │  │   Bucket     │  │  Personaliz. │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    IAM Governance                        │  │
│  │                                                           │  │
│  │  ┌──────────────┐              ┌──────────────┐          │  │
│  │  │ SM Developer │              │ AY Developer │          │  │
│  │  │    Role      │              │    Role     │          │  │
│  │  │ (Tag: SM)    │              │ (Tag: AY)   │          │  │
│  │  └──────────────┘              └──────────────┘          │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes de la Arquitectura

#### 1. Red (VPC)

- **CIDR**: 10.0.0.0/16
- **Availability Zones**: 2 (para alta disponibilidad)
- **Subnets**:
  - SM-Public: 10.0.1.0/24
  - SM-Private: 10.0.2.0/24
  - AY-Public: 10.0.3.0/24
  - AY-Private: 10.0.4.0/24
- **NAT**: NAT Instance (t2.micro) para mantener Free Tier

#### 2. Seguridad

- **Security Groups**: Separados por unidad de negocio
- **IAM Roles**: Acceso basado en tags
- **Encriptación**: S3 con encriptación server-side
- **Network ACLs**: Configurados con reglas restrictivas

#### 3. Pipeline CI/CD

- **Source**: GitHub/GitLab
- **Build**: AWS CodeBuild
- **Security Scans**: Integrados en el pipeline
- **Deploy**: Automático a ambientes

#### 4. Observabilidad

- **CloudWatch Logs**: Logs centralizados
- **CloudWatch Metrics**: Métricas de aplicación
- **S3**: Almacenamiento de logs de largo plazo
- **Dashboards**: Personalizados por unidad

## 🔄 Estrategia de Ramas (Branching Strategy)

### Git Flow Adaptado

```
main (producción)
  │
  ├── develop (desarrollo)
  │     │
  │     ├── feature/SM-* (features unidad SM)
  │     ├── feature/AY-* (features unidad AY)
  │     └── feature/shared-* (features compartidas)
  │
  ├── release/v*.*.* (preparación de release)
  │
  └── hotfix/* (correcciones urgentes)
```

### Flujo de Trabajo

1. **Desarrollo**:
   - Desarrollador crea branch `feature/SM-001` o `feature/AY-001`
   - Hace commit y push
   - Pipeline se ejecuta automáticamente

2. **Pipeline en Feature Branch**:
   - Build
   - Security Scans (advertencias, no bloquea)
   - Tests unitarios

3. **Merge a Develop**:
   - Pull Request requiere aprobación
   - Pipeline más estricto (bloquea en vulnerabilidades críticas)
   - Deploy automático a ambiente de desarrollo

4. **Release**:
   - Crear branch `release/v1.0.0` desde `develop`
   - Pipeline completo con todos los escaneos
   - Deploy a staging
   - Tests de integración

5. **Producción**:
   - Merge de `release/*` a `main`
   - Pipeline de producción (máxima seguridad)
   - Deploy manual con aprobación
   - Deploy a producción

### Configuración de Pipeline por Rama

| Rama | Build | Security Scans | Deploy | Ambiente |
|------|-------|---------------|--------|----------|
| feature/* | ✅ | ⚠️ (advertencias) | ❌ | - |
| develop | ✅ | ✅ (bloquea críticas) | ✅ | Dev |
| release/* | ✅ | ✅ (bloquea todas) | ✅ | Staging |
| main | ✅ | ✅ (bloquea todas) | ✅ (manual) | Prod |

## 🛠️ Matriz de Herramientas

### Infraestructura como Código

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **AWS CDK (C#)** | - Lenguaje familiar (C#)<br>- Type-safe<br>- Reutilización de código<br>- Integración nativa con AWS | Terraform, CloudFormation, Pulumi |
| **CloudFormation** | - Generado por CDK<br>- Gestión nativa de recursos | - |

### Pipeline CI/CD

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **AWS CodePipeline** | - Integración nativa con AWS<br>- Sin costo primeros 30 días<br>- Fácil integración con CodeBuild | GitHub Actions, GitLab CI, Jenkins |
| **AWS CodeBuild** | - 100 min/mes en Free Tier<br>- Integración con CodePipeline<br>- Builds en contenedores | GitHub Actions, GitLab Runner |

### Seguridad

#### SCA (Software Composition Analysis)

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **Trivy** | - Open source<br>- Escanea dependencias y contenedores<br>- Integración fácil<br>- Reportes en múltiples formatos | Snyk, OWASP Dependency-Check, WhiteSource |

#### SAST (Static Application Security Testing)

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **SonarQube** | - Análisis profundo de código<br>- Soporte para C#<br>- Integración con pipelines<br>- Reglas personalizables | CodeQL, Checkmarx, Veracode |

#### Secret Scanning

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **GitLeaks** | - Open source<br>- Rápido<br>- Detecta múltiples tipos de secretos<br>- Fácil integración | TruffleHog, git-secrets, AWS Secrets Manager |

#### IaC Scanning

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **Checkov** | - Soporte para CloudFormation/CDK<br>- Políticas extensas<br>- Integración con CI/CD<br>- Open source | cfn-nag, Terrascan, Snyk IaC |

### Observabilidad

| Herramienta | Justificación | Alternativas Consideradas |
|-------------|---------------|---------------------------|
| **CloudWatch Logs** | - Nativo de AWS<br>- 5 GB/mes en Free Tier<br>- Integración con otros servicios | ELK Stack, Splunk, Datadog |
| **CloudWatch Metrics** | - Nativo de AWS<br>- 10 métricas personalizadas gratis<br>- Dashboards integrados | Prometheus, Grafana, Datadog |

## 🔒 Estrategia de Seguridad

### Separación de Unidades SM y AY

#### 1. Nivel de Red
- **Subnets separadas**: Cada unidad tiene sus propias subnets públicas y privadas
- **Security Groups**: Reglas de firewall independientes
- **Network ACLs**: Control adicional a nivel de subnet

#### 2. Nivel de IAM
- **Roles separados**: `SM-DeveloperRole` y `AY-DeveloperRole`
- **Políticas basadas en tags**: Acceso solo a recursos con tag `BusinessUnit=SM` o `BusinessUnit=AY`
- **Principio de menor privilegio**: Permisos mínimos necesarios

#### 3. Nivel de Aplicación
- **Tags en recursos**: Todos los recursos etiquetados con `BusinessUnit`
- **Políticas de recursos**: Restricciones basadas en tags

### Ejemplo de Política IAM

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["ec2:*", "s3:*"],
      "Resource": "*",
      "Condition": {
        "StringEquals": {
          "aws:ResourceTag/BusinessUnit": "SM"
        }
      }
    }
  ]
}
```

### Encriptación

- **En tránsito**: TLS 1.2+ para todas las comunicaciones
- **En reposo**: 
  - S3: Encriptación server-side (SSE-S3)
  - EBS: Encriptación por defecto
  - RDS: Encriptación habilitada

### Escaneos de Seguridad

1. **Automáticos en cada commit**
2. **Bloqueo en vulnerabilidades críticas**
3. **Reportes consolidados**
4. **Notificaciones a desarrolladores**

## 📊 Observabilidad

### Centralización de Logs

#### CloudWatch Logs
- **Log Group centralizado**: `/aws/devsecops/central`
- **Retención**: 30 días (configurable)
- **Streams separados por unidad**: `/aws/devsecops/SM/*` y `/aws/devsecops/AY/*`

#### S3 para Logs de Largo Plazo
- **Bucket**: `devsecops-logs-{account}-{region}`
- **Versionado**: Habilitado
- **Lifecycle policies**: Archivar a Glacier después de 90 días

### Métricas

- **Aplicación**: Latencia, errores, throughput
- **Infraestructura**: CPU, memoria, red
- **Negocio**: Requests por unidad, costos por unidad

### Dashboards

- **Dashboard SM**: Métricas específicas de unidad SM
- **Dashboard AY**: Métricas específicas de unidad AY
- **Dashboard Global**: Vista consolidada

## 💰 Gestión de Costos (Bonus)

### Alertas de Presupuesto

- **Alerta al 80%**: Notificación temprana
- **Alerta al 100%**: Bloqueo de recursos nuevos
- **Alertas semanales**: Resumen de gastos

### Optimizaciones para Free Tier

1. **NAT Instance** en lugar de NAT Gateway
2. **Eliminación automática** de recursos no usados
3. **Scheduling** de instancias EC2 (apagar en horarios no laborales)
4. **Compresión de logs** antes de enviar a S3

## 🚀 Despliegue

### Ambientes

1. **Development**: Auto-deploy desde `develop`
2. **Staging**: Auto-deploy desde `release/*`
3. **Production**: Deploy manual desde `main`

### Proceso de Despliegue

1. **Validación**: CDK synth y validación de templates
2. **Security Scans**: Todos los escaneos deben pasar
3. **Tests**: Unitarios e integración
4. **Deploy**: CDK deploy con confirmación
5. **Verificación**: Health checks post-deploy

## 📝 Consideraciones de Cambio

### Para Adaptar a Tu Entorno

1. **Región AWS**: Cambiar en `cdk.json` o variables de entorno
2. **CIDR de VPC**: Modificar en `DevSecOpsStack.cs` si hay conflictos
3. **Herramientas de seguridad**: Reemplazar en `buildspec.yml` según preferencias
4. **Repositorio**: Actualizar URLs en configuración del pipeline
5. **Permisos IAM**: Ajustar políticas según necesidades reales

### Variables de Configuración

Crear archivo `config/appsettings.json`:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "AccountId": "123456789012"
  },
  "VPC": {
    "Cidr": "10.0.0.0/16"
  },
  "BusinessUnits": {
    "SM": {
      "PublicSubnetCidr": "10.0.1.0/24",
      "PrivateSubnetCidr": "10.0.2.0/24"
    },
    "AY": {
      "PublicSubnetCidr": "10.0.3.0/24",
      "PrivateSubnetCidr": "10.0.4.0/24"
    }
  }
}
```

## ✅ Checklist de Implementación

- [x] VPC con subnets separadas
- [x] Security Groups configurados
- [x] IAM Roles con separación por tags
- [x] Pipeline CI/CD básico
- [x] Integración de escaneos de seguridad
- [x] CloudWatch Logs centralizados
- [x] S3 para logs de largo plazo
- [ ] Dashboards personalizados (pendiente)
- [ ] Alertas de presupuesto (bonus)
- [ ] Disaster Recovery (bonus)

## 📚 Referencias

- [AWS CDK Documentation](https://docs.aws.amazon.com/cdk/)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Trivy Documentation](https://aquasecurity.github.io/trivy/)
- [Checkov Documentation](https://www.checkov.io/)

