using System;
using UnityEngine;

namespace Terra.Art
{
    /// <summary>Visual definitions only. Eligibility, cost, ownership and simulation remain in gameplay recipes.</summary>
    [CreateAssetMenu(menuName = "Terra/Build action art catalog")]
    public sealed class BuildActionCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string actionId, title, prdSection, dimensionNotes;
            public GameObject prefab;
            public Sprite icon;
            public Vector2 siteSizeMetres;
            public Bounds meshBoundsMetres;
            public float clearanceRadiusMetres;
            public bool landscapeSample;
            public int triangles;
        }

        public Entry[] entries;

        public Entry Find(string actionId)
        {
            // Preserve the current scenario's IDs while wider PRD recipes are still being authored.
            switch (actionId)
            {
                case "cultivate-standard": actionId = "cultivation"; break;
                case "cultivate-margin": actionId = "living-margin"; break;
                case "restore-grass": actionId = "ground-cover-restoration"; break;
                case "water-swale": actionId = "vegetated-swale"; break;
            }
            if (entries == null) return null;
            foreach (var entry in entries) if (entry.actionId == actionId) return entry;
            return null;
        }
    }
}
