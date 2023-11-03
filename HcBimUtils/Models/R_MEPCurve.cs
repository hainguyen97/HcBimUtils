using System.Diagnostics ;
using Autodesk.Revit.DB ;
using HcBimUtils.GeometryUtils ;

namespace HcBimUtils.Models
{
   public class R_MEPCurve
   {
      #region Fields

      private MEPCurve _mepCurve;
      private Document _doc => _mepCurve.Document;

      #endregion Fields

      #region Properties

      public List<Element> RelatedElement { get; set; } = new();
      public Document Doc => _doc;
      public MEPCurve MEPCurve => _mepCurve;
      public ElementId Id => _mepCurve.Id;
      public Connector StartConnector => Util.GetConnectorClosestTo(_mepCurve, StartPoint, false);
      public Connector EndConnector => Util.GetConnectorClosestTo(_mepCurve, EndPoint, false);

      public ICollection<ElementId> Insulations
      {
         get
         {
            try
            {
               return InsulationLiningBase.GetInsulationIds(Doc, Id);
            }
            catch
            {
               return null;
            }
         }
      }

      public XYZ Direction
      {
         get
         {
            var ln = Curve as Line;
            return ln?.Direction;
         }
      }

      public XYZ StartPoint => Curve.GetEndPoint(0) ;

      public XYZ EndPoint => Curve.GetEndPoint(1) ;

      public Location Location => _mepCurve.Location ;

      public Curve Curve
      {
         get => (Location as LocationCurve).Curve;
         set => (Location as LocationCurve).Curve = value;
      }

      public Level ReferenceLevel
      {
         get => _mepCurve.ReferenceLevel;
         set
         {
            _mepCurve.ReferenceLevel = value;
         }
      }

      public double Offset
      {
         get
         {
            return _mepCurve.LevelOffset;
         }
         set
         {
            _mepCurve.LevelOffset = value;
         }
      }

      #endregion Properties

      #region Constructors

      public R_MEPCurve(MEPCurve mepCurve)
      {
         if (mepCurve != null && mepCurve.IsValidObject)
         {
            _mepCurve = mepCurve;
         }
         else
         {
            throw new Exception("MEPCurve is null");
         }
      }

      #endregion Constructors

      #region Functions

      public virtual void CopyInsulation(Element ele)
      {
      }

      public virtual ElementId GetInsulationTypeId() => null;

      public virtual R_MEPCurve BreakCurve(XYZ point) => null;

      public virtual R_MEPCurve Duplicate()
      {
         ICollection<ElementId> copyIds = ElementTransformUtils.CopyElement(_doc, Id, XYZ.Zero);

         foreach (var id in copyIds)
         {
            if (_doc.GetElement(copyIds.FirstOrDefault()) is MEPCurve mepCurve)
            {
               return mepCurve.WrapMepCurve();
            }
         }
         return null;
      }

      public virtual InsulationLiningBase CreateInsulation(ElementId insulationTypeId, double insulationThickness)
      {
         return null;
      }

      public XYZ GetClosestPoint(XYZ point, ref XYZ direction, ref bool isForward)
      {
         double distance1 = StartPoint.DistanceTo(point);
         double distance2 = EndPoint.DistanceTo(point);
         if (distance1 < distance2)
         {
            direction = (EndPoint - StartPoint).Normalize();
            isForward = true;
            return StartPoint;
         }
         else
         {
            direction = (StartPoint - EndPoint).Normalize();
            isForward = false;
            return EndPoint;
         }
      }

      /// <summary>
      /// Xét cả connector ở giữa và ở 2 đầu
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      public Connector GetClosestConnector(XYZ point, bool unusedOnly = false)
      {
         return Util.GetConnectorClosestTo(_mepCurve, point, unusedOnly);
      }

      /// <summary>
      /// Chỉ xét connector ở giữa
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      public Connector GetClosestMiddleConnector(XYZ point)
      {
         ConnectorManager cm = Util.GetConnectorManager(_mepCurve);
         Connector middleConnector = null;
         double minDist = double.MaxValue;
         foreach (Connector c in cm.Connectors)
         {
            if (!c.Origin.IsAlmostEqualTo(StartPoint) && !c.Origin.IsAlmostEqualTo(EndPoint))
            {
               double d = c.Origin.DistanceTo(point);
               if (d < minDist)
               {
                  middleConnector = c;
                  minDist = d;
               }
            }
         }
         return middleConnector;
      }

      public List<Connector> GetConnectors(bool unusedOnly = false)
      {
         List<Connector> connectors = new List<Connector>();
         ConnectorManager cm = Util.GetConnectorManager(_mepCurve);
         if (unusedOnly)
         {
            foreach (Connector cn in cm.UnusedConnectors)
            {
               connectors.Add(cn);
            }
         }
         else
         {
            foreach (Connector cn in cm.Connectors)
            {
               connectors.Add(cn);
            }
         }
         return connectors;
      }

      public void ConnectStartConnector(Connector connector)
      {
         _doc.Create.NewTransitionFitting(StartConnector, connector);
      }

      public void ConnectEndConnector(Connector connector)
      {
         _doc.Create.NewTransitionFitting(EndConnector, connector);
      }

      public void DisconnectAll()
      {
         foreach (Connector cn1 in _mepCurve.ConnectorManager.Connectors)
         {
            foreach (Connector cn2 in cn1.AllRefs)
            {
               cn1.DisconnectFrom(cn2);
            }
         }
      }

      /// <summary>
      /// Lấy element tại 2 đầu
      /// </summary>
      /// <param name="connectorType"></param>
      /// <returns></returns>
      public List<Element> GetRelatedElements(ConnectorType connectorType)
      {
         List<Element> eles = new List<Element>();
         foreach (Connector cn1 in _mepCurve.ConnectorManager.Connectors)
         {
            if (cn1.ConnectorType == connectorType)
            {
               foreach (Connector cn2 in cn1.AllRefs)
               {
                  if (!eles.Any(x => x.Id == cn2.Owner.Id) && cn2.Owner.Id != Id)
                  {
                     eles.Add(cn2.Owner);
                  }
               }
            }
         }
         return eles;
      }

      public List<R_MEPCurve> Split(XYZ startPoint,
         XYZ breakPoint,
         XYZ direction,
         bool isForward,
         bool isUnion,
         double splitLength,
         double minLength,
         double minDistance,
         int step)
      {
         Transaction tr = new Transaction(_doc);
         List<R_MEPCurve> splits = new List<R_MEPCurve>();
         splits.Add(this);
         if (Curve.IsContain(breakPoint))
         {
            Connector closestMiddleConnector = GetClosestMiddleConnector(breakPoint);
            //nếu có connection giữa duct
            if (closestMiddleConnector != null)
            {
               //khoảng cách từ flange đến breakpoint
               double distance = closestMiddleConnector.Origin.DistanceTo(breakPoint);
               bool isMoveBackward = false;
               bool isMoveForward = false;
               //nếu mà khoảng cách từ điểm breakPoint đến tay nhánh cứ nhỏ hơn minDistance thì phải né ra xa
               while (distance.IsSmaller(minDistance, 0.01))
               {
                  if (!isMoveForward)
                  {
                     isMoveBackward = true;
                     //lùi điểm breakpoint lại
                     breakPoint = closestMiddleConnector.Origin.Add(direction.Negate().Multiply(minDistance));
                     closestMiddleConnector = GetClosestMiddleConnector(breakPoint);
                     distance = closestMiddleConnector.Origin.DistanceTo(breakPoint);
                  }
                  else {
                     //tiến điểm breakpoint lên
                     breakPoint = closestMiddleConnector.Origin.Add(direction.Multiply(minDistance));
                     closestMiddleConnector = GetClosestMiddleConnector(breakPoint);
                     distance = closestMiddleConnector.Origin.DistanceTo(breakPoint);
                  }

                  //kiểm tra khoảng cách minLength
                  double length = startPoint.DistanceTo(breakPoint);
                  if (length.IsSmaller(minLength, 0.01))
                  {
                     isMoveBackward = false;
                     isMoveForward = true;
                     //tiến điểm breakpoint lên
                     breakPoint = closestMiddleConnector.Origin.Add(direction.Multiply(minDistance));
                     closestMiddleConnector = GetClosestMiddleConnector(breakPoint);
                     distance = closestMiddleConnector.Origin.DistanceTo(breakPoint);
                  }
               }
               if (Curve.IsContain(breakPoint))
               {
                  //nếu có lùi lại thì mới làm tròn
                  if (isMoveBackward)
                  {
                     //làm tròn xuống
                     double length = startPoint.DistanceTo(breakPoint);
                     double lengthMm = UnitConverter.MillimetersFromInternalUnits(length);
                     lengthMm = lengthMm.RoundDown(step);
                     length = UnitConverter.MillimetersToInternalUnits(lengthMm);
                     breakPoint = startPoint.Add(direction.Multiply(length));
                  }
                  else if (isMoveForward)
                  {
                     //làm tròn lên
                     double length = startPoint.DistanceTo(breakPoint);
                     double lengthMm = UnitConverter.MillimetersFromInternalUnits(length);
                     lengthMm = lengthMm.RoundUp(step);
                     length = UnitConverter.MillimetersToInternalUnits(lengthMm);
                     breakPoint = startPoint.Add(direction.Multiply(length));
                  }
                  if (isForward)
                  {
                     R_MEPCurve newMEPCurve = BreakCurve(breakPoint);
                     splits.Add(newMEPCurve);
                     if (isUnion)
                     {
                        Connector c1 = newMEPCurve.GetClosestConnector(breakPoint);
                        Connector c2 = GetClosestConnector(breakPoint);
                        tr.Start("Create Fitting");
                        _doc.Create.NewUnionFitting(c1, c2);
                        tr.Commit();
                     }
                     startPoint = breakPoint;
                     breakPoint = breakPoint.Add(direction.Multiply(splitLength));
                     splits.AddRange(Split(startPoint, breakPoint, direction, isForward, isUnion, splitLength, minLength, minDistance, step));
                  }
                  else
                  {
                     R_MEPCurve newMEPCurve = BreakCurve(breakPoint);
                     if (isUnion)
                     {
                        Connector c1 = newMEPCurve.GetClosestConnector(breakPoint);
                        Connector c2 = GetClosestConnector(breakPoint);
                        tr.Start("Create Fitting");
                        _doc.Create.NewUnionFitting(c1, c2);
                        tr.Commit();
                     }
                     startPoint = breakPoint;
                     breakPoint = breakPoint.Add(direction.Multiply(splitLength));
                     splits.AddRange(newMEPCurve.Split(startPoint, breakPoint, direction, isForward, isUnion, splitLength, minLength, minDistance, step));
                  }
               }
            }
            //nếu không có connection giữa duct
            else
            {
               if (isForward)
               {
                  R_MEPCurve newMEPCurve = BreakCurve(breakPoint);
                  splits.Add(newMEPCurve);
                  if (isUnion)
                  {
                     Connector c1 = newMEPCurve.GetClosestConnector(breakPoint);
                     Connector c2 = GetClosestConnector(breakPoint);
                     try
                     {
                        tr.Start("Create Fitting");
                        _doc.Create.NewUnionFitting(c1, c2);
                        tr.Commit();
                     }
                     catch
                     {
                        tr.RollBack();
                     }
                  }
                  breakPoint = breakPoint.Add(direction.Multiply(splitLength));
                  splits.AddRange(Split(startPoint, breakPoint, direction, isForward, isUnion, splitLength, minLength, minDistance, step));
               }
               else
               {
                  R_MEPCurve newMEPCurve = BreakCurve(breakPoint);
                  if (isUnion)
                  {
                     Connector c1 = newMEPCurve.GetClosestConnector(breakPoint);
                     Connector c2 = GetClosestConnector(breakPoint);
                     try
                     {
                        tr.Start("Create Fitting");
                        _doc.Create.NewUnionFitting(c1, c2);
                        tr.Commit();
                     }
                     catch
                     {
                        tr.RollBack();
                     }
                  }
                  breakPoint = breakPoint.Add(direction.Multiply(splitLength));
                  splits.AddRange(newMEPCurve.Split(startPoint, breakPoint, direction, isForward, isUnion, splitLength, minLength, minDistance, step));
               }
            }
         }
         return splits;
      }

      public List<R_MEPCurve> BreakCurve(List<XYZ> points)
      {
         List<R_MEPCurve> splits = new List<R_MEPCurve>();
         if (_doc != null && _mepCurve != null && points != null && points.Count > 0)
         {
            R_MEPCurve r_MEPCurve = new R_MEPCurve(_mepCurve);
            splits.Add(r_MEPCurve);
            points = points
                  .OrderBy(x => x.DistanceTo(r_MEPCurve.StartPoint))
                  .ToList();
            foreach (XYZ point in points)
            {
               if (point.DistanceTo(r_MEPCurve.StartPoint) > 0.01
                     && point.DistanceTo(r_MEPCurve.EndPoint) > 0.01)
               {
                  var newSplit = BreakCurve(point);
                  if (newSplit != null && newSplit.Id.IsValid())
                  {
                     splits.Add(newSplit);
                  }
               }
            }
         }
         return splits;
      }

      //public FamilyInstance NewElbowFitting(R_MEPCurve r_MEPCurve1, XYZ intersect)
      //{
      //   FamilyInstance newFitting = null;
      //   if (_mepCurve != null && r_MEPCurve1 != null && intersect != null)
      //   {
      //      Connector c1 = GetClosestConnector(intersect);
      //      List<Connector> c2s = r_MEPCurve1.GetConnectors(true);
      //      if (c1 != null && c2s != null && c2s.Count > 0)
      //      {
      //         foreach (var c2 in c2s)
      //         {
      //            try
      //            {
      //               newFitting = Doc.Create.NewElbowFitting(c1, c2);
      //               return newFitting;
      //            }
      //            catch (Exception ex)
      //            {
      //               Debug.Print(ex.Message);
      //               continue;
      //            }
      //         }
      //      }
      //   }
      //   return newFitting;
      //}
      public FamilyInstance NewElbowFitting(R_MEPCurve r_MEPCurve1)
      {
         FamilyInstance newFitting = null;
         if (_mepCurve != null && r_MEPCurve1 != null)
         {
            XYZ intersect = CurveUtils.GetIntersections(Curve, r_MEPCurve1.Curve, true).FirstOrDefault();
            Connector c1 = GetClosestConnector(intersect, true);
            List<Connector> c2s = r_MEPCurve1.GetConnectors(true).OrderBy(x => x.Origin.DistanceTo(intersect)).ToList();
            if (c1 != null && c2s.Count > 0)
            {
               foreach (var c2 in c2s)
               {
                  if (!c1.IsConnected && !c2.IsConnected)
                  {
                     try
                     {
                        newFitting = Doc.Create.NewElbowFitting(c1, c2);
                        return newFitting;
                     }
                     catch (Exception ex)
                     {
                        Debug.Print(ex.Message);
                     }
                  }
               }
            }
         }
         return newFitting;
      }

      public bool IsValid()
      {
         return MEPCurve.IsValidElement();
      }

      #endregion Functions
   }
}