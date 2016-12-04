using Ensage;
using Ensage.Heroes;
using SharpDX;

namespace MeepoSharp
{
    public class JungleCamps
    {
        public Meepo meepos { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 StackPosition { get; set; }
        public Vector3 WaitPosition { get; set; }
        public Team Team { get; set; }
        public int Id { get; set; }
        public bool Farming { get; set; }
        public int LvlReq { get; set; }
        public bool Visible { get; set; }
        public int VisTime { get; set; }
        public bool Stacking { get; set; }
        public bool Stacked { get; set; }
        public bool Ancients { get; set; }
        public bool Empty { get; set; }
        public int State { get; set; }
        public int AttackTime { get; set; }
        public int Creepscount { get; set; }
        public int Starttime { get; set; }
    }
}
