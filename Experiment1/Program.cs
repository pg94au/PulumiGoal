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

        var program2 = CreateProgram2();

        var destroy = args.Any() && args[0] == "destroy";
        var projectName = "Experiment1";
        var stackName = "experiment1";

        var stackArgs = new InlineProgramArgs(projectName, stackName, program1);
        var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

        await stack.Workspace.InstallPluginAsync("aws", "v4.0.0");

        await stack.SetConfigAsync("aws:region", new ConfigValue("ca-central-1"));

        await stack.RefreshAsync(new RefreshOptions {OnStandardOutput = Console.WriteLine});

        if (destroy)
        {
            await stack.DestroyAsync(new DestroyOptions {OnStandardOutput = Console.WriteLine});
        }
        else
        {
            var result = await stack.UpAsync(new UpOptions {OnStandardOutput = Console.WriteLine});

            if (result.Summary.ResourceChanges != null)
            {
                foreach (var change in result.Summary.ResourceChanges)
                {
                    Console.WriteLine($"{change.Key}: {change.Value}");
                }
            }

            Console.WriteLine($"FooVpcId: {result.Outputs["FooVpcId"].Value}");
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

    private static PulumiFn CreateProgram2()
    {
        var program2 = PulumiFn.Create(() =>
        {
            //var pInternateGateway = new InternetGateway("p-internet-gateway", new InternetGatewayArgs
            //{
            //    VpcId = pVpc.Id,
            //});
            //new Tag("InternetGatewayName", new TagArgs
            //{
            //    Key = "Name",
            //    Value = "p-Internet-Gateway",
            //    ResourceId = pInternateGateway.Id
            //});

            return new Dictionary<string, object?>()
            {
            };
        });

        return program2;
    }
}
