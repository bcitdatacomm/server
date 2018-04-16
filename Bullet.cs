/************************************************************************************
SOURCE FILE: 	Bullet.cs

PROGRAM:		server

FUNCTIONS:		Bullet (int id, byte type, Player player)
				Update()
				IsColliding(float x, float z, float r)

DATE:			Mar. 14, 2018

REVISIONS:

DESIGNER:		Benny Wang

PROGRAMMER:	    Benny Wang, Li-Yan Tong	

NOTES:
                This is that class that the server uses to represent bullets in game.
                This class stores all the information related to bullets and has the
                ability to check if a player is colliding with it.
**********************************************************************************/
using System;

public class Bullet
{
    public int BulletId { get; set; }
    public byte PlayerId { get; set; }
    public byte Damage { get; set; }
    public byte Type { get; set; }
    public float Size { get; set; }
    private float speed { get; set; }
    private DateTime deathTime;

    public float X { get; set; }
    public float Z { get; set; }

    private float deltaX;
    private float deltaZ;

    public byte Event { get; set; }

    /************************************************************************************
    FUNCTION:	Bullet

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang, Li-Yan Tong

    INTERFACE:	Bullet(int id, byte type, Player player)
                    int id: The id of the bullet.
                    byte type: The type of the bullet.
                    Player: The player that shot the bullet.

    NOTES:
    Creates a bullet using the position and rotation of the player. The behaviour of the
    bullet is determined by what type is passed in.
    **********************************************************************************/
    public Bullet (int id, byte type, Player player)
	{
		this.BulletId = id;
        this.PlayerId = player.id;
        this.Type = type;

        this.X = player.x;
        this.Z = player.z;

        this.Event = R.Game.Bullet.IGNORE;

        DateTime currentTime = DateTime.Now;

        switch (type)
        {
            case R.Type.KNIFE:
                this.Damage = 70;
                this.Size = 0.5f;
                this.speed = 0.4f;
                this.deathTime = currentTime.AddSeconds(0.03);
                break;
            case R.Type.PISTOL:
                this.Damage = 10;
                this.Size = 0.1f;
                this.speed = 0.4f;
                this.deathTime = currentTime.AddSeconds(1.0);
                break;
            case R.Type.SHOTGUN:
                this.Damage = 13;
                this.Size = 0.25f;
                this.speed = 0.3f;
                this.deathTime = currentTime.AddSeconds(0.4);
                break;
            case R.Type.RIFLE:
                this.Damage = 20;
                this.Size = 0.1f;
                this.speed = 0.6f;
                this.deathTime = currentTime.AddSeconds(2.0);
                break;
            default:
                this.Damage = 0;
                this.Size = 0;
                this.speed = 0;
                this.deathTime = currentTime.AddSeconds(0);
                break;
        }

        // Changed from sin to cos for both
        this.deltaX = (float)(this.speed * Math.Sin(player.r * (Math.PI / 180)));
        this.deltaZ = (float)(this.speed * Math.Cos(player.r * (Math.PI / 180)));
	}

    /************************************************************************************
    FUNCTION:	Update

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	bool Update()

    RETURN: True if the bullet position was updated, false if the bullet's lifetime expired.
                   
    NOTES:
    Updates the position of the bullet. If the bullet's lifetime expires during this call
    the position is not updated and false is returned. Otherwise, true is returned.
    **********************************************************************************/
    public bool Update()
    {
        if (DateTime.Now > this.deathTime)
        {
            // Console.WriteLine("bullet {0} expired at {1}", this.BulletId, DateTime.Now);
            return false;
        }

        this.X += this.deltaX;
        this.Z += this.deltaZ;

        return true;
    }

    /************************************************************************************
    FUNCTION:	IsColliding

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	bool IsColliding(float x, float z, float r)
                    float x: The x position.
                    float z: The z position.
                    float r: The radius.

    RETURN: True if the given circle is colliding with the bullet, false otherwise.
                   
    NOTES:
    Checks whether the given circle specificed by the given x, z, radius, collides with
    the bullet or not.
    **********************************************************************************/
    public bool IsColliding(float x, float z, float r)
    {

        double distance = Math.Sqrt((this.X - x) * (this.X - x) + (this.Z - z) * (this.Z - z));
        double radiusSum = this.Size + r;
        return (distance < radiusSum);
    }
}
