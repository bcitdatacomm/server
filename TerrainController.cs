using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

/*---------------------------------------------------------------------------------------
--	SOURCE FILE:	TerrainController.cs
--
--	PROGRAM:		TerrainController
--
--	FUNCTIONS:		public TerrainController()
--                  public bool GenerateEncoding()
--                  public bool Instantiate()
--                  public byte[] compressByteArray()
--                  public byte[] decompressByteArray()
--                  public void compressData()
--                  public void LoadByteArray()
--
--	DATE:			Feb 16th, 2018
--
--	REVISIONS:		Feb 24th, 2018
--
--	DESIGNERS:		Angus Lam, Benny Wang, Roger Zhang
--
--	PROGRAMMER:		Angus Lam, Benny Wang, Roger Zhang
--
--	NOTES:
--     This is a csharp script to initialize a 2D terrain consists of
--     cactus, bushes and ground.
---------------------------------------------------------------------------------------*/
public class TerrainController
{
    /*
     * Tile Types
     * Ground = 0
     * Cactus = 1
     * Bush = 2
     */
    enum TileTypes
    {
        GROUND,
        CACTUS,
        BUSH,
        BUILDINGS,
        HOTSPOTS
    };

    /*
     * Encoding structure
     * tiles - 2D array
     * buildings - 1D building array
     */
    public struct Encoding
    {
        public byte[,] tiles;
    };

    // The map encoding
    public Encoding Data { get; set; }
    // The compressed version of the map encoding
    public byte[] CompressedData { get; set; }

    // Width of the terrain
    public long Width { get; set; }

    // Length of the terrain
    public long Length { get; set; }

    // Tile size
    public long TileSize { get; set; }

    // Cactus appearing percent
    public float CactusPerc { get; set; }

    // Bush appearing percent
    public float BushPerc { get; set; }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: TerrainController()
    --
    -- DATE: Feb 16th, 2018
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Angus Lam, Benny Wang, Roger Zhang
    --
    -- PROGRAMMER: Angus Lam, Benny Wang, Roger Zhang
    --
    -- INTERFACE: TerrainController()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- The constructor for terrainController, sets the default width, height,
    -- Cactus, bush percent values for the terrain.
    -------------------------------------------------------------------------------------------------*/
    public TerrainController()
    {
        this.Width = R.Game.Terrain.DEFAULT_WIDTH;
        this.Length = R.Game.Terrain.DEFAULT_LENGTH;
        this.TileSize = R.Game.Terrain.DEFAULT_TILE_SIZE;
        this.CactusPerc = R.Game.Terrain.DEFAULT_CACTUS_PERC;
        this.BushPerc = R.Game.Terrain.DEFAULT_BUSH_PERC;
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: GenerateEncoding()
    --
    -- DATE: Feb 18, 2018
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Roger Zhang
    --
    -- PROGRAMMER: Roger Zhang
    --
    -- INTERFACE: GenerateEncoding()
    --
    -- RETURNS: boolean
    --
    -- NOTES:
    -- Generates an encoded 2D array with given width and height.
    -- Populates the map array with tile types based on given coefficients.
    -------------------------------------------------------------------------------------------------*/
    public bool GenerateEncoding()
    {
        Random rand = new Random();
        float randomValue;
        byte[,] map = new byte[this.Width, this.Length];

        for (long i = 0; i < this.Width; i++)
        {
            for (long j = 0; j < this.Length; j++)
            {
                // Check for border
                if (i == 0 || i == this.Width || j == 0 || j == this.Length)
                {
                    map[i, j] = (byte)TileTypes.GROUND;
                }
                else
                {
                    // Changed by Alam

                    // Changed back to just being 0.0 to 1.0
                    randomValue = Convert.ToSingle(rand.NextDouble());

                    // Changed the comparison signs around
                    if (randomValue > R.Game.Terrain.DEFAULT_BUILDING_PERC)
                    {
                        map[i, j] = (byte)TileTypes.BUILDINGS;
                    }
                    else if (randomValue > this.CactusPerc)
                    {
                        map[i, j] = (byte)TileTypes.CACTUS;
                    }
                    else if (randomValue > this.BushPerc)
                    {
                        map[i, j] = (byte)TileTypes.BUSH;
                    }
                    else
                    {
                        map[i, j] = (byte)TileTypes.GROUND;
                    }
                }
            }
        }

        this.Data = new Encoding() { tiles = map };
        this.compressData();

        return true;
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: compressData()
    --
    -- DATE: Jan 23, 2018
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Benny Wang
    --
    -- PROGRAMMER: Benny Wang
    --
    -- INTERFACE: compressData()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Compresses whatever is inside the Data member variable of this class and places it into the
    -- CompressedData member of this class.
    -------------------------------------------------------------------------------------------------*/
    private void compressData()
    {
        byte[] tmp = { };
        List<byte> compressed = new List<byte>();

        tmp = System.BitConverter.GetBytes(this.Width);
        foreach (byte t in tmp)
        {
            compressed.Add(t);
        }
        tmp = System.BitConverter.GetBytes(this.Length);
        foreach (byte t in tmp)
        {
            compressed.Add(t);
        }

        for (int i = 0; i < this.Data.tiles.GetLength(0); i++)
        {
            for (int j = 0; j < this.Data.tiles.GetLength(1); j++)
            {
                compressed.Add(this.Data.tiles[i, j]);
            }
        }

        this.CompressedData = compressByteArray(compressed.ToArray());
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: compressByteArray()
    --
    -- DATE: Feb 28, 2018
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Roger Zhang
    --
    -- PROGRAMMER: Roger Zhang
    --
    -- INTERFACE: compressByteArray(byte[] input)
    --              byte[] input: They byte array to compress.
    --
    -- RETURNS: byte array of compressed data
    --
    -- NOTES:
    -- Compress the byteArrayData to a smaller size using system I/O.
    -------------------------------------------------------------------------------------------------*/
    public static byte[] compressByteArray(byte[] data)
    {
        using (var compressedStream = new MemoryStream())
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }
    }
}
