# JoTilesetMaker
This is a utility to be used with the Jo Engine game dev kit for Sega Saturn. 
It allows you to combine several TGA tiles into a single tileset. As the tileset 
is built, the tileset configuration code is output to the window so that the 
user can copy and paste it into game code. 

The utility performs error checks. All tiles must have the same dimensions. 
The tileset name must be 8 characters or fewer, excluding extension.

## Instructions
1. Add tiles
2. Set tileset file path
3. Set bit depth
4. Set background color
5. Correct any listed errors
6. Build the tileset
7. Copy generated code from the code tab
8. Paste the generated code into your code editor

## TODO
- Implement button enabling
	- copy definition
	- copy initialization
- Test if all bit depths work
- Install/prerequisite story
