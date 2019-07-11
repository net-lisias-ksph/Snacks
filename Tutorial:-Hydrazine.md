Snacks is simple and lightweight out of the box, but it's far more powerful than you might think. In addition to the Air and Stress configs included with Snacks, you can write your own processors and events for custom resources of your own. This tutorial will shows you how to put together a new resource to track: Hydrazine!

I'm a fan of Kuzzter's graphic novels ([Kerbfleet: A Jool Odyssey](https://forum.kerbalspaceprogram.com/index.php?/topic/126293-kerbfleet-a-jool-odyssey-chapter-20-pg-23-shee-oot-indeed-or-is-it-shee-ot/) is just one of them), and one of the fun themes is where Bill Kerman is an expert at turning monopropellant into hard liquor known as "hydrazine" in order to avoid suspicion from Kerbfleet Command. Of course the kerbals get pretty drunk and have some funny moments. To add Hydrazine as a resource to keep track of in Snacks, Let's first list out what we want it to do:

* Give kerbals a way to reduce Stress that doesn't involve an Entertainment Center or Stargazing out a cupola.
* When kerbals get stressed enough, they might take a swig of Hydrazine to calm down.
* If they have a drink, they might pass out.

Now that we have a basic design for the resource, let's set up the config file to implement it. Follow the steps below:

1. Create a new text file with your favorite text editor, and name it Hydrazine.cfg.
2. In the text file, add the following code:
```
SNACKS_RESOURCE_INTRO
{
	name = Hydrazine
	title = Hydrazine!
	description = If you're a fan of the Kerbfleet series by forum user Kuzzter, then this Snacks addon is for you! When kerbals get stressed, they can turn to "Hydrazine," a hard liquor created by Bill Kerman that's distilled from monopropellant and (hopefully) named so that Kerbfleet Command won't realize what the crew has been up to in deep space. An occasional shot of Hydrazine is great at calming nerves, but sometimes a kerbal ties one too many on and passes out.
}
```
The SNACKS_RESOURCE_INTRO config node tells Snacks that it should show the user an introduction screen with the specified description. It's a nice way to let users know that a new resource is being tracked.

3. Now let's define the Hydrazine resource. Since Hydrazine might exist as a resource somewhere, we have to be careful not to cause naming collisions. So add the following to the file:
```
RESOURCE_DEFINITION
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