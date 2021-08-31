# Snacks Continued :: Change Log

* 2016-1202: 1.6.0 (Angel-125) for KSP 1.2.1
	+ Plugin renamed to SnacksUtils to alleviate issues with ModuleManager.
	+ When kerbals faint due to lack of snacks, you now choose from 1 minute, 5 minutes, 10 minutes, 30 minutes, an hour, 2 hours, or a day.
	+ Snacks now supports 24-hour days in addition to 6-hour days. Snack frequency is calculated accordingly.
* 2016-1118: 1.5.7 (Angel-125) for KSP 1.2.1
	+ Fixed snacks calculations and minor GUI update. Thanks for the patch, bounty123! :)
* 2016-1109: 1.5.6 (Angel-125) for KSP 1.2.1
	+ Fixed NRE's that happen in the editor (VAB/SPH)
	+ Snacking frequency is correctly calculated now.
	+ Updated to KSP 1.2.1
	+ Added recyclers to the Mk3 shuttle cockpit and the Mk2 crew cabin.
* 2016-1102: 1.5.5 (Angel-125) for KSP 1.2
	+ Fixed some NREs.
	+ Fixed a situation where the ModuleManager patch wasn't adding snacks to crewed parts; Snacks can now dynamically add them when adding parts to vessels in the VAB/SPH.
* 2016-1023: 1.5.3 (Angel-125) for KSP 1.2
	+ When kerbals go EVA, they take one day's worth of snacks with them.
	+ More code cleanup.
	+ Bug Fixes
* 2016-1018: 1.5.2 (Angel-125) for KSP 1.2
	+ Temporarily disable the partial vessel control penalty.
	+ Added additional checks for vessels created through rescue contracts; any crew listed as "Unowned" will be ignore.
* 2016-1017: 1.5.0 (Angel-125) for KSP 1.2
	+ ISnacksPenalty now has a RemovePenalty method. Snacks will call this each time kerbals don't miss any meals.
	+ ISnacksPenalty now has a GameSettingsApplied method. This is called at startup and when the player changes game settings.
	+ The partial control loss penalty should work now.
	+ New penalty: kerbals can pass out if they miss too many meals.
	+ Updated the KSPedia to improve clarity and to add the new penalty option.
	+ New events
	+ onBeforeSnackTime: Called before snacking begins.
	+ onSnackTime: Called after snacking.
	+ onSnackTick: called during fixed update right after updating the vessel snapshot.
	+ onConsumeSnacks: Called right after calculating snack consumption but before applying any penalties. Gives you to the ability to alter the snack consumption.
	+ onKerbalsMissedMeal: Called when a vessel with kerbals have missed a meal.
* 2016-1013: 1.4.5 (Angel-125) for KSP 1.2
	+ Fixed an issue with snack tins not showing up.
	+ A single kerbal can now consume up to 12 snacks per meal and up to 12 snacks per day.
	+ By default, a single kerbal consumes 1 snack per meal and 3 meals per day.
	+ Reduced Soil storage in the Hitchhiker to 200. This only applies to new vessels.
	+ Reduced Snacks per crew capacity in non-command pods to 200 per crewmember. This only applies to new vessels.
	+ Added the SnacksForExistingSaves.cfg file to specify number of Snacks per command pod and snacks per non-command pod. These are used when installing Snacks into existing saves for vessels already in flight.
	+ Added new ISnacksPenalty interface for mods to use when implementing new penalties. One of the options is to always apply the penalty even with random penalties turned off. Of course the implementation can decide to honor random penalties...
	+ Added a Snacks Trip Planner Excel spreadsheet. You'll find it in the Docs folder. An in-game planner is in the works.
* 2016-1012: 1.4.0 (Angel-125) for KSP 1.1.3
	+ Adjusted Snack production in the MPL; it was way too high. Ore -> Snacks is now 1:10 with mass conservation. A 1.25m Small Holding Tank (holds 300 Ore) now produce 3,000 Snacks.
	+ Added display field to Snack Processor that tells you how the max amount of snacks per day that it can produce.
	+ Moved Snack Tins to the Payload tab.
	+ Added option to show time remaining in days.
	+ When kerbals go hungry, added the option to randomly choose one penalty from the enabled penalties, or to apply all enabled penalties.
	+ Added lab data/experiment data loss as an optional penalty.
	+ You can now register/unregister your own custom penalties. This is particularly useful for addons to Snacks.
	+ Cleaned up some KSPedia issues.
	+ Fixed an issue with adding Snacks to existing saves.
	+ Fixed an issue with vessels spawned from rescue contracts incuring penalties due to being out of Snacks.
* 2016-1010: 1.3.1 (Angel-125) for KSP 1.1.3
	+ No changelog provided
* 2016-1010: 1.3 (Angel-125) for KSP 1.1.3
	+ No changelog provided
* 2016-0915: 1.2.0 (Angel-125) for KSP 1.2 PRE-RELEASE
	+ PRE-RELEASE
	+ Recompiled for KSP 1.2
* 2016-0622: 1.1.7 (Angel-125) for KSP 1.1.3
	+ Updated to KSP 1.1.3
