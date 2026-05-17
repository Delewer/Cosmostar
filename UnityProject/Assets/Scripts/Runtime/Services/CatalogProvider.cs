using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Runtime.Data;
using UnityEngine;

namespace Cosmostar.Runtime.Services
{
    public static class CatalogProvider
    {
        public static VerticalSliceCatalog Load()
        {
            var asset = Resources.Load<VerticalSliceCatalogAsset>("Cosmostar/VerticalSliceCatalog");
            if (asset != null && asset.Catalog != null && asset.Catalog.Missions.Count > 0)
            {
                return asset.Catalog;
            }

            return VerticalSliceBlueprints.CreateDefaultCatalog();
        }
    }
}
