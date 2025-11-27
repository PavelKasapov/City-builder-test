namespace Domain.Models
{
    public struct BuildingSize
    {
        public int Width;
        public int Height;

        public BuildingSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
