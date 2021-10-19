using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public enum PathEdge { Left, Middle, Right }

    [SerializeField] public PathEdge pathStart = PathEdge.Middle;
    [SerializeField] public PathEdge pathEnd = PathEdge.Middle;
    public Vector3 position;

    private void Start()
    {
        position = transform.position;
    }
}