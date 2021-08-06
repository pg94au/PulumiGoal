using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.Automation;
using Pulumi.Aws.Ec2;

class Program
{
    //    static Task<int> Main() => Deployment.RunAsync<MyStack>();

    public static async Task Main(string[] args)
    {
        var program1 = CreateProgram1();

        var destroy = args.Any() && args[0] == "destroy";

        var stack = await LocalWorkspace.CreateOrSelectStackAsync(
            new InlineProgramArgs("Experiment1", "experiment1-a", program1)
            );

        await stack.Workspace.InstallPluginAsync("aws", "v4.0.0");
        await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
        await stack.RefreshAsync(new RefreshOptions {OnStandardOutput = Console.WriteLine});

        if (destroy)
        {
            // Destroy the second stack first
            var outputs = await stack.GetOutputsAsync();
            var program2 = CreateProgram2(outputs["FooVpcId"].Value as string);

            var stack2 = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-b", program2)
            );

            await stack2.Workspace.InstallPluginAsync("aws", "v4.0.0");
            await stack2.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack2.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            await stack2.DestroyAsync(new DestroyOptions {OnStandardOutput = Console.WriteLine});

            // Destroy the first stack second
            await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
        }
        else
        {
            var result = await stack.UpAsync(new UpOptions {OnStandardOutput = Console.WriteLine});

            if (result.Summary?.ResourceChanges != null)
            {
                foreach (var change in result.Summary.ResourceChanges)
                {
                    Console.WriteLine($"{change.Key}: {change.Value}");
                }
            }

            var fooVpcId = result.Outputs["FooVpcId"].Value as string;
            Console.WriteLine($"FooVpcId: {fooVpcId}");

            var program2 = CreateProgram2(fooVpcId);

            var stack2 = await LocalWorkspace.CreateOrSelectStackAsync(
                new InlineProgramArgs("Experiment1", "experiment1-b", program2)
                );

            await stack2.Workspace.InstallPluginAsync("aws", "v4.0.0");
            await stack2.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));
            await stack2.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine });

            var result2 = await stack2.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

            if (result2.Summary?.ResourceChanges != null)
            {
                foreach (var change in result2.Summary.ResourceChanges)
                {
                    Console.WriteLine($"{change.Key}: {change.Value}");
                }
            }
        }
    }

    private static PulumiFn CreateProgram1()
    {
        var program1 = PulumiFn.Create(() =>
        {
            var fooVpc = new Vpc("FooVPC", new VpcArgs()
            {
                CidrBlock = "10.0.0.0/16",
                EnableDnsHostnames = true,
                EnableDnsSupport = true
            });
            new Tag("FooVPCTag", new TagArgs()
            {
                Key = "Name",
                Value = "FooVPC",
                ResourceId = fooVpc.Id
            });

            return new Dictionary<string, object?>()
            {
                ["FooVpcId"] = fooVpc.Id
            };
        });

        return program1;
    }

    private static PulumiFn CreateProgram2(string fooVpcId)
    {
        var program2 = PulumiFn.Create(() =>
        {
            var fooInternateGateway = new InternetGateway("FooInternetGateway", new InternetGatewayArgs
            {
                VpcId = fooVpcId
            });
            new Tag("FooInternetGatewayTag", new TagArgs
            {
                Key = "Name",
                Value = "FooInternetGateway",
                ResourceId = fooInternateGateway.Id
            });

            return new Dictionary<string, object?>()
            {
            };
        });

        return program2;
    }
}
