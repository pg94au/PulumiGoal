using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi.Automation;
using Pulumi.Aws.AutoScaling;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;

namespace Experiment1.WebApplication
{
    public class WebApplicationProgram
    {
        public static PulumiFn Create(string fooVpcId, string fooSubnet1aId, string fooSgLoadBalancerId, string fooLbTargetGroupArn)
        {
            var program = PulumiFn.Create(() =>
            {
                var fooSgAllowSshFromHome = new SecurityGroup("FooSgAllowSshFromHome", new SecurityGroupArgs
                {
                    Name = "FooSgAllowSshFromHome",
                    Description = "Allow SSH from home IP",
                    VpcId = fooVpcId,
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


                var fooSgWebServer = new SecurityGroup("FooSgWebServer", new SecurityGroupArgs
                {
                    Name = "FooSgWebServer",
                    Description = "Security group tailored to the web server requirements",
                    VpcId = fooVpcId
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
                    SecurityGroupId = fooSgLoadBalancerId
                });

                var fooSgRuleWwwToWebServers = new SecurityGroupRule("FooSgRuleWwwToWebServers", new SecurityGroupRuleArgs
                {
                    Type = "egress",
                    Description = "Allow the load balancer to reach internal web servers",
                    FromPort = 80,
                    ToPort = 80,
                    Protocol = "tcp",
                    SourceSecurityGroupId = fooSgWebServer.Id,
                    SecurityGroupId = fooSgLoadBalancerId
                });

                var fooSgRuleWwwFromLoadBalancer = new SecurityGroupRule("FooSgRuleWwwFromLoadBalancer", new SecurityGroupRuleArgs
                {
                    Type = "ingress",
                    Description = "Allow the load balancer to reach internal web servers",
                    FromPort = 80,
                    ToPort = 80,
                    Protocol = "tcp",
                    SourceSecurityGroupId = fooSgLoadBalancerId,
                    SecurityGroupId = fooSgWebServer.Id
                });

                var fooLaunchTemplate = new LaunchTemplate("FooLaunchTemplateWebServer", new LaunchTemplateArgs
                {
                    Name = "FooLaunchTemplateWebServer",
                    UpdateDefaultVersion = true,
                    ImageId = "ami-0a09ff033117a19ea",
                    //ImageId = "ami-0a91af017a93f5740",
                    InstanceType = "t2.nano",
                    KeyName = "MyKeyPair",
                    NetworkInterfaces =
                    {
                        new LaunchTemplateNetworkInterfaceArgs
                        {
                            AssociatePublicIpAddress = "true",
                            SubnetId = fooSubnet1aId, // TODO: Should this  be specified here?
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



                var fooAutoscalingGroup = new Group("FooAutoScalingGroupWebServer", new GroupArgs
                {
                    //AvailabilityZones = { "ca-central-1a", "ca-central-1b" }, // TODO: This should be an output from the infrastructure stack
                    AvailabilityZones = { "ca-central-1a" }, // TODO: This should be an output from the infrastructure stack
                    Name = "FooAutoScalingGroupWebServer",
                    DesiredCapacity = 2,
                    MaxSize = 2,
                    MinSize = 2,
                    LaunchTemplate = new Pulumi.Aws.AutoScaling.Inputs.GroupLaunchTemplateArgs
                    {
                        Id = fooLaunchTemplate.Id,
                        Version = "$Latest"
                    },
                    TargetGroupArns = { fooLbTargetGroupArn },
                });
                //new Tag("FooAutoScalingGroupWebServerTag", new TagArgs
                //{
                //    Key = "Name",
                //    Value = "FooAutoScalingGroupWebServer",
                //    ResourceId = fooAutoscalingGroup.Id
                //});


                return WebApplicationOutputs.ToDictionary();
            });

            return program;
        }
    }

    public class WebApplicationOutputs
    {
        public WebApplicationOutputs(IImmutableDictionary<string, OutputValue> outputs)
        {
        }

        public static IDictionary<string, object?> ToDictionary(
            )
        {
            return new Dictionary<string, object?>
            {
            };
        }
    }
}