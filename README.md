# JoTilesetMaker
This is a utility to be used with the Jo Engine game dev kit for Sega Saturn. 
It allows you to combine several TGA tiles into a single tileset. As the tileset 
is built, the tileset configuration code is output to the window so that the 
user can copy and paste it into game code. 

The utility performs several error checks. All tiles must have the same dimensions. 
The tileset name must be 8 characters or fewer, excluding extension. None of the tiles 
may be compressed.

## TODO
- Add errors list
- Check size mismatch error
- Check file name length
- Implement button enabling
- Use free ui theme
- Add global error handler
- Move strings to resources
- Install/prerequisite story
- Select image bits
- Select background color
- Order images
- Pre-populate file path if already selected
