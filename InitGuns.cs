/*------------------------------------------------------------------------------------------------------------------
-- SOURCE FILE:  InitGuns.cs
--
-- PROGRAM: InitRandomGuns
--
-- FUNCTIONS:
-- InitRandomGuns()
-- InitRandomGuns(int NumPlayers)
-- void printCoordinates()
-- void printExpandedCoordinates()
-- void fromByteArrayToList(byte[] transmittedBytes)
-- void getByteArray()
-- byte[] compressByteArray(byte[] data)
-- WeaponSpell()
-- WeaponSpell(int X,int Z)
-- WeaponSpell(int X, int Z, bool flag)
-- bool CoordinateMatch(WeaponSpell C) 
-- void BasicString()
-- string ExtendedString()
-- byte GenGunType()
-- bool OccupiedCheck(WeaponSpell genC, list<WeaponSpell> Occupied)
-- byte[] PutWeaponIntoBytes(WeaponSpell Weapon)
-- WeaponSpell GetWeaponFromBytes(byte[] weaponinbytes)
-- int numberOfWeapons(int players)
--
-- DATE: April 11th 2018
--
-- REVISIONS: April 9 2018
--		 April 5 2018
--                      March 29 2018
--                      March 26 2018
--                      March 18 2018
--		 March 17 2018
--		 March 16 2018
--		 March 15 2018
--		 March  3  2018		 
--
-- DESIGNER: Alfred Swinton
--
-- PROGRAMMER: Alfred Swinton
--
-- NOTES:
-- The above program is the logic that creates a random set of guns and spreads them randomly across 
-- coordinates clustering them around areas of interest. These guns are put into a byte array that can be 
-- sent across the network to individual clients to be placed on the map. This program also has the relevant 
-- code to depack the byte array on the client side into a simple list to be used to populate the client map with 
-- weapons.  
----------------------------------------------------------------------------------------------------------------------*/


using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace InitGuns
{
    class InitRandomGuns
    {
        static System.Random rand = new System.Random();

        // Valid Coordinates in town
        List<WeaponSpell> TownCoords = new List<WeaponSpell>();

        // Spaces Occupied By other Objects
        List<WeaponSpell> OccupiedSpaces = new List<WeaponSpell>();

        // The array of coordinates for each weapon
        List<WeaponSpell> SpawnedGuns = new List<WeaponSpell>();

        // An array of areas of interest to preferentially scatter guns around
        List<WeaponSpell> HotSpots = new List<WeaponSpell>();

        // An array of bytes to be initialized
        public byte[] pcktarray;
        public byte[] compressedpcktarray;

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: InitRandomGuns()
        --
        -- DATE: April 11th 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: InitRandomGuns()	 	
        --
        -- NOTES:
        -- Creates an empty InitRandomGuns object, can be used client side to fill up local list with data from packet
        -- that has been converted with internal methods.
        -------------------------------------------------------------------------------------------------*/
        public InitRandomGuns()
        {
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: InitRandomGuns(int Numplayers)
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: InitRandomGuns(int Numplayers)	 	
        --			Numplayers: the number of players in the game
        --
        -- NOTES:
        -- Based on the number of players in the game this constructor builds up a number of random guns clustered 
        -- around hotspots/ areas of interest and puts them into a public accessible local byte array that can be sent to -- the clients.
        --
        -------------------------------------------------------------------------------------------------*/
        public InitRandomGuns(int NumPlayers)
        {

            // Add some dummy hotspots NOTE MUST BE AT LEAST clustering AWAY FROM EDGE IN EACH DIRECTION
            HotSpots.Add(new WeaponSpell(250, 250));
            HotSpots.Add(new WeaponSpell(800, 550));
            HotSpots.Add(new WeaponSpell(750, 750));
            HotSpots.Add(new WeaponSpell(900, 100));
            HotSpots.Add(new WeaponSpell(200, 900));

            // Add Set Coordinates for spawn generation in town.
            TownCoords.Add(new WeaponSpell(-161 + R.Init.TOWNWIDTH, 57 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-41 + R.Init.TOWNWIDTH, 88 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(11 + R.Init.TOWNWIDTH, 120 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(41 + R.Init.TOWNWIDTH, 32 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-21 + R.Init.TOWNWIDTH, -3 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(23 + R.Init.TOWNWIDTH, -65 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(72 + R.Init.TOWNWIDTH, -85 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(132 + R.Init.TOWNWIDTH, 45 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(10 + R.Init.TOWNWIDTH, 75 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(18 + R.Init.TOWNWIDTH, 22 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-157 + R.Init.TOWNWIDTH, 107 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-12 + R.Init.TOWNWIDTH, 109 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(9 + R.Init.TOWNWIDTH, 64 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-161 + R.Init.TOWNWIDTH, 57 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-139 + R.Init.TOWNWIDTH, -22 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(30 + R.Init.TOWNWIDTH, -14 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-160 + R.Init.TOWNWIDTH, -88 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(133 + R.Init.TOWNWIDTH, -87 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(134 + R.Init.TOWNWIDTH, 120 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(2 + R.Init.TOWNWIDTH, -15 + R.Init.TOWNHEIGHT));
            TownCoords.Add(new WeaponSpell(-144 + R.Init.TOWNWIDTH, 70 + R.Init.TOWNHEIGHT));

            WeaponSpell Weapon;
            int counter = 0;
            int interestTracker = 0;

            // Keep generating guns depending on number of players
            while (counter < numberOfWeapons(NumPlayers))
            {
                if (counter < numberOfWeapons(NumPlayers) / R.Init.QUOTIENTTOWNGUNS)
                {
                    int getT = rand.Next(TownCoords.Count);
                    Weapon = new WeaponSpell(TownCoords[getT].X, TownCoords[getT].Z, true);
                    TownCoords.RemoveAt(getT);
                }
                else
                {
                    if (rand.NextDouble() > R.Init.PERCENTHOTSPOT)
                    {
                        Weapon = new WeaponSpell(rand.Next(0, R.Init.MAPEND),
                            rand.Next(0, R.Init.MAPEND), true);
                    }
                    else
                    {
                        // Choose a Hotspot at Random
                        interestTracker = rand.Next(0, HotSpots.Count);

                        int a = rand.Next(HotSpots[interestTracker].X - R.Init.CLUSTERING, HotSpots[interestTracker].X + R.Init.CLUSTERING);
                        int b = rand.Next(HotSpots[interestTracker].Z - R.Init.CLUSTERING, HotSpots[interestTracker].Z + R.Init.CLUSTERING);

                        Weapon = new WeaponSpell(a, b, true);
                    }
                }

                // Check if the generated coordinate is already occupied
                // if no add to coordinate array
                if (!OccupiedCheck(Weapon, OccupiedSpaces))
                {
                    counter++;
                    SpawnedGuns.Add(Weapon);

                    // Add a 12x12 box of around the spawned point to prevent weapons spawning inside/near
                    for (int i = Weapon.X - R.Init.OCCURANCESQUARE; i < Weapon.X + R.Init.OCCURANCESQUARE; i++)
                    {
                        for (int j = Weapon.Z - R.Init.OCCURANCESQUARE; j < Weapon.Z + R.Init.OCCURANCESQUARE; j++)
                        {
                            OccupiedSpaces.Add(new WeaponSpell(i, j));
                        }
                    }
                }
            }

            // Create a byte array with those coordinates
            getByteArray();
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: printCoordinates
        --
        -- DATE: April 11 2018

        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: printCoordinates()	 	
        --
        -- RETURNS: 	void	
        --
        -- NOTES:
        -- This function prints all the created weapon coordinates in the local list to the console, useful for debugging 
        -- purposes.
        -------------------------------------------------------------------------------------------------*/
        public void printCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.BasicString() + ",");
            }
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: printExpandedCoordinates
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: printExpandedCoordinates() 	
        --
        -- RETURNS: void
        --
        -- NOTES:
        -- This prints all the created weapon coordinates, types and item ID’s to the console, useful for debugging 
        -- purposes.
        --
        -------------------------------------------------------------------------------------------------*/
        public void printExpandedCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.ExtendedString() + ",");
            }
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: fromByteArrayToList
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: void fromByteArrayToList(byte[] transmittedBytes)
        --	 		byte[] transmittedBytes : An array of weapon bytes, typically sent over the network
        --
        -- RETURNS: void		
        --
        -- NOTES:
        -- This function takes a byte array of generated weapon coordinates (typically the byte array that has been 
        -- sent over the network) and puts it into a publically accessible local list of weapons to be used by clients for 
        -- putting weapons on the map.
        -------------------------------------------------------------------------------------------------*/
        public void fromByteArrayToList(byte[] transmittedBytes)
        {
            int size = transmittedBytes.Length / R.Init.INDWPNPCKT;
            int count = 0;
            byte[] tempw = new byte[R.Init.INDWPNPCKT];
            SpawnedGuns.Clear();

            for (int i = 0; i < size; i++)
            {
                Buffer.BlockCopy(transmittedBytes, count, tempw, 0, R.Init.INDWPNPCKT);
                WeaponSpell tempwpn = GetWeaponFromBytes(tempw);
                SpawnedGuns.Add(tempwpn);
                count += R.Init.INDWPNPCKT;
            }
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: getByteArray
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: void getByteArray()	 	
        --
        -- RETURNS: void	
        --
        -- NOTES: 
        -- Takes an List of Weapons and converts it to and places it in a publically available local byte array.
        -------------------------------------------------------------------------------------------------*/
        public void getByteArray()
        {
            pcktarray = new byte[R.Init.INDWPNPCKT * SpawnedGuns.Count];
            byte[] warray;
            int offset = 0;

            foreach (var w in SpawnedGuns)
            {
                warray = PutWeaponIntoBytes(w);
                Buffer.BlockCopy(warray, 0, pcktarray, offset, R.Init.INDWPNPCKT);
                offset += R.Init.INDWPNPCKT;
            }
            compressedpcktarray = compressByteArray(pcktarray);
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: compressByteArray
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Roger Zhang
        --
        -- PROGRAMMER: Roger Zhang
        --
        -- INTERFACE: byte[] compressByteArray(byte[] data) 	
        --			byte[] data: a compressed version of a byte array 
        --
        -- RETURNS: byte[] data		
        --
        -- NOTES:
        -- This function simply takes a byte array and compresses it so that less data needs to be sent across the 
        -- network.
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

        public class WeaponSpell
        {
            public static int inc = 1;
            public int X { get; set; }
            public int Z { get; set; }
            public int ID { get; set; }
            public byte Type { get; set; }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: WeaponSpell()
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: WeaponSpell()	 	
            --
            -- NOTES:
            -- Empty Weapon Object Constructor.
            -------------------------------------------------------------------------------------------------*/
            public WeaponSpell()
            {
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: WeaponSpell(int X,int Z)
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: WeaponSpell(int X,int Z)	 	
            --
            -- NOTES:
            -- Constructor for an Individual Weapon Object without a type and ID used for normal coordinates rather than 
            -- weapons themselves.
            -------------------------------------------------------------------------------------------------*/
            public WeaponSpell(int X, int Z)
            {
                this.X = X;
                this.Z = Z;
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: WeaponSpell(int X, int Z, bool flag)
            --
            -- DATE: April 11 2017
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: WeaponSpell(int X, int Z, bool flag)
            --			int X: the X coordinate of the weapon, int Z: the Z coordinate of the weapon, bool flag: 	--                                   indicates that the weapon has a type and id 			
            --
            -- NOTES:
            -- This is an individual weapon constructor with associated X and Z coordinates, it will also with the flag give it -- a randomly generated location and type and increment a variable to give it a unique weapon ID.
            -------------------------------------------------------------------------------------------------*/
            public WeaponSpell(int X, int Z, bool flag)
            {
                this.X = X;
                this.Z = Z;
                this.Type = GenGunType();
                this.ID = inc;
                inc++;
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: CoordinateMatch
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: bool CoordinateMatch(WeaponSpell C) 	
            --				
            --
            -- RETURNS: 	bool
            --			true or false based on whether a set of weapons have matching 
            --				coordinates
            --
            -- NOTES:
            -- This function gets a boolean value based on whether or not to weapons have matching coordinates. This is necessary for checking whether there spaces are occupied.
            -------------------------------------------------------------------------------------------------*/
            public bool CoordinateMatch(WeaponSpell C)
            {
                if (this.X == C.X && this.Z == C.Z)
                {
                    return true;
                }
                return false;
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: BasicString()
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: string BasicString() 	
            --				
            --
            -- RETURNS: 	string
            --			a basic string with the X and Z coordinates
            --
            -- NOTES:
            -- This function prints out the basic X and Z coordinates useful for debugging purposes.
            -------------------------------------------------------------------------------------------------*/
            public string BasicString()
            {
                return "(" + X + "," + Z + ")";
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: ExtendedString
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: string ExtendedString()	 	
            --
            -- RETURNS: string 		
            --
            -- NOTES:
            -- This function prints an individual weapon in readable format to the console with type, id and both 
            -- coordinates. Useful for debugging purposes.
            -------------------------------------------------------------------------------------------------*/
            public string ExtendedString()
            {
                return "(ID:" + ID + " Type:" + Type + "," + X + "," + Z + ")";
            }

            /*-------------------------------------------------------------------------------------------------
            -- FUNCTION: GenGunType
            --
            -- DATE: April 11 2018
            --
            -- DESIGNER: Alfred Swinton
            --
            -- PROGRAMMER: Alfred Swinton
            --
            -- INTERFACE: byte GenGunType()	 	
            --
            -- RETURNS: 	byte	
            --		the type of the gun a random value between 1 and 13
            --
            -- NOTES:
            -- Uses Random to determine the type of the gun possible types are between 1 and 13.
            -------------------------------------------------------------------------------------------------*/
            private byte GenGunType()
            {
                // Randomly seeded values
                double seed1 = rand.NextDouble();
                double seed2 = rand.NextDouble();

                // Gun Spawn 35% chance
                if (seed1 > 0.65)
                {
                    if (seed2 > 0.5)
                    {
                        //type 1 -- 50% chance
                        return R.Init.WPN1;
                    }
                    else if (seed2 > 0.15)
                    {
                        //type = 2 -- 35% chance
                        return R.Init.WPN2;
                    }
                    else
                    {
                        //type = 3 -- 15% chance
                        return R.Init.WPN3;
                    }
                }

                // Spell Spawn 65% chance -- 10% chance per spell
                else
                {
                    if (seed2 > 0.9)
                    {
                        return R.Init.WPN4;
                    }
                    else if (seed2 > 0.8)
                    {
                        return R.Init.WPN5;
                    }
                    else if (seed2 > 0.7)
                    {
                        return R.Init.WPN6;
                    }
                    else if (seed2 > 0.6)
                    {
                        return R.Init.WPN7;
                    }
                    else if (seed2 > 0.5)
                    {
                        return R.Init.WPN8;
                    }
                    else if (seed2 > 0.4)
                    {
                        return R.Init.WPN9;
                    }
                    else if (seed2 > 0.3)
                    {
                        return R.Init.WPN10;
                    }
                    else if (seed2 > 0.2)
                    {
                        return R.Init.WPN11;
                    }
                    else if (seed2 > 0.1)
                    {
                        return R.Init.WPN12;
                    }
                    else
                    {
                        return R.Init.WPN13;
                    }
                }
            }
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: OccupiedCheck
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: bool OccupiedCheck(WeaponSpell genC, list<WeaponSpell> Occupied)	 	
        --			WeaponSpell genC: an individual weapon, list<WeaponSpell> Occupied : a list of 
        --				weapons that occupy spaces
        --
        -- RETURNS: bool
        -- 		a boolean value whether or not the weapon which was inputed values have been occupied. 	
        -- NOTES:
        -- This function takes a weapon and return true or false based on whether the coordinates of that weapon are --  already occupied in an occupiedspaces list.
        -------------------------------------------------------------------------------------------------*/
        public static bool OccupiedCheck(WeaponSpell genC, List<WeaponSpell> Occupied)
        {
            return Occupied.Contains(genC);
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: PutWeaponIntoBytes(WeaponSpell Weapon)
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: byte[] PutWeaponIntoBytes(WeaponSpell Weapon)	 	
        --			WeaponSpell Weapon: a weapon object with coordinates, type, id
        -- RETURNS: byte[]		
        --		an individual weapon converted into byte form
        --
        -- NOTES:
        -- This function converts individual weapon objects into bytes used aggregately to create a byte array in 
        -- getByteArray function.
        -------------------------------------------------------------------------------------------------*/
        public static byte[] PutWeaponIntoBytes(WeaponSpell Weapon)
        {
            byte[] wpn = new byte[R.Init.INDWPNPCKT];

            byte[] type = new byte[sizeof(byte)];
            type[0] = Weapon.Type;
            Buffer.BlockCopy(type, 0, wpn, 0, 1);

            byte[] ID = BitConverter.GetBytes(Weapon.ID);
            Buffer.BlockCopy(ID, 0, wpn, R.Init.WEAPONOFFSETID, sizeof(int));

            byte[] X = BitConverter.GetBytes(Weapon.X);
            Buffer.BlockCopy(X, 0, wpn, R.Init.WEAPONOFFSETX, sizeof(int));

            byte[] Z = BitConverter.GetBytes(Weapon.Z);
            Buffer.BlockCopy(Z, 0, wpn, R.Init.WEAPONOFFSETZ, sizeof(int));

            return wpn;
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: GetWeaponFromBytes
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: WeaponSpell GetWeaponFromBytes(byte[] weaponinbytes)	 	
        --			byte[] weaponinbytes : an individual weapon in byte array format 1 byte type, 4 bytes ID, -- 				4 byte X coordinate, 4 bytes Z coordinate
        --
        -- RETURNS: WeaponSpell 
        --			An individual weapon object containing coordinates, type, id		
        --
        -- NOTES:
        -- This function takes a byte array the length of a single weapon and converts it into an object. Used in 
        -- fromByteArraytoList to convert byte array to a list of weapons.
        -------------------------------------------------------------------------------------------------*/
        public WeaponSpell GetWeaponFromBytes(byte[] weaponinbytes)
        {
            WeaponSpell Weapon = new WeaponSpell();

            Weapon.Type = weaponinbytes[0];
            Weapon.ID = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETID);
            Weapon.X = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETX);
            Weapon.Z = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETZ);

            return Weapon;
        }

        /*-------------------------------------------------------------------------------------------------
        -- FUNCTION: numberOfWeapons
        --
        -- DATE: April 11 2018
        --
        -- DESIGNER: Alfred Swinton
        --
        -- PROGRAMMER: Alfred Swinton
        --
        -- INTERFACE: int numberOfWeapons(int players) 	
        --			int players: number of players			
        --
        -- RETURNS: 	int
        --			the number of weapons to be generated
        --
        -- NOTES:
        -- This function calculates the number of weapons to be generated by using the number of players multiplied 
        -- by a weapon multiplier constant.
        -------------------------------------------------------------------------------------------------*/
        public static int numberOfWeapons(int players)
        {
            return players * R.Init.PLAYERMULT;
        }
    }
}
