using System;
using System.Threading.Tasks;
using Pulumi.Automation;

namespace Experiment1.Infrastructure
{
    public class InfrastructureStack
    {
        public static async Task<WorkspaceStack> PrepareAsync()
        {
            var program = InfrastructureProgram.Create();

            var stack = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-infrastructure", program)
            );

            await stack.Workspace.InstallPluginAsync("aws", "v4.15.0");
            await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            return stack;
        }
    }
}
