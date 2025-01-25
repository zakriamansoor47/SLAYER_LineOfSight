using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using Microsoft.Extensions.Logging;

// Used these to remove compile warnings
#pragma warning disable CS8600
#pragma warning disable CS8603

namespace SLAYER_LineOfSight;

public partial class SLAYER_LineOfSight : BasePlugin
{
    // ---------------------------------------
    // Useful Funtions
    // ---------------------------------------
    public CBeam DrawLaserBetween(Vector startPos, Vector endPos, Color color, float life, float width)
    {
        if (startPos == null || endPos == null)
            return null;

        CBeam beam = Utilities.CreateEntityByName<CBeam>("beam");

        if (beam == null)
        {
            Logger.LogError($"Failed to create beam...");
            return null;
        }

        beam.Render = color;
        beam.Width = width;

        beam.Teleport(startPos, QAngle.Zero, Vector.Zero);
        beam.EndPos.X = endPos.X;
        beam.EndPos.Y = endPos.Y;
        beam.EndPos.Z = endPos.Z;
        beam.DispatchSpawn();

        if(life != -1) AddTimer(life, () => {if(beam != null && beam.IsValid) beam.Remove(); }); // destroy beam after specific time

        return beam;
    }
	private System.Numerics.Vector3 ConvertVectorToVector3(Vector vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }
	private Vector CreateNewVector(Vector vector)
    {
        if(vector == null)return null;
        return new Vector(vector.X, vector.Y, vector.Z);
    }
   
    public QAngle CalculateAngle(Vector origin1, Vector origin2)
    {
        if (origin1 == null || origin2 == null)
            return null;

        // Calculate the direction vector from origin1 (player's eye position) to origin2 (target position)
        Vector direction = new Vector(
            origin2.X - origin1.X,
            origin2.Y - origin1.Y,
            origin2.Z - origin1.Z
        );

        // Normalize the direction vector to get the target direction
        direction = Normalize(direction);
        
        // Calculate the yaw angle using the dot product between the forward direction and the target direction
        float yaw = (float)(Math.Atan2(direction.Y, direction.X) * (180.0 / Math.PI));

        // Calculate the pitch angle based on the Z component
        float hypotenuse = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        float pitch = (float)(Math.Atan2(-direction.Z, hypotenuse) * (180.0 / Math.PI));

        // Return the QAngle with the calculated pitch and yaw (roll is not necessary)
        return new QAngle(pitch, yaw, 0);
    }

    // Vector normalization helper method 
    public Vector Normalize(Vector v)
    {
        float length = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector(v.X / length, v.Y / length, v.Z / length);
    }
   
}
