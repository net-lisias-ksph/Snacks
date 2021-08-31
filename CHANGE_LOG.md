# Snacks Continued :: Change Log

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
