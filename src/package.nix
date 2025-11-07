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
  pname = "anamnesis";
  version = builtins.readFile ../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        ./appsettings.Development.json
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./config)
        (lib.fileset.maybeMissing ./obj)

        ./package.nix
      ]
    );
  };

  projectFile = "Anamnesis.csproj";

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = null;

  executables = [ "Anamnesis" ];

  selfContainedBuild = true;

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Anamnesis";
    maintainers = with lib.maintainers; [ drakon64 ];
  };

  passthru.docker = dockerTools.buildLayeredImage {
    name = "anamnesis";
    tag = "latest";

    config = {
      Entrypoint = [ (lib.getExe finalAttrs.finalPackage) ];
    };
  };
})
