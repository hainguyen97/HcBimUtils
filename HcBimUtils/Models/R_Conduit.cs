using System.Diagnostics ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Electrical ;
namespace HcBimUtils.Models
{
   public class R_Conduit : R_MEPCurve
   {
      #region Fields

      private Conduit m_Conduit;

      #endregion Fields

      #region Properties

      private Conduit Conduit => m_Conduit;

      public ConduitType ConduitType => Doc.GetElement(m_Conduit.GetTypeId()) as ConduitType ;

      public double Diameter
      {
         get => m_Conduit.Diameter;
         set
         {
            Parameter paraDiameter = m_Conduit.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            paraDiameter?.Set(value);
         }
      }

      #endregion Properties

      #region Constructor

      public R_Conduit(Conduit conduit) : base(conduit)
      {
         m_Conduit = conduit;
      }

      #endregion Constructor

      #region Functions

      public R_Conduit CreateConduit(XYZ start, XYZ end)
      {
         Conduit newConduit = null;
         R_Conduit newR_Conduit = null;
         try
         {
            newConduit = Conduit.Create(Doc, ConduitType.Id, start, end, ReferenceLevel.Id);
            newR_Conduit = new R_Conduit(newConduit);
         }
         catch (Exception ex)
         {
            Debug.Print(ex.Message);
         }

         if (newConduit != null)
         {
            newR_Conduit.Diameter = Diameter;
         }
         return newR_Conduit;
      }

      public override R_MEPCurve BreakCurve(XYZ point)
      {
         R_Conduit newR_Conduit = null;
         if ( Doc == null || m_Conduit == null || point == null ) return newR_Conduit ;
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

            var startConnector = StartConnector.AllRefs.Cast<Connector>().FirstOrDefault( item => item.Owner is FamilyInstance ) ;

            var hinhchieu = Curve.Project(point).XYZPoint;
            if (hinhchieu.DistanceTo(StartPoint) > 1 && hinhchieu.DistanceTo(EndPoint) > 1)
            {
               var oldCurve = Curve.Clone();
               var newCurve = CurveUtils.BreakCurve(oldCurve, hinhchieu);
               newR_Conduit = Duplicate() as R_Conduit;
               newR_Conduit.Curve = newCurve;
               if (startConnector != null)
               {
                  StartConnector.DisconnectFrom(startConnector);
                  startConnector.ConnectTo(newR_Conduit.StartConnector);
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
         return newR_Conduit;
      }

      #endregion Functions
   }
}