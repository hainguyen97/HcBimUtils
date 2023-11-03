namespace HcBimUtils.RebarShapeModel
{
   public class SegmentLength
   {
      public int Hook { get; set; } = -1;
      public bool IsVariable { get; set; } = false;
      public double Length { get; set; }
      public double Min { get; set; }
      public double Max { get; set; }

      public SegmentLength()
      {
      }
   }
}