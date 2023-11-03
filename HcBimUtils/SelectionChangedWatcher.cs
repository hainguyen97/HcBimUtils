using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace HcBimUtils
{
    public class SelectionChangedWatcher
    {
        public event EventHandler SelectionChanged;

        public List<ElementId> Selection { get; set; }
        private List<int> _lastSelIds;

        public SelectionChangedWatcher(UIControlledApplication a)
        {
            a.Idling += new EventHandler<IdlingEventArgs>(OnIdling);
        }

        private void OnIdling(object sender, IdlingEventArgs e)
        {
            UIApplication uiapp = sender as UIApplication;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            if (uidoc != null)
            {
                var selected = uidoc.Selection.GetElementIds();
                if (selected.Count == 0)
                {
                }
                else // elements are selected
                {
                    if (null == Selection)
                    {
                        // previous selection was null, report change

                        HandleSelectionChanged(selected);
                    }
                    else
                    {
                        if (Selection.Count != selected.Count)
                        {
                            // size has changed, no need to check
                            // selection IDs, report the change

                            HandleSelectionChanged(selected);
                        }
                        else
                        {
                            // count is the same...
                            // compare IDs to see if selection has changed
                            if (SelectionHasChanged(selected))
                            {
                                HandleSelectionChanged(selected);
                            }
                        }
                    }
                }
            }
        }

        private bool SelectionHasChanged(ICollection<ElementId> selected)

        {
            // we have already determined that the size of
            // "selected" is the same as the last selection...

            int i = 0;
            foreach (ElementId e in selected)
            {
#if R24
                if (_lastSelIds[i] != e.Value)
                {
                    return true;
                }
#else
            if (_lastSelIds[i] != e.IntegerValue)
            {
               return true;
            }
#endif

                ++i;
            }

            return false;
        }

        private void HandleSelectionChanged(ICollection<ElementId> selected)
        {
#if R24
#else
            Selection = new List<ElementId>();

            _lastSelIds = new List<int>();

            foreach (ElementId e in selected)
            {
                Selection.Add(e);
                _lastSelIds.Add(e.IntegerValue);
            }

            Call_SelectionChanged();
#endif
        }

        private void Call_SelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }
    }
}
