### Full Documentation

https://github.com/dillondrum70/daedalus-dungeon-generator/wiki


<p align="center">
    <img src="https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/35d9cc2d-6d57-4b8b-9b4b-232f4c9f3e46" alt="Daedalus Banner" style="align-items=center; justify-content=center;" />
</p>

<p align="center">
    <img src="https://img.shields.io/badge/Project%20Status-Release-green" alt="Release Status Badge" />
    <img src="https://img.shields.io/badge/Version-1.0.0-blue" alt="Version Badge" />
</p>

<p align="center">
    <img src="https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/101e5267-8236-4328-afcd-e4628826dfc9" alt="Daedalus Icon" width="100" height="100" style="align-items=center; justify-content=center;" />
</p>

_Notice:  This project is not actively being improved or worked on so the tool is provided as-is for the time being.  Requests for new features may or may not be fulfilled._

# Overview

Daedalus Dungeon Generator is a plugin for Unity that can generate random dungeons using prefabs as puzzle pieces and assembling them inside a grid using Delaunay Tetrahedralization to determine efficient connections between rooms and A* to trace paths between  rooms.  This is a solo project.

This project was created with Unity 2021.3.8f1.  There are no dependencies on other packages in the project.

![Generated Dungeon](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/633241fd-ebcd-49bb-bc53-d74dcc2e19c9)

![First Person POV](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/cd0cf4f9-b2af-4994-82d3-b9db0a2418a0)

# Disclaimer

Project Icon and Banner - Sandi Gerner
* Instagram - https://instagram.com/sandissketchbook?igshid=MzRlODBiNWFlZA==
* Linkedin - https://www.linkedin.com/in/sandi-gerner-a47a4924a/

Models and Textures - Infuse Studios 
* Product Page - https://www.unrealengine.com/marketplace/en-US/product/a5b6a73fea5340bda9b8ac33d877c9e2?sessionInvalidated=true

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

If you get an error about the `/Samples/Scenes/` folder, do not worry.  Unity packages do not support scenes but it is not required for the package to work.

![image](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/74fc2808-745b-4382-9fc8-81c6f2e6e27f)

[DungeonGenerator](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/DungeonGenerator) component added to a game object.

![image](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/c4c07afd-03be-419e-9c9d-bfcb371741a4)

Prefabs set in [DungeonGenerator](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/DungeonGenerator).

# Algorithms

High level overviews of specific concepts

### [Delaunay Tetrahedralization](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/Delaunay-Tetrahedralization)

### [Minimum Spanning Tree](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/Minimum-Spanning-Tree)

### [A* Pathfinding](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/A%2A-Pathfinding)

# Controls

The [Editor Tool](https://github.com/dillondrum70/daedalus-dungeon-generator/wiki/Editor-Tool) lets you control generation at any point in time as long as you are not working in a build.  Otherwise, the play mode controls in the demo scene include:

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

![Grid Debug Draw](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/00a45fe7-3799-4775-8ed8-c33ee8c574f4) 

### 2. Place Rooms

Rooms of various sizes are spawned at random locations within the grid.  We spawn Room prefabs and scale them to fit the grid cell.  If you would like to modify the room's assets, you can visit the Room prefab and swap them out.  Then those assets will be instantiated in place of room cells.

![Randomly Placed Rooms](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/00d3cc80-a759-43bb-8925-94f66facca4c)

### 3. Delaunay Tetrahedralization

Delaunay Tetrahedralization is executed on the rooms treating their centers as the points in the graph.  These will later be used to create a layout of streamlined hallways.

![A Tetrahedron](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/2e1feaf8-254a-4635-94c4-955492fff7a1)

![Tetrahedralized Dungeon](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/46ef57c0-4e03-4255-b2ba-18248823fe43)

### 4. Minimum Spanning Tree & Random Hallways

Using the graph of all edges in the Delaunay Tetrahedralization,  we create a Minimum Spanning Tree of the different edges in said graph.  This way, players in this dungeon will have a fairly directed path throughout the entire dungeon (rather than needing to take long, winding hallways).  We store the edges excluded from the minimum spanning tree to then add a specific percentage back to the graph.  The amount of extra random hallways can be controlled under DungeonGenerator with extraHallwaysFactor.  These extra random hallways add some choice for the player so that there are multiple paths one can take to get to each room in the dungeon.

![MST Example 1](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/ed555400-c66e-470e-a32a-2fe8d3cd26b7)

![MST Example 2](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/4c8e5fbf-35cc-4819-ae4a-d9eacc527aba)

The green line represents the MST.

### 5. A*

After determining which rooms are connected, we loop through each edge (hallway), start at one end, and use A* to pathfind to the other end of the hallway.  The algorithm uses stairs to move upwards.  We use Manhattan Distance as our heuristic for finding H since we can only move in one of the four cardinal directions from any hallway and straight up or down since stairwells include 2 cells vertically.  There are also some extra generation rules we use like forcing paths to end on the same Y level in the grid as the room rather than ending above or below the target cell in the room.

![Dungeon Without Stairs](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/5cac1d31-2d9a-46aa-8f3c-d65d150fd68c)

Without stairs, the hallway cells are disconnected.

![Dungeon With Stairs](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/92ca9408-b8a1-4401-a480-66e1f9e8a55a)

* Red - Hallway
* Blue - Stairs
* Green - Space above staircase so player as room to climb the staircase

### 6. Place Assets

The last step is to place assets.  While many of the assets reside within the prefabs for rooms, hallways, and stairs that are placed earlier, there are other assets not part of those prefabs.  Walls and pillars are the main assets not placed during generation until this step.  Walls because depending on what the surrounding cells are, some hallways and room cells might not have walls on certain sides.  Pillars are placed here instead of in prefabs because stair cases must be rotated to face the correct direction.  This means that if the quartered sections of the pillars in the corners of room prefabs are not of order 4 symmetry, then the meshes will not line up properly causes graphical errors.  Having a separate step for pillars allows for more freedom on the art side.

![Asset Placement](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/396a875d-4641-4b5e-a2e2-a660087bd332)

# Editor Tool

The Editor Tool is used to run the generator in the editor, clear the generated dungeon, and save the generated dungeon to a new prefab file at a given location with a given name.

![Menu Location](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/2c0a2a7c-3030-48b5-82e5-01ca9b25f811)

Menu option that opens the editor.

![Editor Window](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/8a156546-1d98-4efc-9533-5305a531a3a1)

* **Run Generator** - Runs the 3D dungeon generator in editor or in play mode, runs DungeonGenerator.Generate()
* **Clear Dungeon** - Clears all memory from the dungeon component, runs DungeonGenerator.Clear()
* **Save Path** - Directory where the prefab of the dungeon will be saved (Read-Only in EditorWindow)
* **Choose Folder** - Opens a Folder Dialog and sets DungeonGenerator.savePath to the resulting folder path
* **File Name** - The name of the file that the dungeon layout will be saved to (without the .prefab extension)
* **Dungeon Parent Object** - This should remain untouched, but if an empty dungeon is being saved to the prefab when one has been generated, references may have gotten tangled up somehow causing the stored Dungeon Parent Object to be hidden in the hierarchy so this is exposed so the user can simply drag the actual Dungeon GameObject reference into the window to solve the issue
* **Save to Prefab** - Takes savePath and fileName, then saves the dungeon to a prefab at <Save Path> + "/" + fileName + ".prefab"

The resulting prefab should have the following hierarchy architecture with dungeon assets under Paths and Rooms:

![GameObject Structure](https://github.com/dillondrum70/daedalus-dungeon-generator/assets/70776550/93494b7d-1df0-42d2-ae56-7af045d138ae)