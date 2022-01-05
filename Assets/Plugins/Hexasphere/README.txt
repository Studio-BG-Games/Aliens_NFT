************************************
*      HEXASPHERE GRID SYSTEM      *
*     by Ramiro Oliva (Kronnect)   * 
*            README FILE           *
************************************


How to use this asset
---------------------
Firstly, you should run the Demo Scene provided to get an idea of the overall functionality.
Later, you should read the documentation and experiment with the tool.

Hint: to add an hexasphere to your scene, select the option from the top menu GameObject -> 3D Object -> Hexasphere.


Demo Scene
----------
There's one demo scene, located in "Demo" folder. Just go there from Unity, open "Demo" scene and run it.


Documentation/API reference
---------------------------
The PDF is located in the Documentation folder. It contains instructions on how to use the asset and the API so you can control it from your code.


Support
-------
Please read the documentation PDF and browse/play with the demo scene and sample source code included before contacting us for support :-)

* Support: contact@kronnect.com
* Website-Forum: http://kronnect.com
* Twitter: @KronnectGames


Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Hexasphere Grid System will be eventually available on the Asset Store.


Version history
---------------

Version 5.3 31/Oct/2021
- Added Smoothness and Specular Tint options to inspector (compatible with non-extruded modes)

Version 5.2 19/Jul/2021
- Demo scene 1: added example of spawning a prefab and moving around the sphere by cliking on target tile
- API: OnTileClick event will now receive the tile index even if highlight is disabled

Version 5.1 9/Mar/2021
- Generation performance improvements
- Minor improvements to Grid Editor selection of multile tiles
- [Fix] Exported settings were not loaded when disabling/enabling the config component several times

Version 5.0
- Added "Minimum Light" option
- API: added GetTileVertexLatLon(). Returns latitude/longitude of a tile vertex

Version 4.9.1
- [Fix] Fixed bevel style with URP pipeline
- [Fix] Fixed tile colors issue with Unity 2019.3 due to a known Unity pipeline tag bug

Version 4.9
- API: added GetTileAtUV. Returns the tile index corresponding to a UV coordinate of the hexasphere texture
- [Fix] Fixed issue when a tile is temporarily colored/textured and the tile extrusion amount is changed

Version 4.8
- Added "Drag Threshold" property. Defines the minimum rotation angle to consider a drag
- Added "Click Duration" property. Defines the maximum time between button press and release to account for a click

Version 4.7.1
- [Fix] OnTileClick no longer fires when dragging the hexasphere on mobile
- [Fix] LWRP support: fixed lighting direction issue

Version 4.7
- Respect Other UI option added to inspector: prevents interaction with hexasphere when an UI element is under pointer
- Added "ZWrite" option to inspector when transparent mode is enabled
- Added "Double Sided" option to inspector when transparent mode is enabled

Version 4.6.3
- Added support for shadow/lighting to temporary tiles

Version 4.6.2
- [Fix] Fixed issues when building for Android

Version 4.6.1
- API: optimized performance and memory usage of GetTilesWithinSteps
- API: GetTilesWithinSteps and FindPath now can accept an user provided List as argument to reduce memory allocations
- Pathfinding crossing costs now expressed in "float" instead of "int" to improve precision
- [Fix] Fixed compatibility with WPM Globe Edition

Version 4.5
- New "Bevel" option under Extrusion section
- Added "Cast Shadows" to inspector
- Added "Cull Back Tiles" to improve performance
- API: added SetTileVertexElevation / GetTileVertexElevation
- API: added ApplyHeightMapToVertices

Version 4.3
- API: added GetTilesWithinSteps(min, max)
- [Fix] Disabling shadow support on Hexasphere affects other children gameobjects

Version 4.2.5
- API: added GetTilesWithinTwoTiles()
- Lighting now uses directional light intensity and color
- [Fix] Fixed GetTilesWithinSteps bug

Version 4.2.3:
- [Fix] Hexasphere layer is now preserved when recreating child meshes

Version 4.2.2:
- [Fix] Removed runtime log message related to material _MainTex property

Version 4.2.1:
- Transparency no longer global shader values. Can specify custom transparency per hexasphere.
- [Fix] Fixes issue with SetTileTexture when using same texture and different tint colors

Version 4.2:
- Added lighing and self-shadows support
- Added ambient light color

Version 4.1:
- SetTileMaterial now respects material tiling and offset
- Added global Transparency factor for tiles (under Transparent toggle in inspector)
- Added background tile culling option when tile transparency is enabled
- Added tile tint color option (applies to all tiles, colored or non-colored tiles, while default tile color only applies to non-colored tiles)

Version 4.0:
- Support for LWRP
- Added transparent option. Demo: https://youtu.be/wownY4zX-PM

Version 3.7:
- Added lastClickedTileIndex
- API: Added ParentAndAlignToTile utility method (aligns ositions, rotate and scale with the tile)
- API: Added SetTileTextureRotationToNorth
- Fixes and improvements

Version 3.6
- Smart edges: new option to remove edges on adjacent tiles with same material
- Added right button drag option (moves left mouse button behaviour to right button)

Version 3.5
- New option to make extrusion (altitude) value contribute to the crossing cost of tiles
- Grid Editor: added support for multi-tile selection
- API: Added Set/GetTileCrossCost (custom pathfinding per-tile crossing cost)
- API: Added GetTileLatLon/GetTileUV (only available after calling ApplyHeightMap() / ApplyColors())

Version 3.4
- Demo scene 1: new example of adding texts over tile positions (button "Spawn Texts")
- FlyTo() now preserves orientation when allowed axis is set to STRAIGHT
- [Fix] Fixed GetTileCenter bug when includeExtrusion argument is used

Version 3.3
- Ability to hide individual tiles (ToggleTile, HideTile, ShowTile)
- Added example to spawn objects in demo scene 1 (new button "Spawn Objects")

Version 3.2
- Added support for shadow receiving (new option in the inspector)
- Added new rotation mode (Straight) with minimum distance to poles option
- API: Added GetTilesWithinSteps (... criteria)
- API: Added GetTilesWithinDistance (... criteria)

Version 3.1
- New demo scene 3: Quake demo
- Added Rotation Axis option
- Minor general improvements (new API overloads, better click interaction, ...)

Version 3.0
- New Grid Editor: allows tile customization in Scene View at Editor time
- New Wireframe options (extruded mode only): color from tile, color intensity
- Tiles can be grouped to define different path-finding masks
- API: New SetTileGroup / GetTileGroup

Version 2.0
- Added new demo scene: territory extrusion and coloring
- Added ability to rotate sphere pressing right mouse button
- API: new GetTileColor overload, ClearTilesExtrusion, ApplyColors

Version 1.7
- API: Added GetTilesWithinDistance, GetTilesWithinSteps

Version 1.6
- Added inspector buttons to export wireframe and model to asset

Version 1.5.1
- [Fix] Fixed interaction when 2 or more hexaspheres are in the same scene

Version 1.5
- [Fix] Fixed iOS compatibility issues

Version 1.4
- API: Added GetTileAtLocalPos, SetTileTextureRotation, GetTileTextureRotation, GetTileWorldSpaceVertex
- [Fix] Fixed textures vertical orientation

Version 1.3.1
- Support for SM 2.0 with basic features

Version 1.3
- API: Added GetTileNeighbours, GetTileNeighboursTiles, GetTileAtPolarOpposite

Version 1.2
- New inverted mode (sits you at the center of the hexasphere!)
- New VR options

Version 1.1
- Option to use a separated water mask when loading a heightmap
- Option to shift vertices to control pentagon positions
- Loading heightmap now automatically sets tile canCross to false in water positions

Version 1.0 2017-MAY First Release
