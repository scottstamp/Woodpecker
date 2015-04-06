using System;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Woodpecker.Game.Rooms.Pathfinding
{
    public class blisterMolePathfinder
    {
        private roomTileState[,] gridState = null;
        private bool[,] gridUnit = null;
        private float[,] Heightmap = null;
        private byte maxX;
        private byte maxY;
        private PriorityQueueB<blisterMoleNode> listOpen = new PriorityQueueB<blisterMoleNode>(new ComparePFNode());
        private List<blisterMoleNode> listClosed = new List<blisterMoleNode>();
        private const int heuristicEstimate = 2;
        private const int maxCycleLimit = 25000;
        private bool pathFound = false;

        public blisterMolePathfinder(roomTileState[,] gridState, bool[,] gridUnit, float[,] Heightmap)
        {
            this.gridState = gridState;
            this.gridUnit = gridUnit;
            this.Heightmap = Heightmap;
            maxX = (byte)gridUnit.GetUpperBound(0);
            maxY = (byte)gridUnit.GetUpperBound(1);
        }
        public List<blisterMoleNode> findShortestPath(byte X, byte Y, byte goalX, byte goalY)
        {
            sbyte[,] Direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            //sbyte[,] Direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { -1, -1 }, { 1, -1 }, { 1, 1 }, { -1, 1 } };
            int openListCount = 0;
            int closedListCount = 0;
            lock (this)
            {
                blisterMoleNode Parent;
                Parent.X = X;
                Parent.Y = Y;
                Parent.parentX = X;
                Parent.parentY = Y;
                Parent.H = heuristicEstimate;
                Parent.F = heuristicEstimate;
                Parent.G = 0;

                this.listOpen.Push(Parent);
                openListCount++;

                while (openListCount > 0)
                {
                    openListCount--;
                    Parent = this.listOpen.Pop();
                    if (Parent.X == goalX && Parent.Y == goalY)
                    {
                        this.pathFound = true;
                        this.listClosed.Add(Parent);
                        closedListCount++;
                        break;
                    }


                    if (closedListCount > maxCycleLimit)
                        break;

                    for (int i = 0; i < 4; i++)
                    {
                        blisterMoleNode New;
                        New.X = (byte)(Parent.X + Direction[i, 0]);
                        New.Y = (byte)(Parent.Y + Direction[i, 1]);

                        if
                        (
                            New.X < 0 ||
                            New.Y < 0 ||
                            New.X > maxX ||
                            New.Y > maxY ||
                            gridState[New.X, New.Y] != roomTileState.Free ||
                            gridUnit[New.X, New.Y]
                        )
                            continue; // Tile is not valid (out of range, or blocked somehow)

                        int newG = Parent.G + (int)(this.gridState[New.X, New.Y]) + 1; // Cost
                        if (newG == Parent.G) // Same cost as the parent, why taking this route?
                            continue;

                        float hParent = this.Heightmap[Parent.X, Parent.Y];
                        float hNew = this.Heightmap[New.X, New.Y];
                        //if(hParent - 4 >= hNew || hParent + 1.5f <= hNew) // Can't go down more than 4 in height or can't go up more than 1.5 in height
                        if (hParent - 20 >= hNew || hParent + 20 <= hNew) // why the fuck not nils?
                            continue;
                        
                        int listIndex = -1;
                        for (int j = 0; j < openListCount; j++)
                        {
                            blisterMoleNode curNode = listOpen[j];
                            if (curNode.X == New.X && curNode.Y == New.Y)
                            {
                                listIndex = j;
                                break;
                            }
                        }
                        if (listIndex != -1 && listOpen[listIndex].G <= newG) // Better node already in open list
                            continue;

                        listIndex = -1;
                        for (int j = 0; j < closedListCount; j++)
                        {
                            blisterMoleNode curNode = listClosed[j];
                            if (curNode.X == New.X && curNode.Y == New.Y)
                            {
                                listIndex = j;
                                break;
                            }
                        }
                        if (listIndex != -1 && listClosed[listIndex].G <= newG) // Better node already in closed list
                            continue;

                        New.parentX = Parent.X;
                        New.parentY = Parent.Y;
                        New.G = newG;

                        // Calculate heuristic
                        int xD1 = Parent.X - goalX;
                        int xD2 = X - goalX;
                        int yD1 = Parent.Y - goalY;
                        int yD2 = Y - goalY;

                        New.H = heuristicEstimate * (Math.Max(Math.Abs(New.X - goalX), Math.Abs(New.Y - goalY)));
                        New.H = (int)(New.H + Math.Abs(xD1 * yD2 - xD2 * yD1) * 0.001);
                        New.F = New.G + New.H;
                        listOpen.Push(New);
                        openListCount++;
                    }
                    listClosed.Add(Parent);
                    closedListCount++;
                }

                if (this.pathFound)
                {
                    blisterMoleNode topNode = listClosed[closedListCount - 1];
                    for (int j = closedListCount - 1; j >= 0; j--) // Inverse scroll
                    {
                        if (topNode.parentX == listClosed[j].X && topNode.parentY == listClosed[j].Y || j == closedListCount - 1)
                            topNode = listClosed[j];
                        else
                            listClosed.RemoveAt(j);
                    }
                    return listClosed;
                }

                return null;
            }
        }
    }
    public struct blisterMoleNode
    {
        public byte X;
        public byte Y;
        public byte parentX;
        public byte parentY;
        public int H;
        public int F;
        public int G;
    }
    public class PriorityQueueB<T>
    {
        protected List<T> InnerList = new List<T>();
        protected IComparer<T> mComparer;
        public PriorityQueueB()
        {
            mComparer = Comparer<T>.Default;
        }

        public PriorityQueueB(IComparer<T> comparer)
        {
            mComparer = comparer;
        }
        public PriorityQueueB(IComparer<T> comparer, int capacity)
        {
            mComparer = comparer;
            InnerList.Capacity = capacity;
        }
        protected void SwitchElements(int i, int j)
        {
            T h = InnerList[i];
            InnerList[i] = InnerList[j];
            InnerList[j] = h;
        }

        protected virtual int OnCompare(int i, int j)
        {
            return mComparer.Compare(InnerList[i], InnerList[j]);
        }


        public int Push(T item)
        {
            int p = InnerList.Count, p2;
            InnerList.Add(item); // E[p] = O
            do
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            return p;
        }


        public T Pop()
        {
            T result = InnerList[0];
            int p = 0, p1, p2, pn;
            InnerList[0] = InnerList[InnerList.Count - 1];
            InnerList.RemoveAt(InnerList.Count - 1);
            do
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);

            return result;
        }


        public void Update(int i)
        {
            int p = i, pn;
            int p1, p2;
            do	// aufsteigen
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            if (p < i)
                return;
            do	   // absteigen
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);
        }


        public T Peek()
        {
            if (InnerList.Count > 0)
                return InnerList[0];
            return default(T);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public int Count
        {
            get { return InnerList.Count; }
        }

        public void RemoveLocation(T item)
        {
            int index = -1;
            for (int i = 0; i < InnerList.Count; i++)
            {

                if (mComparer.Compare(InnerList[i], item) == 0)
                    index = i;
            }

            if (index != -1)
                InnerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return InnerList[index]; }
            set
            {
                InnerList[index] = value;
                Update(index);
            }
        }
    }
    public class ComparePFNode : IComparer<blisterMoleNode>
    {
        public int Compare(blisterMoleNode x, blisterMoleNode y)
        {
            if (x.F > y.F)
                return 1;
            else if (x.F < y.F)
                return -1;
            return 0;
        }
    }
}

