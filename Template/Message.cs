using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    using SharpDX;
    using SharpDX.Toolkit.Graphics;

    class Message
    {
        float timer;
        bool forever;
        String text;

        public Message()
        {
            forever = false;
            timer = 0;
        }

        public bool isActive()
        {
            return forever || timer > 0;
        }

        public void Update(float time)
        {
            if (timer > 0)
                timer -= time;
        }

        public void displayMessage(String text)
        {
            forever = true;
            this.text = text;
        }

        public void displayMessage(String text, float duration)
        {
            forever = false;
            this.text = text;
            timer = duration;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

            spriteBatch.Begin();
            Vector2 dimensions = font.MeasureString(text);
            spriteBatch.DrawString(font, text, new Vector2(graphicsDevice.BackBuffer.Width / 2 - dimensions.X / 2, graphicsDevice.BackBuffer.Height / 2 - dimensions.Y / 2), Color.White);
            spriteBatch.End();
        }
    }
}
