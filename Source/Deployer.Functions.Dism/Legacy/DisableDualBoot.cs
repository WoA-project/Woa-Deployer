﻿using System;
using System.Threading.Tasks;
using Deployer.Core.Scripting;
using Zafiro.Core.FileSystem;

namespace Deployer.Functions.Legacy
{
    public class DisableDualBoot : DeployerFunction
    {
        private readonly Func<int, LumiaDualBootAssistant> dualBootAssistantFactory;

        public DisableDualBoot(Func<int, LumiaDualBootAssistant> dualBootAssistantFactory, IFileSystemOperations fileSystemOperations, IOperationContext operationContext) : base(fileSystemOperations, operationContext)
        {
            this.dualBootAssistantFactory = dualBootAssistantFactory;
        }

        public Task Execute(int diskNumber)
        {
            return dualBootAssistantFactory(diskNumber).ToogleDualBoot(false);
        }
    }
}