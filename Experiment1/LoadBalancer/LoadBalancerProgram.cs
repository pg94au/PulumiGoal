using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Automation;
using Pulumi.Aws.Alb;
using Pulumi.Aws.Alb.Inputs;
using Pulumi.Aws.Lex.Outputs;

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
                    EnableDeletionProtection = false
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
                    }
                });

                return LoadBalancerOutputs.ToDictionary(Output.Create(true), fooLbWebServer.DnsName);
            });

            return program;
        }
    }

    public class LoadBalancerOutputs
    {
        public bool InService { get; }
        public string LoadBalancerDnsName { get; }

        public LoadBalancerOutputs(IImmutableDictionary<string, OutputValue> outputs)
        {
            InService = (bool)outputs[nameof(InService)].Value;
            LoadBalancerDnsName = (string)outputs[nameof(LoadBalancerDnsName)].Value;
        }

        public static IDictionary<string, object?> ToDictionary(Output<bool> inService, Output<string> loadBalancerDnsName)
        {
            return new Dictionary<string, object?>
            {
                [nameof(InService)] = inService,
                [nameof(LoadBalancerDnsName)] = loadBalancerDnsName
            };
        }
    }
}