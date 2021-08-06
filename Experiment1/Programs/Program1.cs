using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Ec2;

namespace Experiment1.Programs
{
    public class Program1
    {
        public static PulumiFn Create()
        {
            var program = PulumiFn.Create(() =>
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

            return program;
        }
    }
}