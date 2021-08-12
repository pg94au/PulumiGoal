using Pulumi.Automation;
using Pulumi.Aws.Alb;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;

namespace Experiment1.Infrastructure
{
    public class InfrastructureProgram
    {
        public static PulumiFn Create()
        {
            var program = PulumiFn.Create(() =>
            {
                // Build the VPC
                var fooVpc = CreateVpc("FooVpc", "10.0.0.0/16");

                // Build the Internet Gateway
                var fooInternetGateway = CreateInternetGateway("FooInternetGateway", fooVpc);

                // Build the Route Table
                var fooRouteTable = CreateRouteTable("FooRouteTable", fooVpc, fooInternetGateway);

                // Build the Subnets
                var fooSubnet1a = CreateSubnet("FooSubnet1a", "10.0.1.0/24", "ca-central-1a", fooVpc, fooRouteTable);
                var fooSubnet1b = CreateSubnet("FooSubnet1b", "10.0.2.0/24", "ca-central-1b", fooVpc, fooRouteTable);

                // TODO: This could be extracted also...
                var fooSgLoadBalancer = new SecurityGroup("FooSgLoadBalancer", new SecurityGroupArgs
                {
                    Name = "FooSgLoadBalancer",
                    Description = "Security group tailored to the load balancer requirements",
                    VpcId = fooVpc.Id,
                    Tags = { { "Name", "FooSgLoadBalancer" } }
                });


                var fooLbTargetGroup = new TargetGroup("FooLbTargetGroupWebServer", new TargetGroupArgs
                {
                    Name = "FooLbTargetGroupWebServer",
                    Port = 80,
                    Protocol = "HTTP",
                    VpcId = fooVpc.Id,
                    Tags = { { "Name", "FooLbTargetGroupWebServer" } }
                });


                return InfrastructureOutputs.ToDictionary(fooVpc.Id, fooSgLoadBalancer.Id, fooSubnet1a.Id, fooSubnet1b.Id, fooLbTargetGroup.Arn);
            });

            return program;
        }

        private static Vpc CreateVpc(string name, string cidrBlock)
        {
            var fooVpc = new Vpc(name, new VpcArgs()
            {
                CidrBlock = cidrBlock,
                EnableDnsHostnames = true,
                EnableDnsSupport = true,
                Tags = { { "Name", name } }
            });

            return fooVpc;
        }

        private static InternetGateway CreateInternetGateway(string name, Vpc fooVpc)
        {
            var fooInternetGateway = new InternetGateway(name, new InternetGatewayArgs
            {
                VpcId = fooVpc.Id,
                Tags = { { "Name", name } }
            });

            return fooInternetGateway;
        }

        private static RouteTable CreateRouteTable(string name, Vpc fooVpc, InternetGateway fooInternetGateway)
        {
            var fooRouteTable = new RouteTable(name, new RouteTableArgs
            {
                VpcId = fooVpc.Id,
                Routes =
                {
                    new RouteTableRouteArgs
                    {
                        CidrBlock = "0.0.0.0/0",
                        GatewayId = fooInternetGateway.Id
                    }
                },
                Tags = { { "Name", name } }
            });

            return fooRouteTable;
        }

        private static Subnet CreateSubnet(string name, string cidrBlock, string availabilityZone, Vpc vpc, RouteTable routeTable)
        {
            var subnet = new Subnet(name, new SubnetArgs
            {
                VpcId = vpc.Id,
                CidrBlock = cidrBlock,
                AvailabilityZone = availabilityZone,
                Tags = { { "Name", name } }
            });

            var routeTableAssociation = new RouteTableAssociation($"{name}Rta", new RouteTableAssociationArgs
            {
                SubnetId = subnet.Id,
                RouteTableId = routeTable.Id,
            });

            return subnet;
        }
    }
}
