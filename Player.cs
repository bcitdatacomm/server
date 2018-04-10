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
}
