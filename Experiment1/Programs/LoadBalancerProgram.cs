using System.Collections.Generic;
using Pulumi.Automation;
using Pulumi.Aws.Alb;
using Pulumi.Aws.Alb.Inputs;

namespace Experiment1.Programs
{
    public class LoadBalancerProgram
    {
        public static PulumiFn Create(string fooSgLoadBalancerId, string fooSubnet1aId, string fooSubnet1bId, string fooLbTargetGroupArn)
        {
            var program = PulumiFn.Create(() =>
            {
                var fooLbWebServer = new LoadBalancer("FooLbWebServer", new LoadBalancerArgs
                {
                    Name = "FooLbWebServer",
                    Internal = false,
                    LoadBalancerType = "application",
                    SecurityGroups = { fooSgLoadBalancerId },
                    Subnets = { fooSubnet1aId, fooSubnet1bId },
                    EnableDeletionProtection = false
                });

                var fooLbListener = new Listener("FooLbWebServerListener", new ListenerArgs
                {
                    LoadBalancerArn = fooLbWebServer.Arn,
                    Port = 80,
                    Protocol = "HTTP",
                    DefaultActions = {
                        new ListenerDefaultActionArgs
                        {
                            Type = "forward",
                            TargetGroupArn = fooLbTargetGroupArn
                        }
                    }
                });


                return new Dictionary<string, object?>()
                {
                    ["LoadBalancerDns"] = fooLbWebServer.DnsName
                };
            });

            return program;
        }
    }
}