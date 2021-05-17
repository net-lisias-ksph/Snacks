Snacks is simple and lightweight out of the box, but it's far more powerful than you might think. In addition to the Air and Stress configs included with Snacks, you can write your own processors and events for custom resources of your own. This tutorial will shows you how to put together a new resource to track: Hydrazine!

I'm a fan of Kuzzter's graphic novels ([Kerbfleet: A Jool Odyssey](https://forum.kerbalspaceprogram.com/index.php?/topic/126293-kerbfleet-a-jool-odyssey-chapter-20-pg-23-shee-oot-indeed-or-is-it-shee-ot/) is just one of them), and one of the fun themes is where Bill Kerman is an expert at turning monopropellant into hard liquor known as "hydrazine" in order to avoid suspicion from Kerbfleet Command. Of course the kerbals get pretty drunk and have some funny moments. To add Hydrazine as a resource to keep track of in Snacks, Let's first list out what we want it to do:

* Give kerbals a way to reduce Stress that doesn't involve an Entertainment Center or Stargazing out a cupola.
* When kerbals get stressed enough, they might take a swig of Hydrazine to calm down.
* If they have a drink, they might pass out.

Now that we have a basic design for the resource, let's set up the config file to implement it. Follow the steps below:

1. Create a new text file with your favorite text editor, and name it Hydrazine.cfg.
2. In the text file, add the following code:
```
SNACKS_RESOURCE_INTRO:NEEDS[SnacksStress]
{
	name = Hydrazine
	title = Hydrazine!
	description = If you're a fan of the Kerbfleet series by forum user Kuzzter, then this Snacks addon is for you! When kerbals get stressed, they can turn to "Hydrazine," a hard liquor created by Bill Kerman that's distilled from monopropellant and (hopefully) named so that Kerbfleet Command won't realize what the crew has been up to in deep space. An occasional shot of Hydrazine is great at calming nerves, but sometimes a kerbal ties one too many on and passes out.
}
```
The SNACKS_RESOURCE_INTRO config node tells Snacks that it should show the user an introduction screen with the specified description. It's a nice way to let users know that a new resource is being tracked. By the way, the :NEEDS[SnacksStress] qualifier is a Module Manager syntax that tells Module Manager that the config node is only valid if SnacksStress has been defined.

3. Now let's define the Hydrazine resource. Since Hydrazine might exist as a resource somewhere, we have to be careful not to cause naming collisions. So add the following to the file:
```
RESOURCE_DEFINITION:NEEDS[SnacksStress]
{
	name = HydrazineVodka
	displayName = Hydrazine Vodka
	density = 0.001
	unitCost = .5
	flowMode = ALL_VESSEL
	transfer = PUMP
	isTweakable = true
	volume = 0.044
}
```
The RESOURCE_DEFINITION node is standard for KSP. Here we've defined a new resource, Hydrazine Vodka. Kuzzter doesn't define what Hydrazine is, but I suspect it's a pretty pure Vodka. The density is close enough to water that we set it to 0.001, and it's unit volume is 0.044 liters- the typical size of a shot glass.

4. Now we need a way to ~~smuggle~~ transport Hydrazine to crews in need. Let's modify the radial snack tin to provide a secret stash:
```
@PART[radialSnackTin]:NEEDS[SnacksStress]
{
	@MODULE[SnacksResourceSwitcher]
	{
		OPTION
		{
			name = Hydrazine Stash

			RESOURCE
			{
				name = HydrazineVodka
				amount = 17
				maxAmount = 17
			}

			RESOURCE
			{
				name = Snacks
				amount = 148
				maxAmount = 148
			}
		}
	}
}
```
This snippet will modify the radial snack tin and give it a new resource option, called Hydrazine Stash, that contains both Snacks and some Hydrazine. The average bottle of vodka contains 17 shots, so we set the resource amount to 17. If you do the math, you'll find that the Hydrazine takes 0.748 liters. Let's round that up to an even 1 to include the bottle, and another 1 for padding (we wouldn't want the bottle to break). Since Snacks takes up 1 liter per unit, we reduce its total amount accordingly.

5. Now that we have a way to transport Hydrazine, we can make sure that our parts with crew capacity have a bottle stowed away somewhere for special occasions. We can either use Module Manager to patch specific parts, or just let Snacks add the resource to any part with crew capacity. We'll do the latter (the kerbals are going to get pretty sloshed!), so add the following:
```
SNACKS_PART_RESOURCE:NEEDS[SnacksStress]
{
	resourceName = HydrazineVodka
	amount = 17
	maxAmount = 17
	isPerKerbal = false
}
```
The SNACKS_PART_RESOURCE node tells Snacks to add the resource to any part with crew capacity. We specify the amount and max possible amount here to tell the mod how much to add to each part. The _isPerKerbal_ field tells Snacks whether or not to multiply _amount_ and _maxAmount_ by the crew capacity. In this case we don't want to add a bottle for every single seat, we just want to make sure there's at least one bottle per part with crew capacity.

If we wanted our kerbals who go on EVA to have a shot with them, we'd add the following:
```
SNACKS_EVA_RESOURCE:NEEDS[SnacksStress]
{
	resourceName = HydrazineVodka
	amount = 1.0
	maxAmount = 1.0
}
```
This would ensure that when a kerbal steps out of a vessel, he or she takes along a shot of Hydrazine. That shot will be removed from the vessel's current amount.

6. Now we need to set up the process of consuming quantities of Hydrazine. To do that we need to set up a snacks resource processor. Start by adding the following:
```
SNACKS_RESOURCE_PROCESSOR:NEEDS[SnacksStress]
{
	name = TakeAShotOfHydrazine
	secondPerCycle = 21600

	PRECONDITION
	{
		name = CheckResource
		resourceName = Stress
		checkType = checkGreaterOrEqual
		valueToCheck = 10
	}

	PRECONDITION
	{
		name = CheckRandomChance
		dieRollMin = 1
		dieRollMax = 100
		targetNumber = 80
	}
}
```
The name field needs to be something unique, so be creative! The _secondsPerCycle_ field tells Snacks how often to run the resource processor. In this case, 21600 seconds, or once every six hours (a standard day length). There are also two PRECONDITION nodes. a PRECONDITION node specifies what conditions must be met before the resource processor can run. You can have any number of PRECONDITION nodes but all of them must be met before the processor runs.

In this case, we have two preconditions: CheckResource and CheckRandomChance. The CheckResource precondition will look and see if the kerbal or vessel has the resource in the amount specified. In our case, we want to know if the kerbal has 10 units or more of Stress. The second precondition, CheckRandomChance, rolls a random number between 1 and 100. If the result is 80 or more, then the precondition is validated.

7. We have more to do with the SNACKS_RESOURCE_PROCESSOR node, so add the following to tell the processor what do do when the preconditions are met:
```
	CONSUMED_RESOURCE
	{
		resourceName = HydrazineVodka
		amount = 1.0
		showInSnapshot = true
		failureResultAppliesOutcomes = false
	}
```
Here we are telling the resource processor to consume 1 unit of the HydrazineVodka resource. We also want to see the resource show up in the snapshot window. We could set up outcomes to apply if the vessel runs out of Hydrazine, but for our purposes we have no outcomes to apply. For an example of what you could do, take a look at the Air.txt file found in the Snacks/LifeSupportResources folder.

8. So far, so good. We have set up the configs to add Hydrazine to vessels, we have a way to transport an extra bottle of the stuff, and we have a resource processor set up to consume Hydrazine when kerbals get stressed. Now we want to set up what happens when a kerbal consumes Hydrazine. Add the following to the config file:
```
SNACKS_EVENT:NEEDS[SnacksStress]
{
	name = mellowOut
	eventCategory = categoryPostProcessCycle
	kerbalsAffected = affectsAllAssigned

	PRECONDITION 
	{
		name = CheckProcessorResult
		type = resultConsumptionSuccess
		processorName = TakeAShotOfHydrazine
		resourceName = HydrazineVodka
		cyclesRequired = 1
	}

	OUTCOME
	{
		name = ConsumeResource
		resourceName = Stress
		amount = 1
	}
}
```
The SNACKS_EVENT config node is how we define an event. Events are a set of preconditions and outcomes. Just like with the resource processor, all preconditions must be met before outcomes are applied. The _eventCategory_ field tells Snacks what type of event we are defining. In this case, we are defining an event that is checked after a resource process cycle has finished. We could also define a random event card, but that's beyond the scope of this tutorial.

The _kerbalsAffected_ field tells Snacks how to apply the event. In this case, we want to apply it to each kerbal that is out on a mission.

In the above example, we are using the CheckProcessorResult precondition. This precondition lets us examine what happened during a resource process cycle. Here we are looking at the TakeAShotOfHydrazine processor that we defined previously, and we want to see if we successfully consumed HydrazineVodka. The cyclesRequired field tells the precondition that we must have at least one successful cycle in order to validate the precondition.

The OUTCOME node is how we specify what should happen when the preconditions are met. In this case, we are using the ConsumeResource outcome, and we're subtracting one unit of Stress. Putting the above event together, if the processor cycle is successfully completed, then consume one unit of Stress for every kerbal that has 10 or more units of Stress.

9. We have one more event to add: make the kerbal pass out if he or she drank too much. Add this event after the one we created above:
```
SNACKS_EVENT:NEEDS[SnacksStress]
{
	name = drankTooMuch
	eventCategory = categoryPostProcessCycle
	kerbalsAffected = affectsAllAssigned

	PRECONDITION 
	{
		name = CheckProcessorResult
		type = resultConsumptionSuccess
		processorName = TakeAShotOfHydrazine
		resourceName = HydrazineVodka
		cyclesRequired = 1
	}

	PRECONDITION
	{
		name = CheckRandomChance
		dieRollMin = 1
		dieRollMax = 100
		targetNumber = 80
	}

	OUTCOME
	{
		name = ConsumeResource
		resourceName = HydrazineVodka
		randomMin = 1
		randomMax = 3
	}

	OUTCOME
	{
		name = FaintPenalty
		playerMessage = passes out from drinking too much!
		faintDurationSeconds = 180
	}
}
```
You've seen the preconditions before, but the ConsumeResource has two new fields: _randomMin_ and _randomMax_. You can consume a random number of units of the specified resource with those fields. The FaintPenalty is new; it tells Snacks to make the kerbal pass out similarly to how they pass out due to high-g effects. The _playerMessage_ field is part of every outcome and lets you specify a message to show the player when the outcome is applied.

That's it, you've completed the Hydrazine tutorial! The complete file is listed below, and you [download it here](https://github.com/Angel-125/Snacks/blob/master/TutorialFiles/Hydrazine.cfg).

```
SNACKS_RESOURCE_INTRO:NEEDS[SnacksStress]
{
	name = Hydrazine
	title = Hydrazine!
	description = If you're a fan of the Kerbfleet series by forum user Kuzzter, then this Snacks addon is for you! When kerbals get stressed, they can turn to "Hydrazine," a hard liquor created by Bill Kerman that's distilled from monopropellant and (hopefully) named so that Kerbfleet Command won't realize what the crew has been up to in deep space. An occasional shot of Hydrazine is great at calming nerves, but sometimes a kerbal ties one too many on and passes out.
}

RESOURCE_DEFINITION:NEEDS[SnacksStress]
{
	name = HydrazineVodka
	displayName = Hydrazine Vodka
	density = 0.001
	unitCost = .5
	flowMode = ALL_VESSEL
	transfer = PUMP
	isTweakable = true
	volume = 0.044
}

@PART[radialSnackTin]:NEEDS[SnacksStress]
{
	@MODULE[SnacksResourceSwitcher]
	{
		OPTION
		{
			name = Hydrazine

			RESOURCE
			{
				name = HydrazineVodka
				amount = 17
				maxAmount = 17
			}

			RESOURCE
			{
				name = Snacks
				amount = 148
				maxAmount = 148
			}
		}
	}
}

SNACKS_PART_RESOURCE:NEEDS[SnacksStress]
{
	resourceName = HydrazineVodka
	amount = 17
	maxAmount = 17
	isPerKerbal = false
}

SNACKS_RESOURCE_PROCESSOR:NEEDS[SnacksStress]
{
	name = TakeAShotOfHydrazine
	secondPerCycle = 21600

	PRECONDITION
	{
		name = CheckResource
		resourceName = Stress
		checkType = checkGreaterOrEqual
		valueToCheck = 10
	}

	PRECONDITION
	{
		name = CheckRandomChance
		dieRollMin = 1
		dieRollMax = 100
		targetNumber = 80
	}

	CONSUMED_RESOURCE
	{
		resourceName = HydrazineVodka
		amount = 1.0
		showInSnapshot = true
		failureResultAppliesOutcomes = false
	}
}

SNACKS_EVENT:NEEDS[SnacksStress]
{
	name = mellowOut
	eventCategory = categoryPostProcessCycle
	kerbalsAffected = affectsAllAssigned

	PRECONDITION 
	{
		name = CheckProcessorResult
		type = resultConsumptionSuccess
		processorName = TakeAShotOfHydrazine
		resourceName = HydrazineVodka
		cyclesRequired = 1
	}

	OUTCOME
	{
		name = ConsumeResource
		resourceName = Stress
		amount = 1
	}
}

SNACKS_EVENT:NEEDS[SnacksStress]
{
	name = drankTooMuch
	eventCategory = categoryPostProcessCycle
	kerbalsAffected = affectsAllAssigned

	PRECONDITION 
	{
		name = CheckProcessorResult
		type = resultConsumptionSuccess
		processorName = TakeAShotOfHydrazine
		resourceName = HydrazineVodka
		cyclesRequired = 1
	}

	PRECONDITION
	{
		name = CheckRandomChance
		dieRollMin = 1
		dieRollMax = 100
		targetNumber = 80
	}

	OUTCOME
	{
		name = ConsumeResource
		resourceName = HydrazineVodka
		randomMin = 1
		randomMax = 3
	}

	OUTCOME
	{
		name = FaintPenalty
		playerMessage = passes out from drinking too much!
		faintDurationSeconds = 180
	}
}
```