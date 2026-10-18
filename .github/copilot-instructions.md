# Copilot Instructions: Show Mod Updates

## Mod Overview and Purpose

**Mod Name:** Show Mod Updates  
**Mod Description:** Show Mod Updates is a mod for RimWorld designed to keep players informed about changes to their active mods. Whenever you load your game, the mod checks which mods have been updated since your last saved game. It displays a list of these updated mods, along with links to their Steam Workshop pages, changelogs, or any other proprietary links specified by the mod authors. This feature is especially useful for identifying what might be causing new issues or changes in gameplay following updates.

Additionally, the mod detects updates to RimWorld itself. Users can configure it to check all installed mods—not just those currently enabled—and opt to fetch mod information directly from Steam. The mod also includes a setting that allows players to sort the mod list by update date.

In the unfortunate event of a game crash before the update list can be viewed, the mod logs the details of any changed mods to the game's debug log.

## Key Features and Systems

1. **Mod Update Detection:** Scans and identifies mods that have been updated since the last game save.
2. **Link Display:** Provides links to the Steam Workshop, changelogs, and Discord where applicable.
3. **Local vs. Steam Folder Indicator:** Displays icons to show whether mods are located in the local mod folder or the Steam mod folder.
4. **Settings Control:** Users can choose to check all mods or only enabled ones, fetch mod info from Steam, and sort mod lists by update date.
5. **Crash Log Entries:** Captures mod update information in the log if a crash occurs, ensuring no information is lost.
6. **Localization Support:** Features updates like Chinese translation for broader accessibility.

## Coding Patterns and Conventions

- **C# Structure:** The code is organized into multiple classes, each with a specific responsibility, following SOLID principles.
- **Naming Conventions:** Types and members are clearly named to reflect their functionality (e.g., `ShowModUpdatesMod`, `PopulateLinks`).
- **Modular Code:** Code is divided into logically separated files, each dealing with different aspects of functionality.

## XML Integration

- **Defs Management:** The mod uses an XML file, `UpdateInfo.xml`, located in the Defs directory (`1.6/Defs/UpdateInfo.xml`), to define and load update info-related configurations.
- **XML Schema:** The XML structure adheres to RimWorld's modding standards with custom def types for ease of integration and data management.

## Harmony Patching

- **Patch Files:** The mod uses Harmony for method patching to enhance or alter existing gameplay functionality without direct modification.
- **Postfixes:** Defined in files like `OptionListingUtility_DrawOptionListing_Postfix.cs`, offering a way to extend existing methods in RimWorld after their original code is executed.

## Suggestions for Copilot

- **Code Completion:** Copilot can be used to generate boilerplate C# code and XML templates based on existing coding patterns used in the project. This includes class structures, method signatures, and Harmony patches.
- **Error Handling:** Suggest Copilot to provide code snippets for improved error handling to ensure mod stability.
- **Localization Support:** Utilize Copilot for generating localization keys and templates, facilitating translation features.
- **XML Handling:** Copilot can help in generating well-formed XML files in accordance with RimWorld's schema when adding new definitions or settings.

In summary, the Show Mod Updates mod is a valuable tool for any RimWorld player looking to keep track of mod updates effortlessly, enhancing gameplay stability and customization through transparent mod management.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

