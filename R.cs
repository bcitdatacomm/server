namespace R
{
    // Contains all the constants associated with networking and the packet
    public static class Net
    {
        public const ushort PORT = 42069;


        // Contains constants associated with the header type of the packet
        public static class Header
        {
            public const byte INIT_PLAYER = 0;
            public const byte NEW_CLIENT = 69;
            public const byte TICK = 85;
        }

        // Contains constants associated with the packet offset or distance into the packet
        public static class Offset
        {
            // Offsets for client to server packet
            public const int PID = 1;
            public const int X = PID + 1;
            public const int Z = X + 4;
            public const int R = Z + 4;
            public const int INV = R + 4;
            public const int BULLET = INV + 5;

            // Offsets for server to client packet
            public const int DANGER_ZONE = 1;
            public const int TIME = 13;
            public const int HEALTH = 17;
            public const int INVENTORY = 18;
            public const int PLAYERS = 23;

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
        // Terrain Constants
        public static class Terrain
        {

        }

        // Player Constants
        public static class Players
        {

        }
    }
}
