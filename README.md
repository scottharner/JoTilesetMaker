# JoTilesetMaker
This is a utility to be used with the Jo Engine game dev kit for Sega Saturn. 
It allows you to combine several TGA tiles into a single tileset. As the tileset 
is built, the tileset configuration code is output to the window so that the 
user can copy and paste it into game code. 

The utility performs several error checks. All tiles must have the same dimensions. 
The tileset name must be 8 characters or fewer, excluding extension. None of the tiles 
may be compressed.

## TODO
- Check size mismatch error
- Use free ui theme
- Pre-populate file path if already selected
- Move strings to resources
- Implement button enabling
- Test if all bit depths work
- Install/prerequisite story
