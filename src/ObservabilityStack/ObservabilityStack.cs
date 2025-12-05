using Amazon.CDK;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;
using Constructs;
using System.Collections.Generic;

namespace ObservabilityStack
{
    public class ObservabilityStack : Stack
    {
        public ObservabilityStack(Constructs.Construct scope, string id, IStackProps props = null) 
            : base(scope, id, props)
        {
            // 1. CloudWatch Log Groups centralizados por unidad de negocio
            var smLogGroup = new LogGroup(this, "SMLogGroup", new LogGroupProps
            {
                LogGroupName = "/aws/devsecops/SM",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var ayLogGroup = new LogGroup(this, "AYLogGroup", new LogGroupProps
            {
                LogGroupName = "/aws/devsecops/AY",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var logsBucket = new Bucket(this, "LongTermLogsBucket", new BucketProps
            {
                Versioned = true,
                Encryption = BucketEncryption.S3_MANAGED,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                LifecycleRules = new[]
                {
                    new LifecycleRule
                    {
                        Id = "ArchiveOldLogs",
                        Enabled = true,
                        Transitions = new[]
                        {
                            new Transition
                            {
                                StorageClass = StorageClass.INFREQUENT_ACCESS,
                                TransitionAfter = Duration.Days(90)
                            },
                            new Transition
                            {
                                StorageClass = StorageClass.GLACIER,
                                TransitionAfter = Duration.Days(180)
                            }
                        },
                        Expiration = Duration.Days(365)
                    }
                }
            });

            // 3. IAM Role para exportar logs a S3
            var logExportRole = new Role(this, "LogExportRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("logs.amazonaws.com"),
                Description = "Role para exportar logs de CloudWatch a S3"
            });

            // Permisos básicos para escribir en el bucket
            logExportRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "s3:PutObject", "s3:GetBucketLocation" },
                Resources = new[] { $"{logsBucket.BucketArn}/*", logsBucket.BucketArn }
            }));

            // 4. Outputs
            new CfnOutput(this, "SMLogGroupName", new CfnOutputProps
            {
                Value = smLogGroup.LogGroupName,
                Description = "CloudWatch Log Group para unidad SM"
            });

            new CfnOutput(this, "AYLogGroupName", new CfnOutputProps
            {
                Value = ayLogGroup.LogGroupName,
                Description = "CloudWatch Log Group para unidad AY"
            });

            new CfnOutput(this, "CentralLogGroupName", new CfnOutputProps
            {
                Value = "/aws/devsecops/central",
                Description = "CloudWatch Log Group centralizado (creado por DevSecOpsStack)"
            });

            new CfnOutput(this, "LogsBucket", new CfnOutputProps
            {
                Value = logsBucket.BucketName,
                Description = "Bucket S3 para logs de largo plazo"
            });

            new CfnOutput(this, "DashboardUrl", new CfnOutputProps
            {
                Value = $"https://console.aws.amazon.com/cloudwatch/home?region={Region}#dashboards:",
                Description = "URL para crear dashboard manualmente en CloudWatch Console"
            });
        }
    }
}

