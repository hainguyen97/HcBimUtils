using System.Diagnostics ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Electrical ;

using HcBimUtils.GeometryUtils ;

namespace HcBimUtils.Models
{
   public class R_CableTray : R_MEPCurve
   {
      #region Fields

      private CableTray m_CableTray;

      #endregion Fields

      #region Properties

      public CableTray CableTray => m_CableTray;

      public CableTrayType CableTrayType => Doc.GetElement(m_CableTray.GetTypeId()) as CableTrayType ;

      public double Height
      {
         get => m_CableTray.Height;
         set
         {
            var paraHeight = m_CableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM);
            paraHeight?.Set(value);
         }
      }

      public double Width
      {
         get => m_CableTray.Width;
         set
         {
            var paraWidth = m_CableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM);
            paraWidth?.Set(value);
         }
      }

      #endregion Properties

      #region Constructors

      public R_CableTray(CableTray cableTray) : base(cableTray)
      {
         m_CableTray = cableTray;
      }

      #endregion Constructors

      #region Functions

      public R_CableTray CreateCableTray(XYZ start, XYZ end)
      {
         CableTray newCableTray = null;
         R_CableTray newR_CableTray = null;
         try
         {
            newCableTray = CableTray.Create(Doc, CableTrayType.Id, start, end, ReferenceLevel.Id);
            newCableTray.get_Parameter(BuiltInParameter.RBS_CTC_SERVICE_TYPE).Set(m_CableTray.get_Parameter(BuiltInParameter.RBS_CTC_SERVICE_TYPE).AsString());
            newR_CableTray = new R_CableTray(newCableTray);
         }
         catch (Exception ex)
         {
            Debug.Print(ex.Message);
         }

         if (newCableTray != null)
         {
            newR_CableTray.Width = Width;
            newR_CableTray.Height = Height;
         }
         return newR_CableTray;
      }

      public override R_MEPCurve BreakCurve(XYZ point)
      {
         R_CableTray newR_CableTray = null;
         if ( Doc == null || m_CableTray == null || point == null ) return newR_CableTray ;
         var tr = new Transaction(Doc, "Break Curve");
         try
         {
            try
            {
               tr.Start();
            }
            catch {
               // ignored
            }

            Connector startConnector = StartConnector.AllRefs.Cast<Connector>().FirstOrDefault( item => item.Owner is FamilyInstance ) ;

            var hinhchieu = Curve.Project(point).XYZPoint;
            if (hinhchieu.DistanceTo(StartPoint) > 1 && hinhchieu.DistanceTo(EndPoint) > 1)
            {
               var oldCurve = Curve.Clone();
               var newCurve = CurveUtils.BreakCurve(oldCurve, hinhchieu);
               //newR_CableTray = CreateCableTray(StartPoint, hinhchieu);
               newR_CableTray = Duplicate() as R_CableTray;
               newR_CableTray.Curve = newCurve;
               if (startConnector != null)
               {
                  StartConnector.DisconnectFrom(startConnector);
                  startConnector.ConnectTo(newR_CableTray.StartConnector);
               }
               Curve = oldCurve;
            }

            if (tr.HasStarted())
            {
               tr.Commit();
            }
         }
         catch (Exception ex)
         {
            if (tr.HasStarted())
            {
               tr.RollBack();
            }
            Debug.Print(ex.Message);
         }
         return newR_CableTray;
      }

      public override R_MEPCurve Duplicate()
      {
         var copyIds = ElementTransformUtils.CopyElement(Doc, Id, XYZ.Zero);
         foreach (var id in copyIds)
         {
            if (Doc.GetElement(copyIds.FirstOrDefault()) is CableTray mepCurve)
            {
               mepCurve.get_Parameter(BuiltInParameter.RBS_CTC_SERVICE_TYPE).Set(m_CableTray.get_Parameter(BuiltInParameter.RBS_CTC_SERVICE_TYPE).AsString());
               return mepCurve.WrapMepCurve();
            }
         }
         return null;
      }

      #endregion Functions
   }
}