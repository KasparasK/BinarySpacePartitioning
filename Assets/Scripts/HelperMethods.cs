using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    public static Bounds GetCombinedBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds combinedBounds;

        if (renderers.Length == 0)
        {
            Debug.LogError("Mesh must have at least 1 renderer in the hierarchy");
            return new Bounds();
        }
        else
        {
            combinedBounds = renderers[0].bounds;
            if (renderers.Length > 1)
            {
                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }
            }
        }

        return combinedBounds;
    }
}
