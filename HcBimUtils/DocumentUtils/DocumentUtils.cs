using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace HcBimUtils.DocumentUtils
{
    public static class DocumentUtils
    {
        public static FamilySymbol GetFamilySymbol(string symbol, string familyName)
        {
            var sb = new FilteredElementCollector(AC.Document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                .FirstOrDefault(x => x.Name == symbol && x.FamilyName == familyName);
            return sb;
        }

        public static List<Element> AllElementsByCategory(BuiltInCategory builtInCategory, bool activeView = false)
        {
            if (activeView)
            {
                var elements = new FilteredElementCollector(AC.Document, AC.ActiveView.Id).WhereElementIsNotElementType().OfCategory(builtInCategory).ToElements().ToList();
                return elements;
            }
            else
            {
                var elements = new FilteredElementCollector(AC.Document).WhereElementIsNotElementType().OfCategory(builtInCategory).ToElements().ToList();
                return elements;
            }
        }

        public static BuiltInCategory ToBuiltinCategory(this Category cat)
        {
            var result = BuiltInCategory.INVALID;
            if (cat == null)
            {
                return result;
            }
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

        public static Element ToElement(this Reference rf)
        {
            return AC.Document.GetElement(rf);
        }

        public static Element ToElement(this int id)
        {
            return AC.Document.GetElement(new ElementId(id));
        }

        public static Element ToElement(this ElementId id)
        {
            return AC.Document.GetElement(id);
        }

        public static List<Element> ToElements(this List<Reference> rfs)
        {
            return rfs.Select(x => x.ToElement()).ToList();
        }

        public static List<T> GetElementsOfType<T>(this Document document, IEnumerable<ElementId> elementIds) where T : class
        {
            var list = new List<T>();
            foreach (var elementId in elementIds)
            {
                var element = document.GetElement(elementId);
                if (element is T element1)
                {
                    list.Add(element1);
                }
            }
            return list;
        }

        public static List<T> GetElements<T>(this Document document) where T : Element
        {
            return new FilteredElementCollector(document).OfClass(typeof(T)).ToElements().Cast<T>().ToList();
        }

        public static List<ElementId> GetElementsIds<T>(this Document document) where T : Element
        {
            return new FilteredElementCollector(document).OfClass(typeof(T)).ToElementIds().ToList();
        }

        public static List<ElementId> GetElementsIds(this Document revitDocument, Type rvtEltType)
        {
            var list = new List<ElementId>();
            var filteredElementCollector = new FilteredElementCollector(revitDocument);
            filteredElementCollector.OfClass(rvtEltType);
            var elementIterator = filteredElementCollector.GetElementIterator();
            elementIterator.Reset();
            while (elementIterator.MoveNext())
            {
                var element = elementIterator.Current;
                if (element != null)
                {
                    list.Add(element.Id);
                }
            }
            return list;
        }

        public static IList<Element> GetElementsOfCategory(this Document document, BuiltInCategory bic)
        {
            return new FilteredElementCollector(document).OfCategory(bic).ToElements();
        }

        public static IList<Element> GetElementsOfCategory(this Document document, Category bic)
        {
            return new FilteredElementCollector(document).OfCategoryId(bic.Id).ToElements();
        }

        public static IList<Element> FilterElements(this Document document, params Type[] types)
        {
            var filteredElementCollector = new FilteredElementCollector(document);
            IList<ElementFilter> list = (from type in types select new ElementClassFilter(type)).Cast<ElementFilter>().ToList();
            var logicalOrFilter = new LogicalOrFilter(list);
            filteredElementCollector.WherePasses(logicalOrFilter);
            return filteredElementCollector.WhereElementIsNotElementType().ToElements();
        }

        public static IList<Element> FilterElements(this Document document, ElementId viewId, List<Type> types)
        {
            var filteredElementCollector = null == viewId ? new FilteredElementCollector(document) : new FilteredElementCollector(document, viewId);
            IList<ElementFilter> list = (from type in types select new ElementClassFilter(type)).Cast<ElementFilter>().ToList();
            var logicalOrFilter = new LogicalOrFilter(list);
            filteredElementCollector.WherePasses(logicalOrFilter);
            return filteredElementCollector.WhereElementIsNotElementType().ToElements();
        }

        public static IList<Element> FilterElements(this Document document, params BuiltInCategory[] categories)
        {
            var filteredElementCollector = new FilteredElementCollector(document);
            IList<ElementFilter> list = (from builtInCategory in categories select new ElementCategoryFilter(builtInCategory)).Cast<ElementFilter>().ToList();
            var logicalOrFilter = new LogicalOrFilter(list);
            filteredElementCollector.WherePasses(logicalOrFilter);
            return filteredElementCollector.WhereElementIsNotElementType().ToElements();
        }

        public static List<ElementId> FilterElementsIds(this Document document, params BuiltInCategory[] categories)
        {
            var filteredElementCollector = new FilteredElementCollector(document);
            IList<ElementFilter> list = (from builtInCategory in categories select new ElementCategoryFilter(builtInCategory)).Cast<ElementFilter>().ToList();
            var logicalOrFilter = new LogicalOrFilter(list);
            filteredElementCollector.WherePasses(logicalOrFilter);
            return filteredElementCollector.WhereElementIsNotElementType().ToElementIds().ToList();
        }

        public static IList<Element> FilterElements(this Document document, ElementId viewId, params BuiltInCategory[] categories)
        {
            var filteredElementCollector = !(null == viewId) ? new FilteredElementCollector(document, viewId) : new FilteredElementCollector(document);
            IList<ElementFilter> list = (from builtInCategory in categories select new ElementCategoryFilter(builtInCategory)).Cast<ElementFilter>().ToList();
            var logicalOrFilter = new LogicalOrFilter(list);
            filteredElementCollector.WherePasses(logicalOrFilter);
            return filteredElementCollector.WhereElementIsNotElementType().ToElements();
        }

        public static T GetValidElementOrNull<T>(this Document document, ElementId id) where T : Element
        {
            if (id == null) return default;
            if (document.GetElement(id) is T { IsValidObject: true } t)
            {
                return t;
            }
            return default;
        }

        public static List<T> GetElementsFromView<T>(Document document, ElementId viewId) where T : Element
        {
            return new FilteredElementCollector(document, viewId).OfClass(typeof(T)).Cast<T>().ToList();
        }

        public static List<Material> GetAllRevitMaterials(this Document doc)
        {
            if (doc == null)
            {
                return null;
            }
            var filteredElementCollector = new FilteredElementCollector(doc);
            var enumerable = filteredElementCollector.WherePasses(new ElementClassFilter(typeof(Material))).Cast<Material>();
            return enumerable.ToList();
        }

        public static List<RebarShape> GetAllRevitRebarShapes(this Document doc)
        {
            if (doc == null)
            {
                return null;
            }
            var filteredElementCollector = new FilteredElementCollector(doc);
            var enumerable = filteredElementCollector.WherePasses(new ElementClassFilter(typeof(RebarShape))).Cast<RebarShape>();

            return enumerable.ToList();
        }

        public static List<Element> IdsToElements(IEnumerable<ElementId> ids)
        {
            return (from elementId in ids select AC.Document.GetElement(elementId)).ToList();
        }

        public static List<FamilyInstance> AllFamilyInstanceOfTypeInActiveView(this Document doc, string symbolName)
        {
            var col = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().Cast<FamilyInstance>();
            return col.Where(x => x.Symbol.Name == symbolName).ToList();
        }

        public static List<Element> GetFromModelByBiCs(Document doc, BuiltInCategory[] builtInCategories)
        {
            IList<ElementFilter> elementFilterList = new List<ElementFilter>(builtInCategories.Count());
            foreach (var bic in builtInCategories)
            {
                elementFilterList.Add(new ElementCategoryFilter(bic));
            }
            var logicalOrFilter = new LogicalOrFilter(elementFilterList);
            var elementCollector = new FilteredElementCollector(doc);
            elementCollector.WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            return elementCollector.ToList();
        }

        public static List<Element> GetFromViewByBiCs(this Document doc, View v, BuiltInCategory[] builtInCategories)
        {
            IList<ElementFilter> elementFilterList = new List<ElementFilter>(builtInCategories.Length);
            foreach (var builtInCategory1 in builtInCategories)
            {
                var bic = (int)builtInCategory1;
                var builtInCategory = (BuiltInCategory)bic;
                elementFilterList.Add(new ElementCategoryFilter(builtInCategory));
            }
            var logicalOrFilter = new LogicalOrFilter(elementFilterList);
            var elementCollector = new FilteredElementCollector(doc, v.Id);
            elementCollector.WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            return elementCollector.ToList();
        }

        public static List<Element> GetFromActiveViewByBiCs(Document doc, BuiltInCategory[] builtInCategories)
        {
            return GetFromViewByBiCs(doc, doc.ActiveView, builtInCategories);
        }

        public static List<ViewSheet> AllViewSheet(this Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements().Cast<ViewSheet>().ToList();
        }

        public static List<ScheduleSheetInstance> AllScheduleSheetInstancesInView(this Document doc, ViewSheet viewSheet)
        {
            return new FilteredElementCollector(doc, viewSheet.Id).OfClass(typeof(ScheduleSheetInstance)).WhereElementIsNotElementType().Cast<ScheduleSheetInstance>().ToList();
        }
    }
}