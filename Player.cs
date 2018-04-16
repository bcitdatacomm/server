/************************************************************************************
SOURCE FILE: 	Player.cs

PROGRAM:		server

FUNCTIONS:		Player(EndPoint ep, byte id, float x, float z)
				TakeDamage(byte damage)
				IsDead()

DATE:			Mar. 14, 2018

REVISIONS:

DESIGNER:		Benny Wang

PROGRAMMER:	    Benny Wang

NOTES:
				This is the class that the server uses to repersent the players in
				the game.
**********************************************************************************/
using System;
using Networking;

public class Player
{
	public EndPoint ep { get; set; }
	public byte[] buffer { get; set; }
	public byte id { get; set; }

	public float x { get; set; }
	public float z { get; set; }
	public float r { get; set; }
	public byte h { get; set; }

	public int currentWeaponId { get; set; }
	public byte currentWeaponType { get; set; }

    /************************************************************************************
    FUNCTION:	Player

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	Player(EndPoint ep, byte id, float x, float z)
					EndPoint ep: The networking enpdoint of the player.
					byte id: The player id.
					float x: The x position of the player.
					float z: The z position of the player.

    NOTES:
	Creates a player with the given id at the given x, z positoin.
    **********************************************************************************/
	public Player(EndPoint ep, byte id, float x, float z)
	{
		this.ep = ep;
		this.id = id;
		this.x = x;
		this.z = z;
		this.r = 0;
		this.h = 100;
		this.currentWeaponId = 0;
		this.currentWeaponType = 0;
	}

    /************************************************************************************
    FUNCTION:	TakeDamage

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

	INTERFACE:	TakeDamage(byte damage)
					byte damage: The amount of damage to take.

    NOTES:
	Subtracts the given damage from the player. If the player's health is lower than
	the amount of damage given, the players health is set to 0 and their position is
	permanently set to (1000 + id, 1000) to enuser that dead players are stuck in the
	graveyard area.
    **********************************************************************************/
	public void TakeDamage(byte damage)
	{
		if (this.h < damage)
		{
			this.h = 0;
			this.x = 1000 + this.id;
			this.z = 1000;
		}
		else
		{
			this.h -= damage;
		}
	}

    /************************************************************************************
    FUNCTION:	IsDead

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	IsDead()

	RETURN: True if the player is dead, false otherwise.

    NOTES:
	Checks if the player is dead. If the player is dead this function will also set
	their position to (1000 + id, 1000) to ensure that dead players are stuck in the
	graveyard area.
    **********************************************************************************/
	public bool IsDead()
	{
		if (this.h == 0)
		{
			this.x = 1000 + this.id;
			this.z = 1000;
			return true;
		}

		return false;
	}
}
