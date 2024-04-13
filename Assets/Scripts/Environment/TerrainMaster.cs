﻿/****************************************************
 * File: TerrainMaster.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Hybrid Blending Animations
   * Last update: 01/08/2021
*****************************************************/

// TODO: Organize/clean/comment code
// TODO: Check how to retrieve feet positions from the "terrain deformation" project
// TODO: Angle ony in one direction - Should use 2D coordinates to balance orthogonally

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Retrieves terrain data
/// </summary>
public class TerrainMaster : MonoBehaviour
{
    #region Variables

    [Header("Settings")]
    public bool getFeetPositions = false;

    [Header("Debug Terrain")]
    public bool showTerrainNormalAngleDebug;
    public bool printTerrainNormalAngle;
    public bool printFeetPositionsHeightmapWorld;

    // Properties
    public TerrainData TerrainData { get; set; }
    public int HeightmapWidth { get; set; }
    public int HeightmapHeight { get; set; }
    public float SlopeAngle { get; set; }

    // Terrain Data
    private float slopeAngle;
    private Terrain terrain;
    private TerrainData terrainData;
    private int heightmapWidth;
    private int heightmapHeight;
    private float[,] heightmapData;
    private float[,] heightmapBuffer;
    private float[,] heightmapFiltered;
    private int[,] contourmapData;
    private int[,] contourmapBuffer;

    private IKFeetPlacement _feetPlacement = null;
    private Rigidbody _rb = null;

    #endregion

    void Awake()
    {
        // Get terrain information.
        if (!terrain)
            terrain = Terrain.activeTerrain;

        terrainData = terrain.terrainData;
        heightmapWidth = terrainData.heightmapResolution;
        heightmapHeight = terrainData.heightmapResolution;
        heightmapData = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
        heightmapBuffer = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
        heightmapFiltered = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        contourmapData = new int[heightmapHeight, heightmapWidth];
        contourmapBuffer = new int[heightmapHeight, heightmapWidth];
    }

    // Start is called before the first frame update
    void Start()
    {
        // From same GameObject
        _rb = this.GetComponent<Rigidbody>();

        // From other GameObject
        _feetPlacement = FindObjectOfType<IKFeetPlacement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get terrain angle at each frame
        GetTerrainSlope();

        if (getFeetPositions)
        {
            ObtainFeetPositions(_feetPlacement.LeftFootIKPosition);
            ObtainFeetPositions(_feetPlacement.RightFootIKPosition);
        }
    }

    // ----------------------------------//

    /// <summary>
    /// Gets Terrain Slope angle (sign changes if going up or down).
    /// </summary>
    private void GetTerrainSlope()
    {
        // Get terrain slope
        float pos_x = _rb.position.x / terrainData.size.x;
        float pos_z = _rb.position.z / terrainData.size.z;
        Vector3 normal = terrainData.GetInterpolatedNormal(pos_x, pos_z);
        float gradient = terrainData.GetSteepness(pos_x, pos_z);

        // To define if character is climbing up or down with respect to its direction
        Vector3 local_normal = this.transform.InverseTransformDirection(normal);
        SlopeAngle = local_normal.z < 0 ? gradient : -gradient;

        // To visualize the normal
        if (showTerrainNormalAngleDebug)
        {
            Debug.DrawLine(_rb.position, _rb.position + normal, Color.cyan);
        }

        if(printTerrainNormalAngle)
        {
            Debug.Log("[INFO] Slope Angle: " + SlopeAngle);
        }
    }

    private void ObtainFeetPositions(Vector3 footIKPosition)
    {
        int indX_heightmap = (int)((footIKPosition.x / terrainData.size.x) * heightmapWidth);
        int indZ_heightmap = (int)((footIKPosition.z / terrainData.size.z) * heightmapHeight);

        float indX_size = (float)((indX_heightmap * terrainData.size.x) / heightmapWidth);
        float indZ_size = (float)((indZ_heightmap * terrainData.size.z) / heightmapHeight);

        if (printFeetPositionsHeightmapWorld)
        {
            Debug.Log("indX_heightmap: " + indX_heightmap + " | indZ_heightmap: " + indZ_heightmap);
            Debug.Log("indX_size: " + indX_size + " | indZ_size: " + indZ_size);
        }
    }
}
