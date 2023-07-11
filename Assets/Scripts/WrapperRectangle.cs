using UnityEngine;
using static StuPro.Randomizer;

namespace StuPro
{
    public class WrapperRectangle
    {
        protected Obstacle data;

        protected float posX;
        protected float posY;
        protected float width;
        protected float height;

        public WrapperRectangle(float posX, float posY, float width, float height)
        {
            this.posX = posX;
            this.posY = posY;
            this.width = width;
            this.height = height;
        }

        public WrapperRectangle(Obstacle data, float width, float height) : this(0, 0, width, height)
        {
            this.data = data;
        }

        public bool canContain(WrapperRectangle rectangle)
        {
            return width >= rectangle.width && height >= rectangle.height;
        }

        public Obstacle getData()
        {
            return data;
        }

        public float getPosX()
        {
            return posX;
        }

        public void setPosX(float posX)
        {
            this.posX = posX;
        }

        public float getPosY()
        {
            return posY;
        }

        public void setPosY(float posY)
        {
            this.posY = posY;
        }
        public float getWidth()
        {
            return width;
        }
        public float getHeight()
        {
            return height;
        }

        public float getArea()
        {
            return width * height;
        }

        public string ToString()
        {
            return "x: " + posX + ", y: " + posY + " | w: " + width + ", h: " + height;
        }
    }
}