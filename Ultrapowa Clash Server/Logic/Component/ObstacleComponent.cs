namespace UCS.Logic
{
    internal class ObstacleComponent : Component
    {
        public ObstacleComponent(GameObject go) : base(go)
        {
        }

        const int m_vType = 0x01AB3F00;

        public override int Type => 11;
    }
}