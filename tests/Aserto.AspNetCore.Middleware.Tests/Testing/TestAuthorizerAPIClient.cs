using Aserto.AspNetCore.Middleware.Clients;
using Aserto.Authorizer.Authorizer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Aserto.AspNetCore.Middleware.Tests.Testing
{
    public class TestAuthorizerAPIClient : IAuthorizerAPIClient
    {
        private bool allowed = true;
        private IsRequest isRequest = null;
        private string decision = "testdecision";
        private string policyID = "testpolicyID";
        private string policyRoot = "policyRoot";

        public TestAuthorizerAPIClient()
            : this(false)
        {
        }

        public TestAuthorizerAPIClient(bool allowed)
        {
            this.allowed = allowed;
        }

        public TestAuthorizerAPIClient(string policyRoot)
        {
            this.policyRoot = policyRoot;
        }

        public string Decision
        {
            get { return this.decision; }
        }

        public string PolicyID
        {
            get { return this.policyID; }
        }

        public string PolicyRoot
        {
            get { return this.policyRoot; }
        }

        internal IsRequest IsReq
        {
            get { return this.isRequest; }
        }

        public Task<bool> IsAsync(IsRequest isRequest)
        {
            this.isRequest = isRequest;
            return Task.FromResult(this.allowed);
        }
    }
}
