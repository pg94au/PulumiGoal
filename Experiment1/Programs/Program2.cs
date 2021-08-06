using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Alb;

namespace Experiment1.Programs
{
    public class Program2
    {
        public static PulumiFn Create(string fooVpcId)
        {
            var program = PulumiFn.Create(() =>
            {
                var fooLbTargetGroup = new TargetGroup("FooLbTargetGroupWebServer", new TargetGroupArgs
                {
                    Name = "FooLbTargetGroupWebServer",
                    Port = 80,
                    Protocol = "HTTP",
                    VpcId = fooVpcId,
                    Tags = { { "Name", "FooLbTargetGroupWebServer" } }
                });


                return new Dictionary<string, object?>()
                {
                };
            });

            return program;
        }
    }
}