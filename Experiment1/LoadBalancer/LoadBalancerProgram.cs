using Pulumi;
using Pulumi.Automation;
using Pulumi.Aws.Alb;
using Pulumi.Aws.Alb.Inputs;

namespace Experiment1.LoadBalancer
{
    public class LoadBalancerProgram
    {
        public static PulumiFn Create(string fooSgLoadBalancerId, string fooSubnet1aId, string fooSubnet1bId, string fooLbTargetGroupArn, bool inService)
        {
            var program = PulumiFn.Create(() =>
            {
                var fooLbWebServer = new Pulumi.Aws.Alb.LoadBalancer("FooLbWebServer", new LoadBalancerArgs
                {
                    Name = "FooLbWebServer",
                    Internal = false,
                    LoadBalancerType = "application",
                    SecurityGroups = { fooSgLoadBalancerId },
                    Subnets = { fooSubnet1aId, fooSubnet1bId },
                    EnableDeletionProtection = false,
                    Tags = { { "Name", "FooLbWebServer" } }
                });

                var fooLbListener = new Listener("FooLbWebServerListener", new ListenerArgs
                {
                    LoadBalancerArn = fooLbWebServer.Arn,
                    Port = 80,
                    Protocol = "HTTP",
                    DefaultActions = {
                        inService
                            ? new ListenerDefaultActionArgs
                            {
                                Type = "forward",
                                TargetGroupArn = fooLbTargetGroupArn,
                            }
                            : new ListenerDefaultActionArgs
                            {
                                Type = "fixed-response",
                                FixedResponse = new ListenerDefaultActionFixedResponseArgs
                                {
                                    ContentType = "text/plain",
                                    StatusCode = "200",
                                    MessageBody = "Upgrade in progress..."
                                }
                            },
                    },
                    Tags = { { "Name", "FooLbWebServerListener" } }
                });

                return LoadBalancerOutputs.ToDictionary(Output.Create(true), fooLbWebServer.DnsName);
            });

            return program;
        }
    }
}