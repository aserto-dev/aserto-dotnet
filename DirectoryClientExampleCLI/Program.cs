// See https://aka.ms/new-console-template for more information

using Aserto.AspNetCore.Middleware.Clients.Directory.V3;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Directory.Model.V3;
using Aserto.Directory.Reader.V3;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers.Text;

var logggerFactory = new NullLoggerFactory();


var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false);

IConfiguration config = builder.Build();

var apikey = config.GetSection("Directory:APIKey").Value;
var serviceURL = config.GetSection("Directory:ServiceUrl").Value;
var tenantID = config.GetSection("Directory:TenantID").Value;
var insecure = Convert.ToBoolean(config.GetSection("Directory:Insecure").Value);

AsertoDirectoryOptions options = new AsertoDirectoryOptions(serviceURL, apikey, tenantID, insecure);

var opt = Microsoft.Extensions.Options.Options.Create(options);

var client = new DirectoryAPIClient(opt, logggerFactory);

var result = await client.GetObjectsAsync("user", 1);

Console.WriteLine(result.ToString());

var request = new GetManifestRequest();

var manifestget = await client.GetManifest(request);
var manifestBytes = new byte[manifestget.Body.Data.Length];
manifestget.Body.Data.CopyTo(manifestBytes,0);

Console.WriteLine(System.Text.Encoding.UTF8.GetString(manifestBytes));

// Example of set manifest call 
//byte[] manifestContent = File.ReadAllBytes("<path to your manifest file>");

//var setRequest = new SetManifestRequest();
//setRequest.Body = new Body();
//setRequest.Body.Data = ByteString.CopyFrom(manifestContent);
//var manifestSet = await client.SetManifest(setRequest);

Console.WriteLine("Done!");
