using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TreeGame
{

    public class Component
    {
        public Rectangle sprite;
        public float rotation;
        public Vector2 location;
        public string tag;
        public Component leftChild;
        public Component rightChild;
        public Component parent;
        public bool terminal = false;
        public bool tobedeleted = false;

        public Component(Component parent, string tag, Rectangle sprite, float rotation, Vector2 location, bool terminal)
        {
            this.sprite = sprite;
            this.rotation = rotation;
            this.location = location;
            this.tag = tag;
            this.terminal = terminal;
            this.parent = parent;
        }
        public void addRight(Component component) { rightChild = component; }
        public void addLeft(Component component) { leftChild = component; }
        public bool hasLeft() { return this.leftChild != null; }
        public bool hasRight() { return this.rightChild != null; }
        public Component setLeft(string type)
        {
            if (this.leftChild != null) { throw new ArgumentException("INVALID LEFT ADD"); }

            Tuple<Vector2, float> local = this.getLeftLocation();
            bool terminal = true;
            if (type.Equals("smallBranch") || type.Equals("bigBranch")) { terminal = false; }
            this.leftChild = new Component(this, type, Playing.parts[type], local.Item2, local.Item1, terminal);
            return leftChild;
        }
        public Component setRight(string type)
        {
            if (this.rightChild != null) { throw new ArgumentException("INVALID RIGHT ADD"); }

            Tuple<Vector2, float> local = this.getRightLocation();
            bool terminal = true;
            if (type.Equals("smallBranch") || type.Equals("bigBranch")) { terminal = false; }
            this.rightChild = new Component(this, type, Playing.parts[type], local.Item2, local.Item1, terminal);
            return rightChild;
        }
        public Tuple<Vector2, float> getLeftLocation()
        {
            switch (this.tag.Split('-')[0])
            {
                case "bigBranch":
                    {
                        return new Tuple<Vector2, float>(RotateAboutOrigin(new Vector2(this.location.X + 67, this.location.Y + 8), this.location, this.rotation), (float)(this.rotation - 1.28));
                    }
                case "smallBranch":
                    {
                        return new Tuple<Vector2, float>(RotateAboutOrigin(new Vector2(this.location.X + 34, this.location.Y + 9), this.location, this.rotation), (float)(this.rotation - 1.34));
                    }
                default:
                    {
                        throw new ArgumentException("INVALID LEFT REQUEST");
                    }
            }
        }
        public Tuple<Vector2, float> getRightLocation()
        {
            switch (this.tag)
            {
                case "bigBranch":
                    {
                        return new Tuple<Vector2, float>(RotateAboutOrigin(new Vector2(this.location.X + 93, this.location.Y + 8), this.location, this.rotation), (float)(this.rotation + 1.28));
                    }
                case "smallBranch":
                    {
                        return new Tuple<Vector2, float>(RotateAboutOrigin(new Vector2(this.location.X + 64, this.location.Y + 10), this.location, this.rotation), (float)(this.rotation + 1.26));
                    }
                default:
                    {
                        throw new ArgumentException("INVALID RIGHT REQUEST");
                    }
            }

        }

        public bool intersects(Rectangle rect, Vector2 local, float rot)
        {
            foreach (Tuple<Vector2, Vector2> linesA in getLines(Playing.hitboxes[this.tag], this.location, this.rotation))
            {
                //Debug.Print("{0},{1}", linesA.Item1, linesA.Item2);
                foreach (Tuple<Vector2, Vector2> linesB in getLines(rect, local, rot))
                {
                    //Debug.Print("{0},{1}", linesB.Item1, linesB.Item2);
                    if (linesIntersect(linesA.Item1, linesA.Item2, linesB.Item1, linesB.Item2)) { return true; }
                }
            }
            //Debug.Print("Doesn't intersect");
            return false;

        }
        public static List<Tuple<Vector2, Vector2>> getLines(Rectangle rect, Vector2 local, float rot)
        {
            List<Tuple<Vector2, Vector2>> lines = new List<Tuple<Vector2, Vector2>>();
            Vector2 top1 = new Vector2(local.X + rect.X, local.Y + rect.Y);
            Vector2 top2 = Component.RotateAboutOrigin(new Vector2(local.X + rect.Width + rect.X, local.Y + rect.Y), local, rot);
            lines.Add(new Tuple<Vector2, Vector2>(top1, top2));
            Vector2 right1 = top2;
            Vector2 right2 = Component.RotateAboutOrigin(new Vector2(local.X + rect.Width + rect.X, local.Y + rect.Height + rect.Y), local, rot);
            lines.Add(new Tuple<Vector2, Vector2>(right1, right2));
            Vector2 bottom1 = right2;
            Vector2 bottom2 = Component.RotateAboutOrigin(new Vector2(local.X + rect.X, local.Y + rect.Height + rect.Y), local, rot);
            lines.Add(new Tuple<Vector2, Vector2>(bottom1, bottom2));
            Vector2 left1 = bottom2;
            Vector2 left2 = top1;
            lines.Add(new Tuple<Vector2, Vector2>(left1, left2));

            return lines;
        }

        public static bool linesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            // Detect coincident lines (has a problem, read below)
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        public static Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }
    }
    public class Arm
    {
        public Rectangle hand;
        public int numArms = 1;
        public Rectangle arm;
        public Vector2 destination;
        public float rotation;
        public Vector2 location;
        public Vector2 origin;
        public Component leaf;
        public float speed;
        public bool retreating = false;
        public bool tobedeleted = false;
        public string tag;
        private string p;
        private Rectangle hand2;
        private Rectangle arm2;
        private Component component;
        private Vector2 vector2;
        private Playing playing;

        public Arm(string tag, Rectangle hand, Rectangle arm, Component leaf, Vector2 start, Playing playing)
        {
            this.tag = tag;
            this.hand = hand;
            this.arm = arm;
            this.destination = leaf.location;
            this.leaf = leaf;
            this.location = start;
            this.origin = location;
            this.rotation = (float)Math.Atan2(destination.Y - location.Y, destination.X - location.X);
            this.speed = rotation;
            this.playing = playing;
        }
        public void update()
        {
            this.location = RotateAboutOrigin(new Vector2(this.location.X + speed, this.location.Y), this.location, this.rotation);
            if (Vector2.Distance(location, origin) > arm.Width * numArms) { numArms++; }
            if (Vector2.Distance(RotateAboutOrigin(new Vector2(location.X + hand.Width, location.Y), location, rotation), destination) < 5)
            {
                if (leaf.parent != null)
                {
                    playing.dropLeaf(leaf);
                }
                this.speed = speed * -20;
            }
            /*
            Color color = new Color(Game1.rand.Next(0, 255), Game1.rand.Next(0, 255), Game1.rand.Next(0, 255), 255);
            foreach (Tuple<Vector2, Vector2> line in Component.getLines(Game1.hitboxes[this.tag],this.location,this.rotation))
            {
                ingame.lines.Add(new Tuple<Vector2, Vector2, Color>(line.Item1, line.Item2, color));
            }
            */
            if (tobedeleted && this.location.Y < 0) { tobedeleted = true; }
        }
        public void retreat()
        {
            this.speed = speed * -20;
            this.retreating = true;
        }

        public Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }
        public Vector2 getArmVector(int armNum)
        {
            return RotateAboutOrigin(new Vector2(location.X - (arm.Width * armNum), location.Y), location, rotation);
        }
    }
}
