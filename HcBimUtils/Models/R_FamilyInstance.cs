using Autodesk.Revit.DB ;
using HcBimUtils.GeometryUtils ;

namespace HcBimUtils.Models
{
   public class R_FamilyInstance
   {
      private FamilyInstance _familyInstance;
      public FamilyInstance FamilyInstance => _familyInstance;
      public double ReferenceDistance { get; set; }
      public FamilySymbol FamilySymbol => _familyInstance.Symbol;
      public XYZ LocationPoint => (_familyInstance.Location as LocationPoint).Point;
      public bool IsParallelWithVirtualDirection { get; set; }
      public double RotateAngle { get; set; }

      public R_FamilyInstance(FamilyInstance familyInstance)
      {
         _familyInstance = familyInstance;
      }

      public List<Element> GetElementsConnected(ConnectorType connectorType)
      {
         return MepUtils.GetElementsConnected(_familyInstance, connectorType);
      }

      public XYZ GetMiddlePoint()
      {
         ConnectorManager cm = Util.GetConnectorManager(_familyInstance);
         XYZ middle = null;
         foreach (Connector connector in cm.Connectors)
         {
            if (middle == null)
            {
               middle = connector.Origin;
            }
            else
            {
               middle = 0.5 * (middle + connector.Origin);
            }
         }
         return middle;
      }

      public XYZ GetMiddlePoint(XYZ direction)
      {
         ConnectorManager cm = Util.GetConnectorManager(_familyInstance);
         XYZ middle = null;
         foreach (Connector connector in cm.Connectors)
         {
            if (connector.CoordinateSystem.BasisZ.IsParallel(direction))
            {
               if (middle == null)
               {
                  middle = connector.Origin;
               }
               else
               {
                  middle = 0.5 * (middle + connector.Origin);
               }
            }
         }
         return middle;
      }
   }
}