# [Simple Conveyor Game](https://github.com/CharlieUK04/SimpleConveyorGame/tree/main)
### A Simple conveyor game created in unity for a job application.

## Features  
- **On-Screen controls**
- **Multiple layers for conveyors**
- **Random item spawner**
- **Animated conveyor belts**
- **Preview to help place conveyor belts**

## Technologies Used  
- **Game Engine**: [Unity 6000.3.20f1](https://unity.com/releases/editor/whats-new/6000.3.20f1#installs)
- **Code Editor**: [Jetbrains Rider 2026.2.4](https://www.jetbrains.com/rider/)
- **3D Modeller**: [Blender 5.2.3](https://www.blender.org/download/)
- **Language**: C# (.NET Standard)
- **AI Models**: Local Qwen 3.5 model and [Duck.ai](https://duck.ai). Used for basic research, bug testing, camera movement, comments, and improving coding practices.
- **Platforms**: Built for Windows x86 and Linux x86, tested on Linux and Windows through Proton. May run on other hardware if compiled to it, but is untested.
#### Unity packages used
- 2D Sprite
- Cinemachine
- Input System
- TextMeshPro
- All required dependencies

## Running the game
#### Windows
Install the Windows zip, unzip it, and run `SimpleConveyorGame.exe`
#### Linux
Install the Linux zip, unzip it, and run `LinuxBuild.x86_64`. You may need to ensure the file is set to execute as a program, this varies by distro.

## Building from source
#### Prerequisites  
- Unity 6000.3.20f1
#### Installation Steps  
Either click open project and select the project in Unity 6000.3.20f1, or select `add project from disk` in the Unity Hub
Unity should auto install all required packages from the Unity Registry, otherwise install them in Package Manager.
#### Building
At the top of the Unity window, navigate to File > Build Profiles (or press Shift+Ctrl+B), Ensure you've selected the right build platform, and click Build.

## Main Functions
#### Spawn / Delete conveyor
The conveyors are spawned on a 3D array, which handles the x, y, and z axis as an int. The array is made up of the class `PlacedConveyorInfo`, which stores information about each tile, including the GameObject inside of it, and the other tiles taken up by the conveyor inside of it. This information is useful for deleting tiles, and may be useful in the future for new features. The array elements are set to null if the tile is empty. The conveyors are spawned by calculating the placement position on the grid using dimensions of the selected conveyor and the current mouse position, with an offset to ensure the centre of the conveyor is placed on the mouse, instead of the origin (bottom left corner). Then that grid position can be used to calculate whether the tiles that the conveyor would take up are free or not, to ensure conveyors don't overlap. 
#### Conveyor belt animation
To make the conveyor belts properly animate, the UV on the provided models had to be modified. The given models had a flipped UV across their X axis for each side of the conveyor. This meant that offsetting the texture in unity would cause the arrows to point away from the centre, instead of in one direction. To fix this, half of conveyor belts UV was flipped. Whilst in blender, the conveyor belt UVs were also optimised to use more of the texture, and the dimensions of conveyors were changed. This was so that the dimensions would share a common multiple, allowing for cleaner tiling. Once the models were cleaned up, the were animated in Unity. Because materials in Unity are expensive, the materials when the game boots up, with the amount being based off of how many speeds are available to use. The conveyors couldn't share the same material, since this meant that changing the offset of one conveyor belt would change the offset of another, even if the speeds were different. Instead, each speed gets its own material, with the texture offset being changed every frame based on the speed (multiplied by Time.deltaTime to account for frame rate differences). To prevent floating point rounding errors that can be caused by infinite incrementing and decrementing of the offset, the offsets are clamped between 1 and 0 using the modulo `%` function.
#### Item Spawner
The item spawner prefab is a modified short conveyor belt prefab, with an added spawner component. The spawner component takes an array of gameobjects and a spawn speed. It starts a timer to keep track of when the next item needs to spawn, then generates a random number to choose which item to spawn, instantiates it, then resets the timer. Due to the spawner being placed over an existing conveyor component, the instantiated item automatically attaches itself to the conveyor. 
#### Visible Playspace
When the game loads, the play space plane is automatically resized to the size of the play space, to help make it clear where the bounds of the game are. This is done by calculating the bottom left and top right corners of the play space, and resizing the plane to fit both of those corners.

## Project Structure  
```
SimpleConveyorGame/
└── Assets
    ├── InputActions
    │   └── BaseInput.inputactions              #Used for all controls
    ├── Materials
    │   ├── Belt.mat
    │   ├── Black.mat
    │   ├── Metal.mat
    │   ├── PlacableFloor.mat
    │   ├── PlaceLocation.mat
    │   └── UnplaceableFloor.mat
    ├── Prefabs
    │   ├── Conveyors
    │   │   ├── ConveyorSpawner Variant.prefab
    │   │   ├── InclinedConveyor.prefab
    │   │   ├── LongConveyor.prefab
    │   │   └── ShortConveyor.prefab
    │   └── Items
    │       ├── VA_Cardboard_Box.prefab
    │       └── VA_Slim_Can.prefab
    ├── Scenes
    │   └── SampleScene.unity                    #Currently the only scene, used for everything
    ├── Scripts
    │   ├── Conveyors
    │   │   ├── ConveyorBelt.cs                  #Moves items on the conveyor belt
    │   │   ├── ConveyorManager.cs               #Handles the belt animation, speed, and UI images
    │   │   └── ConveyorSpawner.cs               #Handles spawning and deleting conveyor belts
    │   ├── Items
    │   │   ├── Item.cs                          #Makes the object fall if it's not currently on a conveyor
    │   │   └── ItemSpawner.cs                   #Spawns a random item from an array every interval
    │   ├── CameraController.cs                  #Moves the cameras focus position and keeps it in the bounds
    │   ├── ControlsManager.cs                   #Handles all the controls and sends them to the needed functions
    │   ├── CreateVisiblePlane.cs                #Sets the playspace plane to the size of the grid
    │   └── TriggerForwarder.cs                  #Forwards the OnTriggerEnter event to its parent
    ├── Textures
    │   ├── BlankIcon.png
    │   ├── ConveyorSpeedIcons.aseprite
    │   ├── ConveyorSpeedIcons.png
    │   ├── ConveyorTexture.png
    │   ├── ConveyorTypeIcons.aseprite
    │   ├── ConveyorTypeIcons.png
    │   ├── Icon-export.png
    │   ├── Icon.png
    │   └── previewtexture.png
    └── VACAC
        └── BOUTIQUE
            ├── Conveyors
            │   ├── VA_Conveyor_Belt_Generic.fbx
            │   ├── VA_Conveyor_Belt_Short.fbx
            │   └── VA_incline_Conveyor_Belt.fbx
            └── Products
                ├── VA_Cardboard_Box.fbx
                └── VA_Slim_Can.fbx
```

##Troubleshooting and bug fixing
#### Conveyor Belts overlapping
One issue that came up during the creation of this game was the conveyor belts being able to be placed so that they would overlap. This was caused by the occupied cells being incorrectly calculated. The fix to this was flip the x and y of the dimensions if the conveyor is facing up or down, to ensure it calculates the right cells to occupy.
#### Floors above 1 placing objects with an offset from the mouse
This issue happened since the floor height was being set before the floor was changed, so that the plane that the mouse clicks were being detected at was stuck on the previous floor. This was fixed by moving the line that calculated the current floor height to after the floor was changed.
#### Items falling through the conveyors
When items moved from one conveyor belt to another, they would sometimes randomly fall through the gap. This happened due to a slight gap between the conveyor triggers that detects the item. This issue was increased by dropping the item when it gets above the progress of 0.99 instead of 1. Initially this was done due to the assumption that floating point rounding errors may cause the progress to never reach 1, however, due to the progress being constantly incremented, it always surpasses 1 until that check.

## Improvements
#### Modifier Conveyors
Modifier conveyors could be added that would change the rotation or colour of the items that enter it, which could pave the way for adding goals and achievements, such as making a specific combination of the two. This gives the game more accomplishment despite being a sandbox focused game.
#### Economy
One item that would benefit this project is an economy, with a sell conveyor incrementing your money, and each conveyor and item costing money. This could be paired with modifier conveyors, incentivising the player to modify the items in as many ways as they can to increase their profit. 
#### Curved Conveyors
Due to the assets provided, curved conveyors couldn't be added, although this would be a great addition to the game. This would require a rework of the conveyor movement code, as currently it only has a start and end position, making smooth curves impossible.
#### Sounds and music
Since no sound effects or music were provided, they were unable to be added into the game. However, if this project were to be worked on further, copyright free sound effects and songs would be sourced or created, which would give this game much more feedback, and make the items feel alot more realistic
