# **AutoSynthesis**
---
Auto-crafter for Final Fantasy XIV. This application will automatically repeat crafting the same item by sending blind inputs into the game. The app can hit up to three in-game macros, accept collectable crafts (if enabled), reset food and/or syrup (if enabled), and will automatically start the next craft. Users can also set exactly how many items they want the app to craft, or they can let is run indefinitley. The app also runs in the background without requiring window focus on either the app or the game. 


![AutoSynthesis Overview](https://github.com/Saad888/AutoSynthesis/blob/master/RapidSynthesis/Resources/ReadMe%20Images/Full%20UI.PNG)


This application runs by blindly sending inputs into the game. It does not attach to the process, read any network data, or have any direct interactions with the game itself, it will simply send keystrokes into the game. As such, it relies on the user setting up the craft correctly and ensuring all inputs on the app are set correctly. If the cycle desyncs at any point because of an event in the game (e.g. running out of material), the app will not be aware and will continue to attempt crafting until the user interjects. 

---

## Table of Contents:
1. Basics of the Interface
2. Using the App
   - Setting Up the Game
   - Macros
   - Consumables
   - Settings
   - Profiles
   - Starting Crafting
5. Reading the Crafting Window
6. Installation and Setup
7. F.A.Q.

---

## 1. Basics of the Interface
[IMAGE]

1. Settings for using Macros
2. Settings for consumables
3. Settings for the craft
4. Profiles for settings
5. Crafting progress and updates

---

## 2. Using the App

### Setting Up the Game
Before starting the crafting from the app, the game needs to be set up. Autosynthesis begins assuming that a craft has been initiated. In other words, the first action AutoSynthesis does is the first Macro. Before hitting the "Start Crafting" button, make sure your chracter is crouched down ready to craft. AutoSynthesis also requires key inputs, so any macros, food/syrup, etc. must have keyboard shortcuts associated with them 

### Macros
[Image]
You can program the app to hit up to three macros. You *must* enter a keybind and timer for at least one macro.  
1. Registering a keybind: Either **Double Click** or click on and hit **Enter**/**Space** when the keybind textboxes are highlighted. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with those Macros in game.   
2. Setting the timer: Either click on or tab over to the timer textboxes next to the keybind selections and enter a the duration of the macro in **seconds**.   
3. If you wish to use *two* or *three* macros, you must also enable them by clicking on the checkboxes next to the labels.  

### Consumables
[Image]  
You can *optionally* enable these settings to refresh your consumables as they run out. For the sake of safety, consuamables will be flagged to refresh within two minutes of expiring. They will be refreshed between crafts.   
NOTE: Activating either of these means you will need to register a keybind for "Cancel", see the Settings section below.   
1. If using either food or syrups, click on the checkbox to enable them during the craft. 
1. Registering a keybind: Either **Double Click** on the textbox or click on the textbox and hit **Enter**/**Space**. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with the consumable in game.
2. Setting the timer: Either click on or tab over to the timer textboxes next to the keybind selections and enter the remaining duration of the consumable in **minutes**.
3. Because food durations can vary based on FC and Squadron buffs, select the duration that food will last when it is refreshed.

### Settings
[Image]   
Here is where you will apply various settings related to the crafting itself.   
1. Registering a keybind: Either **Double Click** on the textbox or click on the textbox and hit **Enter**/**Space**. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with "Confirm" and "Cancel".
2. Here you can set an specfic number of items to be crafted. If left unchecked, the autocrafter will simply keep attempting to craft until inturrupted by the user. 
3. Enabling this setting wil linstruct the autocrafter to automatically accept the window prompt for collectable crafting after the craft is complete. 
NOTE: The keybinds for "Confirm" and "Cancel" in game can be found under Keybind > System. "Cancel" only needs to be set if using a consumable.   

### Profiles
[Images]
Here you can save, load, and delete profiles for your settings, to make it easier to swap between setttings when working on different crafts.   

### Starting Crafting
[Images]
Once all your settings have been set correctly, *and your character in game is crouched ready to craft*, press the Start Crafting button to begin.      
To stop crafting, simply press the button once again. 

---
## 3. Reading the Crafting Window
[Image]
1. **Progress Bars**: These give a visual indicator for the crafting progress.   
   - The first bar displays your overall progress (the number of crafts completed overall).   
   - The second bar displays progress for a single craft.  
   - The third bar displays progress for a single macro.   
2. **Text Boxes**: These will give general information about the crafting process.  
   - The first line displays what the application is currently doing.  
   - The second line displays what inputs are being sent into the game.  
   - The third line displays the remaining time on any consumables before they will be flagged to be reused.   
   
---
## 4. Installation and Setup
// TO DO

---
## 5. F.A.Q.




FAQ:
- No controller or mouse inputs, only keyboard
- Focus on game is not required
- 
