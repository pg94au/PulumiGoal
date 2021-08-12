using System;
using System.Linq;
using System.Threading.Tasks;
using Experiment1.Infrastructure;
using Experiment1.LoadBalancer;
using Experiment1.WebApplication;
using Pulumi.Automation;
using Pulumi.Aws.Lex.Outputs;

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
            // Prepare our stacks
            var infrastructureStack = await InfrastructureStack.PrepareAsync();
            var webApplicationStack = await PrepareWebApplicationStackAsync(infrastructureStack, true);
            var loadBalancerStack = await PrepareLoadBalancerStackAsync(infrastructureStack, true);

            // We destroy stacks in the reverse order in which they were brought up...
            await loadBalancerStack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine, OnStandardError = Console.Error.WriteLine });
            await webApplicationStack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine, OnStandardError = Console.Error.WriteLine });
            await infrastructureStack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine, OnStandardError = Console.Error.WriteLine });
        }

        private static async Task Up()
        {
            bool environmentExists = false;

            using (var infrastructureStack = await InfrastructureStack.PrepareAsync())
            {
                if (await infrastructureStack.GetOutputsAsync().ContinueWith(outputs => !outputs.Result.IsEmpty))
                {
                    using (var loadBalancerStack = await PrepareLoadBalancerStackAsync(infrastructureStack, true))
                    {
                        environmentExists = await loadBalancerStack.GetOutputsAsync().ContinueWith(outputs => !outputs.Result.IsEmpty);
                    }
                }
            }

            if (environmentExists)
            {
                await UpgradeExisting();
            }
            else
            {
                await InitialDeployment();
            }
        }

        private static async Task InitialDeployment()
        {
            Console.WriteLine(">>> INITIAL DEPLOYMENT");

            Prompt("DEPLOYING INFRASTRUCTURE");
            var infrastructureStack = await InfrastructureStack.PrepareAsync();
            var infrastructureResult = await infrastructureStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(infrastructureResult.Summary);

            Prompt("DEPLOYING WEB APPLICATION");
            var webApplicationStack = await PrepareWebApplicationStackAsync(infrastructureStack, true);
            var webApplicationResult = await webApplicationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(webApplicationResult.Summary);

            Prompt("DEPLOYING LOAD BALANCER");
            var loadBalancerStack = await PrepareLoadBalancerStackAsync(infrastructureStack, true);
            var loadBalancerResult = await loadBalancerStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(loadBalancerResult.Summary);

            var loadBalancerOutputs = new LoadBalancerOutputs(loadBalancerResult.Outputs);

            Console.WriteLine($"LoadBalancerDnsName => {loadBalancerOutputs.LoadBalancerDnsName}");
            Console.WriteLine($"InService => {loadBalancerOutputs.InService}");
        }

        private static async Task UpgradeExisting()
        {
            Console.WriteLine(">>> UPGRADE");

            using var infrastructureStack = await InfrastructureStack.PrepareAsync();
            
            Prompt("UPGRADING EXISTING INFRASTRUCTURE");
            var infrastructureResult = await infrastructureStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(infrastructureResult.Summary);

            Prompt("UPGRADING EXISTING DEPLOYMENT, WE NEED TO DIVERT THE LOAD BALANCER HERE BEFORE WE START");

            using var divertedLoadBalancerStack = await PrepareLoadBalancerStackAsync(infrastructureStack, false);
            var divertedLoadBalancerStackResult = await divertedLoadBalancerStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(divertedLoadBalancerStackResult.Summary);

            Prompt("SCALING DOWN EXISTING WEB APPLICATION NOW (NOTHING CAN RUN WHILE DATABASE UPDATES)");
            using (var webApplicationDownStack = await PrepareWebApplicationStackAsync(infrastructureStack, false))
            {
                var webApplicationDownResult = await webApplicationDownStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
                ReportOnUpdateSummary(webApplicationDownResult.Summary);
            }

            Prompt("HERE WE CAN RUN DATABASE MIGRATIONS BECAUSE OUR APPLICATION IS GONE");

            Prompt("NOW THAT THE DATABASE IS MIGRATED, WE CAN BRING THE APPLICATION STACK UP AGAIN");
            using (var webApplicationUpStack = await PrepareWebApplicationStackAsync(infrastructureStack, true))
            {
                var webApplicationResult = await webApplicationUpStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
                ReportOnUpdateSummary(webApplicationResult.Summary);
            }

            // TODO: Do we need to wait until all of our instances are actually up and ready before we can set the load balancer back?

            Prompt("UPGRADE OF WEB APPLICATION IS COMPLETE, WE NEED TO SET THE LOAD BALANCER BACK");
            using var loadBalancerStack = await PrepareLoadBalancerStackAsync(infrastructureStack, true);

            var loadBalancerResult = await loadBalancerStack.UpAsync(new UpOptions { OnStandardError = Console.Error.WriteLine, OnStandardOutput = Console.WriteLine });
            ReportOnUpdateSummary(loadBalancerResult.Summary);

            var loadBalancerOutputs = new LoadBalancerOutputs(loadBalancerResult.Outputs);

            Console.WriteLine($"LoadBalancerDnsName => {loadBalancerOutputs.LoadBalancerDnsName}");
            Console.WriteLine($"InService => {loadBalancerOutputs.InService}");
        }

        private static async Task<WorkspaceStack> PrepareWebApplicationStackAsync(WorkspaceStack infrastructureStack, bool inService)
        {
            var outputs = await infrastructureStack.GetOutputsAsync();
            var infrastructureOutputs = new InfrastructureOutputs(outputs);

            var webApplicationStack = await WebApplicationStack.PrepareAsync(
                infrastructureOutputs.VpcId,
                infrastructureOutputs.Subnet1aId,
                infrastructureOutputs.Subnet1bId,
                infrastructureOutputs.LoadBalancerId,
                infrastructureOutputs.LoadBalancerTargetGroupArn,
                inService
                );

            return webApplicationStack;
        }

        private static async Task<WorkspaceStack> PrepareLoadBalancerStackAsync(WorkspaceStack infrastructureStack, bool inService)
        {
            var outputs = await infrastructureStack.GetOutputsAsync();
            var infrastructureOutputs = new InfrastructureOutputs(outputs);

            var loadBalancerStack = await LoadBalancerStack.PrepareAsync(
                infrastructureOutputs.LoadBalancerId,
                infrastructureOutputs.Subnet1aId,
                infrastructureOutputs.Subnet1bId,
                infrastructureOutputs.LoadBalancerTargetGroupArn,
                inService
            );

            return loadBalancerStack;
        }

        private static void ReportOnUpdateSummary(UpdateSummary updateSummary)
        {
            Console.WriteLine("### SUMMARY ###");
            if (updateSummary.ResourceChanges != null)
            {
                foreach (var change in updateSummary.ResourceChanges)
                {
                    Console.WriteLine($"{change.Key}: {change.Value}");
                }
            }
            Console.WriteLine("###");
        }

        private static void Prompt(string message)
        {
            Console.WriteLine($"*** {message} ***");
            for (;;)
            {
                Console.WriteLine("Type OK to continue");
                var entry = Console.ReadLine();
                if (entry == "OK")
                {
                    Console.WriteLine("Starting");
                    break;
                }
            }
        }
    }
}
