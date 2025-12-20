using System;
using System.Collections.Generic;
using UnityEngine;
using static Kamgam.Terrain25DLib.BezierPoint;
using static Kamgam.Terrain25DLib.BezierSpline;

namespace Kamgam.Terrain25DLib
{
    [CreateAssetMenu(fileName = "TerrainDataAsset", menuName = "DataAsset/TerrainDataAsset")]
    public class TerrainLevelData : ScriptableObject
    {
        public Vector3 CameraZoom;
        public List<SplineShape> Shapes;
        public List<SplinePoint> Objects;
        public List<FolliagePos> foliagePos;
        public List<BridgePoint> Bridges;
    }
}

[Serializable]
public class SplineShape
{
    public bool Mode2D;
    public BooleanOp CombinationType = BooleanOp.Add;
    public Vector3 Position;
    public Vector3 Rotation;
    public List<SplinePoint> Points;
}

[Serializable]
public class SplinePoint
{
    public int ID;
    public string Name;
    public Vector3 Position;
    public Vector3 Rotation;

    public HandleType HandleType;
    public Vector3 Handle1Pos;
    public Vector3 Handle2Pos;
}

[Serializable]
public class FolliagePos
{
    public List<Vector3> Position;

    public FolliagePos(List<Vector3> _init)
    {
        Position = _init;
    }
}

[Serializable]
public class BridgePoint
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public Vector3 Pos1;
    public Vector3 Handle1;
    public Vector3 Pos2;
    public Vector3 Handle2;
}