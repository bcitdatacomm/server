using System;

namespace R
{
    // Contains all the constants associated with networking and the packet
    public static class Net
    {
        public const ushort PORT = 42069;
        public const Int32 TCP_BUFFER_SIZE = 8192;
	    public const ushort MAX_PLAYERS = 30;
        public const ushort TIMEOUT = 30;
        public const short TIMEOUT_ERRNO = -11;

        // Contains constants associated with the header type of the packet
        public static class Header
        {
            public const byte INIT_PLAYER = 0;
            public const byte TICK = 85;
            public const byte NEW_CLIENT = 69;
            public const byte ACK = 170;

			public const byte TERRAIN_DATA = 55;
			public const byte SPAWN_DATA = 56;
        }

        // Contains constants associated with the packet offset or distance into the packet
        public static class Offset
        {
            // Offsets for client to server packet
            public const int PID = 1;
            public const int X = PID + 1;
            public const int Z = X + 4;
            public const int R = Z + 4;
            public const int WEAPON_ID = R + 4;
            public const int WEAPON_TYPE = WEAPON_ID + 4;
            public const int BULLET_ID = WEAPON_TYPE + 1;
            public const int BULLET_TYPE = BULLET_ID + 4;

            // Offsets for server to client packet
            public const int DANGER_ZONE = 1;
            public const int TIME = 13;
            public const int HEALTH = 17;
            public const int INVENTORY = 18;
            public const int PLAYERS = 23;
            public const int BULLETS = 443;
            public const int WEAPONS = 653;

            public static class Player
            {
                public const int ID = 0;
                public const int X = 1;
                public const int Z = 5;
                public const int R = 9;
                public const int W = 13;
            }

            public static class Bullet
            {
                public const int OWNER = 0;
                public const int ID = 1;
                public const int TYPE = 5;
                public const int CHANGE = 6;
            }

            public static class Weapon
            {
                public const int ID = 0;
                public const int CHANGE = 4;
            }
        }

        // Contains related to size of the data being sent
        public static class Size
        {
            // Packet sizes
            public const int SERVER_TICK = 865;
            public const int CLIENT_TICK = 24;
            public const int PLAYER_DATA = 14;
        }

    }

    // Contains Constants Related to the game
    public static class Game
    {
        public const int TICK_RATE = 64;
        public const double TICK_INTERVAL = (double)1000 / (double)TICK_RATE;
        public const float GAME_TIMER_INIT = 900000f; // 15 mins

        // Terrain Constants
        public static class Terrain
        {
            // Define default constants
            public const long DEFAULT_WIDTH = 1000;
            public const long DEFAULT_LENGTH = 1000;
            public const long DEFAULT_TILE_SIZE = 20;
            public const long DEFAULT_COLLIDER_SIZE = 20;
            // For the gun objects
            public const int GUN_OBJECT_SIZE = 13;
            public const int ID_BYTE_SIZE = 4;
            public const int X_BYTE_SIZE = 4;
            public const int Z_BYTE_SIZE = 4;
            public const int INV_BYTE_SIZE = 5;
            public const int BUL_BYTE_SIZE = 5;
            public const int ID_OFFSET = 1;
            public const int X_OFFSET = 5;
            public const int Z_OFFSET = 9;
            public const int INV_OFFSET = 14;
            public const int BUL_OFFSET = 19;
            // Changed to a percentage - ALam
            public const float DEFAULT_BUSH_PERC = 0.9993f;
            public const float DEFAULT_CACTUS_PERC = 0.9995f;
            public const float DEFAULT_BUILDING_PERC = 0.9997f;
            // Terrain name
            public const string DEFAULT_NAME = "Terrain";
        }

        // Player Constants
        public static class Players
        {
            public const int RADIUS = 1;
        }

        public static class Bullet
        {
            public const byte ADD = 1;
            public const byte REMOVE = 0;
            public const byte IGNORE = 255;
        }

        // Danger zone constant
        public static class DangerZone
        {
            public const float ZONE_CENTER_POOL_WIDTH = R.Game.Terrain.DEFAULT_WIDTH - 250;
            public const float ZONE_CENTER_POOL_HEIGHT = R.Game.Terrain.DEFAULT_LENGTH - 250;
            public const byte ZONE_DAMAGE_PER_SEC = 5;
            public const float RAD_RATE_PHASE1 = 0.5f; // ratio of radius to reduce per time unit -- phase 1
            public const float RAD_RATE_PHASE2 = 0.2f; // ratio of radius to reduce per time unit -- phase 1
            public const float RAD_RATE_PHASE3 = 0.3f; // ratio of radius to reduce per time unit -- phase 1
            public const float TIME_UNIT_TO_SHRINK = 120000f; // milliseconds. time  to shrink zone
            public const float TIME_UNIT_TO_PAUSE = 60000f; // milliseconds. time amount to pause shrinking
            public const float GAME_TIMER_PHASE1_START = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE;
            public const float GAME_TIMER_PHASE1_END = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE - R.Game.DangerZone.TIME_UNIT_TO_SHRINK;
            public const float GAME_TIMER_PHASE2_START = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE * 2 - R.Game.DangerZone.TIME_UNIT_TO_SHRINK;
            public const float GAME_TIMER_PHASE2_END = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE * 2 - R.Game.DangerZone.TIME_UNIT_TO_SHRINK * 2;
            public const float GAME_TIMER_PHASE3_START = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE * 3 - R.Game.DangerZone.TIME_UNIT_TO_SHRINK * 2;
            public const float GAME_TIMER_PHASE3_END = R.Game.GAME_TIMER_INIT - R.Game.DangerZone.TIME_UNIT_TO_PAUSE * 3- R.Game.DangerZone.TIME_UNIT_TO_SHRINK * 3;
        }

    }

    public static class Type
    {
        public const byte KNIFE = 1;
        public const byte PISTOL = 2;
        public const byte SHOTGUN = 3;
        public const byte RIFLE = 4;
    }

    public static class Init
    {
        public const int PLAYERMULT = 8;
        public const int WEAPONOFFSETID = 1;
        public const int WEAPONOFFSETX = 5;
        public const int WEAPONOFFSETZ = 9;
        public const int COORDBYTES = 4;
        public const int IDBYTES = 4;
        public const int INDWPNPCKT = 13;
        public const int QUOTIENTTOWNGUNS = 4;
        public const int CLUSTERING = 50;
        public const int TOWNHEIGHT = 500;
        public const int TOWNWIDTH = 500;
        public const int OCCURANCESQUARE = 12;
        public const int WPN1 = 1;
        public const int WPN2 = 2;
        public const int WPN3 = 3;
        public const int WPN4 = 4;
        public const int WPN5 = 5;
        public const int WPN6 = 6;
        public const int WPN7 = 7;
        public const int WPN8 = 8;
        public const int WPN9 = 9;
        public const int WPN10 = 10;
        public const int WPN11 = 11;
        public const int WPN12 = 12;
        public const int WPN13 = 13;
        public const int MAPEND = 1001;
        public const double PERCENTHOTSPOT = 0.75;
    }
}
