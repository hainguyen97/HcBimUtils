using System.Diagnostics ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Mechanical ;
using Autodesk.Revit.UI ;

namespace HcBimUtils.Models
{
   public class R_Duct : R_MEPCurve
   {
      #region Field

      private Duct m_Duct;

      #endregion Field

      #region Property

      public Duct Duct => m_Duct;

      public double Width
      {
         get => m_Duct.Width ;
         set
         {
            var paraWidth = m_Duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            paraWidth?.Set(value);
         }
      }

      public double Height
      {
         get => m_Duct.Height ;
         set
         {
            var paraHeight = m_Duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
            paraHeight?.Set(value);
         }
      }

      public double Diameter
      {
         get => m_Duct.Diameter;
         set
         {
            var paraDiameter = m_Duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
            paraDiameter?.Set(value);
         }
      }

      public double InsulationThickness => ( from id in Insulations select Doc.GetElement( id ) as DuctInsulation into ductInsulation select ductInsulation.Thickness ).Sum() ;

      #endregion Property

      #region Constructor

      public R_Duct(Duct duct) : base(duct)
      {
         if (duct is { IsValidObject: true })
         {
            m_Duct = duct;
         }
         else
         {
            throw new Exception("Duct is null");
         }
      }

      #endregion Constructor

      #region Function

      public override R_MEPCurve BreakCurve(XYZ point)
      {
         if ( Doc == null || m_Duct == null || point == null ) return null ;
         var tr = new Transaction(Doc, "Break Curve");
         try
         {
            tr.Start();
         }
         catch {
            // ignored
         }

         try
         {

            var newSplit = MechanicalUtils.BreakCurve(Doc, Id, point);
            if (tr.HasStarted())
            {
               tr.Commit();
            }
            return new R_Duct(Doc.GetElement(newSplit) as Duct);
         }
         catch (Exception ex)
         {
            if (tr.HasStarted())
            {
               tr.RollBack();
            }
            Debug.Print(ex.Message);
         }
         return null;
      }

      public R_Duct CreateDuct(MechanicalSystemType sType, DuctType type, Level level, XYZ start, XYZ end)
      {
         Duct newDuct = null;
         R_Duct newR_Duct = null;
         try
         {
            newDuct = Duct.Create(Doc, sType.Id, type.Id, level.Id, start, end);
            newR_Duct = new R_Duct(newDuct);
         }
         catch (Exception ex)
         {
            TaskDialog.Show("Warning", ex.Message);
         }

         if (newDuct != null)
         {
            newR_Duct.Height = Height;
            newR_Duct.Width = Width;
            newR_Duct.Diameter = Diameter;
         }
         return newR_Duct;
      }

      public override ElementId GetInsulationTypeId()
      {
         if (Insulations is { Count: > 0 })
         {
            DuctInsulation di = Doc.GetElement(Insulations.FirstOrDefault()) as DuctInsulation;
            return di.GetTypeId();
         }
         return null;
      }

      public override void CopyInsulation(Element ele)
      {
         if ( Insulations is not { Count: > 0 } || ! ele.IsValidElement() ) return ;
         var di = Doc.GetElement(Insulations.FirstOrDefault()) as DuctInsulation;
         DuctInsulation.Create(Doc, ele.Id, di.GetTypeId(), InsulationThickness);
      }

      public override InsulationLiningBase CreateInsulation(ElementId insulationTypeId, double insulationThickness)
      {
         return DuctInsulation.Create(Doc, Id, insulationTypeId, insulationThickness);
      }

      #endregion Function
   }
}