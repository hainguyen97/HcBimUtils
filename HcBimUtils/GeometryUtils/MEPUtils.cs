using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using HcBimUtils.Models;

namespace HcBimUtils.GeometryUtils
{
    public static class MepUtils
    {
        public static FamilySymbol GetZFittingSymbol(Document doc, ElementId zFamilyId)
        {
            var zFittingFamily = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(Family)).Where(x => x.Id == zFamilyId).Cast<Family>().FirstOrDefault();
            if (zFittingFamily != null)
            {
                var symbolId = zFittingFamily.GetFamilySymbolIds().FirstOrDefault();
                if (symbolId != null && symbolId.IsValid())
                {
                    return doc.GetElement(symbolId) as FamilySymbol;
                }
            }
            else
            {
                throw new Exception("Family Z Fitting is not existed in current project.");
            }

            return null;
        }

        public static FamilyInstance CreateZFitting(Document doc, ElementId zFamilyId, XYZ point, double offset, double angle, R_Duct rMepCurve)
        {
            if (rMepCurve == null || point == null)
            {
                return null;
            }

            var fs = GetZFittingSymbol(doc, zFamilyId);
            FamilyInstance fi = null;

            if (fs == null) return null;
            try
            {
                XYZ direction = null;
                var isForward = false;
                rMepCurve.GetClosestPoint(point, ref direction, ref isForward);

                fi = doc.Create.NewFamilyInstance(point, fs, direction.Negate(), rMepCurve.MEPCurve, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                fi.GetTransform();

                //rotate fitting
                //hệ số nếu offset > 0 thì là 1, không thì là -1
                var factor = offset > 0 ? 1 : -1;
                ElementTransformUtils.RotateElement(doc, fi.Id, Line.CreateUnbound(point, direction.Negate()), -0.5 * Math.PI * -factor);

                //set parameter cho fitting
                fi.LookupParameter("Duct Width").Set(rMepCurve.Height);
                fi.LookupParameter("Duct Height").Set(rMepCurve.Width);
                fi.LookupParameter("Offset").Set(Math.Abs(offset));
                fi.LookupParameter("Angle").Set(angle);

                //connect element với fitting
                var connectorFi = Util.GetConnectorClosestTo(fi, point, false);
                var connectorDuct = rMepCurve.GetClosestConnector(point);
                connectorDuct.ConnectTo(connectorFi);
            }
            catch
            {
                // ignored
            }

            return fi;
        }

        public static R_MEPCurve WrapMepCurve(this Element ele)
        {
            return ele switch
            {
                Duct duct => new R_Duct(duct),
                Pipe pipe => new R_Pipe(pipe),
                Conduit conduit => new R_Conduit(conduit),
                CableTray tray => new R_CableTray(tray),
                FlexPipe pipe => new R_FlexPipe(pipe),
                _ => null
            };
        }

        public static List<Connector> GetClosestConnectors(Element ele1, Element ele2)
        {
            if (ele1 == null || ele2 == null) return null;
            var connectors = new List<Connector>();
            var cm1 = Util.GetConnectorManager(ele1);
            var cm2 = Util.GetConnectorManager(ele2);
            var distance = double.MaxValue;
            foreach (Connector cn1 in cm1.Connectors)
            {
                foreach (Connector cn2 in cm2.Connectors)
                {
                    if (!(cn1.Origin.DistanceTo(cn2.Origin) < distance)) continue;
                    distance = cn1.Origin.DistanceTo(cn2.Origin);
                    connectors.Clear();
                    connectors.Add(cn1);
                    connectors.Add(cn2);
                }
            }

            return connectors;
        }

        public static List<Element> GetElementsConnected(FamilyInstance fitting, ConnectorType connectorType)
        {
            if (fitting == null) return null;
            var cm = fitting.MEPModel.ConnectorManager;

            return (from Connector cn1 in cm.Connectors where cn1.ConnectorType == connectorType from Connector cn2 in cn1.AllRefs where cn2.ConnectorType == connectorType select cn2.Owner).ToList();
        }

        public static List<FamilyInstance> GetElementsConnected(MEPCurve mepCurve, ConnectorType connectorType)
        {
            if (mepCurve == null) return null;
            var cm = mepCurve.ConnectorManager;

            return (from Connector cn1 in cm.Connectors where cn1.ConnectorType == connectorType from Connector cn2 in cn1.AllRefs where cn2.ConnectorType == connectorType where cn2.Owner is FamilyInstance select cn2.Owner as FamilyInstance).ToList();
        }

        public static void GetElementsConnected(Element element, ConnectorType connectorType, ref List<Element> allRefs)
        {
            if (element == null) return;
            allRefs.Add(element);
            switch (element)
            {
                case MEPCurve curve:
                    {
                        var list = GetElementsConnected(curve, connectorType);
                        foreach (var item in list)
                        {
                            if (allRefs.All(x => x.Id != item.Id))
                            {
                                GetElementsConnected(item, connectorType, ref allRefs);
                            }
                        }

                        break;
                    }
                case FamilyInstance instance:
                    {
                        FamilyInstance fi = instance;
                        var list = GetElementsConnected(fi, connectorType);
                        foreach (var item in list)
                        {
                            if (allRefs.All(x => x.Id != item.Id))
                            {
                                GetElementsConnected(item, connectorType, ref allRefs);
                            }
                        }

                        break;
                    }
            }
        }

        public static void GetElementsConnectedBetween(Element element, ConnectorType connectorType, ref List<Element> allRefs, Element end)
        {
            if (element == null) return;
            allRefs.Add(element);
            allRefs.Add(end);
            switch (element)
            {
                case MEPCurve curve:
                    {
                        var list = GetElementsConnected(curve, connectorType);
                        foreach (var item in list)
                        {
                            if (allRefs.All(x => x.Id != item.Id))
                            {
                                GetElementsConnected(item, connectorType, ref allRefs);
                            }
                        }

                        break;
                    }
                case FamilyInstance instance:
                    {
                        var list = GetElementsConnected(instance, connectorType);
                        foreach (var item in list)
                        {
                            if (allRefs.All(x => x.Id != item.Id))
                            {
                                GetElementsConnected(item, connectorType, ref allRefs);
                            }
                        }

                        break;
                    }
            }
        }
    }
}