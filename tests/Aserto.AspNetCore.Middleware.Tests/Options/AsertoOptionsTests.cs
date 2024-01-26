using Aserto.AspNetCore.Middleware.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Aserto.AspNetCore.Middleware.Tests.Options
{
    public class AsertoOptionsTests
    {
        private AsertoOptions getValidOptions()
        {
            var options = new AsertoOptions();
            options.PolicyName = "YOUR_POLICY_NAME";
            options.PolicyRoot = "policyRoot";
            return options;
        }

        [Fact]
        public void NoPolicyNameAllows()
        {
            var options = getValidOptions();
            options.PolicyName = string.Empty;

            Assert.True(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NullOptionsThrow()
        {
            AsertoOptions options = null;

            Assert.Throws<ArgumentNullException>(() => AsertoOptions.Validate(options));
        }
    }
}
