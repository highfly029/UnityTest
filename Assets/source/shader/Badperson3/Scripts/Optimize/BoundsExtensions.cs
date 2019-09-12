using UnityEngine;

public static class BoundsExtensions {
    public static void Union(this Bounds b1, Bounds b2) {
        var minx = Mathf.Min(b1.min.x, b2.min.x);
        var miny = Mathf.Min(b1.min.y, b2.min.y);
        var minz = Mathf.Min(b1.min.z, b2.min.z);

        var maxx = Mathf.Max(b1.max.x, b2.max.x);
        var maxy = Mathf.Max(b1.max.y, b2.max.y);
        var maxz = Mathf.Max(b1.max.z, b2.max.z);

        b1.min = new Vector3(minx, miny, minz);
        b1.max = new Vector3(maxx, maxy, maxz);
    }
}
