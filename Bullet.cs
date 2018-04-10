using System;

class Bullet
{
    public int BulletId { get; set; }
    public byte PlayerId { get; set; }
    public int Damage { get; set; }
    public byte Type { get; set; }
    public float Size { get; set; }
    private float speed { get; set; }
    private DateTime deathTime;

    public float X { get; set; }
    public float Z { get; set; }

    private float deltaX;
    private float deltaZ;

    public Bullet (int id, byte type, connectionData player)
	{
		this.BulletId = id;
        this.PlayerId = player.id;
        this.Type = type;

        this.X = player.x;
        this.Z = player.z;

        DateTime currentTime = DateTime.Now;

        switch (type)
        {
            case R.Type.KNIFE:
                this.Damage = 70;
                this.Size = 0.5f;
                this.speed = 0.4f;
                this.deathTime = currentTime.AddSeconds(0.1);
                break;
            case R.Type.PISTOL:
                this.Damage = 10;
                this.Size = 0.2f;
                this.speed = 0.4f;
                this.deathTime = currentTime.AddSeconds(1.0);
                break;
            case R.Type.SHOTGUN:
                this.Damage = 13;
                this.Size = 0.2f;
                this.speed = 0.3f;
                this.deathTime = currentTime.AddSeconds(0.4);
                break;
            case R.Type.RIFLE:
                this.Damage = 20;
                this.Size = 0.2f;
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

        this.deltaX = (float)(this.speed * Math.Cos(player.r * (Math.PI / 180)));
        this.deltaZ = (float)(this.speed * Math.Sin(player.r * (Math.PI / 180)));
	}

    public bool Update()
    {
        if (this.deathTime > DateTime.Now)
        {
            return false;
        }

        this.X += this.deltaX;
        this.Z += this.deltaZ;

        return true;
    }

    public bool isColliding(float x, float z, float r)
    {
        double distance = Math.Sqrt((this.X - x) * (this.X - x) + (this.Z - z) * (this.Z - z));
        return (distance < (this.Size + r));
    }
}
