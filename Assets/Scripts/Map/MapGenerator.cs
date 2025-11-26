using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public MapConfig config;

    public List<List<MapNode>> GenerateMap()
    {
        if (config == null)
        {
            Debug.LogError("MapConfig is missing!");
            return null;
        }

        List<List<MapNode>> map = new List<List<MapNode>>();

        // 1. Generate Layers and Nodes
        for (int i = 0; i < config.layers; i++)
        {
            List<MapNode> layerNodes = new List<MapNode>();
            int nodeCount = (i == 0 || i == config.layers - 1) ? 1 : Random.Range(config.minNodesPerLayer, config.maxNodesPerLayer + 1);
            
            // Force 1 node for Boss layer
            if (i == config.layers - 1) nodeCount = 1;

            for (int j = 0; j < nodeCount; j++)
            {
                NodeType type = GetRandomNodeType(i);
                if (i == config.layers - 1) type = NodeType.Boss;
                else if (i == 0) type = NodeType.Combat; // First node always combat

                MapNode node = new MapNode(type, i, j);
                node.position = new Vector2(i * config.nodeSpacingX, 0); // X increases with layer
                layerNodes.Add(node);
            }
            map.Add(layerNodes);
        }

        // 2. Center Nodes vertically
        for (int i = 0; i < map.Count; i++)
        {
            float layerHeight = (map[i].Count - 1) * config.nodeSpacingY;
            for (int j = 0; j < map[i].Count; j++)
            {
                map[i][j].position.y = (j * config.nodeSpacingY) - (layerHeight / 2f);
            }
        }

        // 3. Connect Nodes
        ConnectLayers(map);

        return map;
    }

    private void ConnectLayers(List<List<MapNode>> map)
    {
        for (int i = 0; i < map.Count - 1; i++)
        {
            List<MapNode> currentLayer = map[i];
            List<MapNode> nextLayer = map[i + 1];

            // Ensure every node in current layer has at least one child
            foreach (var node in currentLayer)
            {
                int attempts = 0;
                while (node.outgoingConnectionIndices.Count == 0 && attempts < 10)
                {
                    int targetIndex = Random.Range(0, nextLayer.Count);
                    ConnectNodes(node, nextLayer[targetIndex]);
                    attempts++;
                }
            }

            // Ensure every node in next layer has at least one parent
            foreach (var nextNode in nextLayer)
            {
                int attempts = 0;
                while (nextNode.incomingConnectionIndices.Count == 0 && attempts < 10)
                {
                    int sourceIndex = Random.Range(0, currentLayer.Count);
                    ConnectNodes(currentLayer[sourceIndex], nextNode);
                    attempts++;
                }
            }
            
            // Add some random extra connections
            int extraConnections = Mathf.Max(0, (currentLayer.Count + nextLayer.Count) / 2);
            for(int k=0; k<extraConnections; k++)
            {
                 int sourceIndex = Random.Range(0, currentLayer.Count);
                 int targetIndex = Random.Range(0, nextLayer.Count);
                 ConnectNodes(currentLayer[sourceIndex], nextLayer[targetIndex]);
            }
        }
    }

    private void ConnectNodes(MapNode source, MapNode target)
    {
        if (!source.outgoingConnectionIndices.Contains(target.nodeIndex))
        {
            source.outgoingConnectionIndices.Add(target.nodeIndex);
        }
        if (!target.incomingConnectionIndices.Contains(source.nodeIndex))
        {
            target.incomingConnectionIndices.Add(source.nodeIndex);
        }
    }

    private NodeType GetRandomNodeType(int layerIndex)
    {
        // Simple weighted random
        int totalWeight = 0;
        foreach (var w in config.nodeWeights) totalWeight += w.weight;

        int rnd = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var w in config.nodeWeights)
        {
            currentWeight += w.weight;
            if (rnd < currentWeight) return w.type;
        }

        return NodeType.Combat;
    }
}
