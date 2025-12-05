# Matriz de Herramientas - Justificación Técnica

## 📊 Resumen Ejecutivo

Esta matriz detalla las herramientas seleccionadas para la prueba técnica de DevSecOps, incluyendo justificaciones técnicas, alternativas consideradas y criterios de selección.

## 🏗️ Infraestructura como Código (IaC)

### AWS CDK con C#

**Decisión**: Usar AWS CDK con C# como lenguaje principal

**Justificación**:
1. **Familiaridad del lenguaje**: El equipo trabaja con C# como herramienta principal
2. **Type Safety**: C# proporciona verificación de tipos en tiempo de compilación
3. **Reutilización**: Constructs reutilizables y bibliotecas compartidas
4. **Integración nativa**: Integración perfecta con servicios AWS
5. **Ecosistema**: Amplio soporte de la comunidad y documentación

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **Terraform** | - Multi-cloud<br>- Estado inmutable<br>- Gran comunidad | - HCL no es familiar<br>- Curva de aprendizaje<br>- Gestión de estado | ❌ Rechazado |
| **CloudFormation** | - Nativo de AWS<br>- Sin costo | - YAML/JSON verboso<br>- Menos flexible | ❌ Rechazado (CDK genera CF) |
| **Pulumi** | - Múltiples lenguajes<br>- Buen soporte C# | - Menor adopción<br>- Costo adicional | ⚠️ Considerado |

**Criterios de Selección**:
- ✅ Familiaridad del equipo (C#)
- ✅ Type safety
- ✅ Integración con AWS
- ✅ Sin costo adicional
- ✅ Soporte de la comunidad

## 🔄 Pipeline CI/CD

### AWS CodePipeline + CodeBuild

**Decisión**: Usar servicios nativos de AWS para el pipeline

**Justificación**:
1. **Integración nativa**: Sin configuración adicional de conectividad
2. **Free Tier**: CodePipeline gratis primeros 30 días, CodeBuild 100 min/mes
3. **Escalabilidad**: Maneja proyectos de cualquier tamaño
4. **Seguridad**: Integración con IAM y VPC
5. **Simplicidad**: Configuración mediante YAML (buildspec.yml)

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **GitHub Actions** | - Gratis para repos públicos<br>- Integración con GitHub<br>- Gran ecosistema | - Requiere GitHub<br>- Límites en repos privados | ⚠️ Alternativa viable |
| **GitLab CI** | - Integrado en GitLab<br>- Muy flexible<br>- Buen soporte | - Requiere GitLab<br>- Configuración más compleja | ⚠️ Alternativa viable |
| **Jenkins** | - Open source<br>- Muy flexible<br>- Gran comunidad | - Requiere servidor propio<br>- Mantenimiento | ❌ Rechazado |

**Criterios de Selección**:
- ✅ Integración con AWS
- ✅ Costo (Free Tier)
- ✅ Facilidad de configuración
- ✅ Escalabilidad

## 🔒 Seguridad

### SCA: Trivy

**Decisión**: Trivy para Software Composition Analysis

**Justificación**:
1. **Open Source**: Sin costo de licencia
2. **Multi-propósito**: Escanea dependencias, contenedores, IaC
3. **Rapidez**: Escaneos rápidos comparado con alternativas
4. **Integración**: Fácil integración en pipelines
5. **Reportes**: Múltiples formatos (JSON, table, SARIF)

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **Snyk** | - Excelente detección<br>- Integración con IDEs<br>- Base de datos actualizada | - Costo para equipos<br>- Límites en plan gratuito | ⚠️ Alternativa premium |
| **OWASP Dependency-Check** | - Open source<br>- Estándar de la industria | - Más lento<br>- Requiere más configuración | ⚠️ Alternativa viable |
| **WhiteSource** | - Detección avanzada<br>- Gestión de licencias | - Costo alto<br>- Complejidad | ❌ Rechazado |

**Criterios de Selección**:
- ✅ Open source
- ✅ Rapidez
- ✅ Fácil integración
- ✅ Buena detección

### SAST: SonarQube

**Decisión**: SonarQube para Static Application Security Testing

**Justificación**:
1. **Análisis profundo**: Detecta vulnerabilidades complejas
2. **Soporte C#**: Excelente soporte para .NET
3. **Reglas personalizables**: Adaptable a necesidades específicas
4. **Integración**: Plugins para múltiples herramientas
5. **Community Edition**: Gratis para proyectos open source

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **CodeQL** | - Gratis (GitHub)<br>- Muy potente<br>- Detección avanzada | - Curva de aprendizaje<br>- Requiere GitHub | ⚠️ Alternativa viable |
| **Checkmarx** | - Excelente detección<br>- Soporte enterprise | - Costo muy alto<br>- Complejidad | ❌ Rechazado |
| **Veracode** | - Análisis completo<br>- Soporte enterprise | - Costo alto<br>- SaaS requerido | ❌ Rechazado |

**Nota**: Para esta prueba, también se puede usar CodeQL si se usa GitHub Actions.

**Criterios de Selección**:
- ✅ Soporte C#
- ✅ Análisis profundo
- ✅ Costo razonable
- ✅ Integración con pipelines

### Secret Scanning: GitLeaks

**Decisión**: GitLeaks para detección de secretos

**Justificación**:
1. **Open Source**: Sin costo
2. **Rapidez**: Escaneos muy rápidos
3. **Detección amplia**: Detecta múltiples tipos de secretos
4. **Fácil integración**: CLI simple
5. **Actualizaciones**: Base de reglas actualizada regularmente

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **TruffleHog** | - Buena detección<br>- Integración con GitHub | - Más lento<br>- Requiere más recursos | ⚠️ Alternativa viable |
| **git-secrets** | - Simple<br>- Nativo de Git | - Menos detecciones<br>- Solo Git hooks | ⚠️ Alternativa básica |
| **AWS Secrets Manager** | - Gestión nativa<br>- Integración AWS | - No escanea código<br>- Costo | ❌ Uso complementario |

**Criterios de Selección**:
- ✅ Open source
- ✅ Rapidez
- ✅ Buena detección
- ✅ Fácil integración

### IaC Scanning: Checkov

**Decisión**: Checkov para escaneo de infraestructura como código

**Justificación**:
1. **Soporte CDK/CloudFormation**: Escanea código generado por CDK
2. **Políticas extensas**: Más de 1000 políticas predefinidas
3. **Open Source**: Sin costo
4. **Integración**: Fácil integración en CI/CD
5. **Actualizaciones**: Base de políticas actualizada regularmente

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **cfn-nag** | - Específico CloudFormation<br>- Rápido | - Solo CloudFormation<br>- Menos políticas | ⚠️ Alternativa viable |
| **Terrascan** | - Multi-cloud<br>- Buen soporte | - Menos políticas para AWS<br>- Más lento | ⚠️ Alternativa viable |
| **Snyk IaC** | - Buena detección<br>- Integración Snyk | - Costo<br>- Requiere cuenta Snyk | ❌ Rechazado |

**Nota**: También se usa Trivy para IaC scanning como complemento.

**Criterios de Selección**:
- ✅ Soporte CDK/CloudFormation
- ✅ Políticas extensas
- ✅ Open source
- ✅ Integración fácil

## 📊 Observabilidad

### CloudWatch Logs + Metrics

**Decisión**: CloudWatch para observabilidad centralizada

**Justificación**:
1. **Nativo de AWS**: Sin configuración adicional
2. **Free Tier**: 5 GB logs, 10 métricas personalizadas
3. **Integración**: Integración perfecta con otros servicios AWS
4. **Dashboards**: Dashboards personalizables incluidos
5. **Alertas**: Sistema de alarmas integrado

**Alternativas Consideradas**:

| Herramienta | Pros | Contras | Decisión |
|-------------|------|---------|----------|
| **ELK Stack** | - Muy potente<br>- Open source<br>- Flexible | - Requiere infraestructura<br>- Mantenimiento<br>- Costo de EC2 | ⚠️ Alternativa para escala |
| **Datadog** | - Excelente UI<br>- Múltiples integraciones | - Costo alto<br>- SaaS requerido | ❌ Rechazado |
| **Prometheus + Grafana** | - Open source<br>- Muy flexible | - Requiere infraestructura<br>- Configuración compleja | ⚠️ Alternativa avanzada |

**Criterios de Selección**:
- ✅ Nativo de AWS
- ✅ Free Tier
- ✅ Integración
- ✅ Facilidad de uso

### S3 para Logs de Largo Plazo

**Decisión**: S3 para almacenamiento de logs históricos

**Justificación**:
1. **Costo**: Muy económico para almacenamiento
2. **Durabilidad**: 99.999999999% (11 9's)
3. **Lifecycle**: Políticas para archivar a Glacier
4. **Free Tier**: 5 GB primeros 12 meses
5. **Integración**: Fácil integración con CloudWatch

**Criterios de Selección**:
- ✅ Costo
- ✅ Durabilidad
- ✅ Integración AWS

## 📈 Resumen de Decisiones

| Categoría | Herramienta | Tipo | Costo | Justificación Principal |
|-----------|-------------|------|-------|-------------------------|
| IaC | AWS CDK (C#) | Propietario (AWS) | Gratis | Familiaridad y type safety |
| Pipeline | CodePipeline + CodeBuild | Propietario (AWS) | Free Tier | Integración nativa |
| SCA | Trivy | Open Source | Gratis | Rapidez y multi-propósito |
| SAST | SonarQube | Open Source | Gratis (CE) | Análisis profundo C# |
| Secret Scan | GitLeaks | Open Source | Gratis | Rapidez y detección |
| IaC Scan | Checkov | Open Source | Gratis | Políticas extensas |
| Observabilidad | CloudWatch | Propietario (AWS) | Free Tier | Integración nativa |
| Almacenamiento | S3 | Propietario (AWS) | Free Tier | Costo y durabilidad |

## 🔄 Estrategia de Migración/Alternativas

Si necesitas cambiar alguna herramienta:

### Cambiar de CodePipeline a GitHub Actions

1. Crear `.github/workflows/ci.yml`
2. Reemplazar buildspec.yml con steps de GitHub Actions
3. Usar `aws-actions/configure-aws-credentials`
4. Mantener misma estructura de escaneos

### Cambiar de SonarQube a CodeQL

1. Instalar CodeQL CLI
2. Crear queries personalizadas
3. Integrar en pipeline
4. Generar reportes SARIF

### Cambiar de CloudWatch a ELK

1. Desplegar Elasticsearch en EC2
2. Configurar Logstash para ingesta
3. Configurar Kibana para visualización
4. Actualizar aplicaciones para enviar a Logstash

## ✅ Checklist de Herramientas

- [x] AWS CDK (C#) - IaC
- [x] CodePipeline - Orquestación
- [x] CodeBuild - Build
- [x] Trivy - SCA
- [x] SonarQube - SAST
- [x] GitLeaks - Secret Scanning
- [x] Checkov - IaC Scanning
- [x] CloudWatch - Observabilidad
- [x] S3 - Almacenamiento

## 📚 Referencias

- [Trivy vs Snyk Comparison](https://aquasecurity.github.io/trivy/)
- [SonarQube Documentation](https://docs.sonarqube.org/)
- [Checkov Policies](https://www.checkov.io/2.Basics/Getting%20Started.html)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)

