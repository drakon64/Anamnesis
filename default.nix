{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
{
  repository = pkgs.callPackage ./src/Repository/package.nix { };
  dashboard = pkgs.callPackage ./src/Dashboard/package.nix { };

  client = pkgs.callPackage ./src/Client/package.nix {
    terraform-config-inspect = pkgs.callPackage (import ./lon.nix).terraform-config-inspect-nix { };
  };
}
