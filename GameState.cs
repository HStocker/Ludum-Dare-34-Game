using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace TreeGame
{
    public class Playing
    {
        Texture2D stick;
        Texture2D texture;
        Vector2 ZERO = new Vector2(0, 0);
        SpriteFont output;

        int compIndex = 0;
        int tabCounter = 0;
        int createCounter = 0;

        public static Rectangle smallBranch = new Rectangle(151, 79, 107, 33);
        public static Rectangle bigBranch = new Rectangle(151, 189, 156, 33);
        public static Rectangle thorn = new Rectangle(151, 0, 90, 43);
        public static Rectangle plant = new Rectangle(267, 0, 151, 169);
        public static Rectangle trunk = new Rectangle(0, 0, 151, 344);
        public static Rectangle leaf = new Rectangle(151, 116, 37, 24);
        public static Rectangle twig = new Rectangle(151, 140, 28, 18);

        public static Rectangle arm1 = new Rectangle(1, 366, 96, 53);
        public static Rectangle hand1 = new Rectangle(97, 366, 123, 53);
        Rectangle hand1Hitbox = new Rectangle(0, 0, 123, 53);

        public static Rectangle arm2 = new Rectangle(72, 445, 50, 27);
        public static Rectangle hand2 = new Rectangle(122, 445, 50, 27);
        Rectangle hand2Hitbox = new Rectangle(0, 0, 50, 27);

        Rectangle HAT = new Rectangle(310, 172, 664, 590);

        public List<Arm> arms = new List<Arm>();
        int armTimer = 0;
        int newArm = 15000;
        Component current;

        List<Vector2> armSpawns = new List<Vector2>();

        List<Component> components = new List<Component>();
        public static Dictionary<string, Rectangle> parts = new Dictionary<string, Rectangle>();
        List<string> partString = new List<string>();
        public static Dictionary<string, Rectangle> hitboxes = new Dictionary<string, Rectangle>();

        bool Menu = true;
        bool End = false;
        bool DEV = false;
        bool paused = false;
        int devTimer = 0;

        bool Choosing = false;
        int choiceIndex = 0;
        SetChild setChild;
        GetLocation getLocation;
        Rectangle tempDraw;

        List<Component> leaves = new List<Component>();
        List<Component> thorns = new List<Component>();
        int growth = 2;

        Rectangle twigHitbox = new Rectangle(0, 4, 26, 10);
        Rectangle trunkHitbox = new Rectangle(64, 4, 20, 340);
        Rectangle bigBranchHitbox = new Rectangle(0, 0, 150, 14);
        Rectangle leafHitbox = new Rectangle(0, 0, 31, 18);
        Rectangle smallBranchHitbox = new Rectangle(0, 0, 102, 12);
        Rectangle thornHitbox = new Rectangle(20, 0, 70, 13);

        List<Component> droppingLeaves = new List<Component>();

        public List<Tuple<Vector2, Vector2, Color>> lines = new List<Tuple<Vector2, Vector2, Color>>();
        public static Random rand = new Random();

        Song song;  // Put the name of your song in instead of "song_title"
        public void entering(Game1 game)
        {

            song = game.Content.Load<Song>("ScottJoplin-TheEntertainer1902");
            MediaPlayer.Play(song);
            MediaPlayer.Volume -= .5f;
            MediaPlayer.IsRepeating = true;
            stick = game.Content.Load<Texture2D>("Stick");
            output = game.Content.Load<SpriteFont>("Output18pt");

            texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData<Color>(new Color[] { Color.White });

            partString.Add("bigBranch");
            parts.Add("bigBranch", bigBranch);
            partString.Add("smallBranch");
            parts.Add("smallBranch", smallBranch);
            partString.Add("thorn");
            parts.Add("thorn", thorn);
            partString.Add("leaf");
            parts.Add("leaf", leaf);
            partString.Add("twig");
            parts.Add("twig", twig);

            hitboxes.Add("trunk", trunkHitbox);
            hitboxes.Add("bigBranch", bigBranchHitbox);
            hitboxes.Add("smallBranch", smallBranchHitbox);
            hitboxes.Add("leaf", leafHitbox);
            hitboxes.Add("thorn", thornHitbox);
            hitboxes.Add("plant", new Rectangle());
            hitboxes.Add("twig", twigHitbox);
            hitboxes.Add("hand1", hand1Hitbox);
            hitboxes.Add("hand2", hand2Hitbox);

            armSpawns.Add(new Vector2(-20, -20));
            armSpawns.Add(new Vector2(100, -20));
            armSpawns.Add(new Vector2(300, -20));
            armSpawns.Add(new Vector2(500, -20));
            armSpawns.Add(new Vector2(700, -20));
            armSpawns.Add(new Vector2(900, -20));
            armSpawns.Add(new Vector2(1100, -20));
            armSpawns.Add(new Vector2(1350, -20));

            populate();

            Random rand = new Random();
            foreach (Component component in components)
            {
                Color color = new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255), 255);
                foreach (Tuple<Vector2, Vector2> line in Component.getLines(hitboxes[component.tag], new Vector2(component.location.X, component.location.Y), component.rotation))
                {
                    if (component.tag.Equals("trunk")) { Debug.Print("{0},{1},{2},{3},{4}", hitboxes[component.tag].X, hitboxes[component.tag].Y, hitboxes[component.tag].Width, hitboxes[component.tag].Height, component.location); }
                    lines.Add(new Tuple<Vector2, Vector2, Color>(line.Item1, line.Item2, color));
                }
            }
        }
        public void populate()
        {
            components.Add(new Component(null, "trunk", trunk, 0f, new Vector2(567, 432), false));
            components.Add(new Component(components[0], "bigBranch", bigBranch, -.22f, new Vector2(642, 576), false));
            components.Add(new Component(components[0], "smallBranch", smallBranch, -2.84f, new Vector2(633, 651), false));
            components[0].rightChild = components[1];
            components[0].leftChild = components[2];
            components.Add(components[2].setLeft("leaf"));
            components.Add(components[1].setRight("leaf"));
            leaves.Add(components[3]);
            leaves.Add(components[4]);
            components.Add(new Component(null, "plant", plant, 0f, new Vector2(28, 654), false));


            current = components[0];

        }

        public void reset()
        {
            components.Clear();
            leaves.Clear();
            thorns.Clear();
            arms.Clear();
            populate();
            newArm = 15000;
            growth = 0;
        }

        public void update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Escape)) { Menu = true; paused = true; createCounter = 0; }

            if (Menu)
            {
                if (state.IsKeyDown(Keys.Z)) { Menu = false; paused = false; }
                if (paused && state.IsKeyDown(Keys.C)) { this.reset(); Menu = false; paused = false; createCounter = 0; }
            }
            else if (!End)
            {
                if (leaves.Count < 1) { End = true; }
                for (int i = 0; i < arms.Count; i++)
                {
                    if (arms[i].tobedeleted) { arms.RemoveAt(i); i--; }
                }
                for (int i = 0; i < thorns.Count; i++)
                {
                    if (thorns[i].tobedeleted) { thorns.RemoveAt(i); i--; }
                }
                if (devTimer > 1000) { DEV = !DEV; devTimer = 0; }

                if (DEV)
                {
                    current = components[compIndex % components.Count];

                    if (createCounter >= 320 && state.IsKeyDown(Keys.D1))
                    {
                        components.Add(new Component(null, string.Format("smallBranch-{0}", components.FindAll(a => a.tag.Split('-')[0].Equals("smallBranch")).Count), smallBranch, 0f, ZERO, false));
                        createCounter = 0;
                        compIndex = components.Count - 1;
                    }
                    if (createCounter >= 320 && state.IsKeyDown(Keys.D2))
                    {
                        components.Add(new Component(null, string.Format("thorn-{0}", components.FindAll(a => a.tag.Split('-')[0].Equals("thorn")).Count), thorn, 0f, ZERO, false));
                        createCounter = 0;
                        compIndex = components.Count - 1;
                    }
                    if (createCounter >= 320 && state.IsKeyDown(Keys.D3))
                    {
                        components.Add(new Component(null, string.Format("plant-{0}", components.FindAll(a => a.tag.Split('-')[0].Equals("plant")).Count), plant, 0f, ZERO, false));
                        createCounter = 0;
                        compIndex = components.Count - 1;
                    }
                    if (createCounter >= 320 && state.IsKeyDown(Keys.D4))
                    {
                        components.Add(new Component(null, string.Format("bigBranch-{0}", components.FindAll(a => a.tag.Split('-')[0].Equals("bigBranch")).Count), bigBranch, 0f, ZERO, false));
                        createCounter = 0;
                        compIndex = components.Count - 1;
                    }

                    if (tabCounter >= 120 && state.IsKeyDown(Keys.Tab)) { compIndex++; tabCounter = 0; }
                    if (tabCounter < 120) { tabCounter += gameTime.ElapsedGameTime.Milliseconds; }
                    if (createCounter < 320) { createCounter += gameTime.ElapsedGameTime.Milliseconds; }

                    if (state.IsKeyDown(Keys.W)) { current.location.Y -= 1; }
                    if (state.IsKeyDown(Keys.A)) { current.location.X -= 1; }
                    if (state.IsKeyDown(Keys.D)) { current.location.X += 1; }
                    if (state.IsKeyDown(Keys.S)) { current.location.Y += 1; }
                    if (state.IsKeyDown(Keys.Q)) { current.rotation -= .02f; }
                    if (state.IsKeyDown(Keys.E)) { current.rotation += .02f; }
                }
                else if (!Choosing)
                {
                    if (tabCounter < 120) { tabCounter += gameTime.ElapsedGameTime.Milliseconds; }
                    if (createCounter < 320) { createCounter += gameTime.ElapsedGameTime.Milliseconds; }

                    if (createCounter >= 320 && state.IsKeyDown(Keys.Z))
                    {
                        if (current.hasLeft()) { current = current.leftChild; }
                        else if (!current.terminal)
                        {
                            Choosing = true;
                            setChild = current.setLeft;
                            getLocation = current.getLeftLocation;
                            tempDraw = parts[partString[choiceIndex]];
                            /*
                            components.Add(current.setLeft("bigBranch"));
                            current = components[0];
                            createCounter = 0;
                            */
                        }
                        else
                        {
                            current = components[0];
                        }
                        createCounter = 0;
                    }
                    if (createCounter >= 320 && state.IsKeyDown(Keys.C))
                    {
                        if (current.hasRight()) { current = current.rightChild; }
                        else if (!current.terminal)
                        {
                            Choosing = true;
                            setChild = current.setRight;
                            getLocation = current.getRightLocation;
                            tempDraw = parts[partString[choiceIndex]];
                            /*
                            components.Add(current.setRight("bigBranch"));
                            current = components[0];
                            createCounter = 0;
                            */
                        }
                        else
                        {
                            current = components[0];
                        }
                        createCounter = 0;
                    }
                }
                if (Choosing)
                {
                    if (createCounter < 320) { createCounter += gameTime.ElapsedGameTime.Milliseconds; }

                    if (state.IsKeyDown(Keys.Z) && createCounter >= 320)
                    {
                        Component collidedWith;
                        if (!collides(partString[choiceIndex], getLocation().Item1, getLocation().Item2, out collidedWith, "NONE"))
                        {
                            components.Add(setChild(partString[choiceIndex]));
                            if (partString[choiceIndex].Equals("leaf")) { leaves.Add(components[components.Count - 1]); growth++; }
                            else if (partString[choiceIndex].Equals("thorn")) { thorns.Add(components[components.Count - 1]); }

                            Color color = new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255), 255);
                            foreach (Tuple<Vector2, Vector2> line in Component.getLines(hitboxes[partString[choiceIndex]], getLocation().Item1, getLocation().Item2))
                            {
                                lines.Add(new Tuple<Vector2, Vector2, Color>(line.Item1, line.Item2, color));
                            }

                        }
                        Choosing = false;
                        current = components[0];
                        choiceIndex = 0;
                        createCounter = 0;
                    }
                    else if (state.IsKeyDown(Keys.C) && createCounter >= 320)
                    {
                        choiceIndex++;
                        choiceIndex = choiceIndex % (parts.Count - 1);
                        tempDraw = parts[partString[choiceIndex]];
                        createCounter = 0;
                    }
                }

                foreach (Arm arm in arms)
                {
                    arm.update();
                    Component collidedWith;
                    if (!arm.retreating && collides(arm.tag, arm.location, arm.rotation, out collidedWith, "thorn"))
                    {
                        try
                        {
                            dropThorn(collidedWith);
                            arm.retreat();
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    /*
                    foreach (Component thorn in thorns)
                    {
                        if (thorn.intersects(arm.hand, arm.location, arm.rotation))
                        {
                            dropThorn(thorn);
                            arm.retreat();
                            break;
                        }
                    }*/
                }

                armTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (armTimer > newArm - leaves.Count * 400 && leaves.Count > 0)
                {
                    newArm -= 400;
                    Random rand = new Random();
                    int ind = rand.Next(0, 2);
                    switch (ind)
                    {
                        case 0:
                            {
                                arms.Add(new Arm("hand1", hand1, arm1, leaves[rand.Next(0, leaves.Count)], armSpawns[rand.Next(0, armSpawns.Count)], this));
                                break;
                            }
                        case 1:
                            {
                                arms.Add(new Arm("hand2", hand2, arm2, leaves[rand.Next(0, leaves.Count)], armSpawns[rand.Next(0, armSpawns.Count)], this));
                                break;
                            }
                    }
                    armTimer = 0;
                }

                for (int i = 0; i < droppingLeaves.Count; i++)
                {
                    droppingLeaves[i].location.Y += 4;
                    if (droppingLeaves[i].location.Y > 780) { droppingLeaves.RemoveAt(i); i--; }
                }
            }
            else
            {
                if (state.IsKeyDown(Keys.Z))
                {
                    this.reset();
                    this.End = false;
                }
            }
        }
        public void draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Begin();
            if (Menu)
            {
                spriteBatch.Draw(stick, new Vector2(350, 20), HAT, Color.White, 0f, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                if (!paused)
                {
                    spriteBatch.DrawString(output, "PRESS Z TO START", new Vector2(520, 613), Color.Black);
                    spriteBatch.DrawString(output, "Z: Descend left/ Confirm select", new Vector2(420, 640), Color.Black);
                    spriteBatch.DrawString(output, "C: Descend right/ Cycle select", new Vector2(420, 667), Color.Black);
                    spriteBatch.DrawString(output, "Don't let the hands get all your leaves!  Use thorns to protect them!!", new Vector2(120, 714), Color.Black);
                }
                else
                {
                    spriteBatch.DrawString(output, "PRESS Z TO RESUME", new Vector2(520, 613), Color.Black);
                    spriteBatch.DrawString(output, "PRESS C TO RESTART", new Vector2(515, 640), Color.Black);

                }
            }
            else if (!End)
            {
                foreach (Component component in components)
                {
                    Color color = Color.White;
                    if (current.Equals(component)) { color = Color.DarkKhaki; }
                    spriteBatch.Draw(stick, component.location, component.sprite, color, component.rotation, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);

                    if (DEV) spriteBatch.Draw(texture, new Vector2(component.location.X + hitboxes[component.tag].X, component.location.Y + hitboxes[component.tag].Y),
                         hitboxes[component.tag], Color.Black, component.rotation, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);

                }
                if (Choosing)
                {
                    Color color = new Color(0, 0, 0, 120);
                    Component collidedWith;
                    if (collides(partString[choiceIndex], getLocation().Item1, getLocation().Item2, out collidedWith, "NONE")) { color = new Color(100, 50, 50, 120); }
                    spriteBatch.Draw(stick, getLocation().Item1, tempDraw, color, getLocation().Item2, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                    if (DEV)
                    {
                        spriteBatch.Draw(texture, new Vector2(getLocation().Item1.X + hitboxes[partString[choiceIndex]].X, getLocation().Item1.Y + hitboxes[partString[choiceIndex]].Y),
                            hitboxes[partString[choiceIndex]], Color.Black, getLocation().Item2, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                    }
                }

                foreach (Arm arm in arms)
                {
                    spriteBatch.Draw(stick, arm.location, arm.hand, Color.White, arm.rotation, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                    for (int i = 0; i < arm.numArms; i++)
                    {
                        spriteBatch.Draw(stick, arm.getArmVector(i + 1), arm.arm, Color.White, arm.rotation, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                    }
                }
                /*
                    foreach (Tuple<Vector2, Vector2, Color> line in lines)
                    {
                        spriteBatch.Draw(texture, line.Item1, new Rectangle(0, 0, 7, 7), line.Item3, 0f, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
                        spriteBatch.Draw(texture, line.Item2, new Rectangle(0, 0, 7, 7), line.Item3, 0f, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);

                    }
                
                
                spriteBatch.DrawString(output, string.Format("{0}: {1},{2},{3}", current.tag, current.location.X, current.location.Y, current.rotation), ZERO, Color.Black);
                spriteBatch.DrawString(output, string.Format("{0}", devTimer), new Vector2(0, 24), Color.Black);
                if (DEV) spriteBatch.DrawString(output, "DEV", new Vector2(0, 48), Color.Black);
                else if (Choosing) spriteBatch.DrawString(output, "CHOOSING", new Vector2(0, 48), Color.Black);
                else spriteBatch.DrawString(output, "PLAYING", new Vector2(0, 48), Color.Black);
                spriteBatch.DrawString(output, string.Format("{0},{1}", partString[choiceIndex], choiceIndex), new Vector2(0, 72), Color.Black);
                */
            }
            else
            {
                spriteBatch.DrawString(output, string.Format("Game Over!!\n{0} Leaves grown!!\nPress Z to restart", growth), new Vector2(200, 200), Color.Black);
            }

            spriteBatch.End();
        }


        public bool collides(string type, Vector2 location, float rotation, out Component collidedWith, string filter)
        {
            //Debug.Print("==========================================");
            foreach (Component component in components)
            {
                if (filter == "NONE" || component.tag.Equals(filter))
                {
                    //make sure it is not colliding with parent or other child
                    Component otherChild = null;
                    if (current.hasRight()) { otherChild = current.rightChild; }
                    else if (current.hasLeft()) { otherChild = current.leftChild; }
                    //Debug.Print("{0},{1},{2}", !current.Equals(component), !component.Equals(otherChild), !component.intersects(hitboxes[type], location, rotation));
                    if (!current.Equals(component) && !component.Equals(otherChild))
                    {
                        //Debug.Print("{0},{1}", component.tag, type);
                        if (component.intersects(hitboxes[type], location, rotation))
                        {
                            collidedWith = component;
                            return true;
                        }
                    }
                }
            }
            collidedWith = null;
            return false;
        }
        public delegate Component SetChild(string type);
        public delegate Tuple<Vector2, float> GetLocation();
        public void dropLeaf(Component leaf)
        {
            if (leaf.parent.hasLeft() && leaf.parent.leftChild.Equals(leaf)) { leaf.parent.leftChild = null; components.Add(leaf.parent.setLeft("twig")); }
            else { leaf.parent.rightChild = null; components.Add(leaf.parent.setRight("twig")); }
            leaves.Remove(leaf);
            leaf.parent = null;
            droppingLeaves.Add(leaf);
        }
        public void dropThorn(Component thorn)
        {
            if (thorn.parent.hasLeft() && thorn.parent.leftChild.Equals(thorn)) { thorn.parent.leftChild = null; }
            else { thorn.parent.rightChild = null; }
            thorns.Remove(thorn);
            thorn.parent = null;
            droppingLeaves.Add(thorn);
        }
    }
}
