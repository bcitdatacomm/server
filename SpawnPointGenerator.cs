/************************************************************************************
SOURCE FILE: 	SpawnPointGenerator.cs

PROGRAM:		server

FUNCTIONS:		SpawnPointGenerator()
				populateStack()
				GetNextSpawnPoint()

DATE:			Mar. 14, 2018

REVISIONS:

DESIGNER:		Benny Wang

PROGRAMMER:	    Benny Wang

NOTES:
                This is the class that generates spawn points for players.
**********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

class SpawnPointGenerator
{
    private static Random rng = new Random();
    private Stack<List<float>> spawnPoints;

    /************************************************************************************
    FUNCTION:	SpawnPointGenerator

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	SpawnPointGenerator()

    NOTES:
    Creates a spawn point generator and geneartes the inital list of random spawn points.
    **********************************************************************************/
    public SpawnPointGenerator()
    {
        this.populateStack();
    }

    /************************************************************************************
    FUNCTION:	populateStack

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	populateStack()

    NOTES:
    Generates a a stack of randomly pseudo-random spawn points. All spawn points generated
    are guaranteed to be around the edge of the town in the center of the map.
    **********************************************************************************/
    private void populateStack()
    {
        List<float> point;
        this.spawnPoints = new Stack<List<float>>();

        // Top Left Corner
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(-100, -80));
            point.Add(rng.Next(80, 100));
            this.spawnPoints.Push(point);
        }

        // Bottom Left Corner
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(-100, -80));
            point.Add(rng.Next(-100, -80));
            this.spawnPoints.Push(point);
        }

        // Top Right Corner
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(80, 100));
            point.Add(rng.Next(80, 100));
            this.spawnPoints.Push(point);
        }

        // Bottom Right Corner
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(80, 100));
            point.Add(rng.Next(-100, -80));
            this.spawnPoints.Push(point);
        }

        // Right Center
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(80, 100));
            point.Add(rng.Next(-79, 80));
            this.spawnPoints.Push(point);
        }

        // Left Center
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(-100, -80));
            point.Add(rng.Next(-79, 80));
            this.spawnPoints.Push(point);
        }

        // Top Center
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(-79, 80));
            point.Add(rng.Next(80, 100));
            this.spawnPoints.Push(point);
        }

        // Bottom Center
        for (int i = 0; i < 5; i++)
        {
            point = new List<float>();
            point.Add(rng.Next(-79, 80));
            point.Add(rng.Next(-100, -80));
            this.spawnPoints.Push(point);
        }
    }

    /************************************************************************************
    FUNCTION:	GetNextSpawnPoint

    DATE:		Mar. 14, 2018

    REVISIONS:

    DESIGNER:	Benny Wang

    PROGRAMMER:	Benny Wang

    INTERFACE:	GetNextSpawnPoint()

    RETURN: A List of 2 floats, the first being the x position, the second being the z position.

    NOTES: 
    Pops a random amount of spawn points off the stack of randomly generated spawn points.
    The last one to be poped of is returned as the spawn point to use.
    **********************************************************************************/
    public List<float> GetNextSpawnPoint()
    {
        List<float> point = new List<float>();

        for (int i = 0; i < rng.Next(0, 39); i++)
        {
            if (this.spawnPoints.Count == 0)
            {
                this.populateStack();
                point = this.spawnPoints.Pop();
            }
            else
            {
                point = this.spawnPoints.Pop();
            }
        }

        return point;
    }
}
