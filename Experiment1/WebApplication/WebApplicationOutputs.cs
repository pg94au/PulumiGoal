using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi.Automation;

namespace Experiment1.WebApplication
{
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