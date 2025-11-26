{
  pkgs,
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  jq,
  terraform-config-inspect,
  ...
}:
let
  fs = lib.fileset;
in
buildDotnetModule (finalAttrs: {
  pname = "anamnesis-client";
  version = builtins.readFile ../../version;

  src = fs.toSource {
    root = ./.;

    fileset = fs.difference (./.) (
      fs.unions [
        (lib.fileset.maybeMissing ./bin)
        (lib.fileset.maybeMissing ./obj)
        ./.gitignore

        ./package.nix
        (lib.fileset.maybeMissing ./deps.json)
      ]
    );
  };

  projectFile = "Anamnesis.Client.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = dotnetCorePackages.runtime_10_0;

  executables = [ "Anamnesis.Client" ];

  postFixup = ''
    wrapProgram $out/bin/Anamnesis.Client --prefix PATH : ${
      lib.makeBinPath [
        jq
        terraform-config-inspect
      ]
    }
  '';

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "Anamnesis.Client";
    maintainers = with lib.maintainers; [ drakon64 ];
  };
})
