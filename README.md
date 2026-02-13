Welcome to the Map Generator Tool (name pending)!

What Does It Do?
- As the name implies, it generations a tilemap for the users whenever they want, allowing them to either save the map as an asset, or procedurally generate the map for whatever projects they have in mind. The specific map this generator is trying to make is an "objective-oriented" map which means that aside from just generating a map itself, it will also generate locations for different placs of interest accross the map, which can be used as a basis for simple level designs

How To Find & Use This Tool
1) When you go to the Scenes folder and open up the "SampleScene", you'll see an object called the "Map Generator" in the heirarchy window. Select it

2) In the inspector, you'll see that the Map Generator object contains a script called "MapGenScript" - this is where all the magic happens

3) The two buttons at the top are pretty straightforward
- "Generate The Map!" just generates the map. It takes a second or two to work though. This is used for in-editor purposes
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
     
  - MAP MAKER TYPE - 
     
  - GEN PRESETS - the parameters which alter the resulting tilemap
    - Map Maker Type - determines the style of generation
      - Blob - generates a somewhat circular map with major objectives evenly spread out. Encourages players to explore
      - Level - generates a linear map with major objectives connect through a single line. Encourages players to get the job done
    - Gen Preset - controls the output based on the Map Maker Type
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
   
See a Video Demo:
https://youtu.be/QPgFElA61vU

LIMITATIONS
- This tool runs SLOW depending on the settings. The tool has never caused the editor to crash, but there have been freak incidents where the generation takes several minutes to complete. Be careful with map size and chunk scale, and be ESPECIALLY careful with map scale. Massive maps take an incredibly long time to load. Be sure to keep these values low whenever possible.


Some More Examples

<img width="505" height="507" alt="image" src="https://github.com/user-attachments/assets/d2325bec-be5d-430e-a366-19ebe74818ad" />
<img width="550" height="445" alt="image" src="https://github.com/user-attachments/assets/cbba3fec-caf5-47b8-9385-c792ae17b6cc" />
<img width="538" height="351" alt="image" src="https://github.com/user-attachments/assets/57626577-1000-4c11-b43d-3efa0d809907" />
<img width="395" height="345" alt="image" src="https://github.com/user-attachments/assets/3269bf90-ed8d-400b-8e4e-807ba5106271" />
<img width="580" height="399" alt="image" src="https://github.com/user-attachments/assets/6b9b2624-c064-4ddc-bdd5-2b60ce259b51" />
<img width="844" height="319" alt="image" src="https://github.com/user-attachments/assets/deed2e47-0d83-4586-9372-b1e96ae4298b" />
<img width="695" height="331" alt="image" src="https://github.com/user-attachments/assets/4fca8906-a2f0-4bcf-af7f-b79ae9c8ac0a" />





