public enum PowerupType
{
    Bullets,
    Missiles,
    Health,
    Boost
}

public interface IPowerupReceiver
{
    void ReceivePowerup(PowerupType type, int amount);
}