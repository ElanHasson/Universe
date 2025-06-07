using System.Numerics;
using Orleans;

namespace Universe.Abstractions.Serialization;

[GenerateSerializer]
public struct Vector3Surrogate
{
    [Id(0)]
    public float X { get; set; }
    
    [Id(1)]
    public float Y { get; set; }
    
    [Id(2)]
    public float Z { get; set; }
}

[RegisterConverter]
public sealed class Vector3SurrogateConverter : IConverter<Vector3, Vector3Surrogate>
{
    public Vector3 ConvertFromSurrogate(in Vector3Surrogate surrogate)
    {
        return new Vector3(surrogate.X, surrogate.Y, surrogate.Z);
    }

    public Vector3Surrogate ConvertToSurrogate(in Vector3 value)
    {
        return new Vector3Surrogate
        {
            X = value.X,
            Y = value.Y,
            Z = value.Z
        };
    }
}