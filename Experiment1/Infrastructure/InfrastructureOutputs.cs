using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Automation;

namespace Experiment1.Infrastructure
{
    public class InfrastructureOutputs
    {
        public string VpcId { get; }
        public string LoadBalancerId { get; }
        public string Subnet1aId { get; }
        public string Subnet1bId { get; }
        public string LoadBalancerTargetGroupArn { get; }

        public InfrastructureOutputs(IImmutableDictionary<string, OutputValue> outputs)
        {
            VpcId = (string)outputs[nameof(VpcId)].Value;
            LoadBalancerId = (string)outputs[nameof(LoadBalancerId)].Value;
            Subnet1aId = (string)outputs[nameof(Subnet1aId)].Value;
            Subnet1bId = (string)outputs[nameof(Subnet1bId)].Value;
            LoadBalancerTargetGroupArn = (string)outputs[nameof(LoadBalancerTargetGroupArn)].Value;
        }

        public static IDictionary<string, object?> ToDictionary(
            Output<string> vpcId,
            Output<string> loadBalancerId,
            Output<string> subnet1aId,
            Output<string> subnet1bId,
            Output<string> loadBalancerTargetGroupArn
        )
        {
            return new Dictionary<string, object?>
            {
                [nameof(VpcId)] = vpcId,
                [nameof(LoadBalancerId)] = loadBalancerId,
                [nameof(Subnet1aId)] = subnet1aId,
                [nameof(Subnet1bId)] = subnet1bId,
                [nameof(LoadBalancerTargetGroupArn)] = loadBalancerTargetGroupArn
            };
        }
    }
}