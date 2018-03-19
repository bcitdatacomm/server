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
            public const byte TICK = 85;
            public const byte NEW_CLIENT = 69;
            public const byte ACK = 170;
        }

        // Contains constants associated with the packet offset or distance into the packet
        public static class Offset
        {
            // Offsets for server to client packet
            public const int DANGER_ZONE = 1;
            public const int PLAYER_POSITIONS = DANGER_ZONE + 12;
            public const int PLAYER_ROTATIONS = PLAYER_POSITIONS + 240;
            public const int PLAYER_IDS = PLAYER_ROTATIONS + 120;
            public const int PLAYER_INVENTORIES = PLAYER_IDS + 30;
            public const int GAME_TIME = PLAYER_INVENTORIES + 150;
            public const int PLAYER_HEALTH = GAME_TIME + 4;
            public const int WEAPON_DIFF = PLAYER_HEALTH + 1;
            public const int BULLET_DIFF = WEAPON_DIFF + 150;

            // Offsets for client to server packet
            public const int PID = 1;
            public const int X = PID + 1;
            public const int Z = X + 4;
            public const int R = Z + 4;
            public const int INV = R + 4;
            public const int BULLET = INV + 5;
        }

        // Contains related to size of the data being sent
        public static class Size
        {
            // Packet sizes
            public const int SERVER_TICK = 918;
            public const int CLIENT_TICK = 24;
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
