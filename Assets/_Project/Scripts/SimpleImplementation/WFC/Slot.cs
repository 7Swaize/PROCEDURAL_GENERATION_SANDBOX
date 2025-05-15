using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class Slot {
        #region INSTANCE VARS

        private bool _isCollapsed;
        private Vector3Int _position;
        private Prototype _prototype;
        private List<Prototype> _possiblePrototypes;
        private WaveFunctionCollapseMapAbstract _map;

        #endregion

        #region READ-ONLY PROPERTIES

        public bool IsCollapsed => _isCollapsed;
        public Prototype Prototype => _prototype;
        public List<Prototype> PossiblePrototypes => _possiblePrototypes;
        public GameObject Prefab => _prototype == null ? null : _prototype?.GetPrefab(IsCollapsed);
        public int Entropy => _possiblePrototypes.Count;

        #endregion
        
        #region GETTER METHODS

        public Slot GetNeighbor(Orientations.Orientation direction) => _map.GetSlot(_position + Orientations.GetDirectionVector(direction));
        public Vector3Int GetNeighborPosition(Orientations.Orientation direction) => _position + Orientations.GetDirectionVector(direction);
        
        public Vector3Int GetPosition() => _position;
        public Quaternion GetRotation() => Prefab.transform.rotation;
        public Vector3 GetScale() => Prefab.transform.localScale;

        #endregion

        #region SETTERS

        public bool RemovePrototype(Prototype prototype) => !IsCollapsed && _possiblePrototypes.Remove(prototype);
        public void SetPossiblePrototypes(List<Prototype> possiblePrototypes) => _possiblePrototypes = possiblePrototypes;
        public void SetCollapsed(bool isCollapsed) => _isCollapsed = isCollapsed;

        #endregion
        
        
        public Slot(Vector3Int position, List<Prototype> possiblePrototypes, WaveFunctionCollapseMapAbstract map) {
            _position = position;
            _possiblePrototypes = possiblePrototypes;
            _map = map;
        }

        public bool Collapse() {
            if (_isCollapsed || _possiblePrototypes.Count == 0) {
                return false;
            }
            
            // TODO: Add weights feature -> Shannon Entropy?
            List<Prototype> orderedPrototypes = _possiblePrototypes
                .AsValueEnumerable()
                .OrderByDescending(p => p.prototypeWeight)
                .ThenBy(_ => Random.value)
                .ToList();

            foreach (Prototype prototype in orderedPrototypes) {
                if (ValidatePrototype(prototype)) {
                    _prototype = prototype;
                    _isCollapsed = true;

                    return true;
                }
            }
            
            return false;
        }

        public void CollapseDefault(Prototype prototypeToCollapseTo) {
            _prototype = prototypeToCollapseTo;
            _isCollapsed = true;
        }

        // TODO: Is this really needed? Test without it.
        // TODO: Set up something where if entropy is 0?
        private bool ValidatePrototype(Prototype prototype) {
            bool isValid = Orientations.HorizontalDirections
                .AsValueEnumerable()
                .All(dir => {
                    Slot neighbor = GetNeighbor(dir);
                    if (neighbor == null || !neighbor.IsCollapsed) return true;

                    var oppositeDir = Orientations.GetOppositeDirectionVector(dir);
                    return neighbor._prototype.possibleConnections[oppositeDir].Contains(prototype);
                });
            
            return isValid;
        }
    }
}