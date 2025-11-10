# About  
This project uses `PKHeX.Core` and PKHeX's `IPlugin` interface to add the ability to view and remove the stored Shiny Pokémon in Legends: ZA save files
This Repository is owned by [@Omni-KingZeno](https://github.com/Omni-KingZeno)<br>

## Building  
This project requires an IDE that supports compiling .NET based code, such as Visual Studio 2022, and the [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

### Regular Builds
Regular builds will usually succeed unless there are changes that are incompatible with the NuGet [PKHeX.Core](https://www.nuget.org/packages/PKHeX.Core) package dependency specified in the `.csproj` files of the projects. 
- Clone the ZAShinyStoragePlugin repository using: `$ git clone https://github.com/Omni-KingZeno/ZAShinyStoragePlugin.git`.
- Right-click on the solution and click `Rebuild`.
- The compiled DLL `ZAShinyStoragePlugin.dll` will be in the `ZAShinyStoragePlugin\bin\Release\net9.0-windows` directory.

## Usage  
To use the plugins:
- Create a folder named `plugins` in the same directory as PKHeX.exe if it doesn't already exist.
- Put the `ZAShinyStoragePlugin.dll` in the `plugins` folder. If you downloaded the plugin from online, you may need to unblock it.
- Start or restart PKHeX.exe.
- The plugin should be available for use in the `Tools` dropdown menu. `Tools > ZA Shiny Storage Manager`

## Contributing
To contribute to the repository, you can submit a pull request to the repository. Try to follow a format similar to the current codebase. All contributions are greatly appreciated!

## Credits
- [@kwsch](https://github.com/kwsch) for providing the IPlugin interface in PKHeX making this plugin possible.
- [@Lusamine](https://github.com/Lusamine) for the research of the Stored Shiny block in Legends: Z-A save files.
  
<div align="center">
  <img src="https://hitscounter.dev/api/hit?url=https%3A%2F%2Fgithub.com%2FOmni-KingZeno%2FZAShinyStoragePlugin&label=VIews&icon=github&color=%236f42c1&message=&style=flat&tz=US%2FEastern">
</div>
