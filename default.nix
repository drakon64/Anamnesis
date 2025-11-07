{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
pkgs.callPackage ./src/Server/package.nix { }
