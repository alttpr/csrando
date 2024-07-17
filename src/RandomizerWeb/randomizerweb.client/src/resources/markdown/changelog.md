# Randomizer Changes

## 2024-07-15 - Version 0.1.7
* Gameplay Changes
  * Fixed a bug when Z1 Overworld shuffle was enabled that cause the coast item and armos item to not be placed properly.
  * Fixed a bug in M1 where the top right energy tank location did not get its item written properly.

## 2024-07-15 - Version 0.1.6
* Gameplay Changes
  * Added an initial implementation of Zelda 1 Overworld Shuffle (WARNING: Be aware that this may be very buggy)
  * Fixed a bug that cause Turtle Rock and Misery Mire entry logic checks not to work properlyin Zelda 3
  * Fixed a bug with ammo calcuation in Super Metroid that caused ammo checks to be broken
  * Reverted the music engine change for Z1 since it caused notes to be stuck
  * Fixed the issue where Bombos would be invisible in Z1 dungeons
  * Fixed issues with the Super Metroid logic where it didn't properly connect the starting location.


## 2024-07-15 - Version 0.1.5
* Gameplay Changes
  * Moved the Up+A (Up+B on a SNES controller) shorcuts in Z1 and M1 to controller one.
  * Fixed a placement issue where Long Beam and Screw Attach when placed in M1 would not function correctly.
  * Fixed the crash in M1 after defeating Mother Brain if it was not the final boss.
  * Moved the auto-saving in M1 to direclty when the Password screen shows up.
  * Fixed an issue in Z1 where it was not saving items correctly, it now saves when the "Continue/Save/Retry" screen shows up.
  * Fixed a bug in the music engine for Z1 and M1 that should now make some music and effects sound better.

	
## 2024-07-13 - Version 0.1.4
* Systems Changes
  * The website has been updated to use a version of the website used for SMZ3.

* Gameplay Changes
  * Fixed an issue where the Z1 triforces and SM bosses goal was not correctly written to the ROM.
  * Fixed an issue where certain items (like Hookshot) did not work properly when placed in M1.


## Version 0.1.3
* Systems Changes
	* Reworked a lot of internal data, so beware that this might cause new issues
	* Added support for custom sprites in SM and added basic sprite selection support to the website for Z3 and SM

* Gameplay Changes
	* Hopefully fixed the random Z1 transition crashes

## Version 0.1.2
* Gameplay Changes
  * Added selectable crystal/boss/triforce goals for Z1, Z3 and SM
  * Fixed a bug where pausing in SM would corrupt items graphics
  * Fixed the incorrect Z3 standard state
  * Fixed an issue with some entrances in Z3 going to the wrong place
  * Rewrote the quick swap functionality in Z1 to work better