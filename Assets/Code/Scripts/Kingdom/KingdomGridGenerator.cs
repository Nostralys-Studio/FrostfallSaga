using System.Collections.Generic;
using System.Linq;
using FrostfallSaga.Grid.Cells;
using FrostfallSaga.Kingdom;
using FrostfallSaga.Procedural;
using FrostfallSaga.Utils;
using UnityEngine;

// ReSharper disable All

namespace FrostfallSaga.Grid
{
    public class KingdomGridGenerator : ABaseGridGenerator
    {
        private readonly PerlinTerrainManager _perlinTerrainManager;
        private readonly VoronoiBiomeManager _voronoiBiomeManager;

        public KingdomGridGenerator(KingdomCell hexPrefab, int gridWidth, int gridHeight, BiomeTypeSO[] availableBiomes,
            Transform parentGrid, float noiseScale, int seed)
            : base(hexPrefab, gridWidth, gridHeight, availableBiomes, parentGrid, noiseScale, seed)
        {
            _perlinTerrainManager = new PerlinTerrainManager(noiseScale, seed);
            _voronoiBiomeManager = new VoronoiBiomeManager(gridWidth, gridHeight, availableBiomes.Length, seed);
        }

        public override Dictionary<Vector2Int, Cell> GenerateGrid()
        {
            Dictionary<Vector2Int, Cell> gridCells = new();

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Vector3 centerPosition = HexMetrics.Center(AHexGrid.CELL_SIZE, x, y);
                    Cell cell = Object.Instantiate(HexPrefab, centerPosition, Quaternion.identity, ParentGrid);
                    cell.name = $"Cell[{x};{y}]";
                    int biomeIndex = _voronoiBiomeManager.GetClosestBiomeIndex(x, y);
                    BiomeTypeSO selectedBiome = AvailableBiomes[biomeIndex];
                    SetupCell(cell, x, y, selectedBiome);
                    gridCells[new Vector2Int(x, y)] = cell;
                }
            }

            return gridCells;
        }

        private void SetupCell(Cell cell, int x, int y, BiomeTypeSO selectedBiome)
        {
            float perlinValue = _perlinTerrainManager.GetNoiseValue(x, y);
            TerrainTypeSO selectedTerrain = GetTerrainTypeFromPerlinValue(perlinValue, selectedBiome);
            cell.Setup(new Vector2Int(x, y), ECellHeight.LOW, selectedTerrain, selectedBiome);
            cell.GenerateRandomAccessibility(EAccessibilityGenerationMode.FLIP_IF_ACCESSIBLE);
            cell.SetTerrain(selectedTerrain);
        }

        private static TerrainTypeSO GetTerrainTypeFromPerlinValue(float perlinValue, BiomeTypeSO selectedBiome)
        {
            TerrainTypeSO[] availableTerrains = selectedBiome.TerrainTypeSO;

            if (availableTerrains == null || availableTerrains.Length == 0)
            {
                Debug.LogError("No terrain types available for the current biome.");
                return null;
            }

            int terrainCount = availableTerrains.Length;
            float segmentSize = 1f / terrainCount;

            for (int i = 0; i < terrainCount; i++)
            {
                if (perlinValue < (i + 1) * segmentSize)
                {
                    return availableTerrains[i];
                }
            }

            return availableTerrains[terrainCount - 1];
        }

        public override string ToString()
        {
            return "BaseGridGenerator:\n" +
                   $"- HexPrefab: {HexPrefab.name}\n" +
                   $"- GridWidth: {GridWidth}\n" +
                   $"- GridHeight: {GridHeight}\n" +
                   $"- Available Biomes: {(AvailableBiomes != null && AvailableBiomes.Length > 0 ? string.Join(", ", AvailableBiomes.Select(b => b.name)) : "None")}\n" +
                   $"- ParentGrid: {ParentGrid?.name ?? "None"}\n" +
                   $"- NoiseScale: {NoiseScale}\n" +
                   $"- Seed: {Seed}";
        }
    }
}