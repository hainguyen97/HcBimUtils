using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Plumbing ;

namespace HcBimUtils.Models
{
   public class R_FlexPipe : R_MEPCurve
   {
      public FlexPipe FlexPipe { get; set; }
      public R_FlexPipe(FlexPipe flexPipe) : base(flexPipe)
      {
         FlexPipe = flexPipe;
      }
      public override R_MEPCurve BreakCurve(XYZ point)
      {
         return base.BreakCurve(point);
      }

      public override void CopyInsulation(Element ele)
      {
         base.CopyInsulation(ele);
      }

      public override InsulationLiningBase CreateInsulation(ElementId insulationTypeId, double insulationThickness)
      {
         return base.CreateInsulation(insulationTypeId, insulationThickness);
      }

      public override R_MEPCurve Duplicate()
      {
         return base.Duplicate();
      }


      public override ElementId GetInsulationTypeId()
      {
         return base.GetInsulationTypeId();
      }

   }
}
