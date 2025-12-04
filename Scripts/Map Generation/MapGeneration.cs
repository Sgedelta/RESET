using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapGeneration : Node2D
{
    // ----- Grid / area -----
    [ExportCategory("Grid Settings")]
    [Export] public int Columns = 12;
    [Export] public int Rows = 8;
    [Export] public Rect2 WorldRect = new Rect2(-0, -0, 1200, 800);

    // ----- Path settings -----
    [ExportCategory("Path Settings")]
    [Export(PropertyHint.Range, "1,10,1")]
    public int PathLengthParam { get; set; } = 5;
    [Export] public int MaxWaypoints = 16;
    [Export] public bool AllowDiagonalPath = true;
    [Export] public PackedScene PathPointMarkerScene; 

    // ----- Tower settings -----
    [ExportCategory("Tower Settings")]
    [Export] public PackedScene TowerScene; // your tower tscn
    [Export] public NodePath TowersRootPath; // where to parent towers (optional)
    [Export] public float TowerCandidateAltitudeT = 0.5f; // 0..1 position on altitude line
    [Export] public float MinDistanceToPath = 48f; // px
    [Export] public float MinDistanceBetweenTowers = 96f; // px

    // ----- Slots / density -----
    [ExportCategory("Slots / density")]
    [Export] public int TotalAspectSlotsBudget = 20; // total slots to distribute
    [Export] public float SlotDensity = 0.5f; // probability factor when creating towers slots

    // ----- Misc -----
    [ExportCategory("Misc")]
    [Export] public bool DebugDraw = true;

    // runtime
    private Vector2 cellSize;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    // minimum number of waypoints (was missing in your file)
    private const int MinWaypoints = 3;

    public override void _Ready()
    {
        rng.Randomize();
    }

    public void Generate()
    {
        if (Columns <= 1 || Rows <= 1 || WorldRect.Size == Vector2.Zero)
        {
            GD.PrintErr("Invalid grid or world rect.");
            return;
        }
        cellSize = new Vector2(WorldRect.Size.X / Columns, WorldRect.Size.Y / Rows);

        var gridPathPoints = CreatePathGridPoints();
        if (gridPathPoints == null || gridPathPoints.Count < 3)
        {
            GD.PrintErr("Path generation failed.");
            return;
        }

        var pathNode = CreatePath2DFromGrid(gridPathPoints);

        var towerCandidates = ComputeTowerCandidates(gridPathPoints);

        var placedTowers = PlaceTowersFromCandidates(towerCandidates);

        DistributeSlotsToTowers(placedTowers);

        GD.Print($"MapGenerator: Generated path with {gridPathPoints.Count} waypoints, placed {placedTowers.Count} towers.");
    }


    private List<Vector2> CreatePathGridPoints()
    {
        float t = Mathf.Clamp(PathLengthParam, 1, 10) / 10f;
        int count = Mathf.RoundToInt(Mathf.Lerp(MinWaypoints, MaxWaypoints, t));

        var used = new HashSet<(int, int)>();
        var points = new List<(int, int)>();

        int startX = rng.RandiRange(1, Math.Max(1, Columns - 2));
        int startY = rng.RandiRange(1, Math.Max(1, Rows - 2));
        points.Add((startX, startY));
        used.Add((startX, startY));

        int attempts = 0;
        while (points.Count < count && attempts < count * 30)
        {
            attempts++;
            var (cx, cy) = points.Last();

            var offsets = new List<(int, int)>()
            {
                (1,0), (-1,0), (0,1), (0,-1)
            };
            if (AllowDiagonalPath)
            {
                offsets.AddRange(new (int, int)[] { (1, 1), (1, -1), (-1, 1), (-1, -1) });
            }

            offsets = offsets.OrderBy(x => rng.Randi()).ToList();

            bool placed = false;
            foreach (var off in offsets)
            {
                int nx = cx + off.Item1;
                int ny = cy + off.Item2;
                if (nx < 0 || nx >= Columns || ny < 0 || ny >= Rows) continue;
                if (used.Contains((nx, ny))) continue;

                if (rng.Randf() < 0.15f)
                {
                    nx = rng.RandiRange(0, Columns - 1);
                    ny = rng.RandiRange(0, Rows - 1);
                    if (used.Contains((nx, ny))) continue;
                }

                points.Add((nx, ny));
                used.Add((nx, ny));
                placed = true;
                break;
            }

            if (!placed)
            {
                int rx = rng.RandiRange(0, Columns - 1);
                int ry = rng.RandiRange(0, Rows - 1);
                if (!used.Contains((rx, ry)))
                {
                    points.Add((rx, ry));
                    used.Add((rx, ry));
                }
            }
        }

        var worldPoints = points.Select(p => GridToWorld(p.Item1, p.Item2)).ToList();
        if (DebugDraw) DrawPathDebug(worldPoints);
        return worldPoints;
    }

    private Path2D CreatePath2DFromGrid(List<Vector2> pathPoints)
    {
        //var path = new Path2D();
        // var curve = new Curve2D();
        //  curve.BakeInterval = 4f;
        /*  for (int i = 0; i < pathPoints.Count; i++)
          {
              curve.AddPoint(pathPoints[i]);
          }*/

        // Use the new Path2D that you created manually!
        var path = GetNode<Path2D>("new Enemy Path");  // <-- IMPORTANT

        var curve = new Curve2D();
        curve.BakeInterval = 4f;


        path.Curve = curve;
         AddChild(path);
      
        if (DebugDraw)
         {
             // optional visual markers
             for (int i = 0; i < pathPoints.Count; i++)
             {
                 if (PathPointMarkerScene != null)
                 {
                     var m = (Node2D)PathPointMarkerScene.Instantiate();
                     m.GlobalPosition = pathPoints[i];
                     path.AddChild(m);
                 }
             }
         }

         return path;

       
    }

    private List<Vector2> ComputeTowerCandidates(List<Vector2> pathPoints)
    {
        var candidates = new List<Vector2>();

        for (int i = 0; i + 2 < pathPoints.Count; i++)
        {
            var A = pathPoints[i];
            var B = pathPoints[i + 1];
            var C = pathPoints[i + 2];
            var midAC = (A + C) * 0.5f;

            var altitudePoint = B.Lerp(midAC, TowerCandidateAltitudeT);

            var snapped = WorldToGridCenter(altitudePoint);
            candidates.Add(snapped);
        }

        var unique = new List<Vector2>();
        foreach (var cand in candidates)
        {
            if (!unique.Any(u => u.DistanceTo(cand) < (Mathf.Min(cellSize.X, cellSize.Y) * 0.5f)))
                unique.Add(cand);
        }

        if (DebugDraw) DrawCandidatesDebug(unique);
        return unique;
    }



    private List<Node2D> PlaceTowersFromCandidates(List<Vector2> candidates)
    {
        Node towersRoot = null;
        if (!TowersRootPath.IsEmpty)
            towersRoot = GetNodeOrNull<Node>(TowersRootPath);

        if (towersRoot == null) towersRoot = this;

        var placed = new List<Node2D>();
        foreach (var cand in candidates)
        {
            if (GetDistanceToPath(cand) < MinDistanceToPath) continue;
            bool tooClose = placed.Any(t => t.GlobalPosition.DistanceTo(cand) < MinDistanceBetweenTowers);
            if (tooClose) continue;

            if (TowerScene == null)
            {
                var node = new Node2D();
                node.GlobalPosition = cand;
                towersRoot.AddChild(node);
                placed.Add(node);
            }
            else
            {
                var tower = (Node2D)TowerScene.Instantiate();
                tower.GlobalPosition = cand;
                towersRoot.AddChild(tower);
                placed.Add(tower);
            }
        }

        if (placed.Count == 0 && pathLengthFallback(pathPoints: candidates))
        {
            var fallback = GridToWorld(Columns / 2, Rows / 2);
            var node = TowerScene != null ? (Node2D)TowerScene.Instantiate() : new Node2D();
            node.GlobalPosition = fallback;
            towersRoot.AddChild(node);
            placed.Add(node);
        }

        if (DebugDraw) DrawPlacedTowersDebug(placed);
        return placed;
    }


    private void DistributeSlotsToTowers(List<Node2D> placedTowers)
    {
        if (placedTowers.Count == 0 || TotalAspectSlotsBudget <= 0) return;

        int remainingBudget = TotalAspectSlotsBudget;
        foreach (var t in placedTowers)
        {
            int give = 0;
            if (rng.Randf() < SlotDensity)
            {
                give = rng.RandiRange(1, Math.Max(1, remainingBudget));
            }

            t.SetMeta("AspectSlots", give);
            remainingBudget = Math.Max(0, remainingBudget - give);
            if (remainingBudget == 0) break;
        }

        int idx = 0;
        while (remainingBudget > 0 && placedTowers.Count > 0)
        {
            var t = placedTowers[idx % placedTowers.Count];
            int cur = t.HasMeta("AspectSlots") ? (int)t.GetMeta("AspectSlots") : 0;
            t.SetMeta("AspectSlots", cur + 1);
            remainingBudget--;
            idx++;
        }

        foreach (var t in placedTowers)
        {
            if (t.HasMethod("ApplyGeneratedAspectSlots"))
            {
                int slots = t.HasMeta("AspectSlots") ? (int)t.GetMeta("AspectSlots") : 0;
                t.Call("ApplyGeneratedAspectSlots", slots);
            }
        }
    }


    private Vector2 GridToWorld(int gx, int gy)
    {
        float x = WorldRect.Position.X + (gx + 0.5f) * cellSize.X;
        float y = WorldRect.Position.Y + (gy + 0.5f) * cellSize.Y;
        return new Vector2(x, y);
    }

    private Vector2 WorldToGridCenter(Vector2 worldPos)
    {
        int gx = Mathf.Clamp((int)((worldPos.X - WorldRect.Position.X) / cellSize.X), 0, Columns - 1);
        int gy = Mathf.Clamp((int)((worldPos.Y - WorldRect.Position.Y) / cellSize.Y), 0, Rows - 1);
        return GridToWorld(gx, gy);
    }

    private float GetDistanceToPath(Vector2 p)
    {
        Path2D path = FindFirstPath2DChild();
        if (path == null) return float.MaxValue;

        var curve = path.Curve;
        if (curve == null || curve.PointCount < 2) return float.MaxValue;

        float best = float.MaxValue;
        Vector2 last = curve.GetPointPosition(0);
        for (int i = 1; i < curve.PointCount; i++)
        {
            Vector2 cur = curve.GetPointPosition(i);
            float d = DistancePointSegment(p, last, cur);
            if (d < best) best = d;
            last = cur;
        }
        return best;
    }

    private float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float ab2 = ab.LengthSquared();
        if (ab2 == 0) return ap.Length();
        float t = Mathf.Clamp(ap.Dot(ab) / ab2, 0f, 1f);
        Vector2 proj = a + ab * t;
        return p.DistanceTo(proj);
    }

    private void DrawPathDebug(List<Vector2> points)
    {
        if (!DebugDraw) return;
        foreach (var p in points)
        {
            var m = new ColorRect();
            m.Color = new Color(0, 0.5f, 1, 0.35f);
            m.Size = new Vector2(8, 8);
            m.Position = p - new Vector2(4, 4);
            AddChild(m);
        }
    }

    private void DrawCandidatesDebug(List<Vector2> cands)
    {
        if (!DebugDraw) return;
        foreach (var p in cands)
        {
            var m = new ColorRect();
            m.Color = new Color(1, 0.5f, 0, 0.5f);
            m.Size = new Vector2(6, 6);
            m.Position = p - new Vector2(3, 3);
            AddChild(m);
        }
    }

    private void DrawPlacedTowersDebug(List<Node2D> towers)
    {
        if (!DebugDraw) return;
        foreach (var t in towers)
        {
            var m = new ColorRect();
            m.Color = new Color(0.8f, 0.2f, 0.2f, 0.55f);
            m.Size = new Vector2(12, 12);
            m.Position = t.GlobalPosition - new Vector2(6, 6);
            AddChild(m);
        }
    }

    private bool pathLengthFallback(List<Vector2> pathPoints)
    {
        return pathPoints != null && pathPoints.Count > 0;
    }

    private Path2D FindFirstPath2DChild()
    {
        foreach (var obj in GetChildren())
        {
            if (obj is Path2D p) return p;
        }
        return null;
    }
}
