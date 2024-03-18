// See https://aka.ms/new-console-template for more information

using Aserto.AspNetCore.Middleware.Clients.Directory.V3;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Directory.Exporter.V3;
using Aserto.Directory.Importer.V3;
using Aserto.Directory.Model.V3;
using Aserto.Directory.Reader.V3;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers.Text;
using System.Linq.Expressions;
using System.Numerics;

var logggerFactory = new NullLoggerFactory();


var builder = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false);

IConfiguration config = builder.Build();

var apikey = config.GetSection("Directory:APIKey").Value;
var serviceURL = config.GetSection("Directory:ServiceUrl").Value;
var tenantID = config.GetSection("Directory:TenantID").Value;
var insecure = Convert.ToBoolean(config.GetSection("Directory:Insecure").Value);

AsertoDirectoryOptions options = new AsertoDirectoryOptions(apiKey: apikey, tenantID: tenantID, insecure: insecure);
options.DirectoryServiceUrl= serviceURL;

var client = new Aserto.AspNetCore.Middleware.Clients.Directory.V3.Directory(options, logggerFactory);

// Example of get graph call with citadel users with explanation
//var graphResult = await client.GetGraphAsync("user", "rick@the-citadel.com", "manager", "user", "morty@the-citadel.com","", true);
//Console.WriteLine(graphResult.ToString());

// Example get objects async call
var result = await client.GetObjectsAsync("user", 1);

Console.WriteLine(result.ToString());
// Example of get manifest request
var request = new GetManifestRequest();

var manifestget = await client.GetManifestAsync(request);
var manifestBytes = new byte[manifestget.Body.Data.Length];
manifestget.Body.Data.CopyTo(manifestBytes,0);

Console.WriteLine(System.Text.Encoding.UTF8.GetString(manifestBytes));

// Example of set manifest call 
//byte[] manifestContent = File.ReadAllBytes("<path to your manifest file>");

//var setRequest = new SetManifestRequest();
//setRequest.Body = new Body();
//setRequest.Body.Data = ByteString.CopyFrom(manifestContent);
//var manifestSet = await client.SetManifestAsync(setRequest);

// Example of import object
var importRequest = new ImportRequest();
importRequest.Object = new Aserto.Directory.Common.V3.Object() { Id = "testImport", DisplayName = "testImport", Type="user" };

var importResponse = client.ImportAsync(importRequest);
await foreach (var item in importResponse)
{
    Console.WriteLine(item.Object.ToString());
}

// Example of export all data
var exportRequest = new ExportRequest();
exportRequest.Options = ((uint)Aserto.Directory.Exporter.V3.Option.Data) ;
var exportResponse = client.ExportAsync(exportRequest);
await foreach(var item in exportResponse){
    Console.WriteLine(item.Object.ToString());
}

Console.WriteLine("Done!");
