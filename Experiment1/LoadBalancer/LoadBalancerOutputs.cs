using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Automation;

namespace Experiment1.LoadBalancer
{
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