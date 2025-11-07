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
  pname = "anamnesis-server";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        ./appsettings.Development.json
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./config)
        (lib.fileset.maybeMissing ./obj)

        ./package.nix

        ./Dockerfile
      ]
    );
  };

  projectFile = "Anamnesis.Server.csproj";

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = null;

  executables = [ "Anamnesis.Server" ];

  selfContainedBuild = true;

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Anamnesis.Server";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker = dockerTools.buildLayeredImage {
    name = "anamnesis-server";
    tag = "latest";

    config = {
      Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];
    };
  };
})
