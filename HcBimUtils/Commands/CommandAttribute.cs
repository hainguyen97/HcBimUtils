namespace HcBimUtils.Commands
{
    public class CommandAttribute : Attribute
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string LargeImage { get; set; }
    }
}
