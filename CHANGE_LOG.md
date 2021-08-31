# Snacks Continued :: Change Log

* 2021-0422: 1.27.1 (Angel-125) for KSP 1.11.2
	+ Fixed issue where Snacks wasn't being added to crewable parts.
* 2021-0326: 1.27.0 (Angel-125) for KSP 1.11.2
	+ Added the ability to store the SOCS Oxium Candle as a cargo part.
	+ SNACKS_PART_RESOURCE now supports unitsPerDay and daysLifeSupport. They specify the number of units per day that is consumed (per kerbal), and the number of days of life support to add to the part, respectively. If they're specified, then amount and maxAmount are ignored.
	+ Fixed missing resource infos in the editor's
* 2021-0322: 1.26.1 (Angel-125) for KSP 1.11.2
	+ Fixed issue where the PAW would flicker whenever adjusting settings on another part.
	+ Fixed issue where players could not copy parts with Snacks resources in them.
* 2021-0313: 1.26.0 (Angel-125) for KSP 1.11.1
	+ Changes
			- If a kerbal on EVA consumes a resource and it dips below minimum safe levels, then the player will receive a warning message.
			- In Debug mode, the Snack Time button will become available in the Snacks flight app window.
			- Fixed Snacks App Window not updating resource values properly after snack time.
			- Fixed NRE generated when opening the Snacks window and there is a kerbal on EVA.
			- Fixed issue where EVA resources were consumed when the jetpack is used. NOTE: Use the in-flight Snacks window to see the EVA kerbal's resources, they're no longer
			- Fixed crash issue that occurs when placing parts with crew capacity in symmetry.
			- Fixed issue in Snacks App Window where kerbals on EVA would display their crew count.
* 2020-0806: 1.25.2 (Angel-125) for KSP 1.10.1
	+ Fix Stresstimator showing up in Space Center window when it shouldn't.
	+ Fix corrected density of Hydrazine.
* 2020-0730: 1.25.1 (Angel-125) for KSP 1.10.1
	+ Fix SnacksConverter not restoring skills when the converter is activated and a kerbal transfers to another part.
	+ You can now estimate a vessel's max Stress capacity in the VAB/SPH. Requires Stress to be enabled (rename the Stress.txt file in LifeSupportResources folder to Stress.cfg).
	+ New Feature
			- Stresstimator: If you have Stress enabled, then you'll get a new button in the in-flight Snacks window to open the Stresstimator. This window helps you estimate the max Stress that your crew can take based on the crewable parts that you select. Since kerbals can get Stressed Out when moving from vessels with a lot of available crew capacity to vessels without much available crew capacity, the Stresstimator helps you avoid kerbals getting Stressed Out if they have accumulated Stress, you move them to a docked vessel, and then undock the vessel. Given the game design, it's very difficult to do the Stress estimate automagically, so the Stresstimator is better than nothing...
* 2020-0728: 1.25.0 (Angel-125) for KSP 1.10.1
	+ Fix background converters not respecting locked resources for unloaded vessels.
	+ Fix resource processors not respecting locked resources for unloaded vessels.
	+ Fix condition summary not showing Stressed Out condition.
	+ Fix Stress not recalculated when a vessel docks or undocks.
	+ Fix SOCS Cannister explosion check not being checked.
	+ Fix SOCS Cannister automatically shuts down when it runs out of SOCS Fuel.
	+ Fix SOCS Cannister user messages are more appropriate now.
	+ If kerbals get Stressed Out they might start stress eating. If you run out of Snacks they'll get even more Stressed Out.
	+ Kerbals wait to get Stressed Out before they start consuming "Hydrazine."
	+ You can now disable email notifications of converters running out of resources or storage space via the Snacks Settings menu.
	+ Background processors no longer consume ElectricCharge for simplicity; it's either that or bog the game down with finding and running power generators.
	+ You can now customize the SnacksConverter's criticalSuccessMessage, successMessage, failMessage, and criticalFailMessage displayed during yielded resource checks.
* 2020-0425: 1.24.5 (Angel-125) for KSP 1.9.1
	+ Bug fixes
* 2020-0215: 1.24.4 (Angel-125) for KSP 1.9.0
	+ Snacks (and other life support resources) will now be added to vessels that are loaded into the VAB/SPH.
* 2020-0211: 1.24.2 (Angel-125) for KSP 1.9.0
	+ Compatibility update
* 2019-1022: 1.24.1 (Angel-125) for KSP 1.8.0
	+ Bug fixes
* 2019-1021: 1.24.0 (Angel-125) for KSP 1.8.0
	+ Updated for KSP 1.8
* 2019-0926: 1.23.2 (Angel-125) for KSP 1.7.3
	+ Reduced timewarp ElectricCharge cap to 3x.
	+ New game settings: you can turn on/off ElectricCharge consumption for Snacks converters during background processing. Default is ON.
* 2019-0908: 1.23.1 (Angel-125) for KSP 1.7.3
	+ Fixed issue with kerbals not suffering any penalties when a ship processed in the background has locked snack tins.
	+ Experienced kerbals can now process inputs and outputs without affecting ElectricCharge consumption.
* 2019-0830: 1.23.0 (Angel-125) for KSP 1.7.3
	+ You can now disable Snacks/Soil resource processing if desired. Just rename Snacks.cfg to Snacks.txt in the LifeSupportResources folder.
* 2019-0817: 1.22.2 (Angel-125) for KSP 1.7.3
	+ Fixed flow mode issues for ElectricCharge consumption.
* 2019-0807: 1.22.1 (Angel-125) for KSP 1.7.3
	+ Removed test code from the simulator.
* 2019-0806: 1.22 (Angel-125) for KSP 1.7.3
	+ Added support for Dynamic Batteries.
	+ Added ability to interrupt the resource consumption simulator. NOTE: duration estimates will be unavailable.
	+ Fix Inability to view vessels not on or around the current world.
	+ Fix Missing roster resource names when added to the kerbal.
* 2019-0727: 1.21 (Angel-125) for KSP 1.7.3
	+ CheckResources can now check resource percent levels.
	+ Fix Simulator window not running simulations after closing and reopening the window in the same scene.
	+ Fix NRE generated by FaintPenalty.
	+ Fix estimated time remaining display showing 1 hour even when out of resources.
	+ Fix Snacks converters and their derivatives give players a break on ElectricCharge consumption past 100x timewarp.
	+ Fix background converters honor infinite electricity and infinite propellant debug settings.
	+ Fix "Hydrazine" Vodka display name.
* 2019-0716: 1.20.3 (Angel-125) for KSP 1.7.3
	+ Fix snack tin symmetry issues.
	+ Fix NREs when changing resources.
* 2019-0630: 1.20.2 (Angel-125) for KSP 1.7.3
	+ When I started reworking Snacks to add in the penalty system, I tried to follow the same design philosophies that Squad did when making KerbNet: make it a challenge but don't brick your game or save. I've kept that philosophy and stuck to the original concept as a lightweight life support system as I've made improvements over the years. This update is the collmination of weeks of work that keeps the simplistic life support out of the box but opens the doors to so much more. All it needs is a bit of legwork on your part, but there are plenty of examples.
	+ I'm happy to say that Snacks is feature complete!
	+ Custom Life Support Resources: Snacks now has the ability to define custom life support resources besides just Snacks! All it takes is a config file. With this feature you can:
			- Define your own life support resource to consume and/or produce.
			- Optionally track its status in the vessel snapshot window- with support in the multithreaded simulator!
			- Optionally apply one or more outcomes (like penalties) if the consumed resource runs out, or if space for the produced resource is full.
	+ As an example of a tracked resource, check out the LifeSupportResources folder for the FreshAir.txt file. Rename it with the .cfg extension to enable it.
	+ NOTE: You can make FreshAir using the stock ISRU and mini ISRU, and all the stock crew cabins have Air Scrubber converters to turn StaleAir into FreshAir.
	+ New Part: SOCS! Similar to the real-world Solid Fuel Oxygen Generator, the Solid Oxium Candle System burns a solid fuel to produce Fresh Air. Once started it can't be stopped and it might explode... It's available at the Survivability tech tree node, and only available if you enable Air.
	+ Roster Resource: Roster resources can now be defined via the SNACKS_ROSTER_RESOURCE config node. A roster resource is a characteristic of a kerbal as opposed to a resource stored in a part. Better yet, the SnacksConverter can work with roster resources- with background processing!
	+ New penalty: The OnStrikePenalty removes a kerbal's skill set without turning him or her into a tourist. That way, should you uninstall the mod for some reason, you won't brick your mission or game.
	+ Stress: You now have an optional resource to keep track of: Stress! Stress reflects the difficulties of living in confined spaces for extended periods of time. The more space available, reflected in a vessel's crew capacity, the longer a kerbal can live and work without getting stressed out. Events like a lack of food and FreshAir, arguments with other crew members, and low gravity can also cause Stress. And when a kerbal gets stressed out, they'll stop using their skills. You can reduce Stress by letting the kerbal hang out in the Cupola, but you won't gain use of their skills while they're relaxing. To enable Stress, just rename the LifeSupportResources/Stress.txt with the .cfg extension and restart your game. If you already have Kerbal Health then you won't need Stress, but it can serve as an example for how to use roster resources.
	+ And if you have BARIS installed, part failures and staging failures will cause Stress!
	+ Events: Random snacking is now reworked into an event system to add flavor to the game. Kerbals might get the munchies, or eat bad food. Maybe a crew member snores and it causes Stress. With a host of preconditions and outcomes, you can make a number of different and entertaining events. Check out the Snacks.cfg file for examples.
	+ Wiki: updates describe all the preconditions, outcomes and events along with other API objects.
* 2019-0712: 1.20.1 (Angel-125) for KSP 1.7.3
	+ Small update to support the new Hydrazine tutorial.
* 2019-0630: 1.20.0 (Angel-125) for KSP 1.7.2
	+ When I started reworking Snacks to add in the penalty system, I tried to follow the same design philosophies that Squad did when making KerbNet: make it a challenge but don't brick your game or save. I've kept that philosophy and stuck to the original concept as a lightweight life support system as I've made improvements over the years. This update is the collmination of weeks of work that keeps the simplistic life support out of the box but opens the doors to so much more. All it needs is a bit of legwork on your part, but there are plenty of examples.
	+ I'm happy to say that Snacks is feature complete!
	+ Custom Life Support Resources: Snacks now has the ability to define custom life support resources besides just Snacks! All it takes is a config file. With this feature you can:
			- Define your own life support resource to consume and/or produce.
			- Optionally track its status in the vessel snapshot window- with support in the multithreaded simulator!
			- Optionally apply one or more outcomes (like penalties) if the consumed resource runs out, or if space for the produced resource is full.
	+ As an example of a tracked resource, check out the LifeSupportResources folder for the FreshAir.txt file. Rename it with the .cfg extension to enable it.
	+ NOTE: You can make FreshAir using the stock ISRU and mini ISRU, and all the stock crew cabins have Air Scrubber converters to turn StaleAir into FreshAir.
	+ New Part: SOCS! Similar to the real-world Solid Fuel Oxygen Generator, the Solid Oxium Candle System burns a solid fuel to produce Fresh Air. Once started it can't be stopped and it might explode... It's available at the Survivability tech tree node, and only available if you enable Air.
	+ Roster Resource: Roster resources can now be defined via the SNACKS_ROSTER_RESOURCE config node. A roster resource is a characteristic of a kerbal as opposed to a resource stored in a part. Better yet, the SnacksConverter can work with roster resources- with background processing!
	+ New penalty: The OnStrikePenalty removes a kerbal's skill set without turning him or her into a tourist. That way, should you uninstall the mod for some reason, you won't brick your mission or game.
	+ Stress: You now have an optional resource to keep track of: Stress! Stress reflects the difficulties of living in confined spaces for extended periods of time. The more space available, reflected in a vessel's crew capacity, the longer a kerbal can live and work without getting stressed out. Events like a lack of food and FreshAir, arguments with other crew members, and low gravity can also cause Stress. And when a kerbal gets stressed out, they'll stop using their skills. You can reduce Stress by letting the kerbal hang out in the Cupola, but you won't gain use of their skills while they're relaxing. To enable Stress, just rename the LifeSupportResources/Stress.txt with the .cfg extension and restart your game. If you already have Kerbal Health then you won't need Stress, but it can serve as an example for how to use roster resources.
	+ And if you have BARIS installed, part failures and staging failures will cause Stress!
	+ Events: Random snacking is now reworked into an event system to add flavor to the game. Kerbals might get the munchies, or eat bad food. Maybe a crew member snores and it causes Stress. With a host of preconditions and outcomes, you can make a number of different and entertaining events. Check out the Snacks.cfg file for examples.
	+ Wiki: updates describe all the preconditions, outcomes and events along with other API objects.
* 2019-0622: 1.16.4 (Angel-125) for KSP 1.7.2
	+ Fix for kerbals taking too many snacks with them when going on EVA.
* 2019-0616: 1.16.3 (Angel-125) for KSP 1.7.2
	+ Fixed issue where Snacks was being consumed by occupants of vessel that's part of a rescue contract. NOTE: Once the craft is loaded into the scene, they will be tracked by Snacks.
* 2019-0609: 1.16.2 (Angel-125) for KSP 1.7.1
	+ Fixed issue where parts weren't receiving their correct allotment of Snacks for parts where the crew capacity is greater than one kerbal.
* 2019-0605: 1.16.1 (Angel-125) for KSP 1.7.1
	+ Updated Snacks Trip Planner in the Docs folder - new Timewarp Calcs tab.
	+ SnacksConverter now has new options:
	+ requiresHomeConnection - Requires connection to homeworld to operate.
	+ minimumCrew - Minimum number of crew that must be in the host part in order to run the converter.
	+ NOTE: Snacks Processor and Soil Recycler don't use these.
		- Fixed NREs generated when converters are added in the editor.
		- Fixed issue where Snacks wasn't being added to parts in the editor.
		- Fixed debug info in SnacksProcessor.
		- Fixed issue where Snack supply window wasn't being updated when the window is opened and snack time happens.
* 2019-0602: 1.16 (Angel-125) for KSP 1.7.1
	+ Added new celestial bodies filter to the snacks supply window.
		- Snacks will now run simulations on a vessel's supplies and converters to estimate how long the vessel's snacks will last.
	+ NOTE: For the simulator to work properly, be sure to visit all your in-flight vessels that have crews aboard after installing this update.
	+ NOTE: The simulator cannot simulate drill operations.
			- Made some improvements to background converter processing. As a bonus, power production & consumption are also run in the background- with Kopernicus support for solar arrays.
			- SnacksConverter now lists the yield resources in the part info window, and shows yield production time remaining in the PAW.
			- Snacks and other resources consumed per day are now calculated based on the solar day length of the homeworld instead of set to the stock 6hrs/24hrs. I'm looking at you, JNSQ...
* 2019-0518: 1.15 (Angel-125) for KSP 1.7.0
	+ Fixed issue where vessels spawned in game for rescue contracts lacked Snacks.
	+ Fixed integration issue with WalkAbout.
	+ The Soil Recycler now uses the Converter Skill from Engineers instead of the Science Skill. Yup, Scientists make Snacks from rocks and (sanitation) Engineers recycle Soil into Snacks.
	+ The converter and recycler won't automatically shut down if they lack an input resource or an output resource is full. Instead they'll wait until they get what they need.
	+ Updated the recycler/processor info view in the editor's part description window.
* 2019-0511: 1.14 (Angel-125) for KSP 1.7.0
	+ Fixed Restock whitelist
* 2019-0422: 1.13 (Angel-125) for KSP 1.7.0
	+ Fixed recycler and processor efficiency calculations.
	+ Other bug fixes.
* 2019-0416: 1.12 (Angel-125) for KSP 1.7.0
	+ Updated for KSP 1.7
	+ Bug fixes
* 2019-0330: 1.11.5 (Angel-125) for KSP 1.6.1
	+ Fixed empty mass for the radial snack tin to be in line with similar parts.
	+ Fixed snack resources being locked by default when Snacks are added to in-flight vessels.
	+ Fixed efficiency processing; processors and recyclers output efficiency should now be 10% to 100% efficient based on their efficiency setting.
	+ Fixed issue where converters and processors wouldn't run in the background without at least one Kerbal aboard.
	+ Fixed issue where the Snacks Processor and Soil Recycler wouldn't produce the proper amount of resources while the vessel is in physics range.
	+ Reduced the amount of Snacks per day produced by the stock Mobile Processing Lab. It was a bit OP...
	+ Snack tins now tell you what their resource options are in the part info view.
	+ Added new SnacksConverter part module. It serves as the basis for the existing Snacks Processor and Soil Recycler. It can also produce "YIELD_RESOURCE" units over time just like the greenhouse from Wild Blue Tools. You can even assign effects to the converter!
	+ NOTE: Snacks won't be getting a greenhouse of its own; it's intended to be a lightweight life support system, but I recognize that some players want more sophisticated capabilities. So the tools are there for others to expand upon...
* 2018-1227: 1.11.2 (Angel-125) for KSP 1.6.1
	+ Recompiled for KSP 1.6
* 2018-1103: 1.11.1 (Angel-125) for KSP 1.5.1
	+ Updated for KSP 1.5.X
	+ The Snack Processor and Soil Recycler will now automatically shut down if the vessel's ElectricCharge reserves drop below 5%.
* 2018-1103: 1.11.0 (Angel-125) for KSP 1.5.1
	+ Updated for KSP 1.5.X
	+ The Snack Processor and Soil Recycler will now automatically shut down if the vessel's ElectricCharge reserves drop below 5%.
* 2018-0403: 1.10.0 (Angel-125) for KSP 1.4.1
	+ Fixed NRE causing the Settings menu to not appear.
	+ Kerbals can now die from a lack of Snacks! This penalty is trned OFF by default, and you can change the number of skipped meals before a kerbal dies in the settings menu. Kerbals listed as exempt will never starve to death.
* 2018-0320: 1.9.0 (Angel-125) for KSP 1.4.1
	+ Recompiled for KSP 1.4.1
* 2018-0221: 1.8.7 (Angel-125) for KSP 1.3.1
	+ Fixed NRE and production issues with the SnackProcessor.
* 2018-0205: 1.8.6 (Angel-125) for KSP 1.3.1
	+ Snack consumption now honors resource locks.
	+ Retextured radial snack tin - Thanks JadeOfMaar! :)
	+ Removed unneeded catch-all - Thanks JadeOfMaar! :)
	+ Fixed bulkhead profiles and tags on inline snack tins - Thanks JadeOfMaar! :)
	+ Add parts to CCK LS category - Thanks JadeOfMaar! :)
* 2017-1007: 1.8.5 (Angel-125) for KSP 1.3.1
	+ Fixed background processing of snacks and soil issues with WBI mods (Pathfinder, Buffalo, etc.).
	+ NOTE: Be sure to visit your spacecraft at least once to ensure that the changes take effect.
		- Updated to KSP 1.3.1.
* 2017-0528: 1.8.0 (Angel-125) for KSP 1.3.0
	+ Time estimates are now measured in years and days; months, though accurate, was getting too confusing.
	+ Snack processors and soil recyclers now run in the background when vessels aren't loaded.
* 2017-0227: 1.7.0 (Angel-125) for KSP 1.2.2
	+ You can now exempt certain kerbals from consuming Snacks and suffering the effects of a lack of Snacks. This is particularly helpful for holograms...
* 2017-0106: 1.6.5 (Angel-125) for KSP 1.2.2
	+ Added a radial snack tin. It holds 150 snacks, 150 Soil, or 75 Snacks and 75 Soil.
* 2016-1211: 1.6.2 (Angel-125) for KSP 1.2.2
	+ KSP 1.2.2 Update
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
