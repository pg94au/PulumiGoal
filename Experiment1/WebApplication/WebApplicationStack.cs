using System;
using System.Threading.Tasks;
using Pulumi.Automation;

namespace Experiment1.WebApplication
{
    public class WebApplicationStack
    {
        public static async Task<WorkspaceStack> PrepareAsync(string fooVpcId, string fooSubnet1aId, string fooSubnet1bId, string fooSgLoadBalancerId, string fooLbTargetGroupArn, bool inService)
        {
            var program1 = WebApplicationProgram.Create(fooVpcId, fooSubnet1aId, fooSubnet1bId, fooSgLoadBalancerId, fooLbTargetGroupArn, inService);

            var stack = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-webApplication", program1)
            );

            await stack.Workspace.InstallPluginAsync("aws", "v4.15.0");
            await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            return stack;
        }
    }
}
