using System;
using System.Collections;
using System.Collections.Generic;

class SpawnPointGenerator
{
    private static Random rng = new Random();
    private Stack<List<float>> spawnPoints;

    public SpawnPointGenerator()
    {
        this.populateStack();
    }

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
