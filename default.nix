{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
{
  server = pkgs.callPackage ./src/Server/package.nix { };
}
