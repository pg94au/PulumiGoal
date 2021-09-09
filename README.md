# PulumiGoal
Playing with Pulumi to learn something about it.


## Experiment1
To show that infrastructure can be divided such that it doesn't all need to be affected by every change.  The goal here is to deploy a web application behind a load balancer, then divert the load balancer to an "out of service" state, update the web application (replace it), and finally set the load balancer back to serving up our new application.

To ensure that pulumi plugins are installed:

    pulumi plugin install
<br/>
<br/>

First run creates infrastructure (hit OK to perform each step, and check what is hosted at the load balancer URL that is provided):

    dotnet run
    
This will deploy the infrastructure, the application and the load balancer as separate stacks.
<br/>
<br/>

Second run does the 'upgrade' (you can change the value of ImageId for the LaunchTemplate in WebApplicationProgram first if desired):

    dotnet run
    
This will divert the load balancer to an out of service response, destroy the web application, allow time for a database upgrade, restore the web application, and refocus the load balancer on the web application again.
<br/>
<br/>


To destroy all stacks at the end:

    dotnet run destroy
