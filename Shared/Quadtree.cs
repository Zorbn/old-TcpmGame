namespace Shared;

public class Quadtree
{
    private const int MaxObjects = 10;
    private const int MaxLevels = 5;

    private readonly int level;
    private readonly List<Collider> colliders;
    private readonly Collider bounds;
    private readonly Quadtree?[] nodes;

    public Quadtree(int level, Collider bounds)
    {
        this.level = level;
        colliders = new List<Collider>();
        this.bounds = bounds;
        nodes = new Quadtree?[4];
    }

    public void Clear()
    {
        colliders.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null) continue;
            
            nodes[i]?.Clear();
            nodes[i] = null;
        }
    }

    private void Split()
    {
        int subWidth = (int)(bounds.Width / 2);
        int subHeight = (int)(bounds.Height / 2);
        int x = (int)bounds.X;
        int y = (int)bounds.Y;

        nodes[0] = new Quadtree(level + 1, new Collider(x + subWidth, y, subWidth, subHeight, null));
        nodes[1] = new Quadtree(level + 1, new Collider(x, y, subWidth, subHeight, null));
        nodes[2] = new Quadtree(level + 1, new Collider(x, y + subHeight, subWidth, subHeight, null));
        nodes[3] = new Quadtree(level + 1, new Collider(x + subWidth, y + subHeight, subWidth, subHeight, null));
    }

    private int GetIndex(Collider col)
    {
        int index = -1;
        float verticalMidpoint = bounds.X + bounds.Width / 2f;
        float horizontalMidpoint = bounds.Y + bounds.Height / 2f;

        bool fitsInTopQuadrant = col.Y < horizontalMidpoint && col.Y + col.Height < horizontalMidpoint;
        bool fitsInBottomQuadrant = col.Y > horizontalMidpoint;
        
        // Check if collider fits in left quadrants
        if (col.X < verticalMidpoint && col.X + col.Width < verticalMidpoint)
        {
            if (fitsInTopQuadrant)
            {
                index = 1;
            }
            else if (fitsInBottomQuadrant)
            {
                index = 2;
            }
        }
        else if (col.X > verticalMidpoint) // Check if collider fits in right quadrants
        {
            if (fitsInTopQuadrant)
            {
                index = 0;
            }
            else if (fitsInBottomQuadrant)
            {
                index = 3;
            }
        }

        return index;
    }

    public void Insert(Collider col)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(col);

            if (index != -1)
            {
                nodes[index]?.Insert(col);
                return;
            }
        }

        colliders.Add(col);

        if (colliders.Count > MaxObjects && level < MaxLevels)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < colliders.Count)
            {
                int index = GetIndex(colliders[i]);
                if (index != -1)
                {
                    nodes[index]?.Insert(colliders[i]);
                    colliders.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public void Retrieve(ref List<Collider> returnObjects, Collider col)
    {
        int index = GetIndex(col);

        if (index != 1 && nodes[0] != null)
        {
            nodes[index]?.Retrieve(ref returnObjects, col);
        }
        
        returnObjects.AddRange(colliders);
    }
}