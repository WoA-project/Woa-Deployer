﻿using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Deployer.Tasks
{
    [TaskDescription("Displaying Markdown document")]
    public class DisplayMarkdownMessage : DeploymentTask
    {
        private readonly string message;

        public DisplayMarkdownMessage(string message, IDeploymentContext deploymentContext,
            IFileSystemOperations fileSystemOperations) : base(deploymentContext, fileSystemOperations)
        {
            this.message = message;
        }

        protected override Task ExecuteCore()
        {
            Log.Information(message);
            return Task.CompletedTask;
        }
    }
}