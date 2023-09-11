### Full Documentation

https://github.com/dillondrum70/daedalus-dungeon-generator/wiki


<p align="center">
    <img src="https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/50956628-71ac-4ea7-aa90-5fd3b9df2367" alt="Daedalus Banner" style="align-items=center; justify-content=center;" />
</p>

<p align="center">
    <img src="https://img.shields.io/badge/Project%20Status-Release-green" alt="Release Status Badge" />
    <img src="https://img.shields.io/badge/Version-1.0.0-blue" alt="Version Badge" />
</p>

<p align="center">
    <img src="https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/b3fb3f1e-d136-49dc-b42f-f7a2acbe7c9f" alt="Daedalus Icon" width="100" height="100" style="align-items=center; justify-content=center;" />
</p>

_Notice:  This project is not actively being improved or worked on so the tool is provided as-is for the time being.  Requests for new features may or may not be fulfilled._

<p align="center">
    <img src="https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/354a8282-76ec-44f2-ae1c-edc9c7ad1039" alt="Demo GIF" />
</p>

# Overview

Daedalus Dungeon Generator is a plugin for Unity that can generate random dungeons using prefabs as puzzle pieces and assembling them inside a grid using Delaunay Tetrahedralization to determine efficient connections between rooms and A* to trace paths between  rooms.  This is a solo project.

This project was created with Unity 2021.3.8f1.  There are no dependencies on other packages in the project.

![Generated Dungeon](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/b700a6f8-8191-404c-ae52-d504647cf43e)

![First Person POV](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/1ecd7777-0e06-403e-80fb-c9ecb1374a07)

# Third Party Notices

Models and Textures - Infuse Studios 
* [Product Page](https://www.unrealengine.com/marketplace/en-US/product/a5b6a73fea5340bda9b8ac33d877c9e2)
* [Unreal Marketplace Agreement](https://www.unrealengine.com/en-US/marketplace-distribution-agreement)
* [Using Unreal Marketplace Assets in Unity](https://marketplacehelp.epicgames.com/s/article/Can-I-use-these-products-in-other-gaming-engines-like-Source-or-Unity?language=en_US)

Project Icon and Banner - Sandi Gerner
* [Instagram](https://instagram.com/sandissketchbook?igshid=MzRlODBiNWFlZA==)
* [Linkedin](https://www.linkedin.com/in/sandi-gerner-a47a4924a/)

# Intent

This project was made as a final project for my AI in Games course at Champlain College.  I decided later on that the project could be helpful to others so I decided to clean it up, add some editor tools to expand its use cases, and add documentation to make it easier to use and learn.  In the future, I may revisit some features to optimize them and add extra functionality.

# Potential Additions

While the project is not actively being worked on, here is a list of features that will be added next:

- [ ] Rooms higher than one cell
- [ ] Customization of light placement during generation
- [ ] Combine CellTypes.STAIRS and CellTypes.STAIRSPACE into a single 1 x 2 prefab for a more elegant solution
- [ ] Add 2D Delaunay Triangulation for cases where circumspheres of tetrahedrons become degenerate.  Additionally, handle cases where points of a tetrahedron or triangle lie on the same line (causing degenerate circumspheres for 3D AND degenerate circumcircles for 2D)
- [ ] Add randomly place props within rooms and hallways (configurable)
- [ ] Create JSON files to save data about the dungeon layout instead of prefabs (requires both producing JSON files from dungeon prefab data and parsing them to spawn those objects in)

Here are a few I've thought of doing but are less likely to happen:
- [ ] Use Wave Function Collapse to place walls, pillars, and other assets
- [ ] Restructure tool to allow for custom rooms that take up multiple cells and have defined entry and exit points that are used during generation (i.e. a great hall with a main door, two doors on the first floor to the right and left, and one door on the second floor at the top of a flight of stairs)

# Getting Started

To generate a dungeon, add a [[DungeonGenerator]] component to an object, set the different types of prefabs in the inspector (examples are included in the package with names corresponding to the variables), then open the editor tool under `Tools > Dungeon Editor` and hit generate.

![DungeonGeneratorComponent](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/a0b99177-ffe0-4a0a-a55a-6f8b892d6fa9)

[[DungeonGenerator]] component added to a game object.

![DungeonGeneratorPrefabs](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/fbd914ea-9594-43ab-a4bc-92f25715d637)

Prefabs set in [[DungeonGenerator]].

![GameObject Structure](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/f13cbbbd-4920-4256-a910-5e7606d4e954)

The resulting prefab should have the above hierarchy architecture with dungeon assets under Paths and Rooms.  This will be automatically set during generation as long as `roomParent` and `pathParent` are null.

# Algorithms

High level overviews of specific concepts

### [[Delaunay Tetrahedralization]]

### [[Minimum Spanning Tree]]

### [[A* Pathfinding]]

# Controls

The [[Editor Tool]] lets you control generation at any point in time as long as you are not working in a build.  Otherwise, the play mode controls in the demo scene include:

|Control |Function |
|:---|:---|
|Enter |Generates a new dungeon. |
|Right Click and Drag |Rotate orbit camera. |
|Scroll Wheel |Zooms in and out. |
|Space |Toggle first person (only works if there is a generated dungeon). |
|WASD |Movement in first person. |
|Mouse |Camera control in first person. |

# Process

### 1. Grid

The first part of the project that acts as the backbone for everything else is the Grid component.  This stores the state of all cells in the dungeon that way they can later be populated with assets.

![Grid Debug Draw](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/6d573558-aae1-451e-a9b1-e66cdd257721) 

### 2. Place Rooms

Rooms of various sizes are spawned at random locations within the grid.  We spawn Room prefabs and scale them to fit the grid cell.  If you would like to modify the room's assets, you can visit the Room prefab and swap them out.  Then those assets will be instantiated in place of room cells.

![Randomly Placed Rooms](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/65a18608-92e5-4cb1-87cb-bc6c8146334b)

### 3. Delaunay Tetrahedralization

Delaunay Tetrahedralization is executed on the rooms treating their centers as the points in the graph.  These will later be used to create a layout of streamlined hallways.

![A Tetrahedron](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/4b54e3de-4b9c-46bc-9ac5-4236f401c5d0)

![Tetrahedralized Dungeon](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/945ac73d-59b5-49a8-bbb0-439588fbbc65)

### 4. Minimum Spanning Tree & Random Hallways

Using the graph of all edges in the Delaunay Tetrahedralization,  we create a Minimum Spanning Tree of the different edges in said graph.  This way, players in this dungeon will have a fairly directed path throughout the entire dungeon (rather than needing to take long, winding hallways).  We store the edges excluded from the minimum spanning tree to then add a specific percentage back to the graph.  The amount of extra random hallways can be controlled under DungeonGenerator with extraHallwaysFactor.  These extra random hallways add some choice for the player so that there are multiple paths one can take to get to each room in the dungeon.

![MST Example 1](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/18a15de9-ab4c-4515-8f00-ccf412d9c8f3)

![MST Example 2](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/8bb614e6-4964-43af-a639-cfff73b72ed5)

The green line represents the MST.

### 5. A*

After determining which rooms are connected, we loop through each edge (hallway), start at one end, and use A* to pathfind to the other end of the hallway.  The algorithm uses stairs to move upwards.  We use Manhattan Distance as our heuristic for finding H since we can only move in one of the four cardinal directions from any hallway and straight up or down since stairwells include 2 cells vertically.  There are also some extra generation rules we use like forcing paths to end on the same Y level in the grid as the room rather than ending above or below the target cell in the room.

![Dungeon Without Stairs](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/87c064eb-5972-4ea1-b1b1-cbeb54fb86f6)

Without stairs, the hallway cells are disconnected.

![Dungeon With Stairs](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/e436ae63-4d07-47c6-ba58-ccd6c5b53b57)

* Red - Hallway
* Blue - Stairs
* Green - Space above staircase so player as room to climb the staircase

### 6. Place Assets

The last step is to place assets.  While many of the assets reside within the prefabs for rooms, hallways, and stairs that are placed earlier, there are other assets not part of those prefabs.  Walls and pillars are the main assets not placed during generation until this step.  Walls because depending on what the surrounding cells are, some hallways and room cells might not have walls on certain sides.  Pillars are placed here instead of in prefabs because stair cases must be rotated to face the correct direction.  This means that if the quartered sections of the pillars in the corners of room prefabs are not of order 4 symmetry, then the meshes will not line up properly causes graphical errors.  Having a separate step for pillars allows for more freedom on the art side.

![Asset Placement](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/58967122-3b9c-4b81-8dcf-64a09dfe57f9)

# Editor Tool

The Editor Tool is used to run the generator in the editor, clear the generated dungeon, and save the generated dungeon to a new prefab file at a given location with a given name.

![Menu Location](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/83eac4c0-4359-4784-b736-259f4708e080)

Menu option that opens the editor.

![Editor Window](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/30a4ce9d-8a01-49f2-abac-c98109985d9a)

* **Run Generator** - Runs the 3D dungeon generator in editor or in play mode, runs DungeonGenerator.Generate()
* **Clear Dungeon** - Clears all memory from the dungeon component, runs DungeonGenerator.Clear()
* **Save Path** - Directory where the prefab of the dungeon will be saved (Read-Only in EditorWindow)
* **Choose Folder** - Opens a Folder Dialog and sets DungeonGenerator.savePath to the resulting folder path
* **File Name** - The name of the file that the dungeon layout will be saved to (without the .prefab extension)
* **Dungeon Parent Object** - This should remain untouched, but if an empty dungeon is being saved to the prefab when one has been generated, references may have gotten tangled up somehow causing the stored Dungeon Parent Object to be hidden in the hierarchy so this is exposed so the user can simply drag the actual Dungeon GameObject reference into the window to solve the issue
* **Save to Prefab** - Takes savePath and fileName, then saves the dungeon to a prefab at <Save Path> + "/" + fileName + ".prefab"

The resulting prefab should have the following hierarchy architecture with dungeon assets under Paths and Rooms:

![GameObject Structure](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/f13cbbbd-4920-4256-a910-5e7606d4e954)