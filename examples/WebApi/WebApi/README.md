## Running the example

In order to run the example you need to just start a server. What we suggest is doing the following:

1. Make sure `web.config` contains your credentials. You can find your credentials in the settings section of your Auth0 Client and in the Aserto Console.
2. Make sure the Aserto Middleware configuration set in the Startup.cs matches the policy name you want to test with. (If you are running with a local Topaz authorizer, the policy instance details will be ignored).
3. Hit F5 to start local web development server.

Go to `http://localhost:44320/api/public` and you'll see the app running :).
To access the private endpoint that is enforced by the Aserto Middleware you need to provide a valid JWT token for your Auth0 configuration and make sure that you Aserto Directory (Topaz Directory) contains the necessary information to resolve your identity. 
