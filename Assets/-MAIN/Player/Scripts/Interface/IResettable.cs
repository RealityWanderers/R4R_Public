public interface IResettable
{
    void ResetObject();  // Resets the object (like respawning)
    void DisableObject(); // Disables the object (like "destroying" it temporarily)
}
