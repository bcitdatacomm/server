using System;

class Bullet
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
                this.deathTime = currentTime.AddSeconds(0.01);
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
        // Console.WriteLine("bullet {0} was created at {1} with its death time set at {2}", this.BulletId, currentTime, this.deathTime);

        // Changed from sin to cos for both
        this.deltaX = (float)(this.speed * Math.Sin(player.r * (Math.PI / 180)));
        this.deltaZ = (float)(this.speed * Math.Cos(player.r * (Math.PI / 180)));
        // Console.WriteLine("Delta X: {0}, Delta Z: {1}", this.deltaX, this.deltaZ);
	}

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

    public bool isColliding(float x, float z, float r)
    {
        
        double distance = Math.Sqrt((this.X - x) * (this.X - x) + (this.Z - z) * (this.Z - z));
        double radiusSum = this.Size + r;
        // Console.WriteLine("X: {0}, Z: {1}, Size: {2}", this.X, this.Z, this.Size);
        // Console.WriteLine("Player x: {0}, Player z: {1}, Player r: {2}", x, z, r);
        // Console.WriteLine("Distance: {0}", distance);
        // Console.WriteLine("Radius sum: {0}", radiusSum);
        return (distance < radiusSum);
    }
}
