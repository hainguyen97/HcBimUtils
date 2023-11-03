using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HcBimUtils.SelectionFilter
{
    public static class ElementSelector
    {
        public static List<Element> GetAlreadySelectedElements(UIDocument uiDocument, ISelectionFilter filter = null)
        {
            List<Element> list = new List<Element>();
            foreach (ElementId elementId in uiDocument.Selection.GetElementIds())
            {
                Element element = uiDocument.Document.GetElement(elementId);
                if (filter != null)
                {
                    if (filter.AllowElement(element))
                    {
                        list.Add(element);
                    }
                }
                else
                {
                    list.Add(element);
                }
            }
            return list;
        }

        public static Element PickObject(UIDocument uiDocument, string selectionDescription, ISelectionFilter filter = null, bool useAlreadySelectedElements = true)
        {
            Element result = null;
            try
            {
                List<Element> list = new List<Element>();
                if (useAlreadySelectedElements)
                {
                    list = GetAlreadySelectedElements(uiDocument, filter);
                }
                if (list.Count == 1)
                {
                    result = list[0];
                }
                else
                {
                    if (filter == null)
                    {
                        Reference reference = uiDocument.Selection.PickObject(ObjectType.Element, selectionDescription);
                        result = uiDocument.Document.GetElement(reference);
                    }
                    else
                    {
                        Reference reference = uiDocument.Selection.PickObject(ObjectType.Element, filter, selectionDescription);
                        result = uiDocument.Document.GetElement(reference);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }
    }
}