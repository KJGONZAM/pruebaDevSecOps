using Amazon.CDK;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;
using Constructs;
using System.Collections.Generic;

namespace PipelineStack
{
    public class PipelineStack : Stack
    {
        public PipelineStack(Constructs.Construct scope, string id, IStackProps props = null) 
            : base(scope, id, props)
        {
            // 1. Bucket S3 para artefactos del pipeline
            var artifactsBucket = new Bucket(this, "PipelineArtifactsBucket", new BucketProps
            {
                BucketName = $"devsecops-pipeline-artifacts-{Account}-{Region?.Replace("-", "")}",
                Versioned = true,
                Encryption = BucketEncryption.S3_MANAGED,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL
            });

            // 2. Role para CodeBuild con permisos necesarios
            var codeBuildRole = new Role(this, "CodeBuildRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                Description = "Role para CodeBuild con permisos de seguridad y despliegue"
            });

            // Permisos para CodeBuild
            codeBuildRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "logs:CreateLogGroup",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents",
                    "s3:GetObject",
                    "s3:PutObject",
                    "s3:GetBucketLocation",
                    "s3:ListBucket",
                    "codebuild:CreateReport",
                    "codebuild:UpdateReport",
                    "codebuild:BatchPutTestCases",
                    "sts:AssumeRole"
                },
                Resources = new[] { "*" }
            }));

            // Permisos para CDK deploy
            codeBuildRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "cloudformation:*",
                    "ssm:GetParameter",
                    "ssm:GetParameters",
                    "ssm:GetParameterHistory",
                    "ssm:GetParametersByPath",
                    "ec2:*",
                    "s3:*",
                    "iam:*",
                    "logs:*",
                    "codebuild:*",
                    "codepipeline:*"
                },
                Resources = new[] { "*" }
            }));

            // 3. Proyecto CodeBuild con escaneos de seguridad
            var buildProject = new Project(this, "SecurityBuildProject", new ProjectProps
            {
                ProjectName = "DevSecOps-Security-Build",
                Role = codeBuildRole,
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.STANDARD_3_0, // Versiones más recientes no disponibles en alpha
                    ComputeType = ComputeType.SMALL,
                    Privileged = true // Necesario para Docker si se usa
                },
                BuildSpec = BuildSpec.FromSourceFilename("pipeline/buildspec.yml"),
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    ["AWS_DEFAULT_REGION"] = new BuildEnvironmentVariable
                    {
                        Value = Region
                    },
                    ["AWS_ACCOUNT_ID"] = new BuildEnvironmentVariable
                    {
                        Value = Account
                    }
                }
            });

            // 4. Pipeline CI/CD
            var pipeline = new Pipeline(this, "DevSecOpsPipeline", new PipelineProps
            {
                PipelineName = "DevSecOps-Security-Pipeline",
                ArtifactBucket = artifactsBucket,
                RestartExecutionOnUpdate = true
            });

            var sourceOutput = new Artifact_("SourceOutput");
            var sourceAction = new S3SourceAction(new S3SourceActionProps
            {
                ActionName = "Source",
                Bucket = artifactsBucket,
                BucketKey = "source.zip",
                Output = sourceOutput
            });

            // Stage 2: Build y Security Scans
            var buildOutput = new Artifact_("BuildOutput");
            
            var buildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                ActionName = "BuildAndSecurityScans",
                Project = buildProject,
                Input = sourceOutput,
                Outputs = new[] { buildOutput },
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    ["SOURCE_VERSION"] = new BuildEnvironmentVariable
                    {
                        Value = sourceAction.Variables.VersionId
                    }
                }
            });

            // Agregar stages al pipeline
            pipeline.AddStage(new StageOptions
            {
                StageName = "Source",
                Actions = new[] { sourceAction }
            });

            pipeline.AddStage(new StageOptions
            {
                StageName = "BuildAndSecurity",
                Actions = new[] { buildAction }
            });

            // 5. Outputs
            new CfnOutput(this, "PipelineName", new CfnOutputProps
            {
                Value = pipeline.PipelineName,
                Description = "Nombre del pipeline CI/CD"
            });

            new CfnOutput(this, "BuildProjectName", new CfnOutputProps
            {
                Value = buildProject.ProjectName,
                Description = "Nombre del proyecto CodeBuild"
            });

            new CfnOutput(this, "ArtifactsBucket", new CfnOutputProps
            {
                Value = artifactsBucket.BucketName,
                Description = "Bucket S3 para artefactos del pipeline"
            });
        }
    }
}

