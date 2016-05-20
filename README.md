# Advanced Optics
This is my [Advanced Optics](http://forum.spiderlinggames.co.uk/forum/main-forum/besiege-early-access/modding/41108-advanced-lasers-v0-2-v0-27-id-778-779) mod for the game [Besiege](http://www.besiege.spiderlinggames.co.uk/).
It currently adds a semi-functional multi-purpose laser & configurable optics system, and will soon have an integrated 16-bit CPU (based on a mix of the x86, ARM & z80 architectures).

## Lasers
The laser block, well, emits a laser. It has many configurable properties though, ranging from abilites to cosmetics and wireless capabilities (see CPU specs for more information).
The laser abilities in the current public (v0.2) release are:
* Fire mode - Ignites whatever the beam touches, and explodes most bombs. Pressing the laser ability key (J by default) makes an explosion at the beam's end-point.
* Kinetic mode - Tries to apply a configured force to whatever the beam touches. Due to its implementation, angling the laser makes it apply the force in rather strange/unexpected directions, and the magnitude of the force applied is proportional to your framerate, while being inversely proportional to the simulation's time scale.
* Freeze mode - Freezes most things, using a bunch of code ripped from ITR's old frost-thrower mod (without permission, sorry! Please forgive me :S). The active ability summons a lightning strike to ignite and fling objects in a sphere around the beam's end-point.
* Cosmetic mode - Doesn't do anything ability-wise.
* Tag mode - made as a joke to tease TGYD (regarding his apparent hatred towards the many colourful creations on the Steam Workshop). Doesn't do anything on its own, but pressing the ability key will set the colour of whatever it hits to the laser beam's colour 99% of the time until the simulation ends.
As for cosmetic options, you can:
* change the colour of the laser beam
* change the thickness of the laser beam's end-point (as a temporary thing; players wanted to change the beam thickness but imo it looks & behaves pretty badly. Might be replaced later with a 'diffuser' optics mode)
* setup some extra decorative beams around the main laser (sine wave, 1/x-style sine wave) or change the main beam to some sci-fi random lightning thing

That's pretty much it for the laser block as it is in v0.2. v1.1.3.0 added basic wireless capabilities, but the design was rather obnoxious to actually work with.. The upcoming CPU (PESWMR, and the PCode compiler) is designed to deal with the issues in the wireless system's design.


(TODO: optics block, features to come & CPU specs)
