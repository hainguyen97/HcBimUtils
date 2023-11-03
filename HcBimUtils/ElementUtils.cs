using System.Collections;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils
{
    public static class ElementUtils
    {
        public static int GetIntElementId(this Element element)
        {
#if R24
            return -1;
#else
            if (element.IsValidElement())
            {
                return element.Id.IntegerValue;
            }
            return -1;
#endif
        }
        public static XYZ GetElementCenter(this Element element, View v = null)
        {
            BoundingBoxXYZ boundingBoxXyz = element.get_BoundingBox(v);
            XYZ xYZ = boundingBoxXyz.Max - boundingBoxXyz.Min;
            return new XYZ(boundingBoxXyz.Min.X + xYZ.X / 2.0, boundingBoxXyz.Min.Y + xYZ.Y / 2.0, boundingBoxXyz.Min.Z + xYZ.Z / 2.0);
        }
        public static bool Delete(this Element ele)
        {
            bool isSuccess = false;

            if (ele.IsValidElement())
            {
                Document doc = ele.Document;
                try
                {
                    doc.Delete(ele.Id);
                }
                catch
                {
                    // ignored
                }
            }

            return isSuccess;
        }
        public static List<ElementId> GetElementIdInGroup(Group group)
        {
            List<ElementId> eleIds = new List<ElementId>();
            if (group != null)
            {
                Document doc = group.Document;
                foreach (var memberId in group.GetMemberIds())
                {
                    Element ele = doc.GetElement(memberId);
                    if (ele is Group childGroup)
                    {
                        foreach (var id in GetElementIdInGroup(childGroup))
                        {
                            eleIds.Add(id);
                        }
                    }
                    else
                    {
                        eleIds.Add(ele.Id);
                    }
                }
            }
            return eleIds;
        }
        public static bool IsValidElement(this Element e)
        {
            return e != null && e.IsValidObject;
        }

        public static bool IsValidElementId(this ElementId id)
        {
            return id != null && id != ElementId.InvalidElementId;
        }

        public static bool IsContain(this Element ele, List<Element> elements)
        {
            return false;
        }

        private static bool VerifyCategory(Category rvtCategory, BuiltInCategory eCategory)
        {
            return rvtCategory != null && eCategory == rvtCategory.ToBuiltinCategory();
        }

        public static int GetIdIntegerValue(this Element element)
        {
            if (element == null)
            {
                return ElementId.InvalidElementId.IntegerValue;
            }
            return element.Id.IntegerValue;
        }

        public static XYZ GetCentroid(this Element element, View view)
        {
            XYZ result = null;
            if (element != null)
            {
                BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(view);
                if (boundingBoxXYZ != null)
                {
                    result = (boundingBoxXYZ.Min + boundingBoxXYZ.Max) / 2.0;
                }
            }
            return result;
        }

        public static List<Face> GetFaces(this Element element)
        {
            List<Face> list = new List<Face>();
            List<Solid> solids = element.GetSolids();
            foreach (Solid solid in solids)
            {
                foreach (object obj in solid.Faces)
                {
                    Face item = (Face)obj;
                    list.Add(item);
                }
            }
            return list;
        }

        public static bool HasConcreteMaterial(this Element revitElement)
        {
            return revitElement != null && revitElement.IsConcrete();
        }

        public static bool IsConcrete(this Element revitElement)
        {
            ICollection<ElementId> materialIds = revitElement.GetMaterialIds(false);
            foreach (ElementId elementId in materialIds)
            {
                Material elementMaterial = revitElement.Document.GetElement(elementId) as Material;
                if (elementMaterial.MaterialIsConcrete())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool MaterialIsConcrete(this Material elementMaterial)
        {
            if (elementMaterial != null && !(elementMaterial.StructuralAssetId == ElementId.InvalidElementId))
            {
                Parameter parameter = (elementMaterial.Document.GetElement(elementMaterial.StructuralAssetId) is PropertySetElement propertySetElement) ? propertySetElement.get_Parameter(BuiltInParameter.PHY_MATERIAL_PARAM_CLASS) : null;
                return parameter != null && parameter.AsInteger() == 4;
            }
            return false;
        }

        public static List<Solid> GetSolids(this Element element)
        {
            List<Solid> list = new List<Solid>();
            GeometryElement geometryElement = element.get_Geometry(new Options
            {
                IncludeNonVisibleObjects = true,
                ComputeReferences = true
            });
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Solid solid = geometryObject as Solid;
                GeometryInstance geometryInstance = geometryObject as GeometryInstance;
                if (solid != null && solid.Volume > 1E-06)
                {
                    list.Add(solid);
                }
                if (geometryInstance != null)
                {
                    GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject2 in instanceGeometry)
                    {
                        solid = (geometryObject2 as Solid);
                        if (solid != null && solid.Volume > 1E-06)
                        {
                            list.Add(solid);
                        }
                    }
                }
            }
            return list;
        }

        public static XYZ GetColumnLocationPoint(this Element element)
        {
            if (element?.Location == null)
            {
                return null;
            }
            if (!(element.Location is LocationPoint))
            {
                if (element.Location is LocationCurve)
                {
                    Curve curve = (element.Location as LocationCurve)?.Curve;
                    IList<XYZ> list = curve?.Tessellate();
                    if (list != null && list.Count > 0)
                    {
                        return (from pt in list
                                orderby pt.Z
                                select pt).First<XYZ>();
                    }
                }
                return null;
            }
            return (element.Location as LocationPoint)?.Point;
        }

        public static bool IsSlantedColumn(this Element revitColumn)
        {
            if (!VerifyCategory(revitColumn.Category, BuiltInCategory.OST_StructuralColumns))
            {
                return false;
            }
            Parameter parameter = revitColumn.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM);

            return parameter != null && parameter.HasValue && parameter.AsInteger() == 1;
        }

        public static BuiltInCategory ToBuiltinCategory1(this Category cat)
        {
            BuiltInCategory result = BuiltInCategory.INVALID;
            try
            {
                result = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), cat.Id.ToString());
                return result;
            }
            catch
            {
                return result;
            }
        }

        public static double GetColumnRotation(this Element revitElement)
        {
            if (!VerifyCategory(revitElement.Category, BuiltInCategory.OST_StructuralColumns))
            {
                return 0.0;
            }
            if (revitElement.Location is LocationPoint)
            {
                return ((LocationPoint)revitElement.Location).Rotation;
            }
            return 0.0;
        }

        public static StructuralMaterialType GetFamilyInstanceStructuralMaterialType(this Element revitElement)
        {
            if (revitElement.GetType() != typeof(FamilyInstance))
            {
                return StructuralMaterialType.Generic;
            }
            return ((FamilyInstance)revitElement).StructuralMaterialType;
        }

        public static string GetMark(this Element element)
        {
            if (element == null)
            {
                return string.Empty;
            }
            Parameter parameter = element.get_Parameter(BuiltInParameter.DOOR_NUMBER);
            if (parameter == null)
            {
                return string.Empty;
            }
            string text = parameter.AsString();
            if (text == null)
            {
                return string.Empty;
            }
            return text;
        }

        public static bool HasParameter(this Element element, string parameterName)
        {
            if (element != null)
            {
                IEnumerator enumerator = element.GetParameters(parameterName).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    Parameter parameter = (Parameter)obj;
                    if (parameter != null && parameterName == parameter.Definition.Name)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public static List<Rebar> GetRebarFromHost(this Element rvtElt, Document revitDocument)
        {
            IEnumerable<Rebar> enumerable = new FilteredElementCollector(revitDocument).OfClass(typeof(Rebar))
                .Select(elem => new { elem, rvtRebar = elem as Rebar })

                .Where(@t => @t.rvtRebar.GetHostId() == rvtElt.Id)

                .Select(@t => @t.rvtRebar);

            List<Rebar> list = new List<Rebar>();
            foreach (Rebar item in enumerable)
            {
                list.Add(item);
            }
            return list;
        }

        public static double GetColumnHeight(this Element rvtColumn)
        {
            if (!VerifyCategory(rvtColumn.Category, BuiltInCategory.OST_StructuralColumns))
            {
                return 0.0;
            }

            Parameter parameter = rvtColumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
            Parameter parameter2 = rvtColumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
            Parameter parameter3 = rvtColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
            Parameter parameter4 = rvtColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
            if (parameter == null || parameter2 == null || parameter3 == null || parameter4 == null)
            {
                return 0.0;
            }
            ElementId elementId = parameter.AsElementId();
            ElementId elementId2 = parameter3.AsElementId();
            var level = rvtColumn.Document.GetElement(elementId) as Level;
            var level2 = rvtColumn.Document.GetElement(elementId2) as Level;
            if (level != null && level2 != null)
            {
                double num = level2.ProjectElevation + parameter4.AsDouble();
                double num2 = level.ProjectElevation + parameter2.AsDouble();
                return Math.Abs(num - num2);
            }
            return 0.0;
        }

        public static double GetWallHeight(this Element revitWall, bool bInMeters = true)
        {
            if (!VerifyCategory(revitWall.Category, BuiltInCategory.OST_Walls))
            {
                return 0.0;
            }
            Parameter parameter = revitWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            if (parameter == null)
            {
                return 0.0;
            }
#if R20
            return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), 0);
#else
         return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), UnitTypeId.Meters);
#endif
        }

        public static double GetWallThickness(this Element revitWall, bool bInMeters = true)
        {
            if (!VerifyCategory(revitWall.Category, BuiltInCategory.OST_Walls))
            {
                return 0.0;
            }
            Wall wall = revitWall as Wall;
            if (wall == null)
            {
                return 0.0;
            }
#if R20
            return UnitUtils.ConvertFromInternalUnits(wall.Width, 0);
#else
         return UnitUtils.ConvertFromInternalUnits(wall.Width, UnitTypeId.Meters);
#endif
        }

        public static double GetWallFoundationWidth(this Element revitWallFoundation, bool bInMeters = true)
        {
            if (!VerifyCategory(revitWallFoundation.Category, BuiltInCategory.OST_StructuralFoundation))
            {
                return 0.0;
            }
            WallFoundation wallFoundation = revitWallFoundation as WallFoundation;
            if (wallFoundation == null)
            {
                return 0.0;
            }
            Parameter parameter = wallFoundation.get_Parameter(BuiltInParameter.CONTINUOUS_FOOTING_WIDTH);
            if (parameter == null)
            {
                return 0.0;
            }
            if (bInMeters)
            {
#if R20
                return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), 0);
#else
            return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), UnitTypeId.Meters);
#endif
            }
            return parameter.AsDouble();
        }

        public static double GetWallFoundationHeight(this Element revitWallFoundation, bool bInMeters = true)
        {
            if (!VerifyCategory(revitWallFoundation.Category, BuiltInCategory.OST_StructuralFoundation))
            {
                return 0.0;
            }
            if (!(revitWallFoundation is WallFoundation))
            {
                return 0.0;
            }
            WallFoundationType wallFoundationType = revitWallFoundation.Document.GetElement(revitWallFoundation.GetTypeId()) as WallFoundationType;
            if (wallFoundationType == null)
            {
                return 0.0;
            }
            Parameter parameter = wallFoundationType.get_Parameter(BuiltInParameter.STRUCTURAL_FOUNDATION_THICKNESS);
            if (parameter == null)
            {
                return 0.0;
            }
            if (!bInMeters)
            {
                return parameter.AsDouble();
            }
#if R20
            return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), 0);
#else
         return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), UnitTypeId.Meters);
#endif
        }

        public static double GetWallOffsetFromBaseLevel(this Element revitWall)
        {
            if (!VerifyCategory(revitWall.Category, BuiltInCategory.OST_Walls))
            {
                return 0.0;
            }
            Wall wall = revitWall as Wall;
            if (wall == null)
            {
                return 0.0;
            }
            BoundingBoxXYZ boundingBoxXyz = wall.get_BoundingBox(null);
            if (boundingBoxXyz != null)
            {
                return boundingBoxXyz.Min.Z;
            }

            if (wall.Location is LocationCurve locationCurve && locationCurve.Curve is Line)
            {
                return ((Line)locationCurve.Curve).Origin.Z;
            }
            return 0.0;
        }

        public static Line GetWallBottomLine(this Element revitWall)
        {
            if (!VerifyCategory(revitWall.Category, BuiltInCategory.OST_Walls))
            {
                return Line.CreateBound(XYZ.Zero, XYZ.Zero);
            }
            LocationCurve locationCurve = revitWall.Location as LocationCurve;
            if (locationCurve == null)
            {
                return Line.CreateBound(XYZ.Zero, XYZ.Zero);
            }
            Line line = locationCurve.Curve as Line;
            if (line == null)
            {
                return Line.CreateBound(XYZ.Zero, XYZ.Zero);
            }
            BoundingBoxXYZ boundingBoxXyz = revitWall.get_BoundingBox(null);
            if (boundingBoxXyz == null)
            {
                return line;
            }
            XYZ xyz = new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, boundingBoxXyz.Min.Z);
            XYZ xyz2 = xyz.Add(line.Direction.Multiply(line.Length));

            return Line.CreateBound(xyz, xyz2);
        }

        public static Transform GetWallTransform(Element revitElement)
        {
            Transform transform = Transform.Identity;
            if (revitElement.GetType() != typeof(Wall))
            {
                return transform;
            }

            if (revitElement.Location is LocationCurve locationCurve)
            {
                Line line = locationCurve.Curve as Line;
                if (line != null)
                {
                    BoundingBoxXYZ boundingBoxXyz = revitElement.get_BoundingBox(null);
                    transform = Transform.CreateTranslation(new XYZ(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, boundingBoxXyz?.Min.Z ?? line.GetEndPoint(0).Z));
                    transform.BasisX = new XYZ(line.Direction.X, line.Direction.Y, line.Direction.Z);
                    transform.BasisZ = XYZ.BasisZ;
                    transform.BasisY = transform.BasisZ.CrossProduct(transform.BasisX);
                }
            }
            return transform;
        }
        public static List<Line> LinesGeometry(this Element element, Document doc)
        {
            List<Line> lineList = new List<Line>();
            GeometryElement geometryElement = element.get_Geometry(new Options()
            {
                ComputeReferences = true,
                View = doc.ActiveView
            });
            if (geometryElement == null)
                return lineList;
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Line)
                    lineList.Add(geometryObject as Line);
            }
            return lineList;
        }
    }
}