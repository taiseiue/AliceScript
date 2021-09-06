using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces
{
    static class Alice_Drawing_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Drawing");

            space.Add(new ColorObject(0,0,0));
            space.Add(new ColorsObject());

            NameSpaceManerger.Add(space);
        }
    }
    class NColorFunc : FunctionBase
    {
        public NColorFunc()
        {
            this.FunctionName = "newcolor";
            MinimumArgCounts = 3;
            Run += NColorFunc_Run;
        }

        private void NColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            ColorObject c = new ColorObject(e.Args[0].AsInt(), e.Args[1].AsInt(), e.Args[2].AsInt());

            e.Return = new Variable(c);
        }
    }
    class ColorObject : ObjectBase
    {
        public void init()
        {
            ClassName = "Color";
            this.Properties.Add("A", new Variable(255));
            this.Properties.Add("R", new Variable(255));
            this.Properties.Add("G", new Variable(255));
            this.Properties.Add("B", new Variable(255));


            this.Functions.Add("newcolor", new NColorFunc());
        }
        public ColorObject(int r, int g, int b, int a = 255)
        {
            Color = new Color();

            Color.FromArgb(a, r, g, b);

            init();

        }
        public ColorObject(Color c)
        {
            Color = c;
            init();
        }
        public Color Color;

        public override Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            if (sPropertyName.ToLower() == "a")
            {

                return Task.FromResult(new Variable(Color.A));

            }
            else
            {
                if (sPropertyName.ToLower() == "r")
                {
                    return Task.FromResult(new Variable(Color.R));

                }
                else
                {
                    if (sPropertyName.ToLower() == "g")
                    {
                        return Task.FromResult(new Variable(Color.G));

                    }
                    else
                    {
                        if (sPropertyName.ToLower() == "b")
                        {
                            return Task.FromResult(new Variable(Color.B));

                        }
                        else
                        {
                            return Task.FromResult(new Variable("Color"));
                        }
                    }
                }
            }
        }
    }
    public class ColorsObject : ObjectBase
    {

        public ColorsObject()
        {
            ClassName = "Colors";

            this.Properties.Add("AliceBlue", new Variable(new ColorObject(Color.AliceBlue)));
            this.Properties.Add("AntiqueWhite", new Variable(new ColorObject(Color.AntiqueWhite)));
            this.Properties.Add("Aqua", new Variable(new ColorObject(Color.Aqua)));
            this.Properties.Add("Aquamarine", new Variable(new ColorObject(Color.Aquamarine)));
            this.Properties.Add("Azure", new Variable(new ColorObject(Color.Azure)));
            this.Properties.Add("Beige", new Variable(new ColorObject(Color.Beige)));
            this.Properties.Add("Bisque", new Variable(new ColorObject(Color.Bisque)));
            this.Properties.Add("Black", new Variable(new ColorObject(Color.Black)));
            this.Properties.Add("BlanchedAlmond", new Variable(new ColorObject(Color.BlanchedAlmond)));
            this.Properties.Add("Blue", new Variable(new ColorObject(Color.Blue)));
            this.Properties.Add("BlueViolet", new Variable(new ColorObject(Color.BlueViolet)));
            this.Properties.Add("Brown", new Variable(new ColorObject(Color.Brown)));
            this.Properties.Add("BurlyWood", new Variable(new ColorObject(Color.BurlyWood)));
            this.Properties.Add("CadetBlue", new Variable(new ColorObject(Color.CadetBlue)));
            this.Properties.Add("Chartreuse", new Variable(new ColorObject(Color.Chartreuse)));
            this.Properties.Add("Chocolate", new Variable(new ColorObject(Color.Chocolate)));
            this.Properties.Add("Coral", new Variable(new ColorObject(Color.Coral)));
            this.Properties.Add("CornflowerBlue", new Variable(new ColorObject(Color.CornflowerBlue)));
            this.Properties.Add("Cornsilk", new Variable(new ColorObject(Color.Cornsilk)));
            this.Properties.Add("Crimson", new Variable(new ColorObject(Color.Crimson)));
            this.Properties.Add("Cyan", new Variable(new ColorObject(Color.Cyan)));
            this.Properties.Add("DarkBlue", new Variable(new ColorObject(Color.DarkBlue)));
            this.Properties.Add("DarkCyan", new Variable(new ColorObject(Color.DarkCyan)));
            this.Properties.Add("DarkGoldenrod", new Variable(new ColorObject(Color.DarkGoldenrod)));
            this.Properties.Add("DarkGray", new Variable(new ColorObject(Color.DarkGray)));
            this.Properties.Add("DarkGreen", new Variable(new ColorObject(Color.DarkGreen)));
            this.Properties.Add("DarkKhaki", new Variable(new ColorObject(Color.DarkKhaki)));
            this.Properties.Add("DarkMagenta", new Variable(new ColorObject(Color.DarkMagenta)));
            this.Properties.Add("DarkOliveGreen", new Variable(new ColorObject(Color.DarkOliveGreen)));
            this.Properties.Add("DarkOrange", new Variable(new ColorObject(Color.DarkOrange)));
            this.Properties.Add("DarkOrchid", new Variable(new ColorObject(Color.DarkOrchid)));
            this.Properties.Add("DarkRed", new Variable(new ColorObject(Color.DarkRed)));
            this.Properties.Add("DarkSalmon", new Variable(new ColorObject(Color.DarkSalmon)));
            this.Properties.Add("DarkSeaGreen", new Variable(new ColorObject(Color.DarkSeaGreen)));
            this.Properties.Add("DarkSlateBlue", new Variable(new ColorObject(Color.DarkSlateBlue)));
            this.Properties.Add("DarkSlateGray", new Variable(new ColorObject(Color.DarkSlateGray)));
            this.Properties.Add("DarkTurquoise", new Variable(new ColorObject(Color.DarkTurquoise)));
            this.Properties.Add("DarkViolet", new Variable(new ColorObject(Color.DarkViolet)));
            this.Properties.Add("DeepPink", new Variable(new ColorObject(Color.DeepPink)));
            this.Properties.Add("DeepSkyBlue", new Variable(new ColorObject(Color.DeepSkyBlue)));
            this.Properties.Add("DimGray", new Variable(new ColorObject(Color.DimGray)));
            this.Properties.Add("DodgerBlue", new Variable(new ColorObject(Color.DodgerBlue)));
            this.Properties.Add("Firebrick", new Variable(new ColorObject(Color.Firebrick)));
            this.Properties.Add("FloralWhite", new Variable(new ColorObject(Color.DarkSeaGreen)));
            this.Properties.Add("ForestGreen", new Variable(new ColorObject(Color.ForestGreen)));
            this.Properties.Add("Fuchsia", new Variable(new ColorObject(Color.Fuchsia)));
            this.Properties.Add("Gainsboro", new Variable(new ColorObject(Color.Gainsboro)));
            this.Properties.Add("GhostWhite", new Variable(new ColorObject(Color.GhostWhite)));
            this.Properties.Add("Gold", new Variable(new ColorObject(Color.Gold)));
            this.Properties.Add("Goldenrod", new Variable(new ColorObject(Color.DarkSeaGreen)));
            this.Properties.Add("Gray", new Variable(new ColorObject(Color.Gray)));
            this.Properties.Add("Green", new Variable(new ColorObject(Color.Green)));
            this.Properties.Add("GreenYellow", new Variable(new ColorObject(Color.GreenYellow)));
            this.Properties.Add("HotPink", new Variable(new ColorObject(Color.HotPink)));
            this.Properties.Add("IndianRed", new Variable(new ColorObject(Color.IndianRed)));
            this.Properties.Add("Indigo", new Variable(new ColorObject(Color.Indigo)));
            this.Properties.Add("Ivory", new Variable(new ColorObject(Color.Ivory)));
            this.Properties.Add("Khaki", new Variable(new ColorObject(Color.Khaki)));
            this.Properties.Add("Lavender", new Variable(new ColorObject(Color.Lavender)));
            this.Properties.Add("LavenderBlush", new Variable(new ColorObject(Color.LavenderBlush)));
            this.Properties.Add("LawnGreen", new Variable(new ColorObject(Color.HotPink)));
            this.Properties.Add("LemonChiffon", new Variable(new ColorObject(Color.LemonChiffon)));
            this.Properties.Add("LightBlue", new Variable(new ColorObject(Color.LightBlue)));
            this.Properties.Add("LightCoral", new Variable(new ColorObject(Color.LightCoral)));
            this.Properties.Add("LightCyan", new Variable(new ColorObject(Color.LightCyan)));
            this.Properties.Add("LightGoldenrodYellow", new Variable(new ColorObject(Color.LightGoldenrodYellow)));
            this.Properties.Add("LightGray", new Variable(new ColorObject(Color.LightGray)));
            this.Properties.Add("LightGreen", new Variable(new ColorObject(Color.LightGreen)));
            this.Properties.Add("LightPink", new Variable(new ColorObject(Color.LightPink)));
            this.Properties.Add("LightSalmon", new Variable(new ColorObject(Color.LightSalmon)));
            this.Properties.Add("LightSeaGreen", new Variable(new ColorObject(Color.LightSeaGreen)));
            this.Properties.Add("LightSkyBlue", new Variable(new ColorObject(Color.LightSkyBlue)));
            this.Properties.Add("LightSlateGray", new Variable(new ColorObject(Color.LightSlateGray)));
            this.Properties.Add("LightSteelBlue", new Variable(new ColorObject(Color.LightSteelBlue)));
            this.Properties.Add("LightYellow", new Variable(new ColorObject(Color.LightYellow)));
            this.Properties.Add("Lime", new Variable(new ColorObject(Color.Lime)));
            this.Properties.Add("LimeGreen", new Variable(new ColorObject(Color.LimeGreen)));
            this.Properties.Add("Linen", new Variable(new ColorObject(Color.Linen)));
            this.Properties.Add("Magenta", new Variable(new ColorObject(Color.Magenta)));
            this.Properties.Add("Maroon", new Variable(new ColorObject(Color.Maroon)));
            this.Properties.Add("MediumAquamarine", new Variable(new ColorObject(Color.MediumAquamarine)));
            this.Properties.Add("MediumBlue", new Variable(new ColorObject(Color.MediumBlue)));
            this.Properties.Add("MediumOrchid", new Variable(new ColorObject(Color.MediumOrchid)));
            this.Properties.Add("MediumSeaGreen", new Variable(new ColorObject(Color.MediumSeaGreen)));
            this.Properties.Add("MediumSlateBlue", new Variable(new ColorObject(Color.MediumSlateBlue)));
            this.Properties.Add("MediumSpringGreen", new Variable(new ColorObject(Color.MediumSpringGreen)));
            this.Properties.Add("MediumTurquoise", new Variable(new ColorObject(Color.MediumTurquoise)));
            this.Properties.Add("MediumVioletRed", new Variable(new ColorObject(Color.MediumVioletRed)));
            this.Properties.Add("MidnightBlue", new Variable(new ColorObject(Color.MidnightBlue)));
            this.Properties.Add("MintCream", new Variable(new ColorObject(Color.MintCream)));
            this.Properties.Add("MistyRose", new Variable(new ColorObject(Color.MistyRose)));
            this.Properties.Add("Moccasin", new Variable(new ColorObject(Color.Moccasin)));
            this.Properties.Add("NavajoWhite", new Variable(new ColorObject(Color.NavajoWhite)));
            this.Properties.Add("Navy", new Variable(new ColorObject(Color.Navy)));
            this.Properties.Add("OldLace", new Variable(new ColorObject(Color.OldLace)));
            this.Properties.Add("Olive", new Variable(new ColorObject(Color.Olive)));
            this.Properties.Add("OliveDrab", new Variable(new ColorObject(Color.OliveDrab)));
            this.Properties.Add("Orange", new Variable(new ColorObject(Color.Orange)));
            this.Properties.Add("Orchid", new Variable(new ColorObject(Color.Orchid)));
            this.Properties.Add("OrangeRed", new Variable(new ColorObject(Color.OrangeRed)));
            this.Properties.Add("PaleGoldenrod", new Variable(new ColorObject(Color.PaleGoldenrod)));
            this.Properties.Add("PaleGreen", new Variable(new ColorObject(Color.PaleGreen)));
            this.Properties.Add("PaleTurquoise", new Variable(new ColorObject(Color.PaleTurquoise)));
            this.Properties.Add("PaleVioletRed", new Variable(new ColorObject(Color.PaleVioletRed)));
            this.Properties.Add("PapayaWhip", new Variable(new ColorObject(Color.PapayaWhip)));
            this.Properties.Add("PeachPuff", new Variable(new ColorObject(Color.PeachPuff)));
            this.Properties.Add("Peru", new Variable(new ColorObject(Color.Peru)));
            this.Properties.Add("Pink", new Variable(new ColorObject(Color.Pink)));
            this.Properties.Add("Plum", new Variable(new ColorObject(Color.Plum)));
            this.Properties.Add("PowderBlue", new Variable(new ColorObject(Color.PowderBlue)));
            this.Properties.Add("Purple", new Variable(new ColorObject(Color.Purple)));
            this.Properties.Add("Red", new Variable(new ColorObject(Color.Red)));
            this.Properties.Add("RosyBrown", new Variable(new ColorObject(Color.RosyBrown)));
            this.Properties.Add("RoyalBlue", new Variable(new ColorObject(Color.RoyalBlue)));
            this.Properties.Add("SaddleBrown", new Variable(new ColorObject(Color.SaddleBrown)));
            this.Properties.Add("Salmon", new Variable(new ColorObject(Color.Salmon)));
            this.Properties.Add("SandyBrown", new Variable(new ColorObject(Color.SandyBrown)));
            this.Properties.Add("SeaGreen", new Variable(new ColorObject(Color.SeaGreen)));
            this.Properties.Add("SeaShell", new Variable(new ColorObject(Color.SeaShell)));
            this.Properties.Add("Sienna", new Variable(new ColorObject(Color.Sienna)));
            this.Properties.Add("Silver", new Variable(new ColorObject(Color.Silver)));
            this.Properties.Add("SkyBlue", new Variable(new ColorObject(Color.SkyBlue)));
            this.Properties.Add("SlateBlue", new Variable(new ColorObject(Color.SlateBlue)));
            this.Properties.Add("SlateGray", new Variable(new ColorObject(Color.SlateGray)));
            this.Properties.Add("Snow", new Variable(new ColorObject(Color.Snow)));
            this.Properties.Add("SpringGreen", new Variable(new ColorObject(Color.SpringGreen)));
            this.Properties.Add("SteelBlue", new Variable(new ColorObject(Color.SteelBlue)));
            this.Properties.Add("Tan", new Variable(new ColorObject(Color.Tan)));
            this.Properties.Add("Teal", new Variable(new ColorObject(Color.Teal)));
            this.Properties.Add("Thistle", new Variable(new ColorObject(Color.Thistle)));
            this.Properties.Add("Tomato", new Variable(new ColorObject(Color.Tomato)));
            this.Properties.Add("Transparent", new Variable(new ColorObject(Color.Transparent)));
            this.Properties.Add("Turquoise", new Variable(new ColorObject(Color.Turquoise)));
            this.Properties.Add("Violet", new Variable(new ColorObject(Color.Violet)));
            this.Properties.Add("Wheat", new Variable(new ColorObject(Color.Wheat)));
            this.Properties.Add("White", new Variable(new ColorObject(Color.White)));
            this.Properties.Add("WhiteSmoke", new Variable(new ColorObject(Color.WhiteSmoke)));
            this.Properties.Add("Yellow", new Variable(new ColorObject(Color.Yellow)));
            this.Properties.Add("YellowGreen", new Variable(new ColorObject(Color.YellowGreen)));

            this.Properties.Add("Random", new Variable(""));


        }
        Random r = new Random();
        public override Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            if (sPropertyName.ToLower() != "random")
            {
                return base.GetProperty(sPropertyName, args, script);

            }
            else
            {
                return Task.FromResult(new Variable(new ColorObject(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255))));
            }
        }


    }
}
