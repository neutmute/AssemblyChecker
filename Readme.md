# AssemblyChecker

Enumerates a folder of assemblies to list name, version, CLR target.
Useful for checking the assemblies in your deployment are of expected version / CLR target.

## Sample Usage

### Help

    .\AssemblyChecker.exe /?

### Recurse for an  assembly pattern only including a specific path

    .\AssemblyChecker.exe C:\Code -p="myAssembly.dll" -i="\bin\debug" -r

## Sample output

  Folder=., Assembly=*.* |

  Found 5 assembly files in . |

    Name                 Product                   Version  File Version  Informational Version          Configuration           CLR         Path
    -------------------  ------------------------  -------  ------------  -----------------------------  ----------------------  ----------  ------------------------------------------------------------
    AssemblyChecker      Assembly Checker          1.0.0.0  1.0.0.0       1.0.0 master                   debug                   v4.0.30319  C:\CodeMine\AssemblyChecker\Source\AssemblyChecker\bin\Debug
    Common.Logging       Common Logging Framework  3.3.1.0  3.3.1.0       3.3.1; net-4.5.win32; release  net-4.5.win32; release  v4.0.30319  C:\CodeMine\AssemblyChecker\Source\AssemblyChecker\bin\Debug
    Common.Logging.Core  Common Logging Framework  3.3.1.0  3.3.1.0       3.3.1; portable; release       portable; release       v4.0.30319  C:\CodeMine\AssemblyChecker\Source\AssemblyChecker\bin\Debug
    Kraken.Core          Kraken.Core (Release)     4.5.0.0  4.5.0.0       4.5.0.107                      Release                 v4.0.30319  C:\CodeMine\AssemblyChecker\Source\AssemblyChecker\bin\Debug
    NLog                 NLog v4.4.3               4.0.0.0  4.4.4534      4.4.3                                                  v4.0.30319  C:\CodeMine\AssemblyChecker\Source\AssemblyChecker\bin\Debug
  |

  Finished |
