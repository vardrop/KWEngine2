using KWEngine2.GameObjects;

namespace KWEngine2.Collision
{
    internal struct CollisionPair
    {
        internal GameObject A;
        internal GameObject B;

        public CollisionPair(GameObject a, GameObject b)
        {
            A = a;
            B = b;
        }
    }
}
