using System;
using System.Threading.Tasks;
using Experiment1.Programs;
using Pulumi.Automation;

namespace Experiment1.Stacks
{
    public class Stack2
    {
        public static async Task<WorkspaceStack> PrepareAsync(string fooSgLoadBalancerId, string fooSubnet1aId, string fooSubnet1bId, string fooLbTargetGroupArn)
        {
            var program2 = Program2.Create(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn);

            var stack = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-b", program2)
            );

            await stack.Workspace.InstallPluginAsync("aws", "v4.0.0");
            await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            return stack;
        }
    }
}
