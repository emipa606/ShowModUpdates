# GitHub Copilot Instructions for Show Mod Updates

## Mod Overview and Purpose

**Show Mod Updates** is a RimWorld mod designed to provide players with information about which mods have been updated since their last game session. Upon startup, it compares the current mod versions against those recorded in your most recently saved game. If you load an older save, the mod will adapt by rescanning based on that save. It also checks for updates to the RimWorld game itself. This utility is particularly helpful for diagnosing issues caused by recent mod updates.

## Key Features and Systems

- **Mod Update Checks**: On game startup, the mod checks which active mods have been updated since your last save. It displays this information in a convenient list with links to Steam, changelogs, or other specified URLs.
- **Source Indication**: Displays icons indicating whether a mod is stored locally or in the Steam folder.
- **Extended Options**: Optional settings to check all installed mods, not just the ones that are currently enabled.
- **Sorting Capability**: Allows sorting of the mod list by update date via mod options.
- **Crash-Safe Logging**: If the game crashes before displaying updates, the mod logs changes in the game log.
- **Online Integration**: Fetch mod information directly from Steam and display additional links like Discord if available.

## Coding Patterns and Conventions

### General Structure

- Follow typical C# coding conventions:
  - Use PascalCase for class names, method names, and properties.
  - Use camelCase for local variables and method arguments.
  - Indent with four spaces.
- Code files align with game functionality, and class documentation is advised where necessary to maintain clarity.

### Class Overview

- **Dialog_ModUpdates**: Manages the display window that shows updated mod information.
- **ModWithUpdateInfo**: Wraps mod metadata and populates additional information like links.
- **ShowModUpdatesMod**: Handles mod initialization and settings.
- **ShowModUpdatesSettings**: Defines and manages user-configurable settings for the mod.

## XML Integration

- XML is primarily used for defining mod settings and translations which allow seamless integration with the game’s options menu.
- Maintain XML documents for settings and localization in the appropriate folders. Example: `Languages/English/Keyed/ShowModUpdates.xml`.

## Harmony Patching

- Use Harmony to apply patches where necessary to integrate with existing game methods without altering base game code.
- Common pattern includes creating postfix classes like `OptionListingUtility_DrawOptionListing_Postfix` and `Widgets_ButtonText_Postfix` for adding additional functionality post execution of game functions.

## Suggestions for Copilot

- Utilize Copilot to streamline repetitive tasks, such as creating new XML keys or generating boilerplate class methods.
- When dealing with Harmony, leverage Copilot to generate additional IL or transpiler patches when needed.
- Copilot can assist in link extraction algorithms to populate URLs more efficiently by generating auxiliary functions.

### Example Harmony Use:

csharp
public static class SavedGameLoaderNow_LoadGameFromSaveFileNow_Postfix
{
    // Use Copilot to generate placeholder patch code
    public static void Postfix(string saveFileName)
    {
        // Example of reading save metadata and triggering mod update check
    }
}


By following these instructions and patterns, you will maintain consistency and functionality in developing the Show Mod Updates mod for RimWorld.
