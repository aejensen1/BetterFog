
namespace BetterFog.Assets
{
    public class MoonScale
    {
        public string MoonName { get; set; }
        public float Scale { get; set; }

        public MoonScale() { }

        public override string ToString()
        {
            return $"{MoonName}:{Scale}";
        }
        
        public MoonScale(string moonName, float scale)
        {
            MoonName = moonName;
            Scale = scale;
        }
    }
}
