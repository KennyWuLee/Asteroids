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

    class Explosion
    {
        private const float EXPLOSION_TIME = 1f;
        private static Texture2D texture;
        private float timer;

        public Explosion()
        {
            timer = 0;
        }

        public static void loadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("explosion");
        }

        public bool isActive()
        {
            return timer > 0;
        }

        public void Update(float time)
        {
            if (timer > 0)
                timer -= time;
        }

        public void explode()
        {
            timer = EXPLOSION_TIME;
        }

        public void Draw(SpriteBatch spriteBatch, BasicEffect basicEffect)
        {
            basicEffect.DiffuseColor = (Vector4)Color.White;
            GraphicsDevice graphicsDevice = basicEffect.GraphicsDevice;
            GeometricPrimitive plane = GeometricPrimitive.Plane.New(graphicsDevice);
            graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
            graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.DepthRead);
            graphicsDevice.SetBlendState(graphicsDevice.BlendStates.Additive);
            basicEffect.Texture = texture;
            Matrix billboard = Matrix.BillboardRH(Vector3.Zero, new Vector3(0, 0, 50), Vector3.UnitY, -Vector3.UnitZ);
            basicEffect.World = Matrix.Scaling(10 * (2f - timer)) * billboard;

            plane.Draw(basicEffect);
        }
    }
}
