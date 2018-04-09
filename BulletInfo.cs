class BulletInfo {

    public int bulletId { get; set; }
    public byte playerId { get; set; }
    public int damage { get; set; }
    public byte type { get; set; }
    // type could be byte
    public float size { get; set; }
    // hardcoded size based on bullet type
    // look in Unity for Size
    public float speed { get; set; }
    public float lifetime { get; set; }

    public BulletInfo (int id, byte type, byte playerId)
	{
		this.bulletId = id;
        this.playerId = playerId;
        this.type = type;
        // change all values based on type
        // use switch

        switch(type)
        {
            case R.Type.KNIFE:
                this.damage = 70;
                this.size = 0.5f;
                this.speed = 0.4f;
                this.lifetime = 0.1f;
                break;
            case R.Type.PISTOL:
                this.damage = 10;
                this.size = 0.2f;
                this.speed = 0.4f;
                this.lifetime = 1.0f;
                break;
            case R.Type.SHOTGUN:
                this.damage = 13;
                this.size = 0.2f;
                this.speed = 0.3f;
                this.lifetime = 0.4f;
                break;
            case R.Type.RIFLE:
                this.damage = 20;
                this.size = 0.2f;
                this.speed = 0.6f;
                this.lifetime = 2.0f;
                break;
            default:
                this.damage = 0;
                this.size = 0;
                this.speed = 0;
                this.lifetime = 0;
                break;
        }
	}
}
