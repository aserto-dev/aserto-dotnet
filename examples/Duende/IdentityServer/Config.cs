// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { new ApiScope("api1", "My API") };

        public static IEnumerable<Client> Clients
        {
            get
            {
                var clients = new List<Client>();
                var m2mClient = new Client();
                m2mClient.ClientId = "client";
                //the default client claims prefix is client_type
                m2mClient.ClientClaimsPrefix = "";
                m2mClient.AllowedGrantTypes = GrantTypes.ClientCredentials;
                m2mClient.ClientSecrets.Add(new Secret("secret".Sha256()));
                m2mClient.AllowedScopes.Add("api1");
                m2mClient.Claims.Add(new ClientClaim(ClaimTypes.Email, "mihai.buzgau@aserto.com"));
                clients.Add(m2mClient);

                var mvcClient = new Client();
                mvcClient.ClientId = "mvc";
                mvcClient.ClientSecrets.Add(new Secret("secret".Sha256()));

                mvcClient.AllowedGrantTypes = GrantTypes.Code;

                mvcClient.RedirectUris.Add("https://localhost:5002/signin-oidc");
                mvcClient.PostLogoutRedirectUris.Add("https://localhost:5002/signout-callback-oidc");

                mvcClient.AllowedScopes.Add(IdentityServerConstants.StandardScopes.OpenId);
                mvcClient.AllowedScopes.Add(IdentityServerConstants.StandardScopes.Profile);

                // Allow Email scope
                mvcClient.AllowedScopes.Add(IdentityServerConstants.StandardScopes.Email);
                clients.Add(mvcClient);

                return clients;
            }
        }
    }
}