using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.Collections.Generic;

namespace DevSecOpsStack
{
    public class DevSecOpsStack : Stack
    {
        public DevSecOpsStack(Constructs.Construct scope, string id, IStackProps props = null) 
            : base(scope, id, props)
        {
            // 1. Crear VPC con separación lógica para SM y AY
            var vpc = new Vpc(this, "DevSecOpsVPC", new VpcProps
            {
                MaxAzs = 2, // 2 Availability Zones para alta disponibilidad
                NatGateways = 0, // 1 NAT Gateway (considerar NAT Instance para Free Tier)
                Cidr = "10.0.0.0/16",
                SubnetConfiguration = new ISubnetConfiguration[]
                {
                    // Subnets para unidad SM
                    new SubnetConfiguration
                    {
                        Name = "SM-Public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    },
                    new SubnetConfiguration
                    {
                        Name = "SM-Private",
                        SubnetType = SubnetType.ISOLATED, // ISOLATED porque NatGateways = 0
                        CidrMask = 24
                    },
                    // Subnets para unidad AY
                    new SubnetConfiguration
                    {
                        Name = "AY-Public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    },
                    new SubnetConfiguration
                    {
                        Name = "AY-Private",
                        SubnetType = SubnetType.ISOLATED, // ISOLATED porque NatGateways = 0
                        CidrMask = 24
                    }
                }
            });

            // 2. Security Groups con principio de menor privilegio
            // Security Group para SM
            var smSecurityGroup = new SecurityGroup(this, "SM-SecurityGroup", new SecurityGroupProps
            {
                Vpc = vpc,
                Description = "Security Group para unidad de negocio SM",
                AllowAllOutbound = false
            });

            // Security Group para AY
            var aySecurityGroup = new SecurityGroup(this, "AY-SecurityGroup", new SecurityGroupProps
            {
                Vpc = vpc,
                Description = "Security Group para unidad de negocio AY",
                AllowAllOutbound = false
            });

            var logsBucket = new Bucket(this, "CentralizedLogsBucket", new BucketProps
            {
                BucketName = $"devsecops-logs-{Account?.Replace("-", "")}-{Region?.Replace("-", "")}",
                Versioned = true,
                Encryption = BucketEncryption.S3_MANAGED,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL
            });

            // 4. CloudWatch Log Group para logs centralizados
            var centralLogGroup = new LogGroup(this, "CentralLogGroup", new LogGroupProps
            {
                LogGroupName = "/aws/devsecops/central",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // 5. IAM Roles para separación de unidades
            // Role para desarrolladores SM
            var smDeveloperRole = new Role(this, "SM-DeveloperRole", new RoleProps
            {
                AssumedBy = new AccountRootPrincipal(),
                Description = "Role para desarrolladores de la unidad SM",
                RoleName = "SM-DeveloperRole",
                MaxSessionDuration = Duration.Hours(12)
            });

            // Política que permite acceso solo a recursos con tag BusinessUnit=SM
            smDeveloperRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] 
                { 
                    "ec2:Describe*", 
                    "ec2:Get*",
                    "ec2:List*",
                    "s3:GetObject",
                    "s3:ListBucket",
                    "s3:PutObject",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents",
                    "logs:DescribeLogGroups",
                    "logs:DescribeLogStreams"
                },
                Resources = new[] { "*" },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "StringEquals",
                        new Dictionary<string, object>
                        {
                            { "aws:ResourceTag/BusinessUnit", "SM" }
                        }
                    }
                }
            }));

            // Política para denegar acceso a recursos de AY
            smDeveloperRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.DENY,
                Actions = new[] { "*" },
                Resources = new[] { "*" },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "StringEquals",
                        new Dictionary<string, object>
                        {
                            { "aws:ResourceTag/BusinessUnit", "AY" }
                        }
                    }
                }
            }));

            // Role para desarrolladores AY
            var ayDeveloperRole = new Role(this, "AY-DeveloperRole", new RoleProps
            {
                AssumedBy = new AccountRootPrincipal(),
                Description = "Role para desarrolladores de la unidad AY",
                RoleName = "AY-DeveloperRole",
                MaxSessionDuration = Duration.Hours(12)
            });

            ayDeveloperRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] 
                { 
                    "ec2:Describe*", 
                    "ec2:Get*",
                    "ec2:List*",
                    "s3:GetObject",
                    "s3:ListBucket",
                    "s3:PutObject",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents",
                    "logs:DescribeLogGroups",
                    "logs:DescribeLogStreams"
                },
                Resources = new[] { "*" },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "StringEquals",
                        new Dictionary<string, object>
                        {
                            { "aws:ResourceTag/BusinessUnit", "AY" }
                        }
                    }
                }
            }));

            // Política para denegar acceso a recursos de SM
            ayDeveloperRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.DENY,
                Actions = new[] { "*" },
                Resources = new[] { "*" },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "StringEquals",
                        new Dictionary<string, object>
                        {
                            { "aws:ResourceTag/BusinessUnit", "SM" }
                        }
                    }
                }
            }));

            // 6. IAM Users para cada unidad
            var smDeveloperUser = new User(this, "SM-DeveloperUser", new UserProps
            {
                UserName = "sm-developer",
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("IAMUserChangePassword")
                }
            });

            smDeveloperUser.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "sts:AssumeRole" },
                Resources = new[] { smDeveloperRole.RoleArn }
            }));

            var ayDeveloperUser = new User(this, "AY-DeveloperUser", new UserProps
            {
                UserName = "ay-developer",
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("IAMUserChangePassword")
                }
            });

            ayDeveloperUser.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "sts:AssumeRole" },
                Resources = new[] { ayDeveloperRole.RoleArn }
            }));

            // 7. Lambda Function "Hello World" - Aplicación simple para demostrar el flujo
            var helloWorldFunction = new Function(this, "HelloWorldFunction", new FunctionProps
            {
                Runtime = new Runtime("nodejs18.x"),
                Handler = "index.handler",
                Code = Code.FromInline(@"
exports.handler = async (event) => {
    return {
        statusCode: 200,
        body: JSON.stringify({
            message: 'Hello World from DevSecOps Stack!',
            timestamp: new Date().toISOString()
        }),
        headers: {
            'Content-Type': 'application/json'
        }
    };
};
"),
                Description = "Función Lambda Hello World para demostrar el despliegue",
                Timeout = Duration.Seconds(30),
                MemorySize = 128,
                LogRetention = RetentionDays.ONE_WEEK
            });

            // 8. Outputs
            new CfnOutput(this, "VpcId", new CfnOutputProps
            {
                Value = vpc.VpcId,
                Description = "ID de la VPC creada"
            });

            new CfnOutput(this, "SMLogsBucket", new CfnOutputProps
            {
                Value = logsBucket.BucketName,
                Description = "Bucket S3 para logs centralizados"
            });

            new CfnOutput(this, "CentralLogGroupName", new CfnOutputProps
            {
                Value = centralLogGroup.LogGroupName,
                Description = "CloudWatch Log Group centralizado"
            });

            new CfnOutput(this, "SMDeveloperRoleArn", new CfnOutputProps
            {
                Value = smDeveloperRole.RoleArn,
                Description = "ARN del role para desarrolladores SM"
            });

            new CfnOutput(this, "AYDeveloperRoleArn", new CfnOutputProps
            {
                Value = ayDeveloperRole.RoleArn,
                Description = "ARN del role para desarrolladores AY"
            });

            new CfnOutput(this, "HelloWorldFunctionName", new CfnOutputProps
            {
                Value = helloWorldFunction.FunctionName,
                Description = "Nombre de la función Lambda Hello World"
            });

            new CfnOutput(this, "HelloWorldFunctionArn", new CfnOutputProps
            {
                Value = helloWorldFunction.FunctionArn,
                Description = "ARN de la función Lambda Hello World"
            });

            new CfnOutput(this, "HelloWorldFunctionUrl", new CfnOutputProps
            {
                Value = $"https://console.aws.amazon.com/lambda/home?region={Region}#/functions/{helloWorldFunction.FunctionName}",
                Description = "URL para invocar la función Lambda desde la consola"
            });
        }
    }
}

