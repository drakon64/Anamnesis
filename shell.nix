{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
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
      packages = with pkgs; [
        (callPackage ./. { }).client
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
