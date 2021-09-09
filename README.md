# PulumiGoal
Playing with Pulumi to learn something about it.


## Experiment1
To show that infrastructure can be divided such that it doesn't all need to be affected by every change.  The goal here is to deploy a web application behind a load balancer, then divert the load balancer to an "out of service" state, update the web application (replace it), and finally set the load balancer back to serving up our new application.

pulumi install plugin  (to ensure that pulumi plugins are installed)

dotnet run  (first run creates infrastructure)

(Hit OK to perform each step, and check what is hosted at the load balancer URL that is provided.)

dotnet run  (second run does the 'upgrade' -- you can change the value of ImageId for the LaunchTemplate in WebApplicationProgram first if desired)

(Hit OK to perform each step, checking that the load balancer was diverted and then that a new application is being hosted when it is returned.)

dotnet run destroy  (to tear everything down)
