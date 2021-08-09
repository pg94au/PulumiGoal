using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Alb;
using Pulumi.Aws.AutoScaling;
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
                var fooInternetGateway = new InternetGateway("FooInternetGateway", new InternetGatewayArgs
                {
                    VpcId = fooVpc.Id
                });
                new Tag("FooInternetGatewayTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooInternetGateway",
                    ResourceId = fooInternetGateway.Id
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
                    }
                });
                new Tag("FooRouteTableTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooRouteTable",
                    ResourceId = fooRouteTable.Id
                });

                var fooSubnet1a = new Subnet("FooSubnet1a", new SubnetArgs
                {
                    VpcId = fooVpc.Id,
                    CidrBlock = "10.0.1.0/24",
                    AvailabilityZone = "ca-central-1a"
                });
                new Tag("FooSubnet1aTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSubnet1a",
                    ResourceId = fooSubnet1a.Id
                });

                var fooSubnet1b = new Subnet("FooSubnet1b", new SubnetArgs
                {
                    VpcId = fooVpc.Id,
                    CidrBlock = "10.0.2.0/24",
                    AvailabilityZone = "ca-central-1b"
                });
                new Tag("FooSubnet1bTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSubnet1b",
                    ResourceId = fooSubnet1b.Id
                });

                var fooSubnet1aRta = new RouteTableAssociation("FooSubnet1aRta", new RouteTableAssociationArgs
                {
                    SubnetId = fooSubnet1a.Id,
                    RouteTableId = fooRouteTable.Id
                });

                var fooSubnet1bRta = new RouteTableAssociation("FooSubnet1bRta", new RouteTableAssociationArgs
                {
                    SubnetId = fooSubnet1b.Id,
                    RouteTableId = fooRouteTable.Id
                });

                var fooSgAllowSshFromHome = new SecurityGroup("FooSgAllowSshFromHome", new SecurityGroupArgs
                {
                    Name = "FooSgAllowSshFromHome",
                    Description = "Allow SSH from home IP",
                    VpcId = fooVpc.Id,
                    Ingress =
                    {
                        new SecurityGroupIngressArgs
                        {
                            FromPort = 22,
                            ToPort = 22,
                            Protocol = "tcp",
                            CidrBlocks = { "206.248.172.36/32" }
                        }
                    }
                });
                new Tag("FooSgAllowSshFromHomeTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSgAllowSshFromHome",
                    ResourceId = fooSgAllowSshFromHome.Id
                });

                var fooSgLoadBalancer = new SecurityGroup("FooSgLoadBalancer", new SecurityGroupArgs
                {
                    Name = "FooSgLoadBalancer",
                    Description = "Security group tailored to the load balancer requirements",
                    VpcId = fooVpc.Id
                });
                new Tag("FooSgLoadBalancerTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSgLoadBalancer",
                    ResourceId = fooSgLoadBalancer.Id
                });

                var fooSgWebServer = new SecurityGroup("FooSgWebServer", new SecurityGroupArgs
                {
                    Name = "FooSgWebServer",
                    Description = "Security group tailored to the web server requirements",
                    VpcId = fooVpc.Id
                });
                new Tag("FooSgWebServerTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSgWebServer",
                    ResourceId = fooSgWebServer.Id
                });

                var fooSgRuleLoadBalancerAllowWww = new SecurityGroupRule("FooSgRuleLoadBalancerAllowWww", new SecurityGroupRuleArgs
                {
                    Type = "ingress",
                    Description = "Allow incoming WWW to the load balancer from everywhere",
                    FromPort = 80,
                    ToPort = 80,
                    Protocol = "tcp",
                    CidrBlocks = { "0.0.0.0/0" },
                    SecurityGroupId = fooSgLoadBalancer.Id
                });

                var fooSgRuleWwwToWebServers = new SecurityGroupRule("FooSgRuleWwwToWebServers", new SecurityGroupRuleArgs
                {
                    Type = "egress",
                    Description = "Allow the load balancer to reach internal web servers",
                    FromPort = 80,
                    ToPort = 80,
                    Protocol = "tcp",
                    SourceSecurityGroupId = fooSgWebServer.Id,
                    SecurityGroupId = fooSgLoadBalancer.Id
                });

                var fooSgRuleWwwFromLoadBalancer = new SecurityGroupRule("FooSgRuleWwwFromLoadBalancer", new SecurityGroupRuleArgs
                {
                    Type = "ingress",
                    Description = "Allow the load balancer to reach internal web servers",
                    FromPort = 80,
                    ToPort = 80,
                    Protocol = "tcp",
                    SourceSecurityGroupId = fooSgLoadBalancer.Id,
                    SecurityGroupId = fooSgWebServer.Id
                });

                var fooLaunchTemplate = new LaunchTemplate("FooLaunchTemplateWebServer", new LaunchTemplateArgs
                {
                    Name = "FooLaunchTemplateWebServer",
                    UpdateDefaultVersion = true,
                    ImageId = "ami-0a09ff033117a19ea",
                    InstanceType = "t2.nano",
                    KeyName = "MyKeyPair",
                    NetworkInterfaces =
                    {
                        new LaunchTemplateNetworkInterfaceArgs
                        {
                            AssociatePublicIpAddress = "true",
                            SubnetId = fooSubnet1a.Id,
                            SecurityGroups =
                            {
                                fooSgAllowSshFromHome.Id,
                                fooSgWebServer.Id
                            }
                        }
                    },
                    TagSpecifications = new LaunchTemplateTagSpecificationArgs
                    {
                        ResourceType = "instance",
                        Tags = { { "Name", "FooWebServer" } }
                    }
                });

                var fooLbTargetGroup = new TargetGroup("FooLbTargetGroupWebServer", new TargetGroupArgs
                {
                    Name = "FooLbTargetGroupWebServer",
                    Port = 80,
                    Protocol = "HTTP",
                    VpcId = fooVpc.Id,
                    Tags = { { "Name", "FooLbTargetGroupWebServer" } }
                });

                var fooAutoscalingGroup = new Group("FooAutoScalingGroupWebServer", new GroupArgs
                {
                    Name = "FooAutoScalingGroupWebServer",
                    DesiredCapacity = 2,
                    MaxSize = 2,
                    MinSize = 2,
                    LaunchTemplate = new Pulumi.Aws.AutoScaling.Inputs.GroupLaunchTemplateArgs
                    {
                        Id = fooLaunchTemplate.Id,
                        Version = "$Latest"
                    },
                    TargetGroupArns = { fooLbTargetGroup.Arn }
                });
                //new Tag("FooAutoScalingGroupWebServerTag", new TagArgs
                //{
                //    Key = "Name",
                //    Value = "FooAutoScalingGroupWebServer",
                //    ResourceId = fooAutoscalingGroup.Id
                //});


                return new Dictionary<string, object?>
                {
                    ["FooVpcId"] = fooVpc.Id,
                    ["FooSgLoadBalancerId"] = fooSgLoadBalancer.Id,
                    ["FooSubnet1aId"] = fooSubnet1a.Id,
                    ["FooSubnet1bId"] = fooSubnet1b.Id,
                    ["FooLbTargetGroupArn"] = fooLbTargetGroup.Arn
                };
            });

            return program;
        }
    }
}