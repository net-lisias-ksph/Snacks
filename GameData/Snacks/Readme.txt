Snacks! Continued

Original mod by: Troy Gruetzmacher (tgruetzm)
Continuation by: Michael Billard (Angel-125)

Snacks was originally published by Troy Gruetzmacher (tgruetzm) in August of 2014. It offered a novel and lightweight solution to life support for those that didn't want the complexity of more sophisticated mods like TAC Life Support. Two years later, the game has advanced and while the original author appears to have moved on, it was time to give Snacks an update.

Features:
- Friendly, lightweight life support system
- Highly configurable to support your play style
- Optional consequences that won't brick your mission

Snacks Continued retains the simplicity of the original mod while adding new options. You can configure things like snacks consumed per meal, meals per day, enable/disable recycling, recycling efficiency, and various penalties for hungry kerbals including reputation loss, fines, and partial loss of vehicle control. You can even enable/disable random snacking if desired. Just like with the stock CommNet, the penalties won't brick your mission. All of these options are found in the Game Difficulty screen. And if you're new to Snacks Continued, please consult the KSPedia.

LICENSE
Source code: The MIT License (MIT)
Snack Tin artwork by SQUAD/Porkjet: CC-BY-NC 3.0
Portions of this codebase are CC-BY-NC 3.0 and derived from Taranis Elsu's Window class.

INSTALLATION
Delete any previous instances in GameData/Snacks
Copy the files in the zip folder over to GameData/Snacks

REVISION HISTORY

1.5.1
- Temporarily disable the partial vessel control penalty.
- Added additional checks for vessels created through rescue contracts; any crew listed as "Unowned" will be ignore.

1.5.0
- ISnacksPenalty now has a RemovePenalty method. Snacks will call this each time kerbals don't miss any meals.
- ISnacksPenalty now has a GameSettingsApplied method. This is called at startup and when the player changes game settings.
- The partial control loss penalty should work now.
- New penalty: kerbals can pass out if they miss too many meals.
- Updated the KSPedia to improve clarity and to add the new penalty option.

New events
onBeforeSnackTime: Called before snacking begins.
onSnackTime: Called after snacking.
onSnackTick: called during fixed update right after updating the vessel snapshot.
onConsumeSnacks: Called right after calculating snack consumption but before applying any penalties. Gives you to the ability to alter the snack consumption.
onKerbalsMissedMeal: Called when a vessel with kerbals have missed a meal.

1.4.5
- Fixed an issue with snack tins not showing up.
- A single kerbal can now consume up to 12 snacks per meal and up to 12 snacks per day.
- By default, a single kerbal consumes 1 snack per meal and 3 meals per day.
- Reduced Soil storage in the Hitchhiker to 200. This only applies to new vessels.
- Reduced Snacks per crew capacity in non-command pods to 200 per crewmember. This only applies to new vessels.
- Added the SnacksForExistingSaves.cfg file to specify number of Snacks per command pod and snacks per non-command pod. These are used when installing Snacks into existing saves for vessels already in flight.
- Added new ISnacksPenalty interface for mods to use when implementing new penalties. One of the options is to always apply the penalty even with random penalties turned off. Of course the implementation can decide to honor random penalties...
- Added a Snacks Trip Planner Excel spreadsheet. You'll find it in the Docs folder. An in-game planner is in the works.

1.4.0

- Adjusted Snack production in the MPL; it was way too high. Ore -> Snacks is now 1:10 with mass conservation. A 1.25m Small Holding Tank (holds 300 Ore) now produce 3,000 Snacks.
- Added display field to Snack Processor that tells you how the max amount of snacks per day that it can produce.
- Moved Snack Tins to the Payload tab.
- Added option to show time remaining in days.
- When kerbals go hungry, added the option to randomly choose one penalty from the enabled penalties, or to apply all enabled penalties.
- Added lab data/experiment data loss as an optional penalty.
- You can now register/unregister your own custom penalties. This is particularly useful for addons to Snacks.
- Cleaned up some KSPedia issues.
- Fixed an issue with adding Snacks to existing saves.
- Fixed an issue with vessels spawned from rescue contracts incuring penalties due to being out of Snacks.

1.3.0

- Snacks now have mass and volume. One unit of Snacks takes up 1 liter and masses 0.001 metric tons. 
- Adjusted the MPL's Snack Processor's Ore to Snacks output to account for mass.
- The Snack Processor's efficiency can be improved by those with the Science skill.
- Added several configurable options to KSP's Game Difficulty screen. 
- If recycling is enabled, then kerbals produce Soil when consuming Snacks. Soil is a 1-liter resource that masses 0.001 metric tons. Apparently Soil was part of tgruetzm's original design...
- If recycling is enabled, then the Hitchhiker can convert Soil into Snacks. You can configure recycling efficiency in the Game Difficulty screen.
- New consequences for missing meals:
  * Pay fines per kerbal
  * Lose partial control of the vessel
- Added three sizes of Snack Tins. They can be switched between storing Snacks, Soil, or both. Models and textures courtesy of SQUAD/Porkjet.
- Added KSPedia entries for Snacks!

1.2.0
- Pre-release for KSP 1.2 pre-release.

1.1.6
- Minor updates to the MM patch to help with customization

1.1.5
- Updated to KSP 1.1.2

1.1.4
- Fixed an issue where snacks weren't provided to non-command crewed parts.
- Rebalananced Snack amounts for non-command modules to 400 per crewmember.
NOTE: This will only apply to new vessels. For existing vessles, temporarily rename patch.cfg to patch.txt,
and rename rebalance.txt to rebalance.cfg. Start your game, load your vessels, and then exit the game and rename
the files back to rebalance.txt and patch.cfg.

1.1.3
- Updated to KSP 1.1.1
- Fixed name in versioning file

1.1.2
- Fixed NREs
- Cleaned up the Module Manager patch. Thanks for the hints, Badsector! :)

1.1.1
- Re-added missing Snack Grinder
- Module Manager patch fixed to add Snacks to parts with up to 16 crewmembers
- Snacks won't be added to parts that already have Snacks
- Added MiniAVC support

1.1
- Updated for KSP 1.1
- Removed the need for the ModuleManager patch to equip crewed pods with Snacks.
