using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	/// <summary>
	/// Gets all the classes that inherits from the type given, excluding abstract and it self, should be avoided during live gameplay
	/// </summary>
	/// <param name="TYPE"></param>
	/// <returns></returns>
	public static List<Type> GetChildClasses(Type TYPE)
	{
		return AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly =>
			{
				Type[] types;

				try
				{ types = assembly.GetTypes(); }
				catch (ReflectionTypeLoadException e)
				{ types = e.Types.Where(t => t != null).ToArray(); }

				return types;
			})
			.Where(type => TYPE.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
			.ToList();
	}/// <summary>
     /// Takes the information from the "from" transform, and applies scale, position, and rotation
     /// </summary>
     /// <param name="from"></param>
     /// <param name="to"></param>
    public static void CopyFrom(this Transform to, Transform from)
    {
        to.SetPositionAndRotation(from.position, from.rotation);
        to.localScale = from.localScale;
    }
    public static void SetInfo(this Transform to, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        to.SetPositionAndRotation(pos, rot);
        to.localScale = scale;
    }
    /// <summary>
    /// Gets all the classes that inherits from the type given, excluding abstract and it self, should be avoided during live gameplay
    /// </summary>
    /// <param name="TYPE"></param>
    /// <returns></returns>
    public static List<Type> GetTypesImplementingInterface(Type TYPE)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                Type[] types;

                try
                { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e)
                { types = e.Types.Where(t => t != null).ToArray(); }

                return types;
            })
            .Where(type => TYPE.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
            .ToList();
    }
    public static Vector3 ToVector3(this Vector3Short origin)
    {
        return new Vector3(origin.x, origin.y, origin.z);
    }
}

