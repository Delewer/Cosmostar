using Cosmostar.Core.Models;
using UnityEngine;

namespace Cosmostar.Runtime.Data
{
    [CreateAssetMenu(menuName = "Cosmostar/Vertical Slice Catalog", fileName = "VerticalSliceCatalog")]
    public sealed class VerticalSliceCatalogAsset : ScriptableObject
    {
        public VerticalSliceCatalog Catalog = new VerticalSliceCatalog();
    }
}
