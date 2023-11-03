using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils.DocumentUtils;
using HcBimUtils.MoreLinq;

namespace HcBimUtils.RebarUtils
{
    public static class RebarUtils
    {
        public static Line GetDistributionPath(this Rebar rebar)
        {
#if Version2017
         return rebar.GetDistributionPath();
#else
            return rebar.GetShapeDrivenAccessor().GetDistributionPath();
#endif
        }
        public static double GetArrayLength(this Rebar rebar)
        {
#if Version2017
         return rebar.ArrayLength;
#else
            return rebar.GetShapeDrivenAccessor().ArrayLength;
#endif
        }
        public static XYZ GetNormal(this Rebar rebar)
        {
#if Version2017
         return rebar.Normal;
#else
            return rebar.GetShapeDrivenAccessor().Normal;
#endif
        }
        public static RebarBarType GetRebarBarType(this Rebar rebar)
        {
            Document doc = rebar.Document;
            return doc.GetElement(rebar.GetTypeId()) as RebarBarType;
        }


        public static double BarDiameter(this Rebar rebar)
        {
#if R19 || R20 || R21 || R22
            return rebar.GetRebarBarType().BarDiameter();

#else
         return rebar.GetRebarBarType().BarDiameter();
#endif
        }

        public static double BarDiameter(this RebarBarType rebar)
        {
#if R19 || R20 || R21 || R22
            return rebar.BarDiameter;

#else
         return rebar.BarNominalDiameter;
#endif
        }

        public static double BarDiameter(this RebarBendData rebar)
        {
#if R19 || R20 || R21 || R22
            return rebar.BarDiameter;

#else
         return rebar.BarNominalDiameter;
#endif
        }

        public static double BarDiameter(this RebarBarType type, double diameter)
        {
#if R19 || R20 || R21 || R22
            return type.BarDiameter;
#else
         type.BarModelDiameter = diameter;
         return type.BarNominalDiameter = diameter;

#endif
        }
        public static RebarStyle GetRebarStyle(this Rebar rebar)
        {
            RebarStyle style = RebarStyle.Standard;
            Parameter stylePara = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_STYLE);
            if (stylePara != null)
            {
                style = (RebarStyle)stylePara.AsInteger();
            }
            return style;
        }

        public static AreaReinforcement CreateAreaReinforcement(
           Document doc,
           Element hostElement,
           XYZ majorDirection,
           ElementId areaTypeId,
           bool topIsCreated,
           ElementId topMajorBarTypeId,
           ElementId topMajorHookTypeId,
           double topMajorSpacing,
           ElementId topMinorBarTypeId,
           ElementId topMinorHookTypeId,
           double topMinorSpacing,
           bool bottomIsCreated,
           ElementId bottomMajorBarTypeId,
           ElementId bottomMajorHookTypeId,
           double bottomMajorSpacing,
           ElementId bottomMinorBarTypeId,
           ElementId bottomMinorHookTypeId,
           double bottomMinorSpacing)
        {
            AreaReinforcement areaReinforcement = AreaReinforcement.Create(
               doc,
               hostElement,
               majorDirection,
               areaTypeId,
               topMajorBarTypeId,
               topMajorHookTypeId);

            Parameter topMajorDirectionPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_ACTIVE_TOP_DIR_1);
            Parameter topMajorBarTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_BAR_TYPE_TOP_DIR_1);
            Parameter topMajorHookTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_HOOK_TYPE_TOP_DIR_1);
            Parameter topMajorSpacingPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_1);
            Parameter topMajorNumberOfLines = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_NUMBER_OF_LINES_TOP_DIR_1);

            Parameter topMinorDirectionPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_ACTIVE_TOP_DIR_2);
            Parameter topMinorBarTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_BAR_TYPE_TOP_DIR_2);
            Parameter topMinorHookTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_HOOK_TYPE_TOP_DIR_2);
            Parameter topMinorSpacingPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_2);
            Parameter topMinorNumberOfLines = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_NUMBER_OF_LINES_TOP_DIR_2);

            Parameter bottomMajorDirectionPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_ACTIVE_BOTTOM_DIR_1);
            Parameter bottomMajorBarTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_BAR_TYPE_BOTTOM_DIR_1);
            Parameter bottomMajorHookTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_HOOK_TYPE_BOTTOM_DIR_1);
            Parameter bottomMajorSpacingPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_1);
            Parameter bottomMajorNumberOfLines = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_NUMBER_OF_LINES_BOTTOM_DIR_1);

            Parameter bottomMinorDirectionPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_ACTIVE_BOTTOM_DIR_2);
            Parameter bottomMinorBarTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_BAR_TYPE_BOTTOM_DIR_2);
            Parameter bottomMinorHookTypePara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_HOOK_TYPE_BOTTOM_DIR_2);
            Parameter bottomMinorSpacingPara = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_2);
            Parameter bottomMinorNumberOfLines = areaReinforcement.get_Parameter(BuiltInParameter.REBAR_SYSTEM_NUMBER_OF_LINES_BOTTOM_DIR_2);

            topMajorDirectionPara.SetParameter(Convert.ToInt32(topIsCreated));
            topMajorBarTypePara.SetParameter(topMajorBarTypeId);
            topMajorHookTypePara.SetParameter(topMajorHookTypeId);
            topMajorSpacingPara.SetParameter(topMajorSpacing);

            topMinorDirectionPara.SetParameter(Convert.ToInt32(topIsCreated));
            topMinorBarTypePara.SetParameter(topMinorBarTypeId);
            topMinorHookTypePara.SetParameter(topMinorHookTypeId);
            topMinorSpacingPara.SetParameter(topMinorSpacing);

            bottomMajorDirectionPara.SetParameter(Convert.ToInt32(bottomIsCreated));
            bottomMajorBarTypePara.SetParameter(bottomMajorBarTypeId);
            bottomMajorHookTypePara.SetParameter(bottomMajorHookTypeId);
            bottomMajorSpacingPara.SetParameter(bottomMajorSpacing);

            bottomMinorDirectionPara.SetParameter(Convert.ToInt32(bottomIsCreated));
            bottomMinorBarTypePara.SetParameter(bottomMinorBarTypeId);
            bottomMinorHookTypePara.SetParameter(bottomMinorHookTypeId);
            bottomMinorSpacingPara.SetParameter(bottomMinorSpacing);

            return areaReinforcement;
        }
        public static void SetPartition(List<Rebar> rebars, string partition)
        {
            foreach (Rebar rebar in rebars)
            {
                Parameter partitionPara = rebar.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM);
                partitionPara.Set(partition);
            }
        }
        public static List<Rebar> CopyRebars(List<Rebar> hostRebars, XYZ translation)
        {
            Document doc = hostRebars.FirstOrDefault().Document;
            List<Rebar> copyRebars = new List<Rebar>();

            var copyIds = ElementTransformUtils.CopyElements(doc, hostRebars.Select(x => x.Id).ToList(), translation);

            foreach (ElementId id in copyIds)
            {
                if (doc.GetElement(id) is Rebar rebar)
                {
                    copyRebars.Add(rebar);
                }
            }

            return copyRebars;
        }
        public static Rebar CopyRebar(Rebar hostRebar, XYZ translation)
        {
            Document doc = hostRebar.Document;
            Rebar copyRebar = null;
            var copyIds = ElementTransformUtils.CopyElement(doc, hostRebar.Id, translation);

            foreach (ElementId id in copyIds)
            {
                if (doc.GetElement(id) is Rebar rebar)
                {
                    copyRebar = rebar;
                    break;
                }
            }

            return copyRebar;
        }
        public static void SetSolidRebarIn3DView(View view, List<Rebar> rebars)
        {
            foreach (Rebar rebar in rebars)
            {
                if (rebar != null)
                {
                    rebar.SetUnobscuredInView(view, true);

                    if (view is View3D)
                    {
#if R20 || R21 || R22
                        rebar.SetSolidInView(view as View3D, true);
#endif
                    }
                }
            }
        }
        public static List<Rebar> GetAllRebarsInHost(this Element host)
        {
            List<Rebar> rebars = new List<Rebar>();

            if (host.IsValidElement())
            {
                RebarHostData rebarHost = RebarHostData.GetRebarHostData(host);
                if (rebarHost.IsValidHost())
                {
                    rebars = rebarHost.GetRebarsInHost().ToList();
                }
            }

            return rebars;
        }
        public static List<Curve> GetRebarCurves(this List<Rebar> rebars)
        {
            List<Curve> curves = new List<Curve>();

            int n, nElements = 0, nCurves = 0;

            foreach (Rebar rebar in rebars)
            {
                ++nElements;

                n = rebar.NumberOfBarPositions;

                nCurves += n;

                for (int i = 0; i < n; ++i)
                {
                    IList<Curve> centerlineCurves = rebar.GetCenterlineCurves(true, false, false, MultiplanarOption.IncludeAllMultiplanarCurves, i);

                    // Move the curves to their position.
#if Version2017
               Transform trf = rebar.GetBarPositionTransform(i);

               foreach (Curve c in centerlineCurves)
               {
                  curves.Add(c.CreateTransformed(trf));
               }
#else
                    if (rebar.IsRebarShapeDriven())
                    {
                        RebarShapeDrivenAccessor accessor = rebar.GetShapeDrivenAccessor();

                        Transform trf = accessor.GetBarPositionTransform(i);

                        foreach (Curve c in centerlineCurves)
                        {
                            curves.Add(c.CreateTransformed(trf));
                        }
                    }
                    else
                    {
                        // This is a Free Form Rebar

                        foreach (Curve c in centerlineCurves)
                        {
                            curves.Add(c);
                        }
                    }
#endif
                }
            }
            return curves;
        }

        public static List<Curve> GetRebarCurves(this Rebar rebar)
        {
            List<Curve> curves = new List<Curve>();

            int n = rebar.NumberOfBarPositions;

            for (int i = 0; i < n; ++i)
            {
                IList<Curve> centerlineCurves = rebar.GetCenterlineCurves(true, false, false, MultiplanarOption.IncludeAllMultiplanarCurves,
                    i);

                // Move the curves to their position.
#if Version2017
            Transform trf = rebar.GetBarPositionTransform(i);

            foreach (Curve c in centerlineCurves)
            {
               curves.Add(c.CreateTransformed(trf));
            }
#else

                if (rebar.IsRebarShapeDriven())
                {
                    RebarShapeDrivenAccessor accessor = rebar.GetShapeDrivenAccessor();

                    Transform trf = accessor.GetBarPositionTransform(i);

                    foreach (Curve c in centerlineCurves)
                    {
                        curves.Add(c.CreateTransformed(trf));
                    }
                }
                else
                {
                    // This is a Free Form Rebar

                    foreach (Curve c in centerlineCurves)
                    {
                        curves.Add(c);
                    }
                }
#endif
            }

            return curves;
        }

        public static bool IsStirrupOrTie(this Rebar rebar)
        {
            return !IsStandardRebar(rebar);
        }

        public static bool IsStandardRebar(this Rebar rebar)
        {
            var styleParam = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_STYLE);
            //return styleParam.AsInteger() == 0;
            return !styleParam.AsValueString().Contains("Tie");
        }

        public static RebarShape GetRebarShape(this Rebar rebar)
        {
#if Version2017
         return rebar.RebarShapeId.ToElement() as RebarShape;
#elif Version2018
         return rebar.GetShapeId().ToElement() as RebarShape;
#else
            Document doc = rebar.Document;
            try
            {
                var shapeId = rebar.GetShapeId();

                return doc.GetElement(shapeId) as RebarShape;
            }
            catch
            {
                try
                {
                    return doc.GetElement(rebar.GetAllRebarShapeIds().FirstOrDefault()) as RebarShape;
                }
                catch
                {
                }
            }
            return null;
#endif
        }


        public static RebarShape GetRebarShape(this RebarInSystem rebar)
        {
            return rebar.RebarShapeId.ToElement() as RebarShape;
        }

        public static XYZ RebarCenterPoint(this Rebar rebar)
        {
            IList<Curve> centerlineCurves = rebar.GetCenterlineCurves(true, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);
            int num = 0;
            Curve curve1 = centerlineCurves.Where(x => x.Direction().IsHorizontal()).OrderByDescending(x => x.Length).FirstOrDefault();
            foreach (Curve curve2 in centerlineCurves)
            {
                if (num == 0)
                {
                    curve1 = curve2;
                    num = 1;
                }
                if (num != 0 && curve2.Length > curve1.Length)
                    curve1 = curve2;
            }
            return (curve1.GetEndPoint(0) + curve1.GetEndPoint(1)) / 2;
        }

        public static XYZ RebarCenterPointOfMaxCurve(this Rebar rebar)
        {
            IList<Curve> centerlineCurves = rebar.GetCenterlineCurves(true, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);
            Curve curve1 = centerlineCurves.MaxBy(x => x.Length).First();
            return curve1.Midpoint();

        }

        public static bool IsContainRebar(this Rebar rebar, List<Rebar> listRebar)
        {
            bool flag = false;
            foreach (Rebar element in listRebar)
            {
                if (element.Id == rebar.Id)
                    flag = true;
            }
            return flag;
        }

        public static IList<Element> GetAllReinforcements(this Document rvtDoc)
        {
            return GetAllRebars(rvtDoc)
                .Concat(GetAllWWMs(rvtDoc))
                .Concat(GetAllRebarInSystemsReinforcements(rvtDoc))
                .Concat(GetAllRebarContainers(rvtDoc)).ToList();
        }

        public static List<Rebar> GetRebarsFromElementIds(this List<ElementId> elements)
        {
            List<Rebar> list = new List<Rebar>();
            foreach (ElementId elementId in elements)
            {
                list.Add(AC.Document.GetElement(elementId) as Rebar);
            }
            return list;
        }

        //----------------------------------------------------
        public static IList<Element> GetAllRebars(Document rvtDoc)
        {
            return new FilteredElementCollector(rvtDoc, AC.ActiveView.Id).OfClass(typeof(Rebar)).ToElements();
        }

        //----------------------------------------------------
        public static IList<Element> GetAllWWMs(Document rvtDoc)
        {
            return new FilteredElementCollector(rvtDoc, rvtDoc.ActiveView.Id).OfClass(typeof(FabricSheet)).ToElements();
        }

        //----------------------------------------------------
        public static IList<Element> GetAllRebarInSystemsReinforcements(Document rvtDoc)
        {
            return new FilteredElementCollector(rvtDoc, rvtDoc.ActiveView.Id).OfClass(typeof(RebarInSystem)).ToElements();
        }

        //----------------------------------------------------
        public static IList<Element> GetAllRebarContainers(Document rvtDoc)
        {
            return new FilteredElementCollector(rvtDoc, rvtDoc.ActiveView.Id).OfClass(typeof(RebarContainer)).ToElements();
        }
        public static IList<Curve> ComputeRebarDrivingCurves(this Rebar rebar)
        {
#if Version2017
         return rebar.ComputeDrivingCurves();
#else
            return rebar.GetShapeDrivenAccessor().ComputeDrivingCurves();
#endif
        }

        public static void SetRebarLayoutAsFixedNumber(this Rebar rebar, int number, double arrayLength, bool barsOnNormalSide,
            bool includeFirstBar,
            bool includeLastBar)
        {
            if (rebar != null && number > 0 && arrayLength > 0)
            {
#if Version2017
            rebar.SetLayoutAsFixedNumber(number, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#else
                rebar.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(number, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#endif
                rebar.IncludeFirstBar = includeFirstBar;
                rebar.IncludeLastBar = includeLastBar;
            }
        }

        public static void SetRebarLayoutAsMaximumSpacing(this Rebar rebar, double spacing, double arrayLength, bool barsOnNormalSide,
            bool includeFirstBar,
            bool includeLastBar)
        {
            if (rebar != null && spacing > 0 && arrayLength > 0)
            {
#if Version2017
            rebar.SetLayoutAsMaximumSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#else
                rebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#endif
                rebar.IncludeFirstBar = includeFirstBar;
                rebar.IncludeLastBar = includeLastBar;
            }
        }

        public static void SetRebarLayoutAsMinimumClearSpacing(this Rebar rebar, double spacing, double arrayLength, bool barsOnNormalSide,
            bool includeFirstBar,
            bool includeLastBar)
        {
            if (rebar != null && spacing > 0 && arrayLength > 0)
            {
#if Version2017
            rebar.SetLayoutAsMinimumClearSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#else
                rebar.GetShapeDrivenAccessor().SetLayoutAsMinimumClearSpacing(spacing, arrayLength, barsOnNormalSide, includeFirstBar, includeLastBar);
#endif
                rebar.IncludeFirstBar = includeFirstBar;
                rebar.IncludeLastBar = includeLastBar;
            }
        }

        public static void SetRebarLayoutAsNumberWithSpacing(this Rebar rebar, int number, double spacing, bool barsOnNormalSide,
            bool includeFirstBar,
            bool includeLastBar)
        {
            if (rebar != null && number > 0 && spacing > 0)
            {
#if Version2017
            rebar.SetLayoutAsNumberWithSpacing(number, spacing, barsOnNormalSide, includeFirstBar, includeLastBar);
#else
                rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(number, spacing, barsOnNormalSide, includeFirstBar, includeLastBar);
#endif
                rebar.IncludeFirstBar = includeFirstBar;
                rebar.IncludeLastBar = includeLastBar;
            }
        }

        public static void SetRebarLayoutAsSingle(this Rebar rebar)
        {
#if Version2017
         rebar.SetLayoutAsSingle();
#else
            rebar.GetShapeDrivenAccessor().SetLayoutAsSingle();
#endif
        }

        public static void RebarScaleToBox(this Rebar rebar, XYZ origin, XYZ xVec, XYZ yVec)
        {
#if Version2017
         rebar.ScaleToBox(origin, xVec, yVec);
#else
            rebar.GetShapeDrivenAccessor().ScaleToBox(origin, xVec, yVec);
#endif
        }

        public static XYZ RebarNormal(this Rebar rebar)
        {
#if Version2017
         return rebar.Normal;
#else
            try
            {
                return rebar.GetShapeDrivenAccessor().Normal;
            }
            catch (Exception)
            {
                return rebar.GetFreeFormAccessor().GetCustomDistributionPath().First().Direction();
            }

#endif
        }

        public static double RebarArrayLength(this Rebar rebar)
        {
#if Version2017
         return rebar.ArrayLength;
#else
            return rebar.GetShapeDrivenAccessor().ArrayLength;
#endif
        }

        public static bool RebarBarOnNormalSide(this Rebar rebar)
        {
#if Version2017
         return rebar.BarsOnNormalSide;
#else
            return rebar.GetShapeDrivenAccessor().BarsOnNormalSide;
#endif
        }

        public static Transform GetRebarPositionTransform(this Rebar rebar, int i)
        {
#if Version2017
         return rebar.GetBarPositionTransform(i);
#else
            return rebar.GetShapeDrivenAccessor().GetBarPositionTransform(i);
#endif
        }
    }
}