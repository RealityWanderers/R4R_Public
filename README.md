# 🛹 RAMEN4ROBOTS

A fast-paced VR skating game built around speed, flow, and momentum.
Heavily inspired by Sonic the Hedgehog and Jet Set Radio.

https://github.com/user-attachments/assets/2f111e9d-f808-4e2a-b8f0-16a309cf8e21

If you’re curious to see it in action, check out the:
- 🎥 [Gameplay & DevLogs](https://youtube.com/playlist?list=PLQ3y7VoZK-4M_FGsmGoOwaoK2rG7UcLKG) 
- 💬 [Discord](http://discord.com/invite/zX2XRPvDt5)

A ready to play PC and Quest build can be found here:
- 🎮 [Play the Game](https://ko-fi.com/realitywanderer/shop)
  
If you’d like to support my work and future projects, you can find me on:
- [☕Ko-fi](https://ko-fi.com/realitywanderer)

## 🚧 Project Status:

This version of the project is **no longer actively developed**.

That said, the repository is intentionally kept public as:
- A **learning resource** for developers interested in VR movement systems.
- A reference for VR locomotion, skating physics, and momentum-based design.
- A base for creating **custom levels, or experiments**.

Please see the [LICENSE](https://github.com/RealityWanderers/R4R_Public/tree/main?tab=License-1-ov-file) for details on what is and isn’t allowed. Especially regarding assets and third-party packages.

## 🛠 Using This Project

- Press Download ZIP here:
<img width="596" height="457" alt="image" src="https://github.com/user-attachments/assets/53549325-c2dd-4b71-a867-72bb4d3e9743" />

- Download Unity Hub over at [Unity](https://unity.com/download).
- In Unity Hub at the top right press ADD > ADD PROJECT FROM DISK.
- You might get a notification that the Editor version is not installed. I recommend installing the exact editor version as otherwise issues might arise.
- Open the project, initial import might take a while.
- A Fast reload window might pop up, you can close this. 
- Changes to OVR Plugin detected >  Press Restart Editor.
- Project validation > you might see various yellow and red warnings, these can be ignored as some of these are false. Pressing fix might break some parts of the project. So you can close this. 
- Open Assets > MAIN > Scenes > Level_0 or Level_1 (Other scenes are unfinished, best to use this as a base for custom levels).
- Experiment / Or copy the scene and make a custom level!


## 🌍 Community & Support

While this version is discontinued, development has continued in the form of a spiritual successor.
Curious about this successor or is something not working and need assistance? Feel free to message me over at [Discord](http://discord.com/invite/zX2XRPvDt5)!

## 🎮 Controls

Movement:
- Turning: <br />
R stick left and right or physically turning in real life. Level_0 shows an example for a menu to change turn type and turn settings.
- Skate: <br />
Hold L or R grip and swing your arm.
- Jump: <br />
Hold both L and R grip and swing your arms.
- Air Dash: <br />
Hold both L and R grip and swing your arms while in the air.
- Brake: <br />
Hold both L and R grip for a second to slow down.

Special:
- Homing Attack: <br />
When the green indicator appears do an Air Dash motion. 
- Overclock Boost: <br />
Press forward on the L stick to overclock when you have at least 1 battery segment filled.
- Grab: <br />
Press and hold L or R grip when the grab indicator appears (only in level_1).
- WallGrind: <br />
Jumping into a Billboard attaches you, hold L or R grip and push yourself off the wall to jump off. 
- Phone: <br />
L stick click to open and close, L stick to navigate, Y to select an app. 

## ⚠️ Known Issues:

- Functionality of the phone inside of Level_0 does not work correctly. Works fine in Level_1.
- Overclock boost is bound to the forward left stick, which is also used by the phone menu navigation.
- While holding a grabbable object you slowly slide down.
- Quick Step is enabled in Level_0 but not in Level_1, this is a bool you can toggle in the QuickStep ability script but the RamenBox throw is bound to the same action in Level_1 so keep that in mind. 

## Credits

Created by **Reality Wanderer**

Inspired by:
- Sonic the Hedgehog (classic & boost-era).
- Jet Set Radio.
- High-speed platformers and expressive movement systems.

Thanks to everyone who tested builds, gave feedback, and helped shape this project!
