﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
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
                var fooVpc = new Vpc("FooVPC", new VpcArgs()
                {
                    CidrBlock = "10.0.0.0/16",
                    EnableDnsHostnames = true,
                    EnableDnsSupport = true,
                    Tags = { { "Name", "FooVPC" } }
                });

                // Build the Internet Gateway
                var fooInternetGateway = new InternetGateway("FooInternetGateway", new InternetGatewayArgs
                {
                    VpcId = fooVpc.Id,
                    Tags = { { "Name", "FooInternetGateway" } }
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
                            GatewayId = fooInternetGateway.Id
                        }
                    },
                    Tags = { { "Name", "FooRouteTable" }}
                });

                var fooSubnet1a = new Subnet("FooSubnet1a", new SubnetArgs
                {
                    VpcId = fooVpc.Id,
                    CidrBlock = "10.0.1.0/24",
                    AvailabilityZone = "ca-central-1a",
                    Tags = { { "Name", "FooSubnet1a" } }
                });

                var fooSubnet1b = new Subnet("FooSubnet1b", new SubnetArgs
                {
                    VpcId = fooVpc.Id,
                    CidrBlock = "10.0.2.0/24",
                    AvailabilityZone = "ca-central-1b",
                    Tags = { { "Name", "FooSubnet1b" } },
                });

                var fooSubnet1aRta = new RouteTableAssociation("FooSubnet1aRta", new RouteTableAssociationArgs
                {
                    SubnetId = fooSubnet1a.Id,
                    RouteTableId = fooRouteTable.Id,
                });

                var fooSubnet1bRta = new RouteTableAssociation("FooSubnet1bRta", new RouteTableAssociationArgs
                {
                    SubnetId = fooSubnet1b.Id,
                    RouteTableId = fooRouteTable.Id
                });


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
    }

    public class InfrastructureOutputs
    {
        public string VpcId { get; }
        public string LoadBalancerId { get; }
        public string Subnet1aId { get; }
        public string Subnet1bId { get; }
        public string LoadBalancerTargetGroupArn { get; }

        public InfrastructureOutputs(IImmutableDictionary<string, OutputValue> outputs)
        {
            VpcId = (string)outputs[nameof(VpcId)].Value;
            LoadBalancerId = (string)outputs[nameof(LoadBalancerId)].Value;
            Subnet1aId = (string)outputs[nameof(Subnet1aId)].Value;
            Subnet1bId = (string)outputs[nameof(Subnet1bId)].Value;
            LoadBalancerTargetGroupArn = (string)outputs[nameof(LoadBalancerTargetGroupArn)].Value;
        }

        public static IDictionary<string, object?> ToDictionary(
            Output<string> vpcId,
            Output<string> loadBalancerId,
            Output<string> subnet1aId,
            Output<string> subnet1bId,
            Output<string> loadBalancerTargetGroupArn
        )
        {
            return new Dictionary<string, object?>
            {
                [nameof(VpcId)] = vpcId,
                [nameof(LoadBalancerId)] = loadBalancerId,
                [nameof(Subnet1aId)] = subnet1aId,
                [nameof(Subnet1bId)] = subnet1bId,
                [nameof(LoadBalancerTargetGroupArn)] = loadBalancerTargetGroupArn
            };
        }
    }
}
