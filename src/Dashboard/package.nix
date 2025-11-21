{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  dockerTools,
  ...
}:
let
  fs = lib.fileset;
in
buildDotnetModule (finalAttrs: {
  pname = "anamnesis-dashboard";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        ./appsettings.Development.json
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./obj)
        ./.gitignore

        ./package.nix
        (lib.fileset.maybeMissing ./deps.json)
      ]
    );
  };

  projectFile = "Anamnesis.Dashboard.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_10_0;

  executables = [ "Anamnesis.Dashboard" ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Anamnesis.Dashboard";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker = dockerTools.buildLayeredImage {
    name = "anamnesis-dashboard";
    tag = "latest";

    config.Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];

    contents = [ dockerTools.caCertificates ];
  };
})
