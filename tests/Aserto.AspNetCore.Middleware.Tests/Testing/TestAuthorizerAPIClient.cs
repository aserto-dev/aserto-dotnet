using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Authorizer.V2;
using Google.Protobuf.WellKnownTypes;
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
        private string policyName = "testpolicyName";
        private string policyInstanceLabel = "testpolicyInstanceLabel";

        public TestAuthorizerAPIClient()
            : this(false)
        {
        }

        public TestAuthorizerAPIClient(bool allowed)
        {
            this.allowed = allowed;
        }

        public string Decision
        {
            get { return this.decision; }
        }

        public string PolicyName
        {
            get { return this.policyName; }
        }

        public string PolicyInstanceLabel
        {
            get { return this.policyInstanceLabel; }
        }

        public Func<HttpRequest, string> PolicyPathMapper
        {
            get { return AsertoOptionsDefaults.DefaultPolicyPathMapper; }
        }

        public Func<HttpRequest, Struct> ResourceMapper {
            get { return AsertoOptionsDefaults.DefaultResourceMapper; }
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
