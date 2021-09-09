using System;
using System.Threading.Tasks;
using Pulumi.Automation;

namespace Experiment1.LoadBalancer
{
    public class LoadBalancerStack
    {
        public static async Task<WorkspaceStack> PrepareAsync(string fooSgLoadBalancerId, string fooSubnet1aId, string fooSubnet1bId, string fooLbTargetGroupArn, bool inService)
        {
            var program2 = LoadBalancerProgram.Create(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn, inService);

            var stack = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-loadBalancer", program2)
            );

            await stack.Workspace.InstallPluginAsync("aws", "v4.19.0");
            await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            return stack;
        }
    }
}
