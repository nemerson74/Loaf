using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTilemapPipeline
{
    /// <summary>
    /// A class representing the Tilemap within the content pipeline. This should 
    /// correspond to a runtype class that will duplicate much of the structure.
    /// When we complile the content project, we serialize the state of that structure,
    /// into an .xnb file.  When we load the .xnb file in our game, we deserialize it
    /// into the runtype type. 
    /// 
    /// We need to include the [ContentSerializerRuntimeTypeAttribute] to indicate the 
    /// runtime type (which we specify as a string containing the fully qualified class
    /// name, followed by a comma, and then the namespace).
    /// 
    /// Pipeline classes may (and often do) have additional properties/fields that aren't 
    /// needed in the runtime version.  We can mark those with the [ContentSeriailizerIgnore]
    /// attribute. All attributes that will be serialized/deserialized need to be specified
    /// in the *same order* in both classes to avoid mixing the values up.
    /// </summary>

    [ContentSerializerRuntimeType("LoafGame.HexTilemap, LoafGame")]
    public class HexTilemapContent
    {
        /// <summary>Map dimensions</summary>
        public int MapWidth, MapHeight;

        /// <summary>Tile dimensions</summary>
        public int TileWidth, TileHeight;

        /// <summary>The tileset texture</summary>
        public Texture2DContent TilesetTexture;

        /// <summary>The tileset data</summary>
        public Rectangle[] Tiles;

        /// <summary>The map data</summary>
        public int[] TileIndices;

        /// <summary> The tileset image filename </summary>
        [ContentSerializerIgnore]
        public String TilesetImageFilename;
    }
}
