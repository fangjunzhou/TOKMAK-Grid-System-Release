using FinTOKMAK.GridSystem.Square.Generator;
using NaughtyAttributes;
using UnityEngine;

namespace FinTOKMAK.GridSystem.Square
{
    public class SquareGridManager : MonoBehaviour
    {
        #region Public Field

        /// <summary>
        /// The generator managed by this manager. Use the generator to generate grid and manage grid system.
        /// </summary>
        [Required()]
        public SquareGridGenerator generator;

        /// <summary>
        /// The initial size of the map.
        /// </summary>
        public Vector2Int mapSize;
        
        /// <summary>
        /// The file path current grid manager use to store map data.
        /// </summary>
        public string mapFilePath;

        #endregion

        #region Private Methods
        
        
        
        #endregion
    }
}