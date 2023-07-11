using System.Collections.Generic;
using UnityEngine;
using System;

namespace StuPro
{
    public class QuadTreeNode : WrapperRectangle
    {
        private QuadTreeNode parent;

        private List<QuadTreeNode> children;

        private static System.Random random = new System.Random();

        public QuadTreeNode(float posX, float posY, float width, float height) : base(posX, posY, width, height)
        {
            children = new List<QuadTreeNode>();
        }

        private QuadTreeNode(QuadTreeNode parent, float posX, float posY, float width, float height) : this(posX, posY, width, height)
        {
            this.parent = parent;
        }

        public bool insert(WrapperRectangle rectangle, List<GameObject> avoidList)
        {
            if (isLeaf())
            {
                if (!canContain(rectangle))
                {
                    return false;
                }

                assignRandomPositionTo(rectangle, avoidList);
                splitAround(rectangle);

                data = rectangle.getData();
                posX = rectangle.getPosX();
                posY = rectangle.getPosY();
                width = rectangle.getWidth();
                height = rectangle.getHeight();

                return true;
            }

            List<QuadTreeNode> childrenCopy = new List<QuadTreeNode>(children);
            while (childrenCopy.Count != 0)
            {
                QuadTreeNode selectedChild = SelectChildFrom(childrenCopy);

                if (selectedChild.insert(rectangle, avoidList))
                    return true;

                childrenCopy.Remove(selectedChild);
            }

            return false;
        }

        private QuadTreeNode SelectChildFrom(List<QuadTreeNode> list)
        {
            float totalArea = 0.0f;
            foreach (QuadTreeNode c in list)
            {
                totalArea += c.getArea();
            }

            int randomNumber = random.Next(Mathf.Max(1, (int)Mathf.Floor(totalArea)));

            float counter = 0.0f;
            foreach (QuadTreeNode c in list)
            {
                counter += c.getArea();

                if (Mathf.Round(counter) >= randomNumber)
                    return c;
            }
            return children[0];
        }

        private bool isLeaf()
        {
            return children.Count == 0;
        }


        private float GetDistance(WrapperRectangle rectangle, GameObject obj)
        {
            double ox = obj.transform.position.x;
            double oy = obj.transform.position.z;

            double rx = rectangle.getPosX() -7;
            double ry = rectangle.getPosY() -11;

            double a2 = Math.Pow(ox - rx, 2);
            double b2 = Math.Pow(oy - ry, 2);
            
            float result = (float) Math.Sqrt(a2 + b2);
            return result;
        }


        private void assignRandomPositionTo(WrapperRectangle rectangle, List<GameObject> avoidList)
        {
            rectangle.setPosX((float)(posX + rectangle.getWidth() / 2 + random.NextDouble() * (width - rectangle.getWidth())));
            rectangle.setPosY((float)(posY + rectangle.getHeight() / 2 + random.NextDouble() * (height - rectangle.getHeight())));
            foreach (GameObject obj in avoidList)
            {
                float dist = GetDistance(rectangle,obj);
                if(dist < 2)
                {
                    assignRandomPositionTo(rectangle, avoidList);
                    return;
                }
            }
        }
        private void splitAround(WrapperRectangle rectangle)
        {
            QuadTreeNode topLeftRectangle = new QuadTreeNode(this, posX, posY, rectangle.getPosX() - posX + rectangle.getWidth()/2, rectangle.getPosY() - posY - rectangle.getHeight()/2);
            QuadTreeNode topRightRectangle = new QuadTreeNode(this, topLeftRectangle.getWidth() + posX, posY, width - topLeftRectangle.getWidth(), topLeftRectangle.getHeight() + rectangle.getHeight());
            QuadTreeNode bottomRightRectangle = new QuadTreeNode(this, rectangle.getPosX()-rectangle.getWidth()/2, rectangle.getPosY() + rectangle.getHeight()/2, topRightRectangle.getWidth() + rectangle.getWidth(), height - topRightRectangle.getHeight());
            QuadTreeNode bottomLeftRectangle = new QuadTreeNode(this, posX, topLeftRectangle.getHeight() + posY, width - bottomRightRectangle.getWidth(), height - topLeftRectangle.getHeight());

            children.Add(topLeftRectangle);
            children.Add(topRightRectangle);
            children.Add(bottomRightRectangle);
            children.Add(bottomLeftRectangle);
        }

        public List<QuadTreeNode> getChildren()
        {
            return children;
        }
    }
}