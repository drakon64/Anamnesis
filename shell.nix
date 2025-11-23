{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
let
  terraform-config-inspect = pkgs.callPackage (import ./lon.nix).terraform-config-inspect-nix { };
in
pkgs.mkShellNoCC {
  packages = with pkgs; [
    dotnetCorePackages.sdk_10_0
    graphviz
    lon
    nixfmt-rfc-style
    opentofu
  ];

  passthru = {
    client = pkgs.mkShellNoCC {
      packages = [
        (pkgs.callPackage ./. { }).client
        terraform-config-inspect
      ];
    };

    lon = pkgs.mkShellNoCC {
      packages = [ pkgs.lon ];
    };

    opentofu = pkgs.mkShellNoCC {
      packages = [ pkgs.opentofu ];
    };
  };
}
