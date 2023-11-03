using Autodesk.Revit.DB;
using HcBimUtils.GeometryUtils;

namespace HcBimUtils
{
    public static class BoundingBoxXYZUtils
    {
        public static XYZ TopRight(this BoundingBoxXYZ bbox)
        {
            return bbox.Transform.OfPoint(bbox.Max);
        }
        public static XYZ BottomLeft(this BoundingBoxXYZ bbox)
        {
            return bbox.Transform.OfPoint(bbox.Min);
        }
        public static XYZ BottomRight(this BoundingBoxXYZ bbox)
        {
            return bbox.Transform.OfPoint(new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z));
        }
        public static XYZ TopLeft(this BoundingBoxXYZ bbox)
        {
            return bbox.Transform.OfPoint(new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Max.Z));
        }
        public static BoundingBoxXYZ GetAlignBoundingBoxFromSolid(Solid solid, XYZ direction)
        {
            direction = new XYZ(direction.X, direction.Y, 0);

            Transform transform = Transform.Identity;
            transform.BasisX = direction;
            transform.BasisY = direction.CrossProduct(XYZ.BasisZ);
            transform.BasisZ = XYZ.BasisZ;

            if (!transform.IsConformal || transform.Determinant < 0)
            {
                transform.BasisX = direction;
                transform.BasisY = direction.CrossProduct(XYZ.BasisZ.Negate());
                transform.BasisZ = XYZ.BasisZ;
            }

            Solid solidTransform = SolidUtils.CreateTransformed(solid, transform.Inverse);

            BoundingBoxXYZ bbox = solidTransform.GetBoundingBox();

            bbox.Transform = transform.Multiply(bbox.Transform);

            return bbox;
        }
        public static BoundingBoxXYZ GetAlignedBoundingBoxFromElement(Element elem, double tolerance = 0)
        {
            LocationCurve lc = elem.Location as LocationCurve;
            Document doc = elem.Document;
            if (lc != null && lc.Curve.IsBound)
            {
                Options geomOpts = new Options();
                GeometryElement geometryElement = elem.get_Geometry(geomOpts);
                if (geometryElement != null)
                {
                    XYZ startPoint = lc.Curve.GetEndPoint(0);
                    XYZ endPoint = lc.Curve.GetEndPoint(1);
                    double angle = XYZUtils.SmallestAngleBetweenTwoVectors((endPoint - startPoint).Normalize(), XYZ.BasisZ);

                    // Element thẳng đứng
                    if (angle < Constants.AngleTolerance)
                    {
                        BoundingBoxXYZ bb = elem.get_BoundingBox(null);
                        XYZ p = new XYZ(tolerance, tolerance, 0);
                        bb.Max = bb.Max.Add(p);
                        bb.Min = bb.Min.Subtract(p);
                        return bb;
                    }

                    endPoint = new XYZ(endPoint.X, endPoint.Y, startPoint.Z);
                    XYZ direction = endPoint - startPoint;
                    XYZ normal = XYZ.BasisZ;

                    Transform t = Transform.Identity;
                    t.Origin = startPoint;
                    t.BasisX = direction.Normalize();
                    t.BasisY = normal.CrossProduct(t.BasisX).Normalize();
                    t.BasisZ = t.BasisX.CrossProduct(t.BasisY).Normalize();

                    // check we have a valid matrix

                    if (!t.IsConformal || t.Determinant < 0)
                        return null;

                    Transform modelToElementTransform = t.Inverse;

                    // transform the geometry into the ECS we 
                    // have created to get an aligned bounding box

                    GeometryElement geometryElementTransformed = geometryElement.GetTransformed(modelToElementTransform);

                    BoundingBoxXYZ ecsBoundingBox = geometryElementTransformed.GetBoundingBox();
                    XYZ tolerancePoint = new XYZ(tolerance, tolerance, 0);
                    ecsBoundingBox.Max = ecsBoundingBox.Max.Add(tolerancePoint);
                    ecsBoundingBox.Min = ecsBoundingBox.Min.Subtract(tolerancePoint);

                    // Revit 2013 Shim
                    // ===============
                    // in Revit 2013, the returned bounding box 
                    // is garbage - the Max is hugely negative 
                    // and the Min is hugely positive
                    // if this happens then get geometry points 
                    // in model coordinates, convert to element 
                    // coordinates and create bounding box from 
                    // those
                    // NB. we could use this code for 2014 as 
                    // well, but the calculation is probably not 
                    // as accurate for some situations

                    if (ecsBoundingBox.Max.X < ecsBoundingBox.Min.X)
                    {
                        // get points from all edges in all solids 
                        // - allows for curves by using tessellated 
                        // edges

                        List<XYZ> pts = new List<XYZ>();

                        foreach (Solid solid in geometryElement.OfType<Solid>())
                        {
                            foreach (Edge edge in solid.Edges)
                            {
                                pts.AddRange(edge.Tessellate());
                            }
                        }

                        // transform the points into element 
                        // coordinates

                        ecsBoundingBox = new BoundingBoxXYZ();

                        pts = pts.Select(pt => modelToElementTransform.OfPoint(pt)).ToList();

                        if (pts.Any())
                        {
                            // calculate the bounding box

                            ecsBoundingBox = new BoundingBoxXYZ();

                            ecsBoundingBox.Max = new XYZ(
                              pts.Max(pt => pt.X),
                              pts.Max(pt => pt.Y),
                              pts.Max(pt => pt.Z));

                            ecsBoundingBox.Min = new XYZ(
                              pts.Min(pt => pt.X),
                              pts.Min(pt => pt.Y),
                              pts.Min(pt => pt.Z));
                        }
                        else
                        {
                            // fail-case - if element has 
                            // no solid geometry

                            return null;
                        }
                    }

                    // finally apply the ECS to Model 
                    // transformation back to the bounding box

                    Transform transform = ecsBoundingBox.Transform;
                    Transform transformMultiply = t.Multiply(transform);
                    ecsBoundingBox.Transform = transformMultiply;

                    //GeometryElement testgeoE = geometryElement.GetTransformed(modelToElementTransform);

                    //BoundingBoxXYZ bbox = geometryElement.GetBoundingBox();
                    //bbox.Transform = t;

                    return ecsBoundingBox;
                }
            }
            else
            {
                BoundingBoxXYZ bb = elem.get_BoundingBox(doc.ActiveView);
                XYZ tolerancePoint = new XYZ(tolerance, tolerance, 0);
                bb.Max = bb.Max.Add(tolerancePoint);
                bb.Min = bb.Min.Subtract(tolerancePoint);

                return bb;
            }
            return null;
        }

        /// <summary>
        /// Make this bounding box empty by setting the
        /// Min value to plus infinity and Max to minus.
        /// </summary>
        public static void Clear(this BoundingBoxXYZ bb)
        {
            double infinity = double.MaxValue;
            bb.Min = new XYZ(infinity, infinity, infinity);
            bb.Max = -bb.Min;
        }

        /// <summary>
        /// Expand the given bounding box to include
        /// and contain the given point.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, XYZ p)
        {
            bb.Min = new XYZ(Math.Min(bb.Min.X, p.X),
                Math.Min(bb.Min.Y, p.Y),
                Math.Min(bb.Min.Z, p.Z));

            bb.Max = new XYZ(Math.Max(bb.Max.X, p.X),
                Math.Max(bb.Max.Y, p.Y),
                Math.Max(bb.Max.Z, p.Z));
        }

        /// <summary>
        /// Expand the given bounding box to include
        /// and contain the given points.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, IEnumerable<XYZ> pts)
        {
            bb.ExpandToContain(new XYZ(
                pts.Min<XYZ, double>(p => p.X),
                pts.Min<XYZ, double>(p => p.Y),
                pts.Min<XYZ, double>(p => p.Z)));

            bb.ExpandToContain(new XYZ(
                pts.Max<XYZ, double>(p => p.X),
                pts.Max<XYZ, double>(p => p.Y),
                pts.Max<XYZ, double>(p => p.Z)));
        }

        /// <summary>
        /// Expand the given bounding box to include
        /// and contain the given other one.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, BoundingBoxXYZ other)
        {
            bb.ExpandToContain(other.Min);
            bb.ExpandToContain(other.Max);
        }
        public static BoundingBoxXYZ GetBoundingBoxFromSolid(List<Solid> solids)
        {
            BoundingBoxXYZ finalBbox = null;
            if (solids != null && solids.Count > 0)
            {
                foreach (Solid solid in solids)
                {
                    BoundingBoxXYZ sectionBox = solid.GetBoundingBox();
                    if (finalBbox == null)
                    {
                        finalBbox = new BoundingBoxXYZ();
                        finalBbox.Max = sectionBox.Max;
                        finalBbox.Min = sectionBox.Min;
                    }
                    else
                    {
                        double maxX = finalBbox.Max.X;
                        double maxY = finalBbox.Max.Y;
                        double maxZ = finalBbox.Max.Z;
                        double minX = finalBbox.Min.X;
                        double minY = finalBbox.Min.Y;
                        double minZ = finalBbox.Min.Z;
                        if (sectionBox.Max.X > maxX)
                        {
                            maxX = sectionBox.Max.X;
                        }
                        if (sectionBox.Max.Y > maxY)
                        {
                            maxY = sectionBox.Max.Y;
                        }
                        if (sectionBox.Max.Z > maxZ)
                        {
                            maxZ = sectionBox.Max.Z;
                        }

                        if (sectionBox.Min.X < minX)
                        {
                            minX = sectionBox.Min.X;
                        }
                        if (sectionBox.Min.Y < minY)
                        {
                            minY = sectionBox.Min.Y;
                        }
                        if (sectionBox.Min.Z < minZ)
                        {
                            minZ = sectionBox.Min.Z;
                        }
                        finalBbox.Max = new XYZ(maxX, maxY, maxZ);
                        finalBbox.Min = new XYZ(minX, minY, minZ);
                    }
                }
                if (finalBbox != null)
                {
                    int step = 1;
                    finalBbox.Max += new XYZ(step, step, step);
                    finalBbox.Min -= new XYZ(step, step, step);
                }
            }
            return finalBbox;
        }

        public static double Width(this BoundingBoxXYZ bb)
        {
            return bb.Max.Y - bb.Min.Y;
        }

        public static double Length(this BoundingBoxXYZ bb)
        {
            return bb.Max.X - bb.Min.X;
        }

        public static double Height(this BoundingBoxXYZ bb)
        {
            return bb.Max.Z - bb.Min.Z;
        }

        public static double DistAlongDir(this BoundingBoxXYZ bb, XYZ dir)
        {
            XYZ pt = bb.Transform.OfPoint(bb.Max);
            XYZ xyz = bb.Transform.OfPoint(bb.Min);
            BPlane plane = BPlane.CreateByNormalAndOrigin(dir, xyz);
            return Math.Abs(plane.SignedDistanceTo(pt));
        }

        public static XYZ CenterPoint(this BoundingBoxXYZ bb)
        {
            return bb.Min.Add(0.5 * bb.Max.Subtract(bb.Min));
        }

        public static bool IsPointInBox(this BoundingBoxXYZ bb, XYZ ptGlobal, double tol = 0.0)
        {
            XYZ xyz = bb.Transform.Inverse.OfPoint(ptGlobal);
            return xyz.X.IsBetweenEqual(bb.Min.X, bb.Max.X, tol) && xyz.Y.IsBetweenEqual(bb.Min.Y, bb.Max.Y, tol) && xyz.Z.IsBetweenEqual(bb.Min.Z, bb.Max.Z, tol);
        }

        public static bool IsBoxContained(this BoundingBoxXYZ bb, BoundingBoxXYZ bbContained)
        {
            return bb.IsPointInBox(bbContained.Min, 0.0) && bb.IsPointInBox(bbContained.Max, 0.0);
        }

        public static bool Intersects(this BoundingBoxXYZ bb, BoundingBoxXYZ someOtherbb, double tolerance = 0.0001)
        {
            if (bb == null || someOtherbb == null)
            {
                return false;
            }
            XYZ[] boundaryPoints = bb.GetBoundaryPoints();
            if (boundaryPoints == null)
            {
                return false;
            }
            foreach (XYZ ptGlobal in boundaryPoints)
            {
                if (someOtherbb.IsPointInBox(ptGlobal, tolerance))
                {
                    return true;
                }
            }
            return false;
        }

        public static XYZ[] GetBoundaryPoints(this BoundingBoxXYZ bb)
        {
            if (bb == null)
            {
                return null;
            }
            XYZ min = bb.Min;
            XYZ max = bb.Max;
            return new XYZ[]
            {
                bb.Transform.OfPoint(new XYZ(min.X, min.Y, min.Z)),
                bb.Transform.OfPoint(new XYZ(min.X, max.Y, min.Z)),
                bb.Transform.OfPoint(new XYZ(min.X, max.Y, max.Z)),
                bb.Transform.OfPoint(new XYZ(min.X, min.Y, max.Z)),
                bb.Transform.OfPoint(new XYZ(max.X, min.Y, min.Z)),
                bb.Transform.OfPoint(new XYZ(max.X, max.Y, min.Z)),
                bb.Transform.OfPoint(new XYZ(max.X, max.Y, max.Z)),
                bb.Transform.OfPoint(new XYZ(max.X, min.Y, max.Z))
            };
        }

        public static void Inflate(this BoundingBoxXYZ bb, double inflateQuantityInFeet, bool inflateX = true, bool inflateY = true, bool inflateZ = true)
        {
            if (bb == null)
            {
                return;
            }
            XYZ xyz = new XYZ(inflateX ? inflateQuantityInFeet : 0.0, inflateY ? inflateQuantityInFeet : 0.0, inflateZ ? inflateQuantityInFeet : 0.0);
            bb.Max += xyz;
            bb.Min -= xyz;
        }

        public static Solid SolidFromBoundingbox(this BoundingBoxXYZ bb)
        {
            var min = bb.Min;
            var max = bb.Max;
            var a = min;
            var b = new XYZ(min.X, max.Y, min.Z);
            var c = new XYZ(max.X, max.Y, min.Z);
            var d = new XYZ(max.X, min.Y, min.Z);
            var ab = a.LineByPoints(b);
            var bc = b.LineByPoints(c);
            var cd = c.LineByPoints(d);
            var da = d.LineByPoints(a);
            var cl = new CurveLoop();
            cl.Append(ab);
            cl.Append(bc);
            cl.Append(cd);
            cl.Append(da);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { cl }, XYZ.BasisZ, bb.Height());
        }
    }
}