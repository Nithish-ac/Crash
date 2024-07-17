using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSU
{
    #region DSU
    private Dictionary<PlayerController, PlayerController> parent = new Dictionary<PlayerController, PlayerController>();
    private Dictionary<PlayerController, int> rank = new Dictionary<PlayerController, int>();

    public void Add(PlayerController player)
    {
        if (!parent.ContainsKey(player))
        {
            parent[player] = player;
            rank[player] = 0;
        }
    }

    public PlayerController Find(PlayerController player)
    {
        if (parent[player] != player)
        {
            parent[player] = Find(parent[player]);
        }
        return parent[player];
    }

    public void Union(PlayerController player1, PlayerController player2)
    {
        PlayerController root1 = Find(player1);
        PlayerController root2 = Find(player2);

        if (root1 != root2)
        {
            if (rank[root1] > rank[root2])
            {
                parent[root2] = root1;
            }
            else if (rank[root1] < rank[root2])
            {
                parent[root1] = root2;
            }
            else
            {
                parent[root2] = root1;
                rank[root1]++;
            }
        }
    }

    public IEnumerable<PlayerController> GetAllRoots()
    {
        var roots = new HashSet<PlayerController>();
        foreach (var item in parent)
        {
            roots.Add(Find(item.Key));
        }
        return roots;
    }

    public IEnumerable<PlayerController> GetComponents(PlayerController root)
    {
        var components = new List<PlayerController>();
        foreach (var item in parent)
        {
            if (Find(item.Key) == root)
            {
                components.Add(item.Key);
            }
        }
        return components;
    }
    #endregion
}
