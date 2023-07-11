using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class QuadTree
    {
        private int minWidth;
        private int minHeight;

        private QuadTreeNode root;

        private IMove mover;

        public QuadTree(float posX, float posY, float width, float height, IMove mover)
        {
            root = new QuadTreeNode(posX, posY, width, height);
            this.mover = mover;
        }

        public bool insert(WrapperRectangle toInsert, List<GameObject> avoidList)
        {
            return root.insert(toInsert, avoidList);
        }

        public void iterate()
        {
            List<QuadTreeNode> list = new List<QuadTreeNode>();
            list.Add(root);

            while(list.Count > 0)
            {
                QuadTreeNode current = list[0];

                if (current.getData() != null)
                {
                    mover.Move(current);
                }


                foreach(QuadTreeNode c in current.getChildren())
                {
                    list.Add(c);
                }

                list.Remove(current);
            }
        }
    }
}