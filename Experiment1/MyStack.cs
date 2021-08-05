using Pulumi;
using Pulumi.Aws.Ec2;

class MyStack : Stack
{
    public MyStack()
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

        FooVpcId = fooVpc.Id;


        var fooInternetGateway = new InternetGateway("FooInternetGateway", new InternetGatewayArgs()
        {
            VpcId = fooVpc.Id
        });
        new Tag("FooInternetGatewayTag", new TagArgs()
        {
            Key = "Name",
            Value = "FooInternetGateway",
            ResourceId = fooInternetGateway.Id
        });
    }

    [Output]
    public Output<string> FooVpcId { get; set; }
}
