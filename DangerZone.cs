/*----------------------------------------------------------------------
-- SOURCE FILE:	DangerZone.cs	    - A file containing a class definition
--                                    and functions of the DangerZone class
--
-- PROGRAM:		Server.exe
--
-- FUNCTIONS:
--              public DangerZone(void)
--              public void Update(void)
--              public void HandlePlayer(Player player)
--              public byte[] ToBytes(void)
--
-- DATE:		April 11, 2018
--
-- DESIGNER:	Benny Wang, Jeremy Lee, Luke Lee
--
-- PROGRAMMER:	Jeremy Lee
--
-- NOTES:
-- A class definition to control danger zone in the game.
-- A "danger zone" is where a player character continuously loses its
-- health. A circular zone is initially created by a randomly set center
-- point on the map to define a safe zone, and it shrinks throughout
-- 3 phases which are separated by "safe time". The safe zone doesn't
-- shrink this "safe time".
-- At the end of the game, the safe zone shrinks to its minimum size, 0.
----------------------------------------------------------------------*/
using System;

public class DangerZone
{
    private float fullRad = 0;
    private float zoneCenterPoolWidth = R.Game.DangerZone.ZONE_CENTER_POOL_WIDTH;
    private float zoneCenterPoolHeight = R.Game.DangerZone.ZONE_CENTER_POOL_HEIGHT;
    private float radRatePhase1 = R.Game.DangerZone.RAD_RATE_PHASE1;
    private float radRatePhase2 = R.Game.DangerZone.RAD_RATE_PHASE2;
    private float radRatePhase3 = R.Game.DangerZone.RAD_RATE_PHASE3;
    private float timeUnitToShrink = R.Game.DangerZone.TIME_UNIT_TO_SHRINK;
    private float timeUnitToPause = R.Game.DangerZone.TIME_UNIT_TO_PAUSE;
    private float gameTimerPhase1Start = R.Game.DangerZone.GAME_TIMER_PHASE1_START;
    private float gameTimerPhase1End = R.Game.DangerZone.GAME_TIMER_PHASE1_END;
    private float gameTimerPhase2Start = R.Game.DangerZone.GAME_TIMER_PHASE2_START;
    private float gameTimerPhase2End = R.Game.DangerZone.GAME_TIMER_PHASE2_END;
    private float gameTimerPhase3Start = R.Game.DangerZone.GAME_TIMER_PHASE3_START;
    private float gameTimerPhase3End = R.Game.DangerZone.GAME_TIMER_PHASE3_END;
    private bool phaseEntered = false;
    private float dangerZoneRadius;
    private float dangerZoneX;
    private float dangerZoneZ;
    private float dangerZoneXNew = 0;
    private float dangerZoneZNew = 0;
    private float distToNewX = 0;
    private float distToNewZ = 0;
    private float ratioToShrink = 0;
    private Random random = new Random();
    private float gameTimer;
    private Int32 tickCount; // for danger zone

    /*------------------------------------------------------------------
	-- FUNCTION:	DangerZone
	--
    -- DATE:		April 11, 2018
    --
    -- DESIGNER:	Benny Wang, Jeremy Lee, Luke Lee
    --
    -- PROGRAMMER:	Jeremy Lee
	--
	-- INTERFACE:	public DangerZone(void)
	--
	-- ARGUMENT:    void
	--
	-- RETURNS:	    void
	--
	-- NOTES:
    -- The class constructor of the DangerZone class.
    -- Randomizes the initial coordinate of the center of the safe zone
    -- within a smaller area of the map. Gets the radius of the safe
    -- zone by the length of the diagnal of the smaller are of the map.
	------------------------------------------------------------------*/
    public DangerZone()
    {
        tickCount = 0;
        gameTimer = R.Game.GAME_TIMER_INIT;

        dangerZoneX = (float)((random.NextDouble() * zoneCenterPoolWidth) - (zoneCenterPoolWidth / 2));
        dangerZoneZ = (float)((random.NextDouble() * zoneCenterPoolHeight) - (zoneCenterPoolHeight / 2));

        // length of diagnal line of the zone center pool height
        dangerZoneRadius = Convert.ToSingle(Math.Sqrt(Math.Pow(zoneCenterPoolWidth, 2) + Math.Pow(zoneCenterPoolHeight, 2)));
        fullRad = dangerZoneRadius;
    }

    /*------------------------------------------------------------------
    -- FUNCTION:	Update
    --
    -- DATE:		April 11, 2018
    --
    -- DESIGNER:	Benny Wang, Jeremy Lee, Luke Lee
    --
    -- PROGRAMMER:	Jeremy Lee
    --
    -- INTERFACE:	public void Update(void)
    --
    -- ARGUMENT:    void
    --
    -- RETURNS:	    void
    --
    -- NOTES:
    -- Updates the radius and the center coordinate of the safe zone.
    -- There are 3 phases, 3 pause-periods. When in one of the phases,
    -- the radius is decreased every seconds and the center gradually
    -- moves to the new center. When in a pause-priod, the radius and
    -- the center is fixed. Each phase has different randomized center
    -- coordinates and radius-decrease ratioes. If the game time hits 0,
    -- the radius is fixed to 0 and the center is not moved.
    ------------------------------------------------------------------*/
    public void Update()
    {
        gameTimer = gameTimer - (float)R.Game.TICK_INTERVAL;
		if (gameTimer < 0)
		{
			gameTimer = 0;
		}

        tickCount++; // to check tick

        if (tickCount == R.Game.TICK_RATE)
        {
            if (gameTimer.Equals(gameTimerPhase1Start) && phaseEntered == false) // phase 1
            {
                Console.WriteLine("PHASE 1 Entered");
                ratioToShrink = radRatePhase1;
                phaseEntered = true;
            }
            else if (gameTimer.Equals(gameTimerPhase2Start) && phaseEntered == false) // phase 2
            {
                Console.WriteLine("PHASE 2 Entered");
                ratioToShrink = radRatePhase2;
                phaseEntered = true;
            }
            else if (gameTimer.Equals(gameTimerPhase3Start) && phaseEntered == false) // phase 3
            {
                Console.WriteLine("PHASE 3 Entered");
                ratioToShrink = radRatePhase3;
                phaseEntered = true;
            }
            else if ((gameTimer <= R.Game.GAME_TIMER_INIT && gameTimer > gameTimerPhase1Start)
            || (gameTimer <= gameTimerPhase1End && gameTimer > gameTimerPhase2Start)
            || (gameTimer <= gameTimerPhase2End && gameTimer > gameTimerPhase3Start)
            || (gameTimer <= 0)) // break 1, 2, 3
            {
                ratioToShrink = 0;
                distToNewX = 0;
                distToNewZ = 0;
                phaseEntered = false;
            }

            if (phaseEntered == true)
            {
                dangerZoneXNew = (float)((random.NextDouble() * zoneCenterPoolWidth) - (zoneCenterPoolWidth / 2));
                dangerZoneZNew = (float)((random.NextDouble() * zoneCenterPoolHeight) - (zoneCenterPoolHeight / 2));
                distToNewX = dangerZoneXNew - dangerZoneX;
                distToNewZ = dangerZoneZNew - dangerZoneZ;
                phaseEntered = false;
            }

            dangerZoneRadius -= ((fullRad * ratioToShrink) / (timeUnitToShrink / 1000)); // the division for converting the time unit to sec
            if (dangerZoneRadius < 0)
			{
				dangerZoneRadius = 0;
			}
			dangerZoneX += distToNewX / (timeUnitToShrink / 1000);
            dangerZoneZ += distToNewZ / (timeUnitToShrink / 1000);

            tickCount = 0; // reset tickCount
        }
    }

    /*------------------------------------------------------------------
    -- FUNCTION:	HandlePlayer
    --
    -- DATE:		April 11, 2018
    --
    -- DESIGNER:	Benny Wang, Jeremy Lee, Luke Lee
    --
    -- PROGRAMMER:	Jeremy Lee
    --
    -- INTERFACE:	public void HandlePlayer(Player player)
    --
    -- ARGUMENT:    player                      - The player to check the
    --                                            coordinate of and
    --                                            handle health of.
    --
    -- RETURNS:	    void
    --
    -- NOTES:
    -- Updates the given user's health.
    -- Gets the given user's coordinate and checks if the user is outside
    -- of the safe zone. If the user is outside of the safe zone, then
    -- the user's health decreases by 1 every seconde.
    ------------------------------------------------------------------*/
    public void HandlePlayer(Player player)
    {
        double deltaX = Math.Pow(dangerZoneX - player.x, 2);
        double deltaZ = Math.Pow(dangerZoneZ - player.z, 2);
        float distance = (float)Math.Sqrt(deltaX + deltaZ);

        if ((tickCount % R.Game.TICK_RATE) == 0)
        {
            if (distance >= dangerZoneRadius)
            {
                player.TakeDamage(R.Game.DangerZone.ZONE_DAMAGE_PER_SEC);
            }
        }
    }

    /*------------------------------------------------------------------
    -- FUNCTION:	ToBytes
    --
    -- DATE:		April 11, 2018
    --
    -- DESIGNER:	Benny Wang, Jeremy Lee, Luke Lee
    --
    -- PROGRAMMER:	Jeremy Lee
    --
    -- INTERFACE:	public byte[] ToBytes(void)
    --
    -- ARGUMENT:    void
    --
    -- RETURNS:	    byte[]                  - Danger zone data including
    --                                        safe zone's center
    --                                        coordinate and the radius
    --                                        of the safe zone area.
    --
    -- NOTES:
    -- Converts the danger zone data into a byte array and returns it.
    ------------------------------------------------------------------*/
    public byte[] ToBytes()
    {
        byte[] tmp = new byte[16];

        Array.Copy(BitConverter.GetBytes(dangerZoneX),      0, tmp, 0,  4);
        Array.Copy(BitConverter.GetBytes(dangerZoneZ),      0, tmp, 4,  4);
        Array.Copy(BitConverter.GetBytes(dangerZoneRadius), 0, tmp, 8,  4);
        Array.Copy(BitConverter.GetBytes(gameTimer),        0, tmp, 12, 4);

        return tmp;
    }
}
