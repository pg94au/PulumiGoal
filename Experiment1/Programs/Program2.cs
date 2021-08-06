using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Ec2;

namespace Experiment1.Programs
{
    public class Program2
    {
        public static PulumiFn Create(string fooVpcId)
        {
            var program = PulumiFn.Create(() =>
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

            return program;
        }
    }
}