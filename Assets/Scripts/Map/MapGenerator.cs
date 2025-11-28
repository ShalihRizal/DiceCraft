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

        // 1. Generate Layers and Nodes (Dense Grid)
        int gridHeight = config.maxNodesPerLayer;
        float totalMapHeight = (gridHeight - 1) * config.nodeSpacingY;
        
        // Probability of a node existing in the middle layers
        float nodeProbability = 0.8f; 

        for (int i = 0; i < config.layers; i++)
        {
            List<MapNode> layerNodes = new List<MapNode>();
            
            for (int row = 0; row < gridHeight; row++)
            {
                // Logic for Start/End Nodes
                if (i == 0 || i == config.layers - 1)
                {
                    if (row != gridHeight / 2) continue; // Only center node
                }
                else
                {
                    // Force Neighbors for Start Node (Layer 1)
                    if (i == 1)
                    {
                         // Start Node is at gridHeight/2 (Even Layer 0)
                         // Connects to gridHeight/2 and gridHeight/2 - 1
                         int center = gridHeight / 2;
                         if (row == center || row == center - 1) 
                         {
                             // Force create
                         }
                         else if (Random.value > nodeProbability) continue;
                    }
                    // Force Neighbors for End Node (Layer N-1)
                    else if (i == config.layers - 2)
                    {
                        // End Node is at gridHeight/2 (Layer N, could be Even or Odd)
                        // We need to ensure Layer N-1 has nodes that connect TO it.
                        // If Layer N-1 is Even (and N is Odd): Connects to R and R-1 (of N). 
                        // Wait, connection is Forward.
                        // Layer N-1 (Even) connects to Layer N (Odd).
                        // We want Layer N-1 nodes that connect to EndNode(R).
                        // Even(R) -> Odd(R), Odd(R-1).
                        // So if EndNode is R. We need Even(R) or Even(R+1)?
                        // Let's trace back.
                        // If N is Odd. End is R.
                        // Even(R) connects to Odd(R) and Odd(R-1). So Even(R) connects to End(R).
                        // Even(R+1) connects to Odd(R+1) and Odd(R). So Even(R+1) connects to End(R).
                        
                        // If N is Even. End is R.
                        // Odd(R) connects to Even(R) and Even(R+1). So Odd(R) connects to End(R).
                        // Odd(R-1) connects to Even(R-1) and Even(R). So Odd(R-1) connects to End(R).
                        
                        // Simplified: Just force nodes around the center in the penultimate layer
                        int center = gridHeight / 2;
                        if (row >= center - 1 && row <= center + 1)
                        {
                            // Force create
                        }
                        else if (Random.value > nodeProbability) continue;
                    }
                    else
                    {
                        // Random chance to skip node (create holes)
                        if (Random.value > nodeProbability) continue;
                    }
                }

                NodeType type = GetRandomNodeType(i);
                if (i == config.layers - 1) type = NodeType.Boss;
                else if (i == 0) type = NodeType.Combat;

                MapNode node = new MapNode(type, i, layerNodes.Count); 
                node.rowIndex = row; 
                
                // Hex Grid Position Logic
                float x = i * config.nodeSpacingX;
                float y = (row * config.nodeSpacingY) + ((i % 2) * config.nodeSpacingY * 0.5f);
                y -= totalMapHeight / 2f;

                node.position = new Vector2(x, y);
                layerNodes.Add(node);
            }
            map.Add(layerNodes);
        }

        // 2. Connect Nodes (Strict Hex Adjacency)
        ConnectLayersHex(map);
        
        // 3. Prune Unreachable Nodes (Flood Fill)
        PruneUnreachableNodes(map);

        return map;
    }

    public void RecalculatePositions(List<List<MapNode>> map)
    {
        if (config == null || map == null) return;

        int gridHeight = config.maxNodesPerLayer;
        float totalMapHeight = (gridHeight - 1) * config.nodeSpacingY;

        for (int i = 0; i < map.Count; i++)
        {
            foreach (var node in map[i])
            {
                float x = i * config.nodeSpacingX;
                float y = (node.rowIndex * config.nodeSpacingY) + ((i % 2) * config.nodeSpacingY * 0.5f);
                y -= totalMapHeight / 2f;
                
                node.position = new Vector2(x, y);
            }
        }
    }

    private void ConnectLayersHex(List<List<MapNode>> map)
    {
        for (int i = 0; i < map.Count - 1; i++)
        {
            List<MapNode> currentLayer = map[i];
            List<MapNode> nextLayer = map[i + 1];

            foreach (var node in currentLayer)
            {
                // Strict Hex Neighbors (Forward)
                // Even Col (i): Neighbors at Row, Row-1
                // Odd Col (i): Neighbors at Row, Row+1
                
                List<int> targetRows = new List<int>();
                targetRows.Add(node.rowIndex); // Same row is always a neighbor candidate?
                // Wait, let's re-verify the offset logic.
                // Even Col (0): Y = Row * H.
                // Odd Col (1): Y = Row * H + 0.5H.
                // Neighbor of Even(R) at Odd:
                // Odd(R) is at Y+0.5H. (Down-Right)
                // Odd(R-1) is at Y-0.5H. (Up-Right)
                // So for Even Col: Neighbors are R and R-1.
                
                // Neighbor of Odd(R) at Even(Next is Even):
                // Even(R) is at Y (Up-Right relative to Odd's Y+0.5)
                // Even(R+1) is at Y+H (Down-Right relative to Odd's Y+0.5)
                // So for Odd Col: Neighbors are R and R+1.
                
                if (i % 2 == 0) // Even Layer -> Odd Layer
                {
                    targetRows.Add(node.rowIndex - 1);
                }
                else // Odd Layer -> Even Layer
                {
                    targetRows.Add(node.rowIndex + 1);
                }

                foreach (var targetRow in targetRows)
                {
                    // Find node in next layer with this row index
                    var targetNode = nextLayer.Find(n => n.rowIndex == targetRow);
                    if (targetNode != null)
                    {
                        ConnectNodes(node, targetNode);
                    }
                }
                
                // Extra "Right" Neighbor (Layer i+2, Same Row)
                // User requested 3 traversable nodes: Top-Right, Right, Bottom-Right.
                // "Right" corresponds to the node in the same row, 2 layers ahead (skipping the staggered layer).
                if (i + 2 < map.Count)
                {
                    List<MapNode> twoLayersAhead = map[i + 2];
                    var rightNode = twoLayersAhead.Find(n => n.rowIndex == node.rowIndex);
                    if (rightNode != null)
                    {
                        ConnectNodes(node, rightNode);
                        // Debug.Log($"Connected Node {node.nodeIndex}(L{i}) to Right Node {rightNode.nodeIndex}(L{i+2})");
                    }
                    // else Debug.Log($"Could not find Right Node for {node.nodeIndex}(L{i}) at Row {node.rowIndex} in Layer {i+2}");
                }
            }
        }
    }
    
    private void PruneUnreachableNodes(List<List<MapNode>> map)
    {
        if (map.Count == 0 || map[0].Count == 0) return;
        
        // 1. Forward BFS (Reachable from Start)
        HashSet<MapNode> forwardReachable = new HashSet<MapNode>();
        Queue<MapNode> queue = new Queue<MapNode>();
        
        foreach(var node in map[0])
        {
            forwardReachable.Add(node);
            queue.Enqueue(node);
        }
        
        while(queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            foreach(var connection in current.outgoingConnections)
            {
                if (connection.targetLayer < map.Count)
                {
                    var nextLayer = map[connection.targetLayer];
                    if (connection.targetIndex < nextLayer.Count)
                    {
                        MapNode target = nextLayer[connection.targetIndex];
                        if (!forwardReachable.Contains(target))
                        {
                            forwardReachable.Add(target);
                            queue.Enqueue(target);
                        }
                    }
                }
            }
        }

        // 2. Backward BFS (Can Reach End)
        HashSet<MapNode> backwardReachable = new HashSet<MapNode>();
        queue.Clear();
        
        // Add all nodes in the last layer (Boss)
        foreach(var node in map[map.Count - 1])
        {
            backwardReachable.Add(node);
            queue.Enqueue(node);
        }
        
        while(queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            foreach(var connection in current.incomingConnections)
            {
                if (connection.targetLayer >= 0)
                {
                    var prevLayer = map[connection.targetLayer];
                    if (connection.targetIndex < prevLayer.Count)
                    {
                        MapNode source = prevLayer[connection.targetIndex];
                        if (!backwardReachable.Contains(source))
                        {
                            backwardReachable.Add(source);
                            queue.Enqueue(source);
                        }
                    }
                }
            }
        }
        
        // 3. Intersect: Node must be in BOTH sets
        foreach(var layer in map)
        {
            for(int j = layer.Count - 1; j >= 0; j--)
            {
                MapNode node = layer[j];
                if (!forwardReachable.Contains(node) || !backwardReachable.Contains(node))
                {
                    // Remove connections TO this node from previous layers
                    // This is hard because incoming connections point to source.
                    // We need to find nodes that point TO this node.
                    // Actually, we are just going to Re-Connect everything at the end.
                    // So we don't need to manually remove connections here.
                    
                    layer.RemoveAt(j);
                }
            }
            
            // Re-assign list indices after removal
            for(int j=0; j<layer.Count; j++)
            {
                layer[j].nodeIndex = j;
            }
        }
        
        // 4. Re-Connect Everything
        foreach(var layer in map)
        {
            foreach(var node in layer)
            {
                node.outgoingConnections.Clear();
                node.incomingConnections.Clear();
            }
        }
        ConnectLayersHex(map);
    }

    private void ConnectNodes(MapNode source, MapNode target)
    {
        // Check if connection already exists
        if (!source.outgoingConnections.Exists(c => c.targetLayer == target.layerIndex && c.targetIndex == target.nodeIndex))
        {
            source.outgoingConnections.Add(new NodeConnection(target.layerIndex, target.nodeIndex));
        }
        if (!target.incomingConnections.Exists(c => c.targetLayer == source.layerIndex && c.targetIndex == source.nodeIndex))
        {
            target.incomingConnections.Add(new NodeConnection(source.layerIndex, source.nodeIndex));
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
