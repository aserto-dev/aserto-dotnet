# Change Log

## [Version 0.9.0-preview.1](https://github.com/aserto-dev/aserto-dotnet/tree/v0.9.0-preview.1) (2022-10-13)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.7...v0.9.0-preview.1)

**Changed**
- Bump .NET version to 6
- The middleware now uses the Authorizer V2 APIs
- Improved detection for reserved route keys
- The following configs are no longet mandatory:
  - AuthorizerApiKey
  - TenantID

**Removed**
- Removed `PolicyRoot` config and parameter from mappers
- Removed `PolicyID` config parameter

**Added**
- Added `PolicyName` and `PolicyInstanceLabel` optional parameters

## [Version 0.8.7](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.7) (2022-09-14)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.6...v0.8.7)

**Changed**
- Expose default Policy and Resource mapper.

## [Version 0.8.6](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.6) (2022-06-27)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.5...v0.8.6)

**Changed**
- No longer lowercase the module path when computing the policy path.

## [Version 0.8.5](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.5) (2022-06-06)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.4...v0.8.5)

**Changed**
- The default policy mapper now uses the RouteEndpoint if it exists.


## [Version 0.8.4](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.4) (2022-06-01)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.3...v0.8.4)

**Changed**
- The default policy mapper now checks if any endpoint exists. If it exists it constructs the policy path based on it.


## [Version 0.8.3](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.3) (2022-06-01)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.2...v0.8.3)

**Bugfix**
- The default policy context path mapper now correctly handles casing for route values.

## [Version 0.8.2](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.2) (2022-05-27)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.1...v0.8.2)

**Changed**
- The middleware ships with a default resource context mapper


## [Version 0.8.1](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.1) (2022-05-17)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.8.0...v0.8.1)

**Changed**
- The default policy url to policy mapper is able to map urls that contain colons
- Added the ability to pass a function to map resources is authorization requests


## [Version 0.8.0](https://github.com/aserto-dev/aserto-dotnet/tree/v0.8.0) (2022-02-09)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.1.2...v0.8.0)

**Changed**
- Update Aserto.Authorizer.Client.Grpc to 0.8.0

## [Version 0.1.2](https://github.com/aserto-dev/aserto-dotnet/tree/v0.1.2) (2021-09-20)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.1.1...v0.1.2)

**Changed**
- Update Aserto.Authorizer.Client.Grpc to 0.1.1
- Update 3rd party dependencies
- Integrate with code climate

## [Version 0.1.1](https://github.com/aserto-dev/aserto-dotnet/tree/v0.1.1) (2021-09-03)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.1.0...v0.1.1)

**Changed**
- Added the ability to provide a custom url to Aserto policy mapper
- The default mapper now converts any character that is not alpha, digit, dot or underscore to underscore
- Removed references to IdentityCodext.Mode

## [Version 0.1.0](https://github.com/aserto-dev/aserto-dotnet/tree/v0.1.0) (2021-08-26)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.6...v0.1.0)

**Changed**
- Update Aserto.Authorizer.Client.Grpc to v0.1.0 

## [Version 0.0.6](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.6) (2021-08-11)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.5...v0.0.6)

**Changed**
- Fix for release pipepline

## [Version 0.0.5](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.5) (2021-08-11)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.4...v0.0.5)

**Changed**
- Set the project license to Apache-2.0
- Use Aserto.Authorizer.Client.Grpc instead of Aserto.Authorizer.Grpc
- Use ItentityContext.Type for setting the Aserto Identity Context

## [Version 0.0.4](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.4) (2021-08-05)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.3...v0.0.4)

**Changed**
- Use the Authorization header instead of Aserto-Api-Key when making calls to the Authorizer

## [Version 0.0.3](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.3) (2021-08-02)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.2...v0.0.3)

**Changed**
- Use Aserto.Authorizer.Grpc package instead of Aserto.Protobuf 

## [Version 0.0.2](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.2) (2021-08-02)
[Full Changelog](https://github.com/aserto-dev/aserto-dotnet/compare/v0.0.1...v0.0.2)

**Changed**
- Updated the release pipeline for pushing to NuGet.
- Add CHANGELOG.md to the solution.
- Bumped Aserto.Protobuf to 0.0.50

## [Version 0.0.1](https://github.com/aserto-dev/aserto-dotnet/tree/v0.0.1) (2021-08-02)

Initial release