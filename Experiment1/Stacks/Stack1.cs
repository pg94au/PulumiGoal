using System;
using System.Threading.Tasks;
using Experiment1.Programs;
using Pulumi.Automation;

namespace Experiment1.Stacks
{
    public class Stack1
    {
        public static async Task<WorkspaceStack> PrepareAsync()
        {
            var program1 = Program1.Create();

            var stack = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-a", program1)
            );

            await stack.Workspace.InstallPluginAsync("aws", "v4.0.0");
            await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            return stack;
        }
    }
}
