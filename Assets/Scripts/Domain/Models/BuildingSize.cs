namespace Domain.Models
{
    public struct BuildingSize
    {
        public int Width;
        public int Height;

        public BuildingSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
