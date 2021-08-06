using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;

namespace Experiment1.Programs
{
    public class Program1
    {
        public static PulumiFn Create()
        {
            var program = PulumiFn.Create(() =>
            {
                // Build the VPC
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

                // Build the Internet Gateway
                var fooInternateGateway = new InternetGateway("FooInternetGateway", new InternetGatewayArgs
                {
                    VpcId = fooVpc.Id
                });
                new Tag("FooInternetGatewayTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooInternetGateway",
                    ResourceId = fooInternateGateway.Id
                });

                // Build the Route Table
                var fooRouteTable = new RouteTable("FooRouteTable", new RouteTableArgs
                {
                    VpcId = fooVpc.Id,
                    Routes =
                    {
                        new RouteTableRouteArgs
                        {
                            CidrBlock = "0.0.0.0/0",
                            GatewayId = fooInternateGateway.Id
                        }
                    }
                });
                new Tag("FooRouteTableTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooRouteTable",
                    ResourceId = fooRouteTable.Id
                });


                return new Dictionary<string, object?>()
                {
                    ["FooVpcId"] = fooVpc.Id
                };
            });

            return program;
        }
    }
}