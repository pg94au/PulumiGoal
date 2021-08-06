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
            var stack1 = await Stack1.PrepareAsync();

            // Destroy the second stack first
            var outputs = await stack1.GetOutputsAsync();

            var fooSgLoadBalancerId = (string)outputs["FooSgLoadBalancerId"].Value;
            var fooSubnet1aId = (string)outputs["FooSubnet1aId"].Value;
            var fooSubnet1bId = (string)outputs["FooSubnet1bId"].Value;
            var fooLbTargetGroupArn = (string)outputs["FooLbTargetGroupArn"].Value;

            var stack2 = await Stack2.PrepareAsync(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn);

            await stack2.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });

            // Destroy the first stack second
            await stack1.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
        }

        private static async Task Up()
        {
            var stack1 = await Stack1.PrepareAsync();

            var result = await stack1.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

            ReportOnUpdateSummary(result.Summary);

            var fooVpcId = (string)result.Outputs["FooVpcId"].Value;
            var fooSgLoadBalancerId = (string)result.Outputs["FooSgLoadBalancerId"].Value;
            var fooSubnet1aId = (string)result.Outputs["FooSubnet1aId"].Value;
            var fooSubnet1bId = (string)result.Outputs["FooSubnet1bId"].Value;
            var fooLbTargetGroupArn = (string)result.Outputs["FooLbTargetGroupArn"].Value;

            var stack2 = await Stack2.PrepareAsync(fooSgLoadBalancerId, fooSubnet1aId, fooSubnet1bId, fooLbTargetGroupArn);

            var result2 = await stack2.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

            ReportOnUpdateSummary(result2.Summary);

            var loadBalancerDns = result2.Outputs["LoadBalancerDns"].Value;

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
