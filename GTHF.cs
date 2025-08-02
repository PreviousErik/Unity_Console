using UnityEngine;

public static partial class GTHF
{
    /// <summary>
    /// Toggles a bool and returns the new value for quick and easy access
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static bool Toggle(this ref bool a)
    {
        a = !a;
        return a;
    }

    public static Vector3 RotateVectorAlongUp(this Vector3 origin, float angle)
    {
       return Quaternion.AngleAxis(angle, Vector3.up) * origin;
    }
}

