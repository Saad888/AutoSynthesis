Autosynth should be working as of 6.4. Future patches should not break the application.

WARNINGS: 
- This application is **not** compatible with current versions of XIV Alexander. Please disable XIV Alexander or use the NoClippy plugin  
- F10 does not seem to be working as a keybind any longer. 
- This application also does not work with older versions of windows prior to Windows 10

---

# **AutoSynthesis** - An FFXIV Auto Crafter
Auto-crafter for Final Fantasy XIV. This application will automatically repeat crafting the same item by sending blind inputs into the game. The app can hit up to three in-game macros, accept collectable crafts (if enabled), reset food and/or syrup (if enabled), and will automatically start the next craft. Users can also set exactly how many items they want the app to craft, or they can let is run indefinitley. The app also runs in the background without requiring window focus on either the app or the game. 


![AutoSynthesis Overview](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Full%20UI.PNG)


This application runs by blindly sending inputs into the game. It does not attach to the process, read any network data, or have any direct interactions with the game itself, it will simply send keystrokes into the game. As such, it relies on the user setting up the craft correctly and ensuring all inputs on the app are set correctly. If the cycle desyncs at any point because of an event in the game (e.g. running out of material), the app will not be aware and will continue to attempt crafting until the user interjects. 

---

## Table of Contents:
1. Installation and Setup
2. Using the App
   - Setting Up the Game
   - Macros
   - Consumables
   - Settings
   - Profiles
   - Starting and Stopping Crafting
3. Reading the Crafting Window
4. F.A.Q.
5. Contact Info


---
## 1. Installation
The installation file can be found here:   
https://github.com/Saad888/AutoSynthesis/releases/tag/1.1

---

## 2. Using the App

### Setting Up the Game
Before starting the crafting from the app, the game needs to be in the right state.  The first action that the app will do is the first Macro, so before hitting the "Start Crafting" button, make sure your character is crouched down ready to craft and not in the crafting window, like so:
![Image](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Ready%20to%20Craft.PNG)

AutoSynthesis also requires key inputs, so any macros, food/syrup, etc. must have keybinds associated with them registered properly.

### Macros  
![Macros](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Macros.png)  
You can program the app to hit up to three macros. You *must* enter a keybind and timer for at least one macro.  
1. Registering a keybind: Either **Double Click**, or click once and hit **Enter**/**Space** on the large textboxes. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with those Macros in game.   
2. Setting the timer: Either click on or tab over to the timer textboxes next to the keybind selections and enter a the duration of the macro in **seconds**.   
3. If you wish to use *two* or *three* macros, you must also enable them by clicking on the checkboxes next to the labels.  

### Consumables
![Consumables](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Consumables.png)  
You can enable these refresh your consumables as they run out. They will be refreshed between crafts.   
NOTE: Activating either of these means you will need to register a keybind for "Cancel", see the Settings section below.   
1. If using either food or syrups, click on the checkbox to enable. 
2. Registering a keybind: Either **Double Click** on the textbox or click on the textbox and hit **Enter**/**Space**. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with the consumable in game.
2. Setting the timer: Either click on or tab over to the timer textboxes next to the keybind selections and enter the remaining duration of your *current active buffs* in **minutes**.
3. Because food durations can vary based on FC and Squadron buffs, select the duration that food will last when it is refreshed.

### Settings
![Settings](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Settings.png)  
Here is where you will apply various settings related to the crafting itself.    
1. Registering a keybind: Either **Double Click** on the textbox or click on the textbox and hit **Enter**/**Space**. Once the textbox turns red, press any key (along with any modifiers, like Shift, Control, or Alt) to register the keybinds associated with "Confirm" and "Cancel".
2. Here you can set an specfic number of items to be crafted. If left unchecked, the autocrafter will simply keep attempting to craft until inturrupted by the user. 
3. Enabling this setting wil linstruct the autocrafter to automatically accept the window prompt for collectable crafting after the craft is complete. 
NOTE: The keybinds for "Confirm" and "Cancel" in game can be found under Keybind > System. "Cancel" only needs to be set if using a consumable.   

### Profiles
![Profiles](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Profiles.PNG)  
Here you can save, load, and delete profiles for your settings, to make it easier to swap between setttings when working on different crafts.   

### Starting and Stopping Crafting
Once all your settings have been set correctly, *and your character in game is crouched ready to craft*, press the "Start" button to begin.      

To stop crafting, simply press the button once again. Pressing the button once will cause the process to finish the current craft before stoping. Pressing the button a second time will immediatly cancel the process. 

---
## 3. Reading the Crafting Window
![Crafting Progress](https://github.com/Saad888/AutoSynthesis/blob/master/AutoSynthesis/Resources/ReadMe%20Images/Crafting.PNG)  
1. **Progress Bars**: These give a visual indicator for the crafting progress.   
   - The first bar displays your overall progress (the number of crafts completed overall).   
   - The second bar displays progress for a single craft.  
   - The third bar displays progress for a single macro.   
2. **Text Boxes**: These will give general information about the crafting process.  
   - The first line displays what the application is currently doing.  
   - The second line displays what inputs are being sent into the game.  
   - The third line displays the remaining time on any consumables before they will be flagged to be reused.   
   

---
## 4. F.A.Q.
**1. My craft is getting cancelled after using food or syrup.**
- The most likely cause for this is your character is selecting an object in the background when a craft is starting. For example it might be selecting the market board if you are crafting near it as a new craft starts. Unfortunately there is no fix for this, you *must* be in a position where nothing else on your screen can be selected and start an event (e.g. no other NPCs, Market Board, Signs, etc.). My recommendation is to be facing a wall or being in a room without other entities.

**2. The app isn't sending inputs into the game.**   
- If the game limits FPS when running in the background, this might cause some inputs to get neglected, so ensure to turn that setting off in game. 
- Please check the process name under the details tab for Task Manager, the app is looking for either "ffxiv.exe" or "ffxiv_dx11.exe", if for whatever reason it's neither of these please let me know. 
- Otherwise, please contact me

**3. Does the game need to be in focus?**
- No, you can use other applications and keep FFXIV minimized. Note I can't guarantee inputs will work 100% of the time but in none of my testing has it failed yet. 

**4. Can I use mouse or controller inputs?**  
- No, this app only works with Keyboard inputs. 

**5. My mouse and keyboard freeze for a bit when starting a new craft.**
- This is intentional. If the game is in focus, or if the game is in the background but the mouse is hovering over the game, mouse inputs must freeze otherwise another craft won't start. This is because if the game detects any mouse movement over the application, it will disable the controller cursor and make it impossible to hit the sequence to start the next craft. It should never freeze for more than 0.1 seconds at a time, so if it ever lasts longer than that please let me know. To avoid this happening, once you start the autocraft either fully minimize the game or move it to another monitor.

**6. My crafting is randomly getting cancelled.**
- The most likely cause for this is your character is selecting an object in the background when a craft is starting. For example it might be selecting the market board if you are crafting near it as a new craft starts. Unfortunately there is no fix for this, you *must* be in a position where nothing else on your screen can be selected and start an event (e.g. no other NPCs, Market Board, Signs, etc.). 

**7. Does this violate FFXIV ToS?**
- Yes

---

## 5. Contact Info

If you have any questions, comments, feedback, or run into any issues, please contact me either here or on discord (Xaad#1337).
Also, feel free to follow me on twitter, twitter.com/FeelsXaadMan.

---

## 6. New Features and ToDo

There are no immediate plans for additional devleopment on this autocrafter, as I really don't have any time. Rolling updates are also currently disabled so a new version would have to be releaased with them enabled before new features can be worked on. If circumstances allow, I might continue development, but as of yet there are no plans. 

Possible future features:

1. Full craft timer
2. Auto repair sequence
3. Windows 7 fix
4. Additional language support
5. Process ID selection
6. Sound effect notification for craft completion
