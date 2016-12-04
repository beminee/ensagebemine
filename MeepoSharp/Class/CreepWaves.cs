
using System.Collections.Generic;
using Ensage;
using Ensage.Heroes;
using SharpDX;

namespace MeepoSharp.Class
{
    public class CreepWaves
    {
        public string Name { get; set; }
        public Meepo meepo { get; set; }
        public List<Unit> Creeps { get; set; }
        public Vector3 Position { get; set; }
        public List<Vector3> Coords { get; set; }
    }
}
