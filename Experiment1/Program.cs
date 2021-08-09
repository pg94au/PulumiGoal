using System;
using System.Linq;
using System.Threading.Tasks;
using Experiment1.Stacks;
using Pulumi.Automation;

namespace Experiment1
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var destroy = args.Any() && args[0] == "destroy";

            if (destroy)
            {
                await Destroy();
            }
            else
            {
                await Up();
            }
        }

        private static async Task Destroy()
        {
            // We destroy stacks in the reverse order in which they were brought up...
            var webApplicationStack = await WebApplicationStack.PrepareAsync();

            // Destroy the second stack first
            var outputs = await webApplicationStack.GetOutputsAsync();

            var fooSgLoadBalancerId = (string)outputs["FooSgLoadBalancerId"].Value;
            var fooSubnet1aId = (string)outputs["FooSubnet1aId"].Value;
            var fooSubnet1bId = (string)outputs["FooSubnet1bId"].Value;
            var fooLbTargetGroupArn = (string)outputs["FooLbTargetGroupArn"].Value;

            var loadBalancerStack = await LoadBalancerStack.PrepareAsync(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn);

            await loadBalancerStack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });

            // Destroy the first stack second
            await webApplicationStack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
        }

        private static async Task Up()
        {
            var webApplicationStack = await WebApplicationStack.PrepareAsync();

            var webApplicationResult = await webApplicationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

            ReportOnUpdateSummary(webApplicationResult.Summary);

            var fooVpcId = (string)webApplicationResult.Outputs["FooVpcId"].Value;
            var fooSgLoadBalancerId = (string)webApplicationResult.Outputs["FooSgLoadBalancerId"].Value;
            var fooSubnet1aId = (string)webApplicationResult.Outputs["FooSubnet1aId"].Value;
            var fooSubnet1bId = (string)webApplicationResult.Outputs["FooSubnet1bId"].Value;
            var fooLbTargetGroupArn = (string)webApplicationResult.Outputs["FooLbTargetGroupArn"].Value;

            var loadBalancerStack = await LoadBalancerStack.PrepareAsync(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn);

            var loadBalancerResult = await loadBalancerStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

            ReportOnUpdateSummary(loadBalancerResult.Summary);

            var loadBalancerDns = loadBalancerResult.Outputs["LoadBalancerDns"].Value;

            Console.WriteLine($"LoadBalancerDns: {loadBalancerDns}");
        }

        private static void ReportOnUpdateSummary(UpdateSummary updateSummary)
        {
            if (updateSummary.ResourceChanges != null)
            {
                foreach (var change in updateSummary.ResourceChanges)
                {
                    Console.WriteLine($"{change.Key}: {change.Value}");
                }
            }
        }
}
}
