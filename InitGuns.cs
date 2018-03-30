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


        // Empty Initialization from pulling from byte array
        public InitRandomGuns()
        {
        }

        // Constructor takes the number of players
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

        // Basic Coordinates Printed
        public void printCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.BasicString() + ",");
            }
        }

        // Coordinates Printed with type and ID
        public void printExpandedCoordinates()
        {
            foreach (var w in SpawnedGuns)
            {
                Console.Write(w.ExtendedString() + ",");
            }
        }

        // Take a byteArray and fill spawnedGuns list from bytearray
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

        // Output the list of weapons as a bytearray
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

        // Roger
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
            public static int inc = 0;
            public int X { get; set; }
            public int Z { get; set; }
            public int ID { get; set; }
            public byte Type { get; set; }

            // Empty Constructor
            public WeaponSpell()
            {
            }

            // Plain Coordinate Constructor
            public WeaponSpell(int X, int Z)
            {
                this.X = X;
                this.Z = Z;
            }

            // Coordinate Constructor with Types
            public WeaponSpell(int X, int Z, bool flag)
            {
                this.X = X;
                this.Z = Z;
                this.Type = GenGunType();
                this.ID = inc;
                inc++;
            }

            // Check if two sets of coordinates match
            public bool CoordinateMatch(WeaponSpell C)
            {
                if (this.X == C.X && this.Z == C.Z)
                {
                    return true;
                }
                return false;
            }

            // Basic String output
            public string BasicString()
            {
                return "(" + X + "," + Z + ")";
            }

            // Extended String output with weapon/spell type
            public string ExtendedString()
            {
                return "(ID:" + ID + " Type:" + Type + "," + X + "," + Z + ")";
            }

            // Randomly Generate a type of gun
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

        // Check if a certain set of spaces is occupied
        public static bool OccupiedCheck(WeaponSpell genC, List<WeaponSpell> Occupied)
        {
            return Occupied.Contains(genC);
        }

        // Takes a Weapon Object and puts it into byte array format
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

        // Gets a weapon type from a bytearray format weapon
        public WeaponSpell GetWeaponFromBytes(byte[] weaponinbytes)
        {
            WeaponSpell Weapon = new WeaponSpell();

            Weapon.Type = weaponinbytes[0];
            Weapon.ID = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETID);
            Weapon.X = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETX);
            Weapon.Z = BitConverter.ToInt32(weaponinbytes, R.Init.WEAPONOFFSETZ);

            return Weapon;
        }

        // Generates a number of guns depending on the number of players
        public static int numberOfWeapons(int players)
        {
            return players * R.Init.PLAYERMULT;
        }
    }
}
