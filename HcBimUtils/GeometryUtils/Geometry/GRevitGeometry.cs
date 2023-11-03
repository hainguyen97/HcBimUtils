using System.Collections;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils.Geometry
{
    public static class GRevitGeometry
    {
        public static FaceArray GetAllFaces(this Element rvtElement)
        {
            var faceArray = new FaceArray();
            var geometryElement = rvtElement.get_Geometry(new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            });
            if (null == geometryElement) return faceArray;
            foreach (var geometryObject in geometryElement)
            {
                if (geometryObject is not Solid solid) continue;
                foreach (var obj in solid.Faces)
                {
                    var face = (Face)obj;
                    faceArray.Append(face);
                }
            }
            return faceArray;
        }

        public static List<SolidData> GetElementSolids(Element rvtElement, bool computeReferences, bool skipEmptySolids, bool orderByZ)
        {
            var list = new List<SolidData>();
            if (rvtElement == null)
            {
                return list;
            }
            var geometryElement = rvtElement.get_Geometry(new Options
            {
                DetailLevel = ViewDetailLevel.Fine,
                ComputeReferences = computeReferences
            });
            if (null == geometryElement)
            {
                return list;
            }
            foreach (var geometryObject in geometryElement)
            {
                if (geometryObject == null) continue;
                if (geometryObject is not Solid)
                {
                    if (geometryObject is not GeometryInstance geometryInstance)
                    {
                        continue;
                    }

                    using var enumerator2 = geometryInstance.SymbolGeometry.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current != null && enumerator2.Current is Solid solid)
                        {
                            list.Add(new SolidData(solid, geometryInstance));
                        }
                    }
                    continue;
                }
                list.Add(new SolidData(geometryObject as Solid, null));
            }
            if (skipEmptySolids)
            {
                list.RemoveAll(sd => sd.Solid.Faces == null || sd.Solid.Faces.IsEmpty);
            }
            if (orderByZ)
            {
                list = list.OrderBy(delegate (SolidData sd)
                {
                    var geometryObjectBBox = GetGeometryObjectBBox(sd.Solid, Transform.Identity, sd.GeometryInstance);
                    return geometryObjectBBox == null ? double.MinValue : geometryObjectBBox.Min.Z;
                }).ToList();
            }
            return list;
        }

        public static List<Face> GetFacesFromSolid(this Solid solid)
        {
            var list = new List<Face>();
            if (solid == null) return list;
            list.AddRange(solid.Faces.Cast<Face>());
            return list;
        }

        public static SolidData GetSolidFromFace(Face face, Element element)
        {
            if (face == null || element == null) return null;
            var elementSolids = GetElementSolids(element, face.Reference != null, true, false);
            return elementSolids.Where(solidData => solidData != null && solidData.Solid != null && solidData.Solid.Faces != null).FirstOrDefault(solidData => solidData.Solid.Faces.Cast<object>().Any(obj => obj != null && obj.Equals(face)));
        }

        public static SolidData GetSolidFromEdge(Edge edge, Element element)
        {
            if (edge == null || element == null)
            {
                return null;
            }
            var solidFromFace = GetSolidFromFace(edge.GetFace(0), element);
            if (solidFromFace != null && solidFromFace.Solid != null)
            {
                return solidFromFace;
            }
            return GetSolidFromFace(edge.GetFace(1), element);
        }

        public static SolidData GetSolidFromGeometryObject(GeometryObject geometryObject, Element element)
        {
            if (geometryObject == null || element == null)
            {
                return null;
            }
            var solidFromFace = GetSolidFromFace(geometryObject as Face, element);
            if (solidFromFace != null && solidFromFace.Solid != null)
            {
                return solidFromFace;
            }
            return GetSolidFromEdge(geometryObject as Edge, element);
        }

        public static List<XYZ> GetPointsFromFace(Face rvtFace)
        {
            if (null == rvtFace)
            {
                return null;
            }
            var mesh = rvtFace.Triangulate();
            if (null == mesh) return null;
            return mesh.Vertices.Count <= 0 ? null : mesh.Vertices.ToList();
        }

        public static GeometryElement GetElementGeometry(Element rvtElement, Options geomOpts, bool bOriginalGeom)
        {
            switch (rvtElement)
            {
                case null:
                    Trace.Assert(false, "Revit element should not be null!");
                    return null;
                case FamilyInstance familyInstance when bOriginalGeom:
                    return familyInstance.GetOriginalGeometry(new Options(geomOpts)
                    {
                        ComputeReferences = false
                    });
                default:
                    return rvtElement.get_Geometry(geomOpts);
            }
        }

        public static bool ElementsIntersect(Element element1, Element element2, double tolerance = 0.0001)
        {
            if (element1 == null || element2 == null) return false;
            var bboxAlongElement = GetBBoxAlongElement(element1, false);
            var bboxAlongElement2 = GetBBoxAlongElement(element2, false);
            return bboxAlongElement != null && bboxAlongElement2 != null && bboxAlongElement.Intersects(bboxAlongElement2, tolerance);
        }

        public static BoundingBoxXYZ GetBBox(Element rvtElement, bool bOriginal)
        {
            var elementGeometry = GetElementGeometry(rvtElement, new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            }, bOriginal);
            if (null == elementGeometry)
            {
                return null;
            }
            var elementTransform = GetElementTransform(rvtElement, bOriginal);
            return GetBBox(elementGeometry, elementTransform);
        }

        public static BoundingBoxXYZ GetBBoxAlongElement(Element rvtElement, bool bOriginal)
        {
            var elementGeometry = GetElementGeometry(rvtElement, new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            }, bOriginal);
            if (null == elementGeometry) return null;
            var elementTransform = GetElementTransform(rvtElement, bOriginal);
            var elementTransform2 = GetElementTransform(rvtElement);
            return GetBBoxAlongCs(elementGeometry, elementTransform, elementTransform2);
        }

        // Token: 0x06001854 RID: 6228 RVA: 0x0007267C File Offset: 0x0007087C
        public static List<XYZ> GetBBoxUpperOrLowerCorners(BoundingBoxXYZ bbox, bool bUpper)
        {
            var num = bUpper ? bbox.Max.Z : bbox.Min.Z;
            return new List<XYZ>
            {
                new(bbox.Min.X, bbox.Min.Y, num),
                new(bbox.Max.X, bbox.Min.Y, num),
                new(bbox.Max.X, bbox.Max.Y, num),
                new(bbox.Min.X, bbox.Max.Y, num)
            };
        }

        public static BoundingBoxXYZ GetBBox(GeometryElement rvtGeomElt, Transform rvtEltTrf)
        {
            var array = new double[6];
            const int num = 0;
            const int num2 = 1;
            const int num3 = 2;
            const double maxValue = double.MaxValue;
            const double maxValue2 = double.MaxValue;
            array[num3] = maxValue;
            const double maxValue3 = double.MaxValue;
            array[num2] = maxValue2;
            array[num] = maxValue3;
            const int num5 = 3;
            const int num6 = 4;
            const int num7 = 5;
            const double minValue = double.MinValue;
            const double minValue2 = double.MinValue;
            array[num7] = minValue;
            const double minValue3 = double.MinValue;
            array[num6] = minValue2;
            array[num5] = minValue3;
            if (null != rvtGeomElt)
            {
                foreach (var geometryObject in rvtGeomElt)
                {
                    if (null == geometryObject) continue;
                    switch (geometryObject)
                    {
                        case Solid solid when solid.Faces.IsEmpty || solid.Volume.IsZero():
                            continue;
                        case Solid solid:
                            {
                                IEnumerator enumerator2 = solid.Faces.GetEnumerator();
                                while (enumerator2.MoveNext())
                                {
                                    object obj = enumerator2.Current;
                                    Face face = (Face)obj;
                                    if (face != null)
                                    {
                                        Mesh mesh = face.Triangulate();
                                        if (null != mesh)
                                        {
                                            Mesh mesh2 = mesh.get_Transformed(rvtEltTrf);
                                            foreach (XYZ xyz in mesh2.Vertices)
                                            {
                                                array[0] = Math.Min(array[0], xyz.X);
                                                array[1] = Math.Min(array[1], xyz.Y);
                                                array[2] = Math.Min(array[2], xyz.Z);
                                                array[3] = Math.Max(array[3], xyz.X);
                                                array[4] = Math.Max(array[4], xyz.Y);
                                                array[5] = Math.Max(array[5], xyz.Z);
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                        case GeometryInstance instance:
                            {
                                if (null != instance.SymbolGeometry)
                                {
                                    foreach (var geometryObject2 in instance.SymbolGeometry)
                                    {
                                        var solid2 = geometryObject2 as Solid;
                                        if (null == solid2) continue;
                                        foreach (var obj2 in solid2.Faces)
                                        {
                                            var face2 = (Face)obj2;
                                            var mesh3 = face2.Triangulate();
                                            if (null == mesh3) continue;
                                            var mesh4 = mesh3.get_Transformed(instance.Transform.Multiply(rvtEltTrf));
                                            foreach (var xyz2 in mesh4.Vertices)
                                            {
                                                array[0] = Math.Min(array[0], xyz2.X);
                                                array[1] = Math.Min(array[1], xyz2.Y);
                                                array[2] = Math.Min(array[2], xyz2.Z);
                                                array[3] = Math.Max(array[3], xyz2.X);
                                                array[4] = Math.Max(array[4], xyz2.Y);
                                                array[5] = Math.Max(array[5], xyz2.Z);
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            if (array[0] == 1.7976931348623157E+308 || array[1] == 1.7976931348623157E+308 || array[2] == 1.7976931348623157E+308) return null;
            if (array[3] == -1.7976931348623157E+308) return null;
            if (array[4] != -1.7976931348623157E+308 && array[5] != -1.7976931348623157E+308)
            {
                return new BoundingBoxXYZ
                {
                    Min = new XYZ(array[0], array[1], array[2]),
                    Max = new XYZ(array[3], array[4], array[5])
                };
            }
            return null;
        }

        public static BoundingBoxXYZ GetBBoxAlongCs(GeometryElement rvtGeomElt, Transform rvtGeomTrf, Transform refCoordinateSystem, bool takeCurvesIntoConsideration = false)
        {
            var pointsFromGeometryElement = GetPointsFromGeometryElement(rvtGeomElt, rvtGeomTrf, takeCurvesIntoConsideration);
            return GetBoundingBoxFromPointCloud(pointsFromGeometryElement, refCoordinateSystem);
        }

        public static List<XYZ> GetPointsFromGeometryElement(GeometryElement rvtGeomElt, Transform rvtGeomTrf, bool takeCurvesIntoConsideration = false)
        {
            var list = new List<XYZ>();
            if (null == rvtGeomElt || rvtGeomTrf == null) return list;
            foreach (var geometryObject in rvtGeomElt)
            {
                if (null == geometryObject) continue;
                switch (geometryObject)
                {
                    case Solid o:
                        {
                            if (o.Faces.IsEmpty || o.Volume.IsZero())
                            {
                                continue;
                            }

                            var enumerator2 = o.Faces.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                var obj = enumerator2.Current;
                                var face = (Face)obj;
                                var mesh = face.Triangulate();
                                if (null == mesh) continue;
                                var mesh2 = mesh.get_Transformed(rvtGeomTrf);
                                list.AddRange(mesh2.Vertices);
                            }
                            continue;
                        }
                    case GeometryInstance instance:
                        {
                            if (!(null != instance.SymbolGeometry))
                            {
                                continue;
                            }

                            using var enumerator4 = instance.SymbolGeometry.GetEnumerator();
                            while (enumerator4.MoveNext())
                            {
                                var geometryObject2 = enumerator4.Current;
                                var solid2 = geometryObject2 as Solid;
                                if (null == solid2) continue;
                                list.AddRange(from Face face2 in solid2.Faces select face2.Triangulate() into mesh3 where !(null == mesh3) select mesh3.get_Transformed(instance.Transform.Multiply(rvtGeomTrf)) into mesh4 from item2 in mesh4.Vertices select item2);
                            }
                            continue;
                        }
                    case Curve o when takeCurvesIntoConsideration:
                        {
                            var curve = o.CreateTransformed(rvtGeomTrf);
                            var list2 = curve.Tessellate();
                            list.AddRange(list2);

                            break;
                        }
                }
            }
            return list;
        }

        public static BoundingBoxXYZ GetGeometryObjectBBox(GeometryObject geomObj, Transform refCoordinateSystem, GeometryInstance geometryInstance)
        {
            if (null == geomObj)
            {
                return null;
            }
            var vertices = new List<XYZ>();
            var action = delegate (Face face)
            {
                var mesh = (face != null) ? face.Triangulate() : null;
                if (mesh == null) return;
                if (geometryInstance == null)
                {
                    vertices.AddRange(mesh.Vertices);
                    return;
                }
                var mesh2 = mesh.get_Transformed(geometryInstance.Transform);
                if (null != mesh2)
                {
                    vertices.AddRange(mesh2.Vertices);
                }
            };
            if (geomObj is not Face obj1)
            {
                if (geomObj is not Edge edge)
                {
                    if (geomObj is Solid solid)
                    {
                        if (solid.Faces == null || solid.Faces.IsEmpty || solid.Volume.IsZero()) return null;
                        var enumerator = solid.Faces.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var obj = enumerator.Current;
                            var obj2 = (Face)obj;
                            action(obj2);
                        }
                        goto IL_ED;
                    }
                }
                else
                {
                    vertices.AddRange(edge.Tessellate());
                }
            }
            else
            {
                action(obj1);
            }
        IL_ED:
            return GetBoundingBoxFromPointCloud(vertices, refCoordinateSystem);
        }

        public delegate bool PointFilter(XYZ point);

        public static BoundingBoxXYZ GetBoundingBoxFromPointCloud(List<XYZ> pointCloud, Transform refCoordinateSystem, PointFilter callbackFilter = null)
        {
            if (pointCloud == null || pointCloud.Count == 0)
            {
                return null;
            }
            var plane = BPlane.CreateByNormalAndOrigin(refCoordinateSystem.BasisX.Normalize(), refCoordinateSystem.Origin);
            var plane2 = BPlane.CreateByNormalAndOrigin(refCoordinateSystem.BasisY.Normalize(), refCoordinateSystem.Origin);
            var plane3 = BPlane.CreateByNormalAndOrigin(refCoordinateSystem.BasisZ.Normalize(), refCoordinateSystem.Origin);
            var array = new double[6];
            const int num = 0;
            const int num2 = 1;
            const int num3 = 2;
            const double maxValue = double.MaxValue;
            const double maxValue2 = double.MaxValue;
            array[num3] = maxValue;
            const double maxValue3 = double.MaxValue;
            array[num2] = maxValue2;
            array[num] = maxValue3;
            const int num5 = 3;
            const int num6 = 4;
            const int num7 = 5;
            const double minValue = double.MinValue;
            const double minValue2 = double.MinValue;
            array[num7] = minValue;
            const double minValue3 = double.MinValue;
            array[num6] = minValue2;
            array[num5] = minValue3;
            callbackFilter ??= ((XYZ pt) => pt != null);
            var list = (from pt in pointCloud
                        where callbackFilter(pt)
                        select pt).ToList<XYZ>();
            foreach (var pt2 in list)
            {
                var val = plane.SignedDistanceTo(pt2);
                var val2 = plane2.SignedDistanceTo(pt2);
                var val3 = plane3.SignedDistanceTo(pt2);
                array[0] = Math.Min(array[0], val);
                array[1] = Math.Min(array[1], val2);
                array[2] = Math.Min(array[2], val3);
                array[3] = Math.Max(array[3], val);
                array[4] = Math.Max(array[4], val2);
                array[5] = Math.Max(array[5], val3);
            }

            if (array[0] == 1.7976931348623157E+308 || array[1] == 1.7976931348623157E+308 || array[2] == 1.7976931348623157E+308) return null;
            if (array[3] == -1.7976931348623157E+308) return null;
            if (array[4] != -1.7976931348623157E+308 && array[5] != -1.7976931348623157E+308)
            {
                return new BoundingBoxXYZ
                {
                    Transform = refCoordinateSystem,
                    Min = new XYZ(array[0], array[1], array[2]),
                    Max = new XYZ(array[3], array[4], array[5])
                };
            }
            return null;
        }

        public static void GetFacesNormalTo(Element rvtElement, XYZ vecNormal, ref List<PlanarFace> lstFaces)
        {
            GeometryElement geometryElement = rvtElement.get_Geometry(new Options
            {
                ComputeReferences = true
            });
            if (null == geometryElement)
            {
                return;
            }
            foreach (var geometryObject in geometryElement)
            {
                if (null == geometryObject) continue;
                var solid = geometryObject as Solid;
                var geometryInstance = geometryObject as GeometryInstance;
                if (null != solid)
                {
                    var enumerator2 = solid.Faces.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        var obj = enumerator2.Current;
                        var face = (Face)obj;
                        var planarFace = face as PlanarFace;
                        if (null == planarFace || !Math.Abs(planarFace.FaceNormal.DotProduct(vecNormal)).IsEqual(1.0)) continue;
                        lstFaces ??= new List<PlanarFace>();
                        lstFaces.Add(planarFace);
                    }
                    continue;
                }

                if (null == geometryInstance) continue;
                var xyz = geometryInstance.Transform.OfVector(vecNormal);
                foreach (var geometryObject2 in geometryInstance.SymbolGeometry)
                {
                    var solid2 = geometryObject2 as Solid;
                    if (null == solid2) continue;
                    foreach (var obj2 in solid2.Faces)
                    {
                        var face2 = (Face)obj2;
                        var planarFace2 = face2 as PlanarFace;
                        if (null == planarFace2 || !Math.Abs(planarFace2.FaceNormal.DotProduct(xyz)).IsEqual(1.0)) continue;
                        lstFaces ??= new List<PlanarFace>();
                        lstFaces.Add(planarFace2);
                    }
                }
            }
        }

        public static PlanarFace GetFacesWithNormalFromTransformedElement(Element rvtElt, XYZ normal)
        {
            var list = new List<PlanarFace>();
            PlanarFace planarFace = null;
            var transformed = rvtElt.get_Geometry(new Options()).GetTransformed(Transform.Identity);
            if (null != transformed)
            {
                foreach (var geometryObject in transformed)
                {
                    if (geometryObject is not Solid solid) continue;
                    list.AddRange(from Face face in solid.Faces select face as PlanarFace);
                }
            }
            if (list.Count > 0)
            {
                planarFace = (from i in list
                              where i.FaceNormal.IsAlmostEqualTo(normal)
                              select i).FirstOrDefault();
            }
            return !(planarFace != null) ? null : planarFace;
        }

        private static void GetOrderedSolidFaces(ref Solid crtSolid1, Transform geomTransform, XYZ vecNormal, ref Dictionary<double, List<PlanarFace>> dictFaces, ref Curve modelCurve, Element rvtElement)
        {
            foreach (var obj in crtSolid1.Faces)
            {
                var face = (Face)obj;
                var planarFace = face as PlanarFace;
                if (planarFace == null) continue;
                var xyz = (geomTransform != null) ? geomTransform.OfVector(planarFace.FaceNormal) : planarFace.FaceNormal;
                if (!GGeomTools.IsColinear(xyz, vecNormal) || !GGeomTools.AreCodirectional(xyz, vecNormal)) continue;
                dictFaces ??= new Dictionary<double, List<PlanarFace>>();
                if (modelCurve == null) continue;
                if (Math.Abs(planarFace.FaceNormal.Z) > 4.94065645841247E-324)
                {
                    var key = modelCurve.ComputeNormalizedParameter(planarFace.Origin.Z - modelCurve.GetEndPoint(0).Z);
                    if (!dictFaces.ContainsKey(key))
                    {
                        dictFaces.Add(key, new List<PlanarFace>
                  {
                     planarFace
                  });
                    }
                    else
                    {
                        var flag = false;
                        var text = planarFace.Reference != null ? planarFace.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                        foreach (var planarFace2 in dictFaces[key])
                        {
                            var value = planarFace2.Reference != null ? planarFace2.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                            if (!planarFace2.Equals(planarFace) && !text.Equals(value)) continue;
                            flag = true;
                            break;
                        }
                        if (!flag)
                        {
                            dictFaces[key].Add(planarFace);
                        }
                    }
                }
                else if (Math.Abs(planarFace.FaceNormal.X) > 4.94065645841247E-324)
                {
                    var x = modelCurve.GetEndPoint((planarFace.FaceNormal.X > 0.0) ? 0 : 1).X;
                    if (!dictFaces.ContainsKey(x))
                    {
                        dictFaces.Add(x, new List<PlanarFace>
                  {
                     planarFace
                  });
                    }
                    else
                    {
                        var flag2 = false;
                        var text2 = (planarFace.Reference != null) ? planarFace.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                        foreach (var planarFace3 in dictFaces[x])
                        {
                            var value2 = planarFace3.Reference != null ? planarFace3.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                            if (!planarFace3.Equals(planarFace) && !text2.Equals(value2)) continue;
                            flag2 = true;
                            break;
                        }
                        if (!flag2)
                        {
                            dictFaces[x].Add(planarFace);
                        }
                    }
                }
                else if (Math.Abs(planarFace.FaceNormal.Y) > 4.94065645841247E-324)
                {
                    var y = modelCurve.GetEndPoint((planarFace.FaceNormal.Y > 0.0) ? 0 : 1).Y;
                    if (!dictFaces.ContainsKey(y))
                    {
                        dictFaces.Add(y, new List<PlanarFace>
                  {
                     planarFace
                  });
                    }
                    else
                    {
                        var flag3 = false;
                        var text3 = planarFace.Reference != null ? planarFace.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                        foreach (var planarFace4 in dictFaces[y])
                        {
                            var value3 = planarFace4.Reference != null ? planarFace4.Reference.ConvertToStableRepresentation(rvtElement.Document) : string.Empty;
                            if (!planarFace4.Equals(planarFace) && !text3.Equals(value3)) continue;
                            flag3 = true;
                            break;
                        }
                        if (!flag3)
                        {
                            dictFaces[y].Add(planarFace);
                        }
                    }
                }
            }
        }

        public static Curve GetAnalyticalModelCurveByNormal(XYZ vecNormal, Element rvtElement)
        {
            if (rvtElement is not FamilyInstance familyInstance)
            {
                return null;
            }
            var originalGeometry = familyInstance.GetOriginalGeometry(new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            });
            var list = new List<PlanarFace>();
            if (originalGeometry == null)
            {
                return null;
            }
            foreach (var geometryObject in originalGeometry)
            {
                if (geometryObject == null) continue;
                var solid = geometryObject as Solid;
                if (solid != null)
                {
                    var enumerator2 = solid.Faces.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        var obj = enumerator2.Current;
                        var face = (Face)obj;
                        var planarFace = face as PlanarFace;
                        if (planarFace != null && GGeomTools.IsColinear(planarFace.FaceNormal, vecNormal))
                        {
                            list.Add(planarFace);
                        }
                    }
                    continue;
                }
                var geometryInstance = geometryObject as GeometryInstance;
                if (geometryInstance == null) continue;
                geometryInstance.Transform.OfVector(vecNormal);
                foreach (var geometryObject2 in geometryInstance.SymbolGeometry)
                {
                    var solid2 = geometryObject2 as Solid;
                    if (solid2 == null) continue;
                    list.AddRange(from Face face2 in solid2.Faces select face2 as PlanarFace into planarFace2 where planarFace2 != null && GGeomTools.IsColinear(geometryInstance.Transform.OfVector(planarFace2.FaceNormal), vecNormal) select planarFace2);
                }
            }
            return list.Count >= 2 ? Line.CreateBound(list[0].Origin, list[1].Origin) : null;
        }

        public static void GetFacesWithNormal(Element rvtElement, XYZ vecNormal, ref List<PlanarFace> lstFaces)
        {
            var geometryElement = rvtElement.get_Geometry(new Options
            {
                ComputeReferences = true
            });
            if (null == geometryElement) return;
            foreach (var geometryObject in geometryElement)
            {
                if (null == geometryObject) continue;
                var solid = geometryObject as Solid;
                if (null != solid)
                {
                    var enumerator2 = solid.Faces.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        var obj = enumerator2.Current;
                        var face = (Face)obj;
                        var planarFace = face as PlanarFace;
                        if (null == planarFace || !GGeomTools.IsColinear(planarFace.FaceNormal, vecNormal) || !GGeomTools.AreCodirectional(planarFace.FaceNormal, vecNormal)) continue;
                        lstFaces ??= new List<PlanarFace>();
                        lstFaces.Add(planarFace);
                    }
                    continue;
                }
                var geometryInstance = geometryObject as GeometryInstance;
                if (null == geometryInstance) continue;
                geometryInstance.Transform.OfVector(vecNormal);
                foreach (GeometryObject geometryObject2 in geometryInstance.SymbolGeometry)
                {
                    var solid2 = geometryObject2 as Solid;
                    if (null == solid2) continue;
                    foreach (var obj2 in solid2.Faces)
                    {
                        var face2 = (Face)obj2;
                        var planarFace2 = face2 as PlanarFace;
                        if (null == planarFace2 || !GGeomTools.IsColinear(geometryInstance.Transform.OfVector(planarFace2.FaceNormal), vecNormal) || !GGeomTools.AreCodirectional(geometryInstance.Transform.OfVector(planarFace2.FaceNormal), vecNormal)) continue;
                        lstFaces ??= new List<PlanarFace>();
                        lstFaces.Add(planarFace2);
                    }
                }
            }
        }

        public static ReferenceArray FindExtremeReferenceArray(Element rvtElt, XYZ vecNormal)
        {
            vecNormal = vecNormal.Normalize();
            ReferenceArray referenceArray = null;
            Reference reference = null;
            Reference reference2 = null;
            var geometryElement = rvtElt.get_Geometry(new Options
            {
                ComputeReferences = true
            });
            if (null == geometryElement)
            {
                return null;
            }
            var num = double.MinValue;
            var num2 = double.MinValue;
            foreach (var geometryObject in geometryElement)
            {
                if (null == geometryObject) continue;
                var solid = geometryObject as Solid;
                if (null != solid)
                {
                    if (solid.Volume.IsEqual(0.0) || solid.Faces.IsEmpty)
                    {
                        continue;
                    }
                    var xyz = solid.ComputeCentroid();
                    var enumerator2 = solid.Faces.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        var obj = enumerator2.Current;
                        var face = (Face)obj;
                        var boundingBox = face.GetBoundingBox();
                        var min = boundingBox.Min;
                        var max = boundingBox.Max;
                        var uv = min + 0.5 * (max - min);
                        var xyz2 = face.Evaluate(uv);
                        var xyz3 = face.ComputeNormal(uv).Normalize();
                        if (!xyz3.CrossProduct(vecNormal).IsZeroLength()) continue;
                        if (xyz3.IsAlmostEqualTo(vecNormal))
                        {
                            if (!(xyz.DistanceTo(xyz2) > num)) continue;
                            num = xyz.DistanceTo(xyz2);
                            reference2 = face.Reference;
                        }
                        else if (xyz.DistanceTo(xyz2) > num2)
                        {
                            num2 = xyz.DistanceTo(xyz2);
                            reference = face.Reference;
                        }
                    }
                    continue;
                }
                var geometryInstance = geometryObject as GeometryInstance;
                if (null == geometryInstance) continue;
                foreach (GeometryObject geometryObject2 in geometryInstance.SymbolGeometry)
                {
                    var solid2 = geometryObject2 as Solid;
                    if (null == solid2 || solid2.Volume.IsEqual(0.0) || solid2.Faces.IsEmpty) continue;
                    var xyz4 = solid2.ComputeCentroid();
                    foreach (var obj2 in solid2.Faces)
                    {
                        var face2 = (Face)obj2;
                        var boundingBox2 = face2.GetBoundingBox();
                        var min2 = boundingBox2.Min;
                        var max2 = boundingBox2.Max;
                        var uv2 = min2 + 0.5 * (max2 - min2);
                        var xyz5 = face2.Evaluate(uv2);
                        var xyz6 = face2.ComputeNormal(uv2).Normalize();
                        if (!xyz6.CrossProduct(vecNormal).IsZeroLength()) continue;
                        if (!xyz6.IsAlmostEqualTo(vecNormal))
                        {
                            if (!(xyz4.DistanceTo(xyz5) > num2)) continue;
                            num2 = xyz4.DistanceTo(xyz5);
                            reference = face2.Reference;
                        }
                        else if (xyz4.DistanceTo(xyz5) > num)
                        {
                            num = xyz4.DistanceTo(xyz5);
                            reference2 = face2.Reference;
                        }
                    }
                }
            }

            if (reference == null || reference2 == null) return referenceArray;
            referenceArray = new ReferenceArray();
            referenceArray.Append(reference);
            referenceArray.Append(reference2);
            return referenceArray;
        }

        public static XYZ TransformPoint2Global(XYZ rvtPoint, Transform rvtTransform)
        {
            if (rvtTransform == null)
            {
                return rvtPoint;
            }
            var basisX = rvtTransform.BasisX;
            var basisY = rvtTransform.BasisY;
            var basisZ = rvtTransform.BasisZ;
            var origin = rvtTransform.Origin;
            return new XYZ(rvtPoint.X * basisX.X + rvtPoint.Y * basisY.X + rvtPoint.Z * basisZ.X + origin.X, rvtPoint.X * basisX.Y + rvtPoint.Y * basisY.Y + rvtPoint.Z * basisZ.Y + origin.Y, rvtPoint.X * basisX.Z + rvtPoint.Y * basisY.Z + rvtPoint.Z * basisZ.Z + origin.Z);
        }

        public static XYZ TransformVector2Global(XYZ rvtVector, Transform rvtTransform)
        {
            if (rvtTransform == null)
            {
                return rvtVector;
            }
            var basisX = rvtTransform.BasisX;
            var basisY = rvtTransform.BasisY;
            var basisZ = rvtTransform.BasisZ;
            return new XYZ(rvtVector.X * basisX.X + rvtVector.Y * basisY.X + rvtVector.Z * basisZ.X, rvtVector.X * basisX.Y + rvtVector.Y * basisY.Y + rvtVector.Z * basisZ.Y, rvtVector.X * basisX.Z + rvtVector.Y * basisY.Z + rvtVector.Z * basisZ.Z);
        }

        public static XYZ TransformPoint2Local(XYZ rvtPoint, Transform rvtTransform)
        {
            if (rvtTransform == null)
            {
                return rvtPoint;
            }
            var basisX = rvtTransform.BasisX;
            var basisY = rvtTransform.BasisY;
            var basisZ = rvtTransform.BasisZ;
            var origin = rvtTransform.Origin;
            var num = rvtPoint.X - origin.X;
            var num2 = rvtPoint.Y - origin.Y;
            var num3 = rvtPoint.Z - origin.Z;
            return new XYZ(num * basisX.X + num2 * basisX.Y + num3 * basisX.Z, num * basisY.X + num2 * basisY.Y + num3 * basisY.Z, num * basisZ.X + num2 * basisZ.Y + num3 * basisZ.Z);
        }

        public static XYZ TransformVector2Local(XYZ rvtVector, Transform rvtTransform)
        {
            if (rvtTransform == null)
            {
                return rvtVector;
            }
            var x = rvtVector.X;
            var y = rvtVector.Y;
            var z = rvtVector.Z;
            var basisX = rvtTransform.BasisX;
            var basisY = rvtTransform.BasisY;
            var basisZ = rvtTransform.BasisZ;
            return new XYZ(x * basisX.X + y * basisX.Y + z * basisX.Z, x * basisY.X + y * basisY.Y + z * basisY.Z, x * basisZ.X + y * basisZ.Y + z * basisZ.Z);
        }

        public static Face GetFaceWithNormal(Element rvtElement, XYZ normal)
        {
            var allFaces = GetAllFaces(rvtElement);
            var enumerator = allFaces.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                var face = (Face)obj;
                if (face != null && ((PlanarFace)face).FaceNormal.IsAlmostEqualTo(normal))
                {
                    return face;
                }
            }
            return null;
        }

        public static void GetOriginalSectionWidthAndHeight(this Element rvtElement, out double width, out double height, bool convertToMeter = false)
        {
            width = 0.0;
            height = 0.0;
            var bboxAlongElement = GetBBoxAlongElement(rvtElement, true);
            if (bboxAlongElement != null && bboxAlongElement.Min != null && bboxAlongElement.Max != null)
            {
                width = bboxAlongElement.Max.Y - bboxAlongElement.Min.Y;
                height = bboxAlongElement.Max.Z - bboxAlongElement.Min.Z;
                if (!convertToMeter) return;
                width = width.FootToMet();
                height = height.FootToMet();

                return;
            }
            var familyInstance = rvtElement as FamilyInstance;
            if (familyInstance == null)
            {
                return;
            }
            var originalGeometry = familyInstance.GetOriginalGeometry(new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            });
            var faceArray = new FaceArray();
            if (null != originalGeometry)
            {
                foreach (var geometryObject in originalGeometry)
                {
                    if (geometryObject is not Solid solid) continue;
                    foreach (var obj in solid.Faces)
                    {
                        var face = (Face)obj;
                        faceArray.Append(face);
                    }
                }
            }
            Face face2 = null;
            foreach (var obj2 in faceArray)
            {
                var face3 = (Face)obj2;
                var boundingBox = face3.GetBoundingBox();
                var min = boundingBox.Min;
                var max = boundingBox.Max;
                var uv = min + 0.5 * (max - min);
                face3.Evaluate(uv);
                var xyz = face3.ComputeNormal(uv).Normalize();
                if (Math.Abs(xyz.DotProduct(XYZ.BasisX)).IsEqual(1.0) && face2 == null)
                {
                    face2 = face3;
                }
            }

            if (face2 == null) return;
            var boundingBox2 = face2.GetBoundingBox();
            width = boundingBox2.Max.V - boundingBox2.Min.V;
            height = boundingBox2.Max.U - boundingBox2.Min.U;
            if (!convertToMeter) return;
            width = width.FootToMet();
            height = height.FootToMet();
        }

        public static Transform GetElementTransform(Element revitElement, bool bOriginal = true)
        {
            var result = Transform.Identity;
            if (!bOriginal)
            {
                return result;
            }

            if (revitElement is FamilyInstance familyInstance)
            {
                result = familyInstance.GetTransform();
            }
            else
            {
                result = ElementUtils.GetWallTransform(revitElement);
            }
            return result;
        }
    }
}