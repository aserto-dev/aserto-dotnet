using Aserto.AspNetCore.Middleware.Options;
using Aserto.Directory.Reader.V3;
using Aserto.Directory.Common.V3;
using Aserto.Clients.Options;
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
using static Aserto.Directory.Reader.V3.Reader;
using Aserto.Clients.Directory.V3;

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

            Assert.Throws<ArgumentNullException>(() => new Aserto.Clients.Directory.V3.Directory(null, logggerFactory));
        }

        [Fact]
        public void WrongDirectoryServiceURLThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = new AsertoDirectoryOptions();
            options.DirectoryServiceUrl = "www.someweb.com/path/file";


            Assert.Throws<ArgumentException>(() => new Aserto.Clients.Directory.V3.Directory(options, logggerFactory));
        }

        [Fact]
        public void WrongDirectoryServiceURLFromConfigThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = new AsertoDirectoryOptions();
            _configuration.GetSection("AsertoDirectory").Bind(options);

            Assert.Throws<ArgumentException>(() => new Aserto.Clients.Directory.V3.Directory(options, logggerFactory));
        }

        [Fact]
        [Obsolete]
        async public Task AccesingReaderWithoutAddressThrows()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = new AsertoDirectoryOptions();
            options.DirectoryWriterUrl = "https://localhost:9292";
            var dirClient = new Aserto.Clients.Directory.V3.Directory(options, logggerFactory);
            await Assert.ThrowsAsync<ArgumentException>(() => dirClient.GetObjectAsync("type", "key"));
        }

        [Fact]
        [Obsolete]
        async public Task AccesingReaderWithServiceAddressDoesNotThrow()
        {
            var logggerFactory = new NullLoggerFactory();
            var options = new AsertoDirectoryOptions();
            options.DirectoryWriterUrl = "https://localhost:9292";
            options.DirectoryServiceUrl = "https://localhost:9292";
            options.DirectoryTenantID = "test";            
            var dirClient = new Aserto.Clients.Directory.V3.Directory(options, logggerFactory);
            await Assert.ThrowsAsync<Grpc.Core.RpcException>(() => dirClient.GetObjectAsync("type", "key"));
        }
    }
}
