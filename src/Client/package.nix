{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  ...
}:
let
  fs = lib.fileset;
in
buildDotnetModule {
  pname = "anamnesis-client";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./config)
        (lib.fileset.maybeMissing ./obj)

        ./package.nix
      ]
    );
  };

  projectFile = "Anamnesis.Client.csproj";

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = null;

  executables = [ "Anamnesis.Client" ];

  selfContainedBuild = true;

  nativeBuildInputs = [ stdenv.cc ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Anamnesis.Client";
    maintainers = with lib.maintainers; [ drakon64 ];
  };
}
