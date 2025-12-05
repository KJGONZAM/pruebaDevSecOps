using Amazon.CDK;
using DevSecOpsStack;
using PipelineStack;
using ObservabilityStack;
using System;

namespace DevSecOpsApp
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            // Obtener Account ID y Region desde variables de entorno o AWS CLI
            var accountId = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");
            var region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION");
            
            // Si no están en variables de entorno, usar valores por defecto
            if (string.IsNullOrEmpty(accountId) || accountId == "123456789012")
            {
                accountId = "696795625614"; // Account ID configurado
            }
            
            if (string.IsNullOrEmpty(region))
            {
                region = "us-east-1"; // Región por defecto
            }
            
            var env = new Amazon.CDK.Environment
            {
                Account = accountId,
                Region = region
            };

            // Stack de infraestructura base
            new DevSecOpsStack.DevSecOpsStack(app, "DevSecOpsStack", new StackProps
            {
                Env = env
            });

            // Stack de observabilidad
            new ObservabilityStack.ObservabilityStack(app, "ObservabilityStack", new StackProps
            {
                Env = env
            });

            // Stack del pipeline CI/CD
            new PipelineStack.PipelineStack(app, "PipelineStack", new StackProps
            {
                Env = env
            });

            app.Synth();
        }
    }
}
