Welcome to the Map Generator Tool (name pending)!

What Does It Do?
- As the name implies, it generations a tilemap for the users whenever they want, allowing them to either save the map as an asset, or procedurally generate the map for whatever projects they have in mind. The specific map this generator is trying to make is an "objective-oriented" map which means that aside from just generating a map itself, it will also generate locations for different placs of interest accross the map, which can be used as a basis for simple level designs

How To Find & Use This Tool
1) When you go to the Scenes folder and open up the "SampleScene", you'll see an object called the "Map Generator" in the heirarchy window. Select it

2) In the inspector, you'll see that the Map Generator object contains a script called "MapGenScript" - this is where all the magic happens

3) The two buttons at the top are pretty straightforward
- "Generate The Map!" just generates the map. It takes a second or two to work though.
- "Save The Map" will save a copy of the map to "Assets/MapGenTool/SavedMaps" as a prefab.
4) Below the two buttons are the various inputs and parameters which can be used to change the map output. You get a short description of them if you hover over each parameter, but an explanaition here wouldn't hurt either. 
  - MANDATORY INPUTS - the tool WILL NOT WORK if one of these is missing
    - Map Object
      - This is the gameobject containing the tilemaps which this generator will draw to. This is also the object which will be saved as a prefab when you choose to save the map.
    - Tilemaps (Ground & Wall)
      - The game draws the generated map data to two different maps. the "Ground" map is just a basic tilemap
      - The "Wall" map contains a physics collider to prevent things from going through
    - Tiles (Ground Tile Array & Wall Tile)
      - The generator uses a variety of Ground Tiles (hence the array) to generate terrain shapes
      - The generator uses a single Wall Tile to visually declare the physical bounds of the map
     
  - GEN PRESETS - the parameters which alter the resulting tilemap
    - Map Size - Controls the general size of the map. nothing crazy
    - Map Scale - Acts as a multiplier for the size of the map, and relative distance between POIs
    - Perlin Scale - Controls how visually noisy the Ground tilemap's terrain pattern will look
    - Variance Scale - Affects how spread-out or eratic the map's resulting shape is
    - Chunk Size - The Generator builds the map in tens or hundreds of individual "chunks". this setting determines how large these chunks are
    - Border Width - Controls how wide the physical border of the Wall tilemap will be
    - Objectives - Sets the amount of major objectives which will appear on the map
    - Minor Poi Per Objective - Sets the maximum possible amount of minor objectives which show up around the map
   
  - OTHER INPUTS/DATA - not very important for seeing results
    - Critical Locs - contains an array of all Major Objectives on the map
    - Objective Point Prefas - contains all the possible gameobjects which can be instantiated at major objectives
    - Active Objective Prefabs - contains all the Major Objective gameobjects produced by this generator
   
  - DEBUG TOOLS - used to see different map data
    - Show Chunks - draws white boxes where all ground chunks are located
    - Show Border - draws gray boxes where all wall chunks are located
    - Show Critical Chunks - draws green (start), gold (major objective), or red (final) on chunks with a major objective
    - Show Minor POI - if the previous debug tool is active, this will draw cyan boxes where all minor poi are, and will draw lines connecting them to the related Major Objective

