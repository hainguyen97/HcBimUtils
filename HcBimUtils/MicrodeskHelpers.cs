using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace HcBimUtils
{
    public static class MicrodeskHelpers
    {
        public static string log()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log.txt";
        }

        public static bool IsNumeric(string str)
        {
            bool result = true;
            try
            {
                int.Parse(str);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        //public static double GetSizeFromString(string input)
        //{
        //   string text = input.Split(new char[]
        //   {
        //      '['
        //   })[1];
        //   text = text.Replace(" ]", "");
        //   text = text.Replace(" ", "");
        //   uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
        //   if (num <= 923577301U)
        //   {
        //      if (num <= 501951850U)
        //      {
        //         if (num <= 401286136U)
        //         {
        //            if (num != 334175660U)
        //            {
        //               if (num == 401286136U)
        //               {
        //                  if (text == "14")
        //                  {
        //                     return 14.0;
        //                  }
        //               }
        //            }
        //            else if (text == "18")
        //            {
        //               return 18.0;
        //            }
        //         }
        //         else if (num != 434841374U)
        //         {
        //            if (num != 468396612U)
        //            {
        //               if (num == 501951850U)
        //               {
        //                  if (text == "12")
        //                  {
        //                     return 12.0;
        //                  }
        //               }
        //            }
        //            else if (text == "10")
        //            {
        //               return 10.0;
        //            }
        //         }
        //         else if (text == "16")
        //         {
        //            return 16.0;
        //         }
        //      }
        //      else if (num <= 856466825U)
        //      {
        //         if (num != 822911587U)
        //         {
        //            if (num == 856466825U)
        //            {
        //               if (text == "6")
        //               {
        //                  return 6.0;
        //               }
        //            }
        //         }
        //         else if (text == "4")
        //         {
        //            return 4.0;
        //         }
        //      }
        //      else if (num != 873244444U)
        //      {
        //         if (num != 906799682U)
        //         {
        //            if (num == 923577301U)
        //            {
        //               if (text == "2")
        //               {
        //                  return 2.0;
        //               }
        //            }
        //         }
        //         else if (text == "3")
        //         {
        //            return 3.0;
        //         }
        //      }
        //      else if (text == "1")
        //      {
        //         return 1.0;
        //      }
        //   }
        //   else if (num <= 2003613789U)
        //   {
        //      if (num <= 1902948075U)
        //      {
        //         if (num != 1024243015U)
        //         {
        //            if (num == 1902948075U)
        //            {
        //               if (text == "1-1/2")
        //               {
        //                  return 1.5;
        //               }
        //            }
        //         }
        //         else if (text == "8")
        //         {
        //            return 8.0;
        //         }
        //      }
        //      else if (num != 1933914273U)
        //      {
        //         if (num != 1967469511U)
        //         {
        //            if (num == 2003613789U)
        //            {
        //               if (text == "1-1/4")
        //               {
        //                  return 1.25;
        //               }
        //            }
        //         }
        //         else if (text == "1/4")
        //         {
        //            return 0.25;
        //         }
        //      }
        //      else if (text == "1/2")
        //      {
        //         return 0.5;
        //      }
        //   }
        //   else if (num <= 2381486463U)
        //   {
        //      if (num != 2314375987U)
        //      {
        //         if (num == 2381486463U)
        //         {
        //            if (text == "20")
        //            {
        //               return 20.0;
        //            }
        //         }
        //      }
        //      else if (text == "24")
        //      {
        //         return 24.0;
        //      }
        //   }
        //   else if (num != 2415041701U)
        //   {
        //      if (num != 3276008345U)
        //      {
        //         if (num == 3984786090U)
        //         {
        //            if (text == "2-1/2")
        //            {
        //               return 2.5;
        //            }
        //         }
        //      }
        //      else if (text == "3/4")
        //      {
        //         return 0.75;
        //      }
        //   }
        //   else if (text == "22")
        //   {
        //      return 22.0;
        //   }
        //   return 1.0;
        //}

        public static string[] ConvertToStringArray(Array values)
        {
            string[] array = new string[values.Length];
            int num = 0;
            for (int i = values.GetLowerBound(0); i <= values.GetUpperBound(0); i++)
            {
                for (int j = values.GetLowerBound(1); j <= values.GetUpperBound(1); j++)
                {
                    if (values.GetValue(i, j) == null)
                    {
                        array[num] = "";
                    }
                    else
                    {
                        array[num] = values.GetValue(i, j).ToString();
                    }
                    num++;
                }
            }
            return array;
        }

        public static XYZ PerpIntersection(XYZ p0, XYZ p1, XYZ pX)
        {
            double num = (pX.X - p0.X) * (p1.X - p0.X) + (pX.Y - p0.Y) * (p1.Y - p0.Y) + (pX.Z - p0.Z) * (p1.Z - p0.Z);
            double num2 = p0.DistanceTo(p1);
            num /= num2 * num2;
            return new XYZ(p0.X + num * (p1.X - p0.X), p0.Y + num * (p1.Y - p0.Y), p0.Z + num * (p1.Z - p0.Z));
        }

        public static XYZ Midpoint(XYZ p0, XYZ p1)
        {
            return new XYZ((p0.X + p1.X) / 2.0, (p0.Y + p1.Y) / 2.0, (p0.Z + p1.Z) / 2.0);
        }

        public static XYZ Centroid(XYZ p0, XYZ p1, XYZ p2)
        {
            return new XYZ((p0.X + p1.X + p2.X) / 3.0, (p0.Y + p1.Y + p2.Y) / 3.0, (p0.Z + p1.Z + p2.Z) / 3.0);
        }

        public static double dotProd(XYZ vector1, XYZ vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public static XYZ IntersectionTwoVectors(XYZ upstreamBranch, XYZ downstreamBranch, XYZ downstreamMain, XYZ upstreamMain)
        {
            XYZ xyz = new XYZ(upstreamMain.X - downstreamMain.X, upstreamMain.Y - downstreamMain.Y, upstreamMain.Z - downstreamMain.Z);
            XYZ xyz2 = new XYZ(upstreamBranch.X - downstreamBranch.X, upstreamBranch.Y - downstreamBranch.Y, upstreamBranch.Z - downstreamBranch.Z);
            XYZ source = new XYZ(downstreamMain.X - downstreamBranch.X, downstreamMain.Y - downstreamBranch.Y, downstreamMain.Z - downstreamBranch.Z);
            double num = xyz.DotProduct(xyz);
            double num2 = xyz.DotProduct(xyz2);
            double num3 = xyz2.DotProduct(xyz2);
            double num4 = xyz.DotProduct(source);
            double num5 = xyz2.DotProduct(source);
            double num6 = num * num3 - num2 * num2;
            double num7;
            double num8;
            if (num6 < 1E-08)
            {
                num7 = 0.0;
                if (num2 > num3)
                {
                    num8 = num4 / num2;
                }
                else
                {
                    num8 = num5 / num3;
                }
            }
            else
            {
                num7 = (num2 * num5 - num3 * num4) / num6;
                num8 = (num * num5 - num2 * num4) / num6;
            }
            XYZ xyz3 = new XYZ(downstreamMain.X + num7 * xyz.X, downstreamMain.Y + num7 * xyz.Y, downstreamMain.Z + num7 * xyz.Z);
            XYZ xyz4 = new XYZ(downstreamBranch.X + num8 * xyz2.X, downstreamBranch.Y + num8 * xyz2.Y, downstreamBranch.Z + num8 * xyz2.Z);
            return new XYZ((xyz4.X - xyz3.X) / 2.0 + xyz3.X, (xyz4.Y - xyz3.Y) / 2.0 + xyz3.Y, (xyz4.Z - xyz3.Z) / 2.0 + xyz3.Z);
        }

        public static XYZ GetIntersection(Line line1, Line line2)
        {
            IntersectionResultArray intersectionResultArray;
            if (line1.Intersect(line2, out intersectionResultArray) != SetComparisonResult.Overlap)
            {
                throw new InvalidOperationException("Input lines did not intersect.");
            }
            if (intersectionResultArray == null || intersectionResultArray.Size != 1)
            {
                throw new InvalidOperationException("Could not extract line intersection point.");
            }
            return intersectionResultArray.get_Item(0).XYZPoint;
        }

        public static XYZ ProjectPointOnPlane(XYZ planeNormal, XYZ anyPointOnPlane, XYZ pointToProject)
        {
            double num = SignedDistancePlanePoint(planeNormal, anyPointOnPlane, pointToProject);
            num *= -1.0;
            XYZ right = SetVectorLength(planeNormal, num);
            return pointToProject + right;
        }

        public static double SignedDistancePlanePoint(XYZ planeNormal, XYZ planePoint, XYZ point)
        {
            return planeNormal.X * (point.X - planePoint.X) + planeNormal.Y * (point.Y - planePoint.Y) + planeNormal.Z * (point.Z - planePoint.Z);
        }

        public static XYZ SetVectorLength(XYZ vector, double size)
        {
            return vector.Normalize() * size;
        }

        public static XYZ NormalThreePoints(XYZ p, XYZ q, XYZ r)
        {
            XYZ xyz = q - p;
            XYZ source = r - p;
            return xyz.CrossProduct(source);
        }

        public static double Average(XYZ point)
        {
            return (point.X + point.Y + point.Z) / 3.0;
        }

        public static XYZ FirstPoint(List<XYZ> pList)
        {
            XYZ xyz = pList.ToArray()[0];
            foreach (XYZ xyz2 in pList)
            {
                if (Average(xyz) < Average(xyz2))
                {
                    xyz = xyz2;
                }
            }
            return xyz;
        }

        public static Element GetFamilyType(string name, BuiltInCategory bic, Document doc)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfCategory(bic);
            filteredElementCollector.OfClass(typeof(ElementType));
            foreach (Element element in filteredElementCollector)
            {
                if (element.Name == name)
                {
                    return element;
                }
            }
            return null;
        }

        public static Element GetLevelByName(string name, Document doc)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfCategory(BuiltInCategory.OST_Levels);
            filteredElementCollector.OfClass(typeof(Level));
            foreach (Element element in filteredElementCollector)
            {
                if (element.Name == name)
                {
                    return element;
                }
            }
            return null;
        }

        public static ElementId GetLevel(Document doc, double z)
        {
            Level level = null;
            ElementId result = null;
            if (doc.ActiveView.GenLevel != null)
            {
                result = doc.ActiveView.GenLevel.Id;
            }
            else
            {
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).OfClass(typeof(Level));
                double num = double.MaxValue;
                foreach (Element element in filteredElementCollector)
                {
                    Level level2 = (Level)element;
                    try
                    {
                        if (level2.Elevation <= z)
                        {
                            if (z - level2.Elevation < num)
                            {
                                num = z - level2.Elevation;
                                level = level2;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (level == null)
                {
                    level = filteredElementCollector.FirstElement() as Level;
                }
                result = level.Id;
            }
            return result;
        }

        public static MechanicalSystemType GetMechanicalSystem(Document doc, Connector connector)
        {
            if (connector.Domain != Domain.DomainHvac)
            {
                return null;
            }
            try
            {
                ElementId typeId = (connector.MEPSystem as MechanicalSystem).GetTypeId();
                return doc.GetElement(typeId) as MechanicalSystemType;
            }
            catch
            {
            }
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).OfClass(typeof(MechanicalSystemType));
            connector.DuctSystemType.ToString();
            if (connector.DuctSystemType == DuctSystemType.UndefinedSystemType || connector.DuctSystemType == DuctSystemType.Fitting || connector.DuctSystemType == DuctSystemType.Global)
            {
                return filteredElementCollector.First<Element>() as MechanicalSystemType;
            }
            if (connector.DuctSystemType == DuctSystemType.SupplyAir)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element = enumerator.Current;
                        MechanicalSystemType mechanicalSystemType = (MechanicalSystemType)element;
                        string text = mechanicalSystemType.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text.Contains("Supply Air") || text.Contains("Zuluft") || text.Contains("Soufflage") || text.Contains("Aria di mandata") || text.Contains("給気") || text.Contains("Powietrze nawiewane") || text.Contains("Приточный воздух") || text.Contains("공급 공기") || text.Contains("Suministro de aire") || text.Contains("Suprimento de ar") || text.Contains("送风") || text.Contains("進氣") || text.Contains("Přívod vzduchu"))
                        {
                            return mechanicalSystemType;
                        }
                    }
                    goto IL_3D9;
                }
            }
            if (connector.DuctSystemType == DuctSystemType.ReturnAir)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element2 = enumerator.Current;
                        MechanicalSystemType mechanicalSystemType2 = (MechanicalSystemType)element2;
                        string text2 = mechanicalSystemType2.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text2.Contains("Return Air") || text2.Contains("Umluft") || text2.Contains("Reprise") || text2.Contains("Aria di ritorno") || text2.Contains("還気") || text2.Contains("Powietrze recyrkulac.") || text2.Contains("Рециркулирующий воздух") || text2.Contains("순환 공기") || text2.Contains("Aire de retorno") || text2.Contains("Ar de retorno") || text2.Contains("回风") || text2.Contains("回氣") || text2.Contains("Zpětný vzduch"))
                        {
                            return mechanicalSystemType2;
                        }
                    }
                    goto IL_3D9;
                }
            }
            if (connector.DuctSystemType == DuctSystemType.ExhaustAir)
            {
                foreach (Element element3 in filteredElementCollector)
                {
                    MechanicalSystemType mechanicalSystemType3 = (MechanicalSystemType)element3;
                    string text3 = mechanicalSystemType3.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                    if (text3.Contains("Exhaust Air") || text3.Contains("Abluft") || text3.Contains("Extraction d'air") || text3.Contains("Aria di scarico") || text3.Contains("排気") || text3.Contains("Powietrze zwracane") || text3.Contains("Отработанный воздух") || text3.Contains("배기") || text3.Contains("Aire viciado") || text3.Contains("Ar de exaustão") || text3.Contains("排风") || text3.Contains("排出氣") || text3.Contains("Odváděný vzduch"))
                    {
                        return mechanicalSystemType3;
                    }
                }
            }
        IL_3D9:
            return filteredElementCollector.First<Element>() as MechanicalSystemType;
        }

        public static PipingSystemType GetPipeSystem(Document doc, Connector connector)
        {
            if (connector.Domain != Domain.DomainPiping)
            {
                return null;
            }
            try
            {
                ElementId typeId = (connector.MEPSystem as PipingSystem).GetTypeId();
                return doc.GetElement(typeId) as PipingSystemType;
            }
            catch
            {
            }
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
            connector.PipeSystemType.ToString();
            if (connector.PipeSystemType == PipeSystemType.UndefinedSystemType || connector.PipeSystemType == PipeSystemType.Fitting || connector.PipeSystemType == PipeSystemType.Global)
            {
                return filteredElementCollector.First<Element>() as PipingSystemType;
            }
            if (connector.PipeSystemType == PipeSystemType.SupplyHydronic)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element = enumerator.Current;
                        PipingSystemType pipingSystemType = (PipingSystemType)element;
                        string text = pipingSystemType.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text.Contains("Hydronic Supply") || text.Contains("Vorlauf") || text.Contains("Alimentation hydraulique") || text.Contains("Mandata di sistema idronico") || text.Contains("温水循環(往)") || text.Contains("Zasilanie wody") || text.Contains("Приточная жидкость") || text.Contains("순환수 공급") || text.Contains("Suministro hidrónico") || text.Contains("Suprimento hidrônico") || text.Contains("循环供水") || text.Contains("循環供水") || text.Contains("Přívod teplé vody"))
                        {
                            return pipingSystemType;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.ReturnHydronic)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element2 = enumerator.Current;
                        PipingSystemType pipingSystemType2 = (PipingSystemType)element2;
                        string text2 = pipingSystemType2.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text2.Contains("Hydronic Return") || text2.Contains("Rücklauf") || text2.Contains("Retour hydraulique") || text2.Contains("Ritorno di sistema idronico") || text2.Contains("温水循環(還)") || text2.Contains("Zwrot wody") || text2.Contains("Обратная жидкость") || text2.Contains("순환수 순환") || text2.Contains("Retorno hidrónico") || text2.Contains("Retorno hidrônico") || text2.Contains("循环回水") || text2.Contains("循環回水") || text2.Contains("Zpětné vedení teplé vody"))
                        {
                            return pipingSystemType2;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.Sanitary)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element3 = enumerator.Current;
                        PipingSystemType pipingSystemType3 = (PipingSystemType)element3;
                        string text3 = pipingSystemType3.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text3.Contains("Sanitary") || text3.Contains("Abwasser") || text3.Contains("Sanitaire") || text3.Contains("Acque reflue") || text3.Contains("排水") || text3.Contains("Sanitarny") || text3.Contains("Канализация") || text3.Contains("위생") || text3.Contains("Sanitario") || text3.Contains("Sanitário") || text3.Contains("卫生设备") || text3.Contains("衛生設施") || text3.Contains("Sanitární"))
                        {
                            return pipingSystemType3;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.DomesticHotWater)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element4 = enumerator.Current;
                        PipingSystemType pipingSystemType4 = (PipingSystemType)element4;
                        string text4 = pipingSystemType4.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text4.Contains("Domestic Hot Water") || text4.Contains("Warmwasser") || text4.Contains("Eau chaude sanitaire") || text4.Contains("Acqua calda sanitaria") || text4.Contains("屋内給湯") || text4.Contains("Domowa woda gorąca") || text4.Contains("горячее водоснабжение (внутренние сети)") || text4.Contains("주택용 온수") || text4.Contains("Agua caliente doméstica") || text4.Contains("Água quente residencial") || text4.Contains("家用热水") || text4.Contains("家用熱水") || text4.Contains("Teplá voda v domácnosti"))
                        {
                            return pipingSystemType4;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.DomesticColdWater)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element5 = enumerator.Current;
                        PipingSystemType pipingSystemType5 = (PipingSystemType)element5;
                        string text5 = pipingSystemType5.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text5.Contains("Domestic Cold Water") || text5.Contains("Kaltwasser") || text5.Contains("Eau froide sanitaire") || text5.Contains("Acqua fredda sanitaria") || text5.Contains("屋内給水") || text5.Contains("Domowa woda zimna") || text5.Contains("холодное водоснабжение (внутренние сети)") || text5.Contains("주택용 냉수") || text5.Contains("Agua fría doméstica") || text5.Contains("Água fria residencial") || text5.Contains("家用冷水") || text5.Contains("家用冷水") || text5.Contains("Studená voda v domácnosti"))
                        {
                            return pipingSystemType5;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.FireProtectWet)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element6 = enumerator.Current;
                        PipingSystemType pipingSystemType6 = (PipingSystemType)element6;
                        string text6 = pipingSystemType6.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text6.Contains("Fire Protection Wet") || text6.Contains("Brandschutz - Nass") || text6.Contains("Système sous eau de protection contre les incendies") || text6.Contains("Protezione antincendio a umido") || text6.Contains("湿式防火") || text6.Contains("Mokra ochrona ppoż.") || text6.Contains("Водяная система пожаротушения") || text6.Contains("습식 방화") || text6.Contains("Protección contra incendios húmeda") || text6.Contains("Proteção contra incêndio a água") || text6.Contains("湿式消防系统") || text6.Contains("濕式防火") || text6.Contains("Požární ochrana – vodní prostředky"))
                        {
                            return pipingSystemType6;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.FireProtectDry)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element7 = enumerator.Current;
                        PipingSystemType pipingSystemType7 = (PipingSystemType)element7;
                        string text7 = pipingSystemType7.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text7.Contains("Fire Protection Dry") || text7.Contains("Brandschutz - Trocken") || text7.Contains("Système sous air de protection contre les incendies") || text7.Contains("Protezione antincendio a secco") || text7.Contains("乾式防火") || text7.Contains("Sucha ochrona ppoż.") || text7.Contains("Газовая система пожаротушения") || text7.Contains("건식 방화") || text7.Contains("Protección contra incendios seca") || text7.Contains("Proteção contra incêndio a seco") || text7.Contains("干式消防系统") || text7.Contains("乾式防火") || text7.Contains("Požární ochrana – suché prostředky"))
                        {
                            return pipingSystemType7;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.FireProtectPreaction)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element8 = enumerator.Current;
                        PipingSystemType pipingSystemType8 = (PipingSystemType)element8;
                        string text8 = pipingSystemType8.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text8.Contains("Fire Protection Pre-Action") || text8.Contains("Brandschutz - Vorgesteuert") || text8.Contains("Système à préaction de protection contre les incendies") || text8.Contains("Protezione antincendio preattiva") || text8.Contains("予作動式防火") || text8.Contains("Ochrona ppoż. przedprocesowa") || text8.Contains("Дренчерная система пожаротушения") || text8.Contains("준비 작동식 방화") || text8.Contains("Protección contra incendios preventiva") || text8.Contains("Pré-ação de proteção contra incêndio") || text8.Contains("预作用消防系统") || text8.Contains("預動防火") || text8.Contains("Požární ochrana – předběžná opatření"))
                        {
                            return pipingSystemType8;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.FireProtectOther)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element9 = enumerator.Current;
                        PipingSystemType pipingSystemType9 = (PipingSystemType)element9;
                        string text9 = pipingSystemType9.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text9.Contains("Fire Protection Other") || text9.Contains("Brandschutz - Andere") || text9.Contains("Autre système de protection contre les incendies") || text9.Contains("Altra protezione antincendio") || text9.Contains("その他の防火") || text9.Contains("Inna ochrona ppoż.") || text9.Contains("Другие системы пожаротушения") || text9.Contains("다른 방화") || text9.Contains("Protección contra incendios de otro tipo") || text9.Contains("Outra proteção contra incêndio") || text9.Contains("其他消防系统") || text9.Contains("其他防火") || text9.Contains("Požární ochrana – ostatní"))
                        {
                            return pipingSystemType9;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.Vent)
            {
                using (IEnumerator<Element> enumerator = filteredElementCollector.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Element element10 = enumerator.Current;
                        PipingSystemType pipingSystemType10 = (PipingSystemType)element10;
                        string text10 = pipingSystemType10.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (text10.Contains("Vent") || text10.Contains("Belüftung") || text10.Contains("Aération") || text10.Contains("Ventilazione") || text10.Contains("排気") || text10.Contains("Wentylacyjny") || text10.Contains("Вентиляционное отверстие") || text10.Contains("통기") || text10.Contains("Ventilación") || text10.Contains("Ventilação") || text10.Contains("通气管") || text10.Contains("通風口") || text10.Contains("Průduch"))
                        {
                            return pipingSystemType10;
                        }
                    }
                    goto IL_CD2;
                }
            }
            if (connector.PipeSystemType == PipeSystemType.OtherPipe)
            {
                foreach (Element element11 in filteredElementCollector)
                {
                    PipingSystemType pipingSystemType11 = (PipingSystemType)element11;
                    string text11 = pipingSystemType11.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                    if (text11.Contains("Other") || text11.Contains("Sonstige") || text11.Contains("Autre") || text11.Contains("Altro") || text11.Contains("その他") || text11.Contains("Inne") || text11.Contains("Прочее") || text11.Contains("기타") || text11.Contains("Otro") || text11.Contains("Outro") || text11.Contains("其他") || text11.Contains("其他") || text11.Contains("Ostatní"))
                    {
                        return pipingSystemType11;
                    }
                }
            }
        IL_CD2:
            return filteredElementCollector.First<Element>() as PipingSystemType;
        }

        public static PipeInsulationType GetPipeInsulation(Document doc)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).OfClass(typeof(PipeInsulationType));
            PipeInsulationType result = null;
            foreach (Element element in filteredElementCollector)
            {
                result = (PipeInsulationType)element;
            }
            return result;
        }

        public static Connector[] ConnectorArray(Element e)
        {
            if (e == null)
            {
                return null;
            }
            FamilyInstance familyInstance = e as FamilyInstance;
            ConnectorSet connectorSet = null;
            List<Connector> list = new List<Connector>();
            if (familyInstance != null && familyInstance.MEPModel != null)
            {
                connectorSet = familyInstance.MEPModel.ConnectorManager.Connectors;
            }
            MEPSystem mepsystem = e as MEPSystem;
            if (mepsystem != null)
            {
                connectorSet = mepsystem.ConnectorManager.Connectors;
            }
            MEPCurve mepcurve = e as MEPCurve;
            if (mepcurve != null)
            {
                connectorSet = mepcurve.ConnectorManager.Connectors;
            }
            FabricationPart fabricationPart = e as FabricationPart;
            if (fabricationPart != null)
            {
                connectorSet = fabricationPart.ConnectorManager.Connectors;
            }
            if (connectorSet == null)
            {
                return null;
            }
            foreach (object obj in connectorSet)
            {
                Connector item = (Connector)obj;
                list.Add(item);
            }
            return list.ToArray();
        }

        public static Connector[] UnusedConnectorArray(Element e)
        {
            if (e == null)
            {
                return null;
            }
            FamilyInstance familyInstance = e as FamilyInstance;
            ConnectorSet connectorSet = null;
            List<Connector> list = new List<Connector>();
            if (familyInstance != null && familyInstance.MEPModel != null)
            {
                connectorSet = familyInstance.MEPModel.ConnectorManager.UnusedConnectors;
            }
            MEPSystem mepsystem = e as MEPSystem;
            if (mepsystem != null)
            {
                connectorSet = mepsystem.ConnectorManager.UnusedConnectors;
            }
            MEPCurve mepcurve = e as MEPCurve;
            if (mepcurve != null)
            {
                connectorSet = mepcurve.ConnectorManager.UnusedConnectors;
            }
            if (connectorSet == null)
            {
                return null;
            }
            foreach (object obj in connectorSet)
            {
                Connector item = (Connector)obj;
                list.Add(item);
            }
            return list.ToArray();
        }

        public static Connector ClosestConnector(Connector[] cA1, Connector[] cA2)
        {
            Connector result = null;
            double num = double.MaxValue;
            int num2 = 0;
            foreach (Connector connector in cA1)
            {
                if (cA1[num2].Origin != cA2[0].Origin)
                {
                    double num3 = Math.Sqrt(Math.Pow(cA1[num2].Origin.X - cA2[0].Origin.X, 2.0) + Math.Pow(cA1[num2].Origin.Y - cA2[0].Origin.Y, 2.0) + Math.Pow(cA1[num2].Origin.Z - cA2[0].Origin.Z, 2.0));
                    if (num3 < num)
                    {
                        num = num3;
                        result = cA1[num2];
                    }
                }
                num2++;
            }
            return result;
        }

        public static Connector ClosestAvailableConnector(Connector[] cA1, Connector[] cA2)
        {
            Connector result = null;
            double num = double.MaxValue;
            int num2 = 0;
            foreach (Connector connector in cA1)
            {
                if (connector.IsConnected)
                {
                    num2++;
                }
                else
                {
                    if (cA1[num2].Origin == cA2[0].Origin)
                    {
                        return connector;
                    }
                    double num3 = Math.Sqrt(Math.Pow(cA1[num2].Origin.X - cA2[0].Origin.X, 2.0) + Math.Pow(cA1[num2].Origin.Y - cA2[0].Origin.Y, 2.0) + Math.Pow(cA1[num2].Origin.Z - cA2[0].Origin.Z, 2.0));
                    if (num3 < num)
                    {
                        num = num3;
                        result = cA1[num2];
                    }
                    num2++;
                }
            }
            return result;
        }

        public static Connector ClosestConnectorOfDomain(Connector[] cA1, Connector[] cA2, Domain domain)
        {
            Connector result = null;
            double num = double.MaxValue;
            int num2 = 0;
            for (int i = 0; i < cA1.Length; i++)
            {
                if (cA1[i].IsConnected)
                {
                    num2++;
                }
                else
                {
                    if (cA1[num2].Origin != cA2[0].Origin)
                    {
                        double num3 = Math.Sqrt(Math.Pow(cA1[num2].Origin.X - cA2[0].Origin.X, 2.0) + Math.Pow(cA1[num2].Origin.Y - cA2[0].Origin.Y, 2.0) + Math.Pow(cA1[num2].Origin.Z - cA2[0].Origin.Z, 2.0));
                        if (num3 < num && cA1[num2].Domain == domain)
                        {
                            num = num3;
                            result = cA1[num2];
                        }
                    }
                    num2++;
                }
            }
            return result;
        }

        public static Connector ClosestConnectorOfDomainAndAngle(Connector[] cA1, Connector c)
        {
            Connector result = null;
            double num = double.MaxValue;
            int num2 = 0;
            for (int i = 0; i < cA1.Length; i++)
            {
                if (cA1[i].IsConnected)
                {
                    num2++;
                }
                else
                {
                    if (cA1[num2].Origin != c.Origin)
                    {
                        double num3 = Math.Sqrt(Math.Pow(cA1[num2].Origin.X - c.Origin.X, 2.0) + Math.Pow(cA1[num2].Origin.Y - c.Origin.Y, 2.0) + Math.Pow(cA1[num2].Origin.Z - c.Origin.Z, 2.0));
                        if (num3 < num && cA1[num2].Domain == c.Domain && c.CoordinateSystem.BasisZ.DotProduct(cA1[num2].CoordinateSystem.BasisZ) < -0.9)
                        {
                            num = num3;
                            result = cA1[num2];
                        }
                    }
                    num2++;
                }
            }
            return result;
        }

        public static Connector NearestConnector(Connector[] cA, XYZ startPoint)
        {
            Connector result = null;
            try
            {
                double num = double.MaxValue;
                int num2 = 0;
                foreach (Connector connector in cA)
                {
                    double num3 = Math.Sqrt(Math.Pow(cA[num2].Origin.X - startPoint.X, 2.0) + Math.Pow(cA[num2].Origin.Y - startPoint.Y, 2.0) + Math.Pow(cA[num2].Origin.Z - startPoint.Z, 2.0));
                    if (num3 < num)
                    {
                        num = num3;
                        result = cA[num2];
                    }
                    num2++;
                }
            }
            catch
            {
            }
            return result;
        }

        public static Connector[] NearestConnectors(List<Connector> cs, XYZ basePoint)
        {
            List<Connector> list = new List<Connector>();
            foreach (Connector connector in cs)
            {
                if (Math.Round(connector.Origin.X, 2) == Math.Round(basePoint.X, 2) && Math.Round(connector.Origin.Y, 2) == Math.Round(basePoint.Y, 2))
                {
                    list.Add(connector);
                }
            }
            return list.ToArray();
        }

        public static Connector FarthestConnector(Connector[] cA, XYZ startPoint)
        {
            Connector result = null;
            try
            {
                double num = double.MinValue;
                int num2 = 0;
                foreach (Connector connector in cA)
                {
                    double num3 = Math.Sqrt(Math.Pow(cA[num2].Origin.X - startPoint.X, 2.0) + Math.Pow(cA[num2].Origin.Y - startPoint.Y, 2.0) + Math.Pow(cA[num2].Origin.Z - startPoint.Z, 2.0));
                    if (num3 > num)
                    {
                        num = num3;
                        result = cA[num2];
                    }
                    num2++;
                }
            }
            catch
            {
            }
            return result;
        }

        public static Connector ConnectedConnector(Connector cInput, XYZ origin)
        {
            Connector result = null;
            foreach (object obj in cInput.AllRefs)
            {
                Connector connector = (Connector)obj;
                if (connector.ConnectorType == ConnectorType.End && connector.IsConnected && Math.Round(connector.Origin.X, 2) == Math.Round(origin.X, 2) && Math.Round(connector.Origin.Y, 2) == Math.Round(origin.Y, 2))
                {
                    result = connector;
                }
            }
            return result;
        }

        public static Connector[] ClosestConnectors(Element element1, Element element2, bool align)
        {
            Connector[] array = new Connector[2];
            Connector[] array2 = ConnectorArray(element1);
            Connector[] array3 = ConnectorArray(element2);
            Connector connector = ClosestAvailableConnector(array2, array3);
            Connector connector2;
            if (align)
            {
                connector2 = ClosestConnectorOfDomain(array3, array2, connector.Domain);
            }
            else
            {
                connector2 = ClosestConnectorOfDomainAndAngle(array3, connector);
            }
            if (connector == null)
            {
                return null;
            }
            if (connector2 == null)
            {
                return null;
            }
            array[0] = connector;
            array[1] = connector2;
            return array;
        }

        public static FamilySymbol GetFittingFromRouting(PipeType pipeType, RoutingPreferenceRuleGroupType groupType, double pipeDiameter, string log, Document doc)
        {
            FamilySymbol familySymbol = null;
            RoutingPreferenceManager routingPreferenceManager = pipeType.RoutingPreferenceManager;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("rPM.GetNumberOfRules(group) {0}", routingPreferenceManager.GetNumberOfRules(groupType));
            }
            for (int num = 0; num != routingPreferenceManager.GetNumberOfRules(groupType); num++)
            {
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("rule {0}", num);
                }
                RoutingPreferenceRule rule = routingPreferenceManager.GetRule(groupType, num);
                PrimarySizeCriterion primarySizeCriterion = rule.GetCriterion(0) as PrimarySizeCriterion;
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("width: {0}", pipeDiameter);
                    streamWriter3.WriteLine("psc.MinimumSize: {0}", primarySizeCriterion.MinimumSize * 12.0);
                    streamWriter3.WriteLine("psc.MaximumSize: {0}", primarySizeCriterion.MaximumSize * 12.0);
                }
                if (Math.Round(primarySizeCriterion.MinimumSize * 12.0, 2) <= Math.Round(pipeDiameter, 2) && Math.Round(pipeDiameter, 2) <= Math.Round(primarySizeCriterion.MaximumSize * 12.0, 2))
                {
                    ElementId meppartId = rule.MEPPartId;
                    familySymbol = doc.GetElement(meppartId) as FamilySymbol;
                    using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                    {
                        streamWriter4.WriteLine("fittingFamily.Name: {0}", familySymbol.Name);
                    }
                    if (!familySymbol.Name.Contains("Tap"))
                    {
                        break;
                    }
                }
            }
            return familySymbol;
        }

        public static FamilySymbol GetFittingFromConduit(ConduitType condType, RoutingPreferenceRuleGroupType groupType, double pipeDiameter, string log, Document doc)
        {
            FamilySymbol familySymbol = null;
            RoutingPreferenceManager routingPreferenceManager = condType.RoutingPreferenceManager;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("rPM.GetNumberOfRules(group) {0}", routingPreferenceManager.GetNumberOfRules(groupType));
            }
            for (int num = 0; num != routingPreferenceManager.GetNumberOfRules(groupType); num++)
            {
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("rule {0}", num);
                }
                RoutingPreferenceRule rule = routingPreferenceManager.GetRule(groupType, num);
                PrimarySizeCriterion primarySizeCriterion = rule.GetCriterion(0) as PrimarySizeCriterion;
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("width: {0}", pipeDiameter);
                    streamWriter3.WriteLine("psc.MinimumSize: {0}", primarySizeCriterion.MinimumSize * 12.0);
                    streamWriter3.WriteLine("psc.MaximumSize: {0}", primarySizeCriterion.MaximumSize * 12.0);
                }
                if (Math.Round(primarySizeCriterion.MinimumSize * 12.0, 2) <= Math.Round(pipeDiameter, 2) && Math.Round(pipeDiameter, 2) <= Math.Round(primarySizeCriterion.MaximumSize * 12.0, 2))
                {
                    ElementId meppartId = rule.MEPPartId;
                    familySymbol = doc.GetElement(meppartId) as FamilySymbol;
                    using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                    {
                        streamWriter4.WriteLine("fittingFamily.Name: {0}", familySymbol.Name);
                    }
                    if (!familySymbol.Name.Contains("Tap"))
                    {
                        break;
                    }
                }
            }
            return familySymbol;
        }

        public static void SetComments(Element element, string system, string layer, string description)
        {
            Parameter parameter = element.LookupParameter("Comments");
            Parameter parameter2 = element.LookupParameter("Subcontractor Part Name");
            if (system != null && system != "")
            {
                parameter.Set(system);
            }
            else
            {
                parameter.Set(layer);
            }
            try
            {
                parameter2.Set(description);
            }
            catch
            {
            }
        }

        //public static void SetCommentsAllFlanges(Connector[] connectors, string system, string layer, string log, string shapeOrDescription)
        //{
        //   foreach (Connector connector in connectors)
        //   {
        //      if (connector.IsConnected)
        //      {
        //         ConnectorSet allRefs = connector.AllRefs;
        //         Element element = null;
        //         using (IEnumerator enumerator = allRefs.GetEnumerator())
        //         {
        //            if (enumerator.MoveNext())
        //            {
        //               element = ((Connector)enumerator.Current).Owner;
        //            }
        //         }
        //         FamilyInstance familyInstance = element as FamilyInstance;
        //         try
        //         {
        //            PartType partType = (familyInstance.MEPModel as MechanicalFitting).PartType;
        //         }
        //         catch
        //         {
        //            using (StreamWriter streamWriter = new StreamWriter(log, true))
        //            {
        //               streamWriter.WriteLine("Connected Element {0} is probably a pipe", element.Id);
        //            }
        //         }
        //         using (StreamWriter streamWriter2 = new StreamWriter(log, true))
        //         {
        //            streamWriter2.WriteLine("Coupling Found: {0}", element.Id);
        //         }
        //         MicrodeskHelpers.SetComments(element, system, layer, shapeOrDescription);
        //      }
        //   }
        //}

        public static double RotateXY(XYZ insertion, FamilyInstance newFitting, double deltaX, double deltaY, string log, Document doc)
        {
            double num;
            if (Math.Round(deltaX, 4) == 0.0 && Math.Round(deltaY, 4) < 0.0)
            {
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Pipe draining North");
                }
                num = 4.71238898038469;
            }
            else if (Math.Round(deltaX, 4) == 0.0 && Math.Round(deltaY, 4) > 0.0)
            {
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Pipe draining South");
                }
                num = 1.5707963267948966;
            }
            else if (Math.Round(deltaX, 4) < 0.0 && Math.Round(deltaY, 4) == 0.0)
            {
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("Pipe draining East");
                }
                num = 3.1415926535897931;
            }
            else if (Math.Round(deltaX, 4) > 0.0 && Math.Round(deltaY, 4) == 0.0)
            {
                using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                {
                    streamWriter4.WriteLine("Pipe draining West");
                }
                num = 0.0;
            }
            else if (Math.Round(deltaX, 4) < 0.0)
            {
                using (StreamWriter streamWriter5 = new StreamWriter(log, true))
                {
                    streamWriter5.WriteLine("deltaX < 0");
                }
                num = 3.1415926535897931 + Math.Atan(deltaY / deltaX);
            }
            else
            {
                using (StreamWriter streamWriter6 = new StreamWriter(log, true))
                {
                    streamWriter6.WriteLine("last chance");
                }
                num = Math.Atan(deltaY / deltaX);
            }
            XYZ endpoint = new XYZ(insertion.X, insertion.Y, insertion.Z + 1.0);
            Line axis = Line.CreateBound(insertion, endpoint);
            ElementTransformUtils.RotateElement(doc, newFitting.Id, axis, num);
            return num;
        }

        public static void RotateForSlope(XYZ insertion, Element element, Connector c, double deltaX, double deltaY, double deltaZ, string log, Document doc, Autodesk.Revit.ApplicationServices.Application app, SubTransaction sT)
        {
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Rotating for Slope");
            }
            double num = Math.Sqrt(Math.Pow(deltaX, 2.0) + Math.Pow(deltaY, 2.0));
            double num2 = -Math.Abs(Math.Atan(deltaZ / num));
            XYZ xyz = c.CoordinateSystem.BasisZ.Normalize();
            XYZ endpoint = new XYZ(insertion.X + xyz.Y, insertion.Y - xyz.X, insertion.Z);
            Line axis = Line.CreateBound(insertion, endpoint);
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("angleZ (rads): {0}", Math.Round(num2, 5));
                streamWriter2.WriteLine("angleZ (degs): {0}", Math.Round(num2 * 180.0 / 3.1415926535897931, 5));
                streamWriter2.WriteLine("connectorUnitVector: {0}, {1}", Math.Round(xyz.X, 5), Math.Round(xyz.Y, 5));
            }
            if (Math.Round(num2, 5) == 0.0)
            {
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("No need to rotate");
                }
                return;
            }
            sT.Start();
            ElementTransformUtils.RotateElement(doc, element.Id, axis, num2);
            sT.Commit();
            XYZ source = new XYZ(deltaX, deltaY, deltaZ).Normalize();
            double value = c.CoordinateSystem.BasisZ.DotProduct(source);
            if (Math.Round(value, 5) != 1.0 && Math.Round(value, 5) != -1.0)
            {
                using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                {
                    streamWriter4.WriteLine("Dot product of the correct vector and the fitting connector at its new angle: {0}", Math.Round(value, 5));
                    streamWriter4.WriteLine("Rotated in wrong direction; correcting");
                }
                sT.Start();
                ElementTransformUtils.RotateElement(doc, element.Id, axis, -2.0 * num2);
                sT.Commit();
            }
        }

        public static void RotateElbowForSlope(XYZ insertion, Element element, Connector c, double deltaX, double deltaY, double deltaZ, double bendAngle, string log, Document doc, Autodesk.Revit.ApplicationServices.Application app, SubTransaction sT)
        {
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Rotating for Slope");
            }
            double num = Math.Sqrt(Math.Pow(deltaX, 2.0) + Math.Pow(deltaY, 2.0));
            double num2 = -Math.Abs(Math.Atan(deltaZ / num));
            XYZ xyz = c.CoordinateSystem.BasisZ.Normalize();
            XYZ endpoint = new XYZ(insertion.X + xyz.Y, insertion.Y - xyz.X, insertion.Z);
            Line axis = Line.CreateBound(insertion, endpoint);
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("angleZ (rads): {0}", Math.Round(num2, 5));
                streamWriter2.WriteLine("angleZ (degs): {0}", Math.Round(num2 * 180.0 / 3.1415926535897931, 5));
                streamWriter2.WriteLine("connectorUnitVector: {0}, {1}", Math.Round(xyz.X, 5), Math.Round(xyz.Y, 5));
            }
            sT.Start();
            ElementTransformUtils.RotateElement(doc, element.Id, axis, num2);
            sT.Commit();
            XYZ source = new XYZ(deltaX, deltaY, deltaZ).Normalize();
            double value = c.CoordinateSystem.BasisZ.DotProduct(source);
            if (Math.Round(value, 5) != 1.0 && Math.Round(value, 5) != -1.0)
            {
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("Dot product of the correct vector and the fitting connector at its new angle: {0}", Math.Round(value, 5));
                    streamWriter3.WriteLine("Rotated in wrong direction; correcting");
                }
                sT.Start();
                ElementTransformUtils.RotateElement(doc, element.Id, axis, bendAngle);
                sT.Commit();
            }
        }

        public static void RotateOnProjectedPlaneBAK(XYZ planeNormEndPoint1, XYZ planeNormEndPoint2, XYZ correctCoord, FamilyInstance newFitting, Connector[] connArray, double additionalAng, string log, Document doc, SubTransaction sT)
        {
            Line axis = Line.CreateBound(planeNormEndPoint1, planeNormEndPoint2);
            XYZ xyz = (planeNormEndPoint2 - planeNormEndPoint1).Normalize();
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("planeNormal: {0}, {1}, {2}", Math.Round(xyz.X, 2), Math.Round(xyz.Y, 2), Math.Round(xyz.Z, 2));
            }
            XYZ xyz2 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connArray[0].Origin);
            XYZ xyz3 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connArray[1].Origin);
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("elbowEnd2Da: {0}", Math.Abs((xyz3 - planeNormEndPoint1).GetLength()));
                streamWriter2.WriteLine("elbowEnd2D: {0}", Math.Abs((xyz2 - planeNormEndPoint1).GetLength()));
            }
            Connector connector = connArray[0];
            if ((xyz3 - planeNormEndPoint1).GetLength() > (xyz2 - planeNormEndPoint1).GetLength())
            {
                xyz2 = xyz3;
                connector = connArray[1];
            }
            XYZ xyz4 = ProjectPointOnPlane(xyz, planeNormEndPoint1, correctCoord);
            XYZ xyz5 = new XYZ(xyz2.X - planeNormEndPoint1.X, xyz2.Y - planeNormEndPoint1.Y, xyz2.Z - planeNormEndPoint1.Z);
            XYZ xyz6 = new XYZ(xyz4.X - planeNormEndPoint1.X, xyz4.Y - planeNormEndPoint1.Y, xyz4.Z - planeNormEndPoint1.Z);
            double length = xyz5.GetLength();
            double length2 = xyz6.GetLength();
            double num = 1.0;
            if (xyz.X + xyz.Y + xyz.Z < 0.0)
            {
                num = -1.0;
            }
            double num2 = Math.Acos(xyz5.DotProduct(xyz6) / (length * length2)) * num;
            sT.Start();
            ElementTransformUtils.RotateElement(doc, newFitting.Id, axis, num2);
            sT.Commit();
            XYZ xyz7 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connector.Origin);
            XYZ xyz8 = new XYZ(xyz7.X - planeNormEndPoint1.X, xyz7.Y - planeNormEndPoint1.Y, xyz7.Z - planeNormEndPoint1.Z).CrossProduct(xyz6);
            double length3 = xyz8.GetLength();
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("elbowEnd2D: {0}, {1}, {2}", Math.Round(xyz2.X, 2), Math.Round(xyz2.Y, 2), Math.Round(xyz2.Z, 2));
                streamWriter3.WriteLine("correctCoord2D: {0}, {1}, {2}", Math.Round(xyz4.X, 2), Math.Round(xyz4.Y, 2), Math.Round(xyz4.Z, 2));
                streamWriter3.WriteLine("elbowVector: {0}, {1}, {2}", Math.Round(xyz5.X, 2), Math.Round(xyz5.Y, 2), Math.Round(xyz5.Z, 2));
                streamWriter3.WriteLine("correctCoordVector: {0}, {1}, {2}", Math.Round(xyz6.X, 2), Math.Round(xyz6.Y, 2), Math.Round(xyz6.Z, 2));
                streamWriter3.WriteLine("elbowVectorM: {0}", length);
                streamWriter3.WriteLine("coordVectorM: {0}", length2);
                streamWriter3.WriteLine("Rotation Angle about Centerline (rads): {0}", num2);
                streamWriter3.WriteLine("Rotation Angle about Centerline (degs): {0}", num2 * 180.0 / 3.1415926535897931);
                streamWriter3.WriteLine("crossProd: {0}, {1}, {2}", Math.Round(xyz8.X, 2), Math.Round(xyz8.Y, 2), Math.Round(xyz8.Z, 2));
                streamWriter3.WriteLine("crossProdCheck: {0}", Math.Round(length3, 6));
            }
            if (Math.Round(length3, 6) != 0.0)
            {
                num2 = -2.0 * num2;
                using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                {
                    streamWriter4.WriteLine("The new location of the connector is further away from the correct coordinate than when we started");
                    streamWriter4.WriteLine("New Angle about Centerline (rads): {0}", num2);
                    streamWriter4.WriteLine("New Angle about Centerline (degs): {0}", num2 * 180.0 / 3.1415926535897931);
                }
                sT.Start();
                ElementTransformUtils.RotateElement(doc, newFitting.Id, axis, num2);
                sT.Commit();
            }
        }

        public static void RotateOnProjectedPlane(XYZ planeNormEndPoint1, XYZ planeNormEndPoint2, XYZ correctCoord, FamilyInstance newFitting, Connector[] connArray, double additionalAng, string log, Document doc, SubTransaction sT)
        {
            Line axis = Line.CreateBound(planeNormEndPoint1, planeNormEndPoint2);
            XYZ xyz = (planeNormEndPoint2 - planeNormEndPoint1).Normalize();
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("planeNormal: {0}, {1}, {2}", Math.Round(xyz.X, 2), Math.Round(xyz.Y, 2), Math.Round(xyz.Z, 2));
            }
            XYZ xyz2 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connArray[0].Origin);
            XYZ xyz3 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connArray[1].Origin);
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("elbowEnd2Da: {0}", Math.Abs((xyz3 - planeNormEndPoint1).GetLength()));
                streamWriter2.WriteLine("elbowEnd2D: {0}", Math.Abs((xyz2 - planeNormEndPoint1).GetLength()));
            }
            Connector connector = connArray[0];
            if ((xyz3 - planeNormEndPoint1).GetLength() > (xyz2 - planeNormEndPoint1).GetLength())
            {
                xyz2 = xyz3;
                connector = connArray[1];
            }
            XYZ xyz4 = ProjectPointOnPlane(xyz, planeNormEndPoint1, correctCoord);
            double num = Math.Abs((xyz2 - xyz4).GetLength());
            XYZ xyz5 = new XYZ(xyz2.X - planeNormEndPoint1.X, xyz2.Y - planeNormEndPoint1.Y, xyz2.Z - planeNormEndPoint1.Z);
            XYZ xyz6 = new XYZ(xyz4.X - planeNormEndPoint1.X, xyz4.Y - planeNormEndPoint1.Y, xyz4.Z - planeNormEndPoint1.Z);
            double length = xyz5.GetLength();
            double length2 = xyz6.GetLength();
            double num2 = 1.0;
            if (xyz.X + xyz.Y + xyz.Z < 0.0)
            {
                num2 = -1.0;
            }
            double num3 = Math.Acos(xyz5.DotProduct(xyz6) / (length * length2)) * num2;
            sT.Start();
            ElementTransformUtils.RotateElement(doc, newFitting.Id, axis, num3);
            sT.Commit();
            XYZ xyz7 = ProjectPointOnPlane(xyz, planeNormEndPoint1, connector.Origin);
            XYZ xyz8 = new XYZ(xyz7.X - planeNormEndPoint1.X, xyz7.Y - planeNormEndPoint1.Y, xyz7.Z - planeNormEndPoint1.Z);
            double num4 = Math.Abs((xyz7 - xyz4).GetLength());
            XYZ xyz9 = xyz8.CrossProduct(xyz6);
            double length3 = xyz9.GetLength();
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("elbowEnd2D: {0}, {1}, {2}", Math.Round(xyz2.X, 2), Math.Round(xyz2.Y, 2), Math.Round(xyz2.Z, 2));
                streamWriter3.WriteLine("correctCoord2D: {0}, {1}, {2}", Math.Round(xyz4.X, 2), Math.Round(xyz4.Y, 2), Math.Round(xyz4.Z, 2));
                streamWriter3.WriteLine("elbowVector: {0}, {1}, {2}", Math.Round(xyz5.X, 2), Math.Round(xyz5.Y, 2), Math.Round(xyz5.Z, 2));
                streamWriter3.WriteLine("correctCoordVector: {0}, {1}, {2}", Math.Round(xyz6.X, 2), Math.Round(xyz6.Y, 2), Math.Round(xyz6.Z, 2));
                streamWriter3.WriteLine("elbowVectorM: {0}", length);
                streamWriter3.WriteLine("coordVectorM: {0}", length2);
                streamWriter3.WriteLine("Rotation Angle about Centerline (rads): {0}", num3);
                streamWriter3.WriteLine("Rotation Angle about Centerline (degs): {0}", num3 * 180.0 / 3.1415926535897931);
                streamWriter3.WriteLine("crossProd: {0}, {1}, {2}", Math.Round(xyz9.X, 2), Math.Round(xyz9.Y, 2), Math.Round(xyz9.Z, 2));
                streamWriter3.WriteLine("crossProdCheck: {0}", Math.Round(length3, 6));
                streamWriter3.WriteLine("displacement: {0}", num);
                streamWriter3.WriteLine("displacement2: {0}", num4);
            }
            if (Math.Round(length3, 6) != 0.0 || num4 > num)
            {
                num3 = -2.0 * num3;
                using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                {
                    streamWriter4.WriteLine("The new location of the connector is further away from the correct coordinate than when we started");
                    streamWriter4.WriteLine("New Angle about Centerline (rads): {0}", num3);
                    streamWriter4.WriteLine("New Angle about Centerline (degs): {0}", num3 * 180.0 / 3.1415926535897931);
                }
                sT.Start();
                ElementTransformUtils.RotateElement(doc, newFitting.Id, axis, num3);
                sT.Commit();
            }
        }

        //public static Connector CheckForFlange(Connector newFittingNearest, string system, string layer, string shapeOrDescription, string log)
        //{
        //   if (newFittingNearest.IsConnected)
        //   {
        //      ConnectorSet allRefs = newFittingNearest.AllRefs;
        //      Element element = null;
        //      using (IEnumerator enumerator = allRefs.GetEnumerator())
        //      {
        //         if (enumerator.MoveNext())
        //         {
        //            element = ((Connector)enumerator.Current).Owner;
        //         }
        //      }
        //      FamilyInstance familyInstance = element as FamilyInstance;
        //      try
        //      {
        //         if ((familyInstance.MEPModel as MechanicalFitting).PartType != PartType.PipeFlange)
        //         {
        //            return null;
        //         }
        //         using (StreamWriter streamWriter = new StreamWriter(log, true))
        //         {
        //            streamWriter.WriteLine("Connected Element {0} is a flange or grooved coupling", element.Id);
        //         }
        //      }
        //      catch
        //      {
        //         using (StreamWriter streamWriter2 = new StreamWriter(log, true))
        //         {
        //            streamWriter2.WriteLine("Connected Element {0} is probably a pipe", element.Id);
        //         }
        //         return null;
        //      }
        //      using (StreamWriter streamWriter3 = new StreamWriter(log, true))
        //      {
        //         streamWriter3.WriteLine("Coupling Found: {0}", element.Id);
        //      }
        //      MicrodeskHelpers.SetComments(element, system, layer, shapeOrDescription);
        //      foreach (object obj in allRefs)
        //      {
        //         Connector connector = (Connector)obj;
        //         try
        //         {
        //            if (!connector.IsConnected)
        //            {
        //               newFittingNearest = connector;
        //            }
        //         }
        //         catch
        //         {
        //         }
        //      }
        //      return newFittingNearest;
        //   }
        //   return newFittingNearest;
        //}

        //public static Connector CheckAndDeleteFlange(Connector newFittingNearest, string system, string layer, string log, Document doc)
        //{
        //   if (newFittingNearest.IsConnected)
        //   {
        //      ConnectorSet allRefs = newFittingNearest.AllRefs;
        //      Element element = null;
        //      using (IEnumerator enumerator = allRefs.GetEnumerator())
        //      {
        //         if (enumerator.MoveNext())
        //         {
        //            element = ((Connector)enumerator.Current).Owner;
        //         }
        //      }
        //      FamilyInstance familyInstance = element as FamilyInstance;
        //      try
        //      {
        //         if ((familyInstance.MEPModel as MechanicalFitting).PartType != PartType.PipeFlange)
        //         {
        //            return newFittingNearest;
        //         }
        //         using (StreamWriter streamWriter = new StreamWriter(log, true))
        //         {
        //            streamWriter.WriteLine("Connected Element {0} is a flange or grooved coupling", element.Id);
        //         }
        //      }
        //      catch
        //      {
        //         using (StreamWriter streamWriter2 = new StreamWriter(log, true))
        //         {
        //            streamWriter2.WriteLine("Connected Element {0} is probably a pipe", element.Id);
        //         }
        //         return newFittingNearest;
        //      }
        //      using (StreamWriter streamWriter3 = new StreamWriter(log, true))
        //      {
        //         streamWriter3.WriteLine("Coupling Found: {0}", element.Id);
        //      }
        //      doc.Delete(element.Id);
        //      return newFittingNearest;
        //   }
        //   return newFittingNearest;
        //}

        public static List<Element> FindMateNearby(List<Element> elementsFiltered, BuiltInCategory bic, XYZ point, int noIterations, string log, Document doc)
        {
            for (int i = 0; i <= noIterations; i++)
            {
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list = filteredElementCollector.OfCategory(bic).ToElements();
                XYZ minimumPoint = new XYZ(0.021 + point.X - 0.021 * i, 0.021 + point.Y - 0.021 * i, 0.021 + point.Z - 0.1 * i);
                XYZ maximumPoint = new XYZ(0.021 + point.X + 0.021 * i, 0.021 + point.Y + 0.021 * i, 0.021 + point.Z + 0.1 * i);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                list = filteredElementCollector.WherePasses(filter).OfCategory(bic).ToElements();
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list.Count);
                }
                if (list.Count != 0)
                {
                    using (IEnumerator<Element> enumerator = list.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            Element item = enumerator.Current;
                            elementsFiltered.Add(item);
                            return elementsFiltered;
                        }
                    }
                }
            }
            return elementsFiltered;
        }

        public static List<Element> FindMatesOfCategory(List<Element> elementsFiltered, BuiltInCategory bic, XYZ point, int noIterations, string systemName, string log, Document doc)
        {
            List<ElementId> list = new List<ElementId>();
            if (elementsFiltered.Count == 1)
            {
                list.Add(elementsFiltered[0].Id);
            }
            for (int i = 0; i <= noIterations; i++)
            {
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list2 = filteredElementCollector.OfCategory(bic).ToElements();
                XYZ minimumPoint = new XYZ(0.021 + point.X - 0.021 * i, 0.021 + point.Y - 0.021 * i, 0.021 + point.Z - 0.021 * i);
                XYZ maximumPoint = new XYZ(0.021 + point.X + 0.021 * i, 0.021 + point.Y + 0.021 * i, 0.021 + point.Z + 0.021 * i);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                list2 = filteredElementCollector.WherePasses(filter).Excluding(list).OfCategory(bic).ToElements();
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list2.Count);
                }
                if (list2.Count != 0)
                {
                    foreach (Element element in list2)
                    {
                        string text = element.LookupParameter("Comments").AsString();
                        XYZ xyz = NearestConnector(ConnectorArray(element), point).Origin - point;
                        using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                        {
                            streamWriter2.WriteLine("mate ID: {0}", element.Id);
                            streamWriter2.WriteLine("mateSystemAbbr: {0}", text);
                            streamWriter2.WriteLine("systemAbbreviation: {0}", systemName);
                            streamWriter2.WriteLine("candidateDislocation: {0}", xyz);
                        }
                        if (xyz.GetLength() <= 0.25 && text == systemName)
                        {
                            elementsFiltered.Add(element);
                            return elementsFiltered;
                        }
                    }
                }
            }
            return elementsFiltered;
        }

        public static List<Element> FindMatesOfPartType(List<Element> elementsFiltered, PartType partType, XYZ point, int noIterations, string systemName, string log, Document doc)
        {
            if (elementsFiltered.Count == 1)
            {
                Convert.ToString(elementsFiltered[0].Id);
            }
            for (int i = 0; i <= noIterations; i++)
            {
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list = filteredElementCollector.OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();
                XYZ minimumPoint = new XYZ(0.021 + point.X - 0.021 * i, 0.021 + point.Y - 0.021 * i, 0.021 + point.Z - 0.021 * i);
                XYZ maximumPoint = new XYZ(0.021 + point.X + 0.021 * i, 0.021 + point.Y + 0.021 * i, 0.021 + point.Z + 0.021 * i);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                list = filteredElementCollector.WherePasses(filter).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list.Count);
                }
                if (list.Count != 0)
                {
                    foreach (Element element in list)
                    {
                        if (((element as FamilyInstance).MEPModel as MechanicalFitting).PartType == partType)
                        {
                            string text = element.LookupParameter("Comments").AsString();
                            XYZ xyz = NearestConnector(ConnectorArray(element), point).Origin - point;
                            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                            {
                                streamWriter2.WriteLine("mate ID: {0}", element.Id);
                                streamWriter2.WriteLine("mateSystemAbbr: {0}", text);
                                streamWriter2.WriteLine("systemAbbreviation: {0}", systemName);
                                streamWriter2.WriteLine("candidateDislocation: {0}", xyz);
                            }
                            if (xyz.GetLength() <= 0.25 && text == systemName)
                            {
                                elementsFiltered.Add(element);
                                return elementsFiltered;
                            }
                        }
                    }
                }
            }
            return elementsFiltered;
        }

        public static List<Element> FindMates(List<Element> elementsFiltered, XYZ point, int noIterations, string systemName, string log, Document doc)
        {
            List<ElementId> list = new List<ElementId>();
            if (elementsFiltered.Count > 0)
            {
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Creating Exclusion List");
                }
                Convert.ToString(elementsFiltered[0].Id);
                list.Add(elementsFiltered[0].Id);
            }
            for (int i = 0; i <= noIterations; i++)
            {
                ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
                ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting);
                ElementCategoryFilter filter3 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
                ElementCategoryFilter filter4 = new ElementCategoryFilter(BuiltInCategory.OST_Sprinklers);
                LogicalOrFilter filter5 = new LogicalOrFilter(new LogicalOrFilter(new LogicalOrFilter(filter, filter2), filter3), filter4);
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list2 = filteredElementCollector.WherePasses(filter5).ToElements();
                XYZ minimumPoint = new XYZ(0.021 + point.X - 0.021 * i, 0.021 + point.Y - 0.021 * i, 0.021 + point.Z - 0.021 * i);
                XYZ maximumPoint = new XYZ(0.021 + point.X + 0.021 * i, 0.021 + point.Y + 0.021 * i, 0.021 + point.Z + 0.021 * i);
                BoundingBoxIntersectsFilter filter6 = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                if (list == null || list.Count == 0)
                {
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).ToElements();
                }
                else
                {
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).Excluding(list).ToElements();
                }
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list2.Count);
                }
                if (list2.Count != 0)
                {
                    foreach (Element element in list2)
                    {
                        string text = element.LookupParameter("Comments").AsString();
                        Connector connector = NearestConnector(ConnectorArray(element), point);
                        XYZ xyz = connector.Origin - point;
                        using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                        {
                            streamWriter3.WriteLine("mate ID: {0}", element.Id);
                            streamWriter3.WriteLine("mateSystemAbbr: {0}", text);
                            streamWriter3.WriteLine("systemAbbreviation: {0}", systemName);
                            streamWriter3.WriteLine("candidateDislocation: {0}", xyz);
                        }
                        if (connector.IsConnected)
                        {
                            using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                            {
                                streamWriter4.WriteLine("mate {0} is already connected", element.Id);
                                continue;
                            }
                        }
                        if (xyz.GetLength() <= 0.25 && (text == systemName || systemName == null))
                        {
                            elementsFiltered.Add(element);
                            return elementsFiltered;
                        }
                    }
                }
            }
            using (StreamWriter streamWriter5 = new StreamWriter(log, true))
            {
                streamWriter5.WriteLine("Nothing Found, returning elementsFiltered");
            }
            return elementsFiltered;
        }

        public static List<Element> FindMatesWithCenterPoint(List<Element> elementsFiltered, XYZ point, XYZ centerPoint, int noIterations, string systemName, string log, Document doc)
        {
            List<ElementId> list = new List<ElementId>();
            if (elementsFiltered.Count > 0)
            {
                foreach (Element element in elementsFiltered)
                {
                    list.Add(element.Id);
                }
            }
            for (int i = 0; i <= noIterations; i++)
            {
                ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
                ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
                ElementCategoryFilter filter3 = new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting);
                ElementCategoryFilter filter4 = new ElementCategoryFilter(BuiltInCategory.OST_Sprinklers);
                LogicalOrFilter filter5 = new LogicalOrFilter(new LogicalOrFilter(new LogicalOrFilter(filter, filter3), filter2), filter4);
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list2 = filteredElementCollector.WherePasses(filter5).ToElements();
                XYZ minimumPoint = new XYZ(0.021 + point.X - 0.021 * i, 0.021 + point.Y - 0.021 * i, 0.021 + point.Z - 0.021 * i);
                XYZ maximumPoint = new XYZ(0.021 + point.X + 0.021 * i, 0.021 + point.Y + 0.021 * i, 0.021 + point.Z + 0.021 * i);
                BoundingBoxIntersectsFilter filter6 = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                if (list == null || list.Count == 0)
                {
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).ToElements();
                }
                else
                {
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).Excluding(list).ToElements();
                }
                using (StreamWriter streamWriter = new StreamWriter(log, true))
                {
                    streamWriter.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list2.Count);
                }
                if (list2.Count != 0)
                {
                    Element element2 = null;
                    foreach (Element element3 in list2)
                    {
                        string text = element3.LookupParameter("Comments").AsString();
                        double num = double.MaxValue;
                        Connector connector = NearestConnector(ConnectorArray(element3), centerPoint);
                        XYZ xyz = connector.Origin - point;
                        double length = (connector.Origin - centerPoint).GetLength();
                        using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                        {
                            streamWriter2.WriteLine("mate ID: {0}", element3.Id);
                            streamWriter2.WriteLine("mateSystemAbbr: {0}", text);
                            streamWriter2.WriteLine("systemAbbreviation: {0}", systemName);
                            streamWriter2.WriteLine("candidateDislocation: {0}", xyz);
                            streamWriter2.WriteLine("candidateCenterDisloc: {0}", length);
                        }
                        if (connector.IsConnected)
                        {
                            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                            {
                                streamWriter3.WriteLine("mate {0} is already connected", element3.Id);
                                continue;
                            }
                        }
                        if (xyz.GetLength() > 0.3)
                        {
                            using (StreamWriter streamWriter4 = new StreamWriter(log, true))
                            {
                                streamWriter4.WriteLine("mate {0} is too far", element3.Id);
                                continue;
                            }
                        }
                        if (text != systemName && element3.Category.Name != "Sprinklers")
                        {
                            using (StreamWriter streamWriter5 = new StreamWriter(log, true))
                            {
                                streamWriter5.WriteLine("mate {0}'s comments doesn't match the system", element3.Id);
                                continue;
                            }
                        }
                        if (length < num)
                        {
                            element2 = element3;
                            num = length;
                        }
                    }
                    if (element2 != null)
                    {
                        elementsFiltered.Add(element2);
                        return elementsFiltered;
                    }
                }
            }
            using (StreamWriter streamWriter6 = new StreamWriter(log, true))
            {
                streamWriter6.WriteLine("Nothing Found, returning elementsFiltered");
            }
            return elementsFiltered;
        }

        public static List<Element> FindMatesWithBoundingBox(List<Element> elementsFiltered, XYZ point, XYZ centerPoint, double searchBoxWidth, double searchBoxDepth, double searchBoxHeight, int noIterations, string systemName, string log, Document doc, Autodesk.Revit.ApplicationServices.Application app, bool testConnector)
        {
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("FindMatesWithBoundingBox");
            }
            List<ElementId> list = new List<ElementId>();
            ElementId elementId = null;
            ElementId elementId2 = null;
            if (elementsFiltered.Count > 0)
            {
                list.Add(elementsFiltered[0].Id);
                elementId = elementsFiltered[0].Id;
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Excluding {0} from search", elementId);
                }
            }
            if (elementsFiltered.Count > 1)
            {
                list.Add(elementsFiltered[1].Id);
                elementId2 = elementsFiltered[1].Id;
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("Excluding {0} from search", elementId2);
                }
            }
            using (StreamWriter streamWriter4 = new StreamWriter(log, true))
            {
                streamWriter4.WriteLine("Exclusion List Count {0}", list.Count);
            }
            double num = searchBoxWidth;
            if (searchBoxDepth > num)
            {
                num = searchBoxDepth;
            }
            if (searchBoxHeight > num)
            {
                num = searchBoxHeight;
            }
            for (int i = 0; i <= noIterations; i++)
            {
                using (StreamWriter streamWriter5 = new StreamWriter(log, true))
                {
                    streamWriter5.WriteLine("Iteration {0}", i);
                }
                ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
                ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
                ElementCategoryFilter filter3 = new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting);
                ElementCategoryFilter filter4 = new ElementCategoryFilter(BuiltInCategory.OST_Sprinklers);
                LogicalOrFilter filter5 = new LogicalOrFilter(new LogicalOrFilter(new LogicalOrFilter(filter, filter3), filter2), filter4);
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list2 = filteredElementCollector.WherePasses(filter5).ToElements();
                XYZ minimumPoint = new XYZ(point.X - searchBoxWidth - 0.021 * i, point.Y - searchBoxDepth - 0.021 * i, point.Z - searchBoxHeight - 0.021 * i);
                XYZ maximumPoint = new XYZ(point.X + searchBoxWidth + 0.021 * i, point.Y + searchBoxDepth + 0.021 * i, point.Z + searchBoxHeight + 0.021 * i);
                BoundingBoxIntersectsFilter filter6 = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                if (list == null || list.Count == 0)
                {
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).ToElements();
                }
                else
                {
                    using (StreamWriter streamWriter6 = new StreamWriter(log, true))
                    {
                        streamWriter6.WriteLine("Excluding previous mates");
                    }
                    list2 = filteredElementCollector.WherePasses(filter6).WherePasses(filter5).ToElements();
                }
                using (StreamWriter streamWriter7 = new StreamWriter(log, true))
                {
                    streamWriter7.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list2.Count);
                }
                if (list2.Count != 0)
                {
                    Element element = null;
                    foreach (Element element2 in list2)
                    {
                        if (!(elementId == element2.Id) && !(elementId2 == element2.Id))
                        {
                            string text = element2.LookupParameter("Comments").AsString();
                            double num2 = double.MaxValue;
                            Connector connector = NearestConnector(ConnectorArray(element2), centerPoint);
                            XYZ xyz = connector.Origin - point;
                            double num3 = (connector.Origin - centerPoint).GetLength() - num;
                            using (StreamWriter streamWriter8 = new StreamWriter(log, true))
                            {
                                streamWriter8.WriteLine("mate ID: {0}", element2.Id);
                                streamWriter8.WriteLine("mateSystem: {0}", text);
                                streamWriter8.WriteLine("systemName: {0}", systemName);
                                streamWriter8.WriteLine("candidateDislocation: {0}", xyz);
                                streamWriter8.WriteLine("candidateCenterDisloc: {0}", num3);
                            }
                            if (testConnector)
                            {
                                if (connector.IsConnected)
                                {
                                    using (StreamWriter streamWriter9 = new StreamWriter(log, true))
                                    {
                                        streamWriter9.WriteLine("mate {0} is already connected", element2.Id);
                                        continue;
                                    }
                                }
                                if (xyz.GetLength() > 0.5)
                                {
                                    using (StreamWriter streamWriter10 = new StreamWriter(log, true))
                                    {
                                        streamWriter10.WriteLine("mate {0} is too far", element2.Id);
                                        continue;
                                    }
                                }
                            }
                            if (text != systemName && !text.ToUpper().Contains("BYPASS") && !systemName.ToUpper().Contains("BYPASS") && !systemName.ToUpper().Contains("VALVE") && element2.Category.Name != "Sprinklers")
                            {
                                using (StreamWriter streamWriter11 = new StreamWriter(log, true))
                                {
                                    streamWriter11.WriteLine("mate {0}'s comments doesn't match the system", element2.Id);
                                    continue;
                                }
                            }
                            if (num3 < num2)
                            {
                                element = element2;
                                num2 = num3;
                            }
                        }
                    }
                    if (element != null)
                    {
                        elementsFiltered.Add(element);
                        return elementsFiltered;
                    }
                }
            }
            using (StreamWriter streamWriter12 = new StreamWriter(log, true))
            {
                streamWriter12.WriteLine("Nothing Found, returning elementsFiltered");
            }
            return elementsFiltered;
        }

        public static List<Element> FindConduitWithBoundingBox(List<Element> elementsFiltered, XYZ point, XYZ centerPoint, double searchBoxWidth, double searchBoxDepth, double searchBoxHeight, int noIterations, string systemName, string log, Document doc, Autodesk.Revit.ApplicationServices.Application app, bool testConnector)
        {
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("FindMatesWithBoundingBox");
            }
            List<ElementId> list = new List<ElementId>();
            ElementId elementId = null;
            ElementId elementId2 = null;
            if (elementsFiltered.Count > 0)
            {
                list.Add(elementsFiltered[0].Id);
                elementId = elementsFiltered[0].Id;
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Excluding {0} from search", elementId);
                }
            }
            if (elementsFiltered.Count > 1)
            {
                list.Add(elementsFiltered[1].Id);
                elementId2 = elementsFiltered[1].Id;
                using (StreamWriter streamWriter3 = new StreamWriter(log, true))
                {
                    streamWriter3.WriteLine("Excluding {0} from search", elementId2);
                }
            }
            using (StreamWriter streamWriter4 = new StreamWriter(log, true))
            {
                streamWriter4.WriteLine("Exclusion List Count {0}", list.Count);
            }
            double num = searchBoxWidth;
            if (searchBoxDepth > num)
            {
                num = searchBoxDepth;
            }
            if (searchBoxHeight > num)
            {
            }
            for (int i = 0; i <= noIterations; i++)
            {
                using (StreamWriter streamWriter5 = new StreamWriter(log, true))
                {
                    streamWriter5.WriteLine("Iteration {0}", i);
                }
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IList<Element> list2 = filteredElementCollector.OfCategory(BuiltInCategory.OST_Conduit).ToElements();
                XYZ minimumPoint = new XYZ(point.X - searchBoxWidth - 0.021 * i, point.Y - searchBoxDepth - 0.021 * i, point.Z - searchBoxHeight - 0.021 * i);
                XYZ maximumPoint = new XYZ(point.X + searchBoxWidth + 0.021 * i, point.Y + searchBoxDepth + 0.021 * i, point.Z + searchBoxHeight + 0.021 * i);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(new Outline(minimumPoint, maximumPoint));
                if (list == null || list.Count == 0)
                {
                    list2 = filteredElementCollector.WherePasses(filter).OfCategory(BuiltInCategory.OST_Conduit).ToElements();
                }
                else
                {
                    using (StreamWriter streamWriter6 = new StreamWriter(log, true))
                    {
                        streamWriter6.WriteLine("Excluding previous mates");
                    }
                    list2 = filteredElementCollector.WherePasses(filter).OfCategory(BuiltInCategory.OST_Conduit).ToElements();
                }
                using (StreamWriter streamWriter7 = new StreamWriter(log, true))
                {
                    streamWriter7.WriteLine("Iteration {0} Finds {1} Possible Mates", i, list2.Count);
                }
                if (list2.Count != 0)
                {
                    foreach (Element element in list2)
                    {
                        if (!(elementId == element.Id) && !(elementId2 == element.Id))
                        {
                            string arg = element.LookupParameter("Comments").AsString();
                            Connector connector = NearestConnector(ConnectorArray(element), centerPoint);
                            using (StreamWriter streamWriter8 = new StreamWriter(log, true))
                            {
                                streamWriter8.WriteLine("mate ID: {0}", element.Id);
                                streamWriter8.WriteLine("mateSystem: {0}", arg);
                                streamWriter8.WriteLine("systemName: {0}", systemName);
                            }
                            if (testConnector && connector.IsConnected)
                            {
                                using (StreamWriter streamWriter9 = new StreamWriter(log, true))
                                {
                                    streamWriter9.WriteLine("mate {0} is already connected", element.Id);
                                    continue;
                                }
                            }
                            elementsFiltered.Add(element);
                        }
                    }
                    return elementsFiltered;
                }
            }
            using (StreamWriter streamWriter10 = new StreamWriter(log, true))
            {
                streamWriter10.WriteLine("Nothing Found, returning elementsFiltered");
            }
            return elementsFiltered;
        }

        public static Element CreateModelLine(XYZ p, XYZ q, Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            if (p.IsAlmostEqualTo(q))
            {
                throw new ArgumentException("Expected two different points.");
            }
            Line line = Line.CreateBound(p, q);
            if (null == line)
            {
                throw new Exception("Geometry line creation failed.");
            }
            return doc.Create.NewModelCurve(line, NewSketchPlanePassLine(line, doc, app));
        }

        public static Element CreateDetailLine(XYZ p, XYZ q, Document doc, Autodesk.Revit.DB.View view)
        {
            if (p.IsAlmostEqualTo(q))
            {
                throw new ArgumentException("Expected two different points.");
            }
            Line line = Line.CreateBound(new XYZ(p.X, p.Y, p.Z), new XYZ(q.X, q.Y, q.Z));
            if (null == line)
            {
                throw new Exception("Geometry line creation failed.");
            }
            return doc.Create.NewDetailCurve(view, line);
        }

        public static SketchPlane NewSketchPlanePassLine(Line line, Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            XYZ endPoint = line.GetEndPoint(0);
            XYZ endPoint2 = line.GetEndPoint(1);
            Plane plane = Plane.CreateByNormalAndOrigin(endPoint.CrossProduct(endPoint2), endPoint);
            return SketchPlane.Create(doc, plane);
        }

        public static SketchPlane NewSketchPlaneThreePoints(XYZ p, XYZ q, XYZ r, Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            XYZ xyz = q - p;
            XYZ source = r - p;
            Plane plane = Plane.CreateByNormalAndOrigin(xyz.CrossProduct(source), p);
            return SketchPlane.Create(doc, plane);
        }

        public static void CopyComments(Element selectedPipe, Element newPipe, FamilyInstance newElbow)
        {
            string value = "";
            try
            {
                value = selectedPipe.LookupParameter("Comments").AsString();
            }
            catch
            {
            }
            if (newPipe != null)
            {
                try
                {
                    newPipe.LookupParameter("Comments").Set(value);
                }
                catch
                {
                }
            }
            if (newElbow != null)
            {
                try
                {
                    newElbow.LookupParameter("Comments").Set(value);
                }
                catch
                {
                }
            }
        }

        public static void DrawCableTray(Document doc, SubTransaction sT, Reference selectedReference, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end, ElementId levelId)
        {
            try
            {
                sT.Start();
                CableTray cableTray = doc.GetElement(selectedReference) as CableTray;
                ElementId typeId = cableTray.GetTypeId();
                doc.GetElement(typeId);
                double value = cableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).AsDouble();
                double value2 = cableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).AsDouble();
                CableTray cableTray2 = CableTray.Create(doc, typeId, start, end, levelId);
                Connector[] cA = ConnectorArray(cableTray2);
                cableTray2.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).Set(value);
                cableTray2.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).Set(value2);
                Connector connector = NearestConnector(cA, selectedPoint);
                sT.Commit();
                sT.Start();
                FamilyInstance newElbow = doc.Create.NewElbowFitting(closestConnector1, connector);
                CopyComments(cableTray, cableTray2, newElbow);
                sT.Commit();
            }
            catch
            {
                sT.Commit();
            }
        }

        public static void DrawConduit(Document doc, SubTransaction sT, Reference selectedReference, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end, ElementId levelId)
        {
            try
            {
                sT.Start();
                Conduit conduit = doc.GetElement(selectedReference) as Conduit;
                ElementId typeId = conduit.GetTypeId();
                doc.GetElement(typeId);
                double diameter = conduit.Diameter;
                Conduit conduit2 = Conduit.Create(doc, typeId, start, end, levelId);
                Connector[] array = ConnectorArray(conduit2);
                Connector[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i].Radius = diameter / 2.0;
                }
                Connector connector = NearestConnector(array, selectedPoint);
                sT.Commit();
                sT.Start();
                FamilyInstance newElbow = doc.Create.NewElbowFitting(closestConnector1, connector);
                CopyComments(conduit, conduit2, newElbow);
                sT.Commit();
            }
            catch
            {
                sT.Commit();
            }
        }

        public static void DrawConduitPortAuthority(Document doc, SubTransaction sT, Reference selectedReference, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end, double radius, ElementId levelId)
        {
            try
            {
                sT.Start();
                Conduit conduit = doc.GetElement(selectedReference) as Conduit;
                ElementId typeId = conduit.GetTypeId();
                doc.GetElement(typeId);
                double diameter = conduit.Diameter;
                Conduit conduit2 = Conduit.Create(doc, typeId, start, end, levelId);
                Connector[] array = ConnectorArray(conduit2);
                Connector[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i].Radius = diameter / 2.0;
                }
                Connector connector = NearestConnector(array, selectedPoint);
                sT.Commit();
                sT.Start();
                FamilyInstance newElbow = doc.Create.NewElbowFitting(closestConnector1, connector);
                CopyComments(conduit, conduit2, newElbow);
                doc.Delete(conduit2.Id);
                sT.Commit();
            }
            catch
            {
                sT.Commit();
            }
        }

        public static Pipe DrawPipe(Document doc, PipeType pipeType, double pipeDiameter, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end)
        {
            Pipe result;
            try
            {
                PipingSystemType pipeSystem = GetPipeSystem(doc, closestConnector1);
                ElementId level = GetLevel(doc, selectedPoint.Z);
                Pipe pipe = Pipe.Create(doc, pipeSystem.Id, pipeType.Id, level, start, end);
                Connector[] array = ConnectorArray(pipe);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Radius = pipeDiameter / 2.0;
                }
                result = pipe;
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public static Pipe DrawPipeWithElbow(Document doc, Reference selectedReference, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end, bool includeElbow)
        {
            Pipe result;
            try
            {
                Pipe pipe = doc.GetElement(selectedReference) as Pipe;
                PipeType pipeType = pipe.PipeType;
                double diameter = pipe.Diameter;
                PipingSystemType pipeSystem = GetPipeSystem(doc, closestConnector1);
                ElementId level = GetLevel(doc, selectedPoint.Z);
                Pipe pipe2 = Pipe.Create(doc, pipeSystem.Id, pipeType.Id, level, start, end);
                Connector[] array = ConnectorArray(pipe2);
                Connector[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i].Radius = diameter / 2.0;
                }
                if (includeElbow)
                {
                    Connector connector = NearestConnector(array, selectedPoint);
                    FamilyInstance newElbow = doc.Create.NewElbowFitting(closestConnector1, connector);
                    CopyComments(pipe, pipe2, newElbow);
                }
                else
                {
                    CopyComments(pipe, pipe2, null);
                }
                result = pipe2;
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public static void DrawDuct(Document doc, SubTransaction sT, Reference selectedReference, XYZ selectedPoint, Connector closestConnector1, XYZ start, XYZ end)
        {
            try
            {
                sT.Start();
                Duct duct = doc.GetElement(selectedReference) as Duct;
                DuctType ductType = duct.DuctType;
                MechanicalSystemType mechanicalSystem = GetMechanicalSystem(doc, closestConnector1);
                ElementId level = GetLevel(doc, selectedPoint.Z);
                Duct duct2 = Duct.Create(doc, mechanicalSystem.Id, ductType.Id, level, start, end);
                Connector[] array = ConnectorArray(duct2);
                Connector connector = NearestConnector(array, selectedPoint);
                if (connector.Shape == ConnectorProfileType.Round)
                {
                    double diameter = duct.Diameter;
                    Connector[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        array2[i].Radius = diameter / 2.0;
                    }
                }
                else
                {
                    double width = duct.Width;
                    double height = duct.Height;
                    foreach (Connector connector2 in array)
                    {
                        connector2.Width = width;
                        connector2.Height = height;
                    }
                    if (start.X == end.X && start.Y == end.Y)
                    {
                        XYZ basisZ = closestConnector1.CoordinateSystem.BasisZ;
                        XYZ source = new XYZ(0.0, 1.0, 0.0);
                        double num = basisZ.AngleTo(source);
                        if (basisZ.DotProduct(source) > 0.0 && basisZ.X > 1.0)
                        {
                            num = -num;
                        }
                        Line axis = Line.CreateBound(start, end);
                        if (start.Z > end.Z)
                        {
                            axis = Line.CreateBound(end, start);
                        }
                        ElementTransformUtils.RotateElement(doc, duct2.Id, axis, num);
                    }
                }
                sT.Commit();
                sT.Start();
                FamilyInstance newElbow = doc.Create.NewElbowFitting(closestConnector1, connector);
                CopyComments(duct, duct2, newElbow);
                sT.Commit();
            }
            catch
            {
                sT.Commit();
            }
        }

        public static Element DrawElbow(Connector a, Connector b, Document doc, string layerName, PipeInsulationType pIT, double insulationThickness, string log, Transaction tr)
        {
            Element element = null;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Drawing Elbow");
            }
            try
            {
                element = doc.Create.NewElbowFitting(a, b);
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Elbow Created");
                }
                element.LookupParameter("Comments").Set(layerName);
                try
                {
                    if (pIT != null && insulationThickness > 0.0)
                    {
                        PipeInsulation.Create(doc, element.Id, pIT.Id, insulationThickness);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("Elbow Created");
            }
            return element;
        }

        public static Element DrawConduitElbow(Connector a, Connector b, Document doc, string layerName, string log, Transaction tr)
        {
            Element element = null;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Drawing Elbow");
            }
            try
            {
                element = doc.Create.NewElbowFitting(a, b);
            }
            catch
            {
                return null;
            }
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("Elbow Created: {0}", element.Id);
            }
            try
            {
                element.LookupParameter("Comments").Set(layerName);
            }
            catch
            {
                return element;
            }
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("Elbow Created");
            }
            return element;
        }

        public static Element DrawReducer(Connector a, Connector b, Document doc, string layerName, PipeInsulationType pIT, double insulationThickness, string log, Transaction tr)
        {
            Element element = null;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Drawing Reducer");
            }
            try
            {
                element = doc.Create.NewTransitionFitting(a, b);
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Reducer Created");
                }
                element.LookupParameter("Comments").Set(layerName);
                try
                {
                    if (insulationThickness > 0.0)
                    {
                        PipeInsulation.Create(doc, element.Id, pIT.Id, insulationThickness);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("Reducer Created");
            }
            return element;
        }

        public static Element DrawCoupling(Connector a, Connector b, Document doc, string layerName, PipeInsulationType pIT, double insulationThickness, string log, Transaction tr)
        {
            Element element = null;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Drawing Coupling");
            }
            try
            {
                element = doc.Create.NewUnionFitting(a, b);
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Coupling Created");
                }
                element.LookupParameter("Comments").Set(layerName);
                try
                {
                    if (insulationThickness > 0.0)
                    {
                        PipeInsulation.Create(doc, element.Id, pIT.Id, insulationThickness);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("Coupling Created");
            }
            return element;
        }

        public static Element DrawTee(Connector a, Connector b, Connector c, Document doc, string systemName, string log)
        {
            Element element = null;
            using (StreamWriter streamWriter = new StreamWriter(log, true))
            {
                streamWriter.WriteLine("Drawing Tee");
            }
            try
            {
                element = doc.Create.NewTeeFitting(a, b, c);
                using (StreamWriter streamWriter2 = new StreamWriter(log, true))
                {
                    streamWriter2.WriteLine("Tee Created");
                }
                element.LookupParameter("Comments").Set(systemName);
            }
            catch
            {
            }
            return element;
        }

        public static double GetExtensionLength(Connector connector)
        {
            double result = 1.0;
            if (connector.Domain == Domain.DomainHvac && connector.Shape == ConnectorProfileType.Rectangular)
            {
                result = 4.0 * connector.Width;
            }
            else if (connector.Domain == Domain.DomainHvac && connector.Shape == ConnectorProfileType.Oval)
            {
                result = 4.0 * connector.Width;
            }
            else if (connector.Domain == Domain.DomainHvac && connector.Shape == ConnectorProfileType.Round)
            {
                result = 4.0 * connector.Radius;
            }
            else if (connector.Domain == Domain.DomainCableTrayConduit)
            {
                result = 3.0;
            }
            else if (connector.Domain == Domain.DomainPiping)
            {
                result = 7.0 * connector.Radius;
            }
            return result;
        }

        public static void AlignColinearMEPElements(Element movingElement, Connector stationaryElementClosestConnector, Connector movingElementClosestConnector, Document doc)
        {
            XYZ origin = stationaryElementClosestConnector.Origin;
            XYZ origin2 = movingElementClosestConnector.Origin;
            XYZ basisZ = stationaryElementClosestConnector.CoordinateSystem.BasisZ;
            XYZ xyz = PerpIntersection(origin, origin + basisZ, origin2);
            XYZ xyz2 = xyz - origin2;
            if (!xyz2.IsZeroLength())
            {
                ElementTransformUtils.MoveElement(doc, movingElement.Id, xyz2);
            }
            basisZ = stationaryElementClosestConnector.CoordinateSystem.BasisZ;
            XYZ basisZ2 = movingElementClosestConnector.CoordinateSystem.BasisZ;
            if (basisZ.DotProduct(basisZ2) == -1.0)
            {
                return;
            }
            XYZ right = basisZ.CrossProduct(basisZ2);
            Line axis = Line.CreateBound(xyz, xyz + 1E+16 * right);
            double num = basisZ.AngleTo(basisZ2);
            double angle = 3.1415926535897931 - num;
            ElementTransformUtils.RotateElement(doc, movingElement.Id, axis, angle);
        }

        public static void AlignIntersectingMEPElements(Element movingElement, Connector stationaryElementClosestConnector, Connector movingElementClosestConnector, Connector stationaryElementFarthestConnector, Connector movingElementFarthestConnector, Document doc)
        {
            XYZ basisZ = stationaryElementClosestConnector.CoordinateSystem.BasisZ;
            XYZ origin = stationaryElementClosestConnector.Origin;
            XYZ origin2 = movingElementClosestConnector.Origin;
            XYZ pX = IntersectionTwoVectors(movingElementFarthestConnector.Origin, movingElementClosestConnector.Origin, stationaryElementFarthestConnector.Origin, stationaryElementClosestConnector.Origin);
            XYZ xyz = PerpIntersection(origin, origin + basisZ, pX);
            XYZ basisZ2 = movingElementFarthestConnector.CoordinateSystem.BasisZ;
            XYZ xyz2 = PerpIntersection(xyz, xyz + basisZ2, origin2) - origin2;
            if (xyz2.GetLength() == 0.0)
            {
                return;
            }
            ElementTransformUtils.MoveElement(doc, movingElement.Id, xyz2);
        }

        public static void WriteJournalData(ExternalCommandData commandData)
        {
            IDictionary<string, string> journalData = commandData.JournalData;
            journalData.Clear();
            journalData.Add("Name", "Autodesk.Revit");
            journalData.Add("Information", "This is an example.");
            journalData.Add("Greeting", "Hello Everyone.");
        }

        public static List<FileInfo> GetFamilyFiles(DirectoryInfo rootDirInfo, List<FileInfo> allFiles)
        {
            FileInfo[] array = null;
            try
            {
                array = rootDirInfo.GetFiles("*.rfa");
            }
            catch
            {
            }
            foreach (FileInfo item in array)
            {
                allFiles.Add(item);
            }
            DirectoryInfo[] directories = rootDirInfo.GetDirectories();
            for (int i = 0; i < directories.Length; i++)
            {
                allFiles = GetFamilyFiles(directories[i], allFiles);
            }
            return allFiles;
        }

        public static Category CreateSubCategory(Document doc, string subCatergoryName)
        {
            Category familyCategory = doc.OwnerFamily.FamilyCategory;
            return doc.Settings.Categories.NewSubcategory(familyCategory, subCatergoryName);
        }

        public static DefinitionGroup GetGroup(DefinitionFile myDefinitionFile, string groupName, string log)
        {
            DefinitionGroup definitionGroup = null;
            DefinitionGroups groups = myDefinitionFile.Groups;
            foreach (DefinitionGroup definitionGroup2 in groups)
            {
                if (definitionGroup2.Name == groupName)
                {
                    using (StreamWriter streamWriter = new StreamWriter(log, true))
                    {
                        streamWriter.WriteLine("Group {0} already exists", groupName);
                    }
                    definitionGroup = definitionGroup2;
                    return definitionGroup;
                }
            }
            using (StreamWriter streamWriter2 = new StreamWriter(log, true))
            {
                streamWriter2.WriteLine("Group {0} not found", groupName);
            }
            definitionGroup = groups.Create(groupName);
            using (StreamWriter streamWriter3 = new StreamWriter(log, true))
            {
                streamWriter3.WriteLine("Group {0} created", definitionGroup.Name);
            }
            return definitionGroup;
        }

        public static string OpenExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (.xlsx)|*.xlsx|(.xls)|*.xls";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }

        public static string[] OpenMultipleFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Excel Files (.dwg)|*.dwg";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.ShowDialog();
            return openFileDialog.FileNames;
        }

        public static string GetExcelFile()
        {
            string result = "C:\\default";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel File|*.xls";
            saveFileDialog.Title = "Save an Excel File";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                result = saveFileDialog.FileName;
            }
            return result;
        }

        public static void releaseObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}