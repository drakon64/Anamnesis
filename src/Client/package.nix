{
  lib,
  python3Packages,
}:
let
  fs = lib.fileset;
in
python3Packages.buildPythonApplication {
  pname = "anamnesis";
  version = (builtins.fromTOML (builtins.readFile ./pyproject.toml)).project.version;
  pyproject = true;

  src = fs.toSource {
    root = ./.;
    fileset = fs.difference (./.) (fs.unions [ ./package.nix ]);
  };

  build-system = with python3Packages; [ hatchling ];

  dependencies = with python3Packages; [
    google-cloud-firestore
    google-cloud-storage
  ];

  meta = {
    license = lib.licenses.eupl12;
    mainProgram = "anamnesis";
    maintainers = with lib.maintainers; [ drakon64 ];
  };
}
