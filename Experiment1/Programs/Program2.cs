using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;

namespace Experiment1.Programs
{
    public class Program2
    {
        public static PulumiFn Create(string fooVpcId)
        {
            var program = PulumiFn.Create(() =>
            {
                var fooSubnet1a = new Subnet("FooSubnet1a", new SubnetArgs
                {
                    VpcId = fooVpcId,
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
                    VpcId = fooVpcId,
                    CidrBlock = "10.0.2.0/24",
                    AvailabilityZone = "ca-central-1a"
                });
                new Tag("FooSubnet1bTag", new TagArgs
                {
                    Key = "Name",
                    Value = "FooSubnet1b",
                    ResourceId = fooSubnet1b.Id
                });


                return new Dictionary<string, object?>()
                {
                };
            });

            return program;
        }
    }
}