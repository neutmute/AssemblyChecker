# AssemblyChecker #
Enumerates a folder of assemblies to list name, version, CLR target.
Useful for checking the assemblies in your deployment are of expected version / CLR target.

Example output:
    `
    Found 9 files to enumerate in .
    
    Found 2 assemblies. Proceeding to dump:
    
      Name   Product   Version FileVersion CLR   
      ---------------------  ------------------------------------  --------------  --------------  ----------
      AssemblyCheckerAssemblyChecker   1.0.0.0 1.0.0.0 v4.0.30319
    
      NLog   NLog v2.0.0.2000  2.0.0.0 2.0.0.0 v4.0.30319
    
    Finished
`

