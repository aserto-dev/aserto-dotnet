using Aserto.AspNetCore.Middleware.Options;
using Aserto.Clients.Authorizer;
using Aserto.Authorizer.V2;
using Aserto.Authorizer.V2.Api;
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
        private string policyRoot = "pr";

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

        public string PolicyName
        {
            get { return this.policyName; }
        }

        public string PolicyInstanceLabel
        {
            get { return this.policyInstanceLabel; }
        }

        public string PolicyRoot
        {
            get { return this.policyRoot; }
        }

        public Func<string, HttpRequest, string> PolicyPathMapper
        {
            get { return AsertoOptionsDefaults.DefaultPolicyPathMapper; }
        }

        public Func<string, HttpRequest, Struct> ResourceMapper {
            get { return AsertoOptionsDefaults.DefaultResourceMapper; }
            set { }
        }

        public Func<ClaimsPrincipal, IEnumerable<string>,IdentityContext> IdentityMapper
        {
            get { return null;  }
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

        public Task<List<Module>> ListPoliciesAsync(ListPoliciesRequest request)
        {
            return Task.FromResult(new List<Module>());
        }

        public Task<Module> GetPolicyAsync(GetPolicyRequest request)
        {
            return Task.FromResult<Module>(null);
        }

        public Task<QueryResponse> QueryAsync(QueryRequest request)
        {
            return Task.FromResult(new QueryResponse());
        }

        public Task<CompileResponse> CompileAsync(CompileRequest request)
        {
            return Task.FromResult(new CompileResponse());
        }

        public Task<DecisionTreeResponse> DecisionTreeAsync(DecisionTreeRequest request)
        {
            return Task.FromResult<DecisionTreeResponse>(new DecisionTreeResponse());
        }
    }
}
