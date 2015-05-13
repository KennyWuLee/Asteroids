using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    using SharpDX;
    using SharpDX.Toolkit.Content;
    using SharpDX.Toolkit.Graphics;

    class Laser : Drawable
    {
        private static Texture2D texture;

        private const float RANGE = 750f;
        private const float BLAST_SPEED = 75f;
        private static float BLAST_SIZE = 1;

        public Laser(Vector3 position, Vector3 direction, Vector3 shipVelocity)
        {
            alive = true;
            radius = BLAST_SIZE;
            this.position = position;
            this.velocity = Vector3.Normalize(direction) * BLAST_SPEED + shipVelocity;
        }

        public bool isInRange()
        {
            return position.Length() < RANGE;
        }

        public static void loadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("blast");
        }

        public void Update(float time)
        {
            position += velocity * time;
        }

        public void Draw(BasicEffect basicEffect, bool firstPerson, Ship ship)
        {
            basicEffect.DiffuseColor = (Vector4)Color.White;
            GraphicsDevice graphicsDevice = basicEffect.GraphicsDevice;
            GeometricPrimitive plane = GeometricPrimitive.Plane.New(graphicsDevice, 2 * BLAST_SIZE, 2 * BLAST_SIZE);
            graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);
            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.Additive);
            basicEffect.Texture = texture;
            Matrix billboard;
            if(firstPerson)
                billboard = Matrix.BillboardRH(position, Vector3.Zero, ship.getUp(), Vector3.Zero + ship.getForward());
            else
                billboard = Matrix.BillboardRH(position, new Vector3(0, 0, 50), Vector3.UnitY, -Vector3.UnitZ);
            basicEffect.World = Matrix.Scaling(1) * billboard;

            plane.Draw(basicEffect);
        }
         public static void doubleSize()
        {
            if(BLAST_SIZE < 17)
                BLAST_SIZE *= 1.25f;
        }
         public static void resetSize()
         {
             BLAST_SIZE = 1;
         }

    }
}
