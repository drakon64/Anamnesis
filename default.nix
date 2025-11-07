{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
{
  client = pkgs.callPackage ./src/Client/package.nix { };
  server = pkgs.callPackage ./src/Server/package.nix { };
}
