using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Directory.Reader.V2;
using Aserto.Directory.Common.V2;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Aserto.Directory.Reader.V2.Reader;
using Aserto.AspNetCore.Middleware.Clients.Directory.V2;

namespace Aserto.AspNetCore.Middleware.Tests.Clients
{
    public class DirectoryApiClientTests
    {
        private readonly IConfiguration _configuration;
        public DirectoryApiClientTests()
        {
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"))
               .AddJsonFile(@"appsettings.json", false, false)
               .AddEnvironmentVariables()
               .Build();
        }

        [Fact]
        public void OptionsNullThrows()
        {
            var logggerFactory = new NullLoggerFactory();

            Assert.Throws<ArgumentNullException>(() => new  DirectoryAPIClient(null, logggerFactory));
        }

        [Fact]
        public void WrongDirectoryServiceURLThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = Microsoft.Extensions.Options.Options.Create(new AsertoDirectoryOptions());
            options.Value.DirectoryServiceUrl = "www.someweb.com/path/file";


            Assert.Throws<ArgumentException>(() => new DirectoryAPIClient(options, logggerFactory));
        }

        [Fact]
        public void WrongDirectoryServiceURLFromConfigThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = new AsertoDirectoryOptions();
            _configuration.GetSection("AsertoDirectory").Bind(options);
            var opt = Microsoft.Extensions.Options.Options.Create(options);

            Assert.Throws<ArgumentException>(() => new DirectoryAPIClient(opt, logggerFactory));
        }

        [Fact]
        async public void AccesingReaderWithoutAddressThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = Microsoft.Extensions.Options.Options.Create(new AsertoDirectoryOptions());
            options.Value.DirectoryWriterUrl = "https://localhost:9292";
            var dirClient = new DirectoryAPIClient(options, logggerFactory);
            await Assert.ThrowsAsync<ArgumentException>(() => dirClient.GetObjectAsync("type", "key"));
        }

        [Fact]
        async public void AccesingReaderWithServiceAddressDoesNotThrow()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = Microsoft.Extensions.Options.Options.Create(new AsertoDirectoryOptions());
            options.Value.DirectoryWriterUrl = "https://localhost:9292";
            options.Value.DirectoryServiceUrl = "https://localhost:9292";
            var dirClient = new DirectoryAPIClient(options, logggerFactory);
            await Assert.ThrowsAsync<Grpc.Core.RpcException>(() => dirClient.GetObjectAsync("type", "key"));
        }
    }
}
