using Autodesk.Revit.UI;

namespace HcBimUtils.DocumentUtils
{
    public class ExternalEventHandler : IExternalEventHandler
    {
        protected static ExternalEventHandler instance { get; set; }

        public static ExternalEventHandler Instance => instance ??= new ExternalEventHandler();

        protected static ExternalEvent create { get; set; }

        public ExternalEvent Create()
        {
            if (create == null)
            {
                create = ExternalEvent.Create(Instance);
            }
            return create;
        }

        protected static Action Action;

        public void SetAction(Action parameter)
        {
            Action = parameter;
        }

        public async void Run()
        {
            create.Raise();

            while (create.IsPending)
            {
                await Task.Delay(10);
            }
        }

        public void Execute(UIApplication app)
        {
            var uiDoc = app.ActiveUIDocument;

            if (uiDoc == null)
            {
                TaskDialog.Show("Notification", " no document, nothing to do");
                return;
            }

            Action();
        }

        public string GetName()
        {
            return "HCTools";
        }
    }

    public class ExternalEventHandlers : IExternalEventHandler
    {
        public List<Action> Actions { get; set; } = new();

        public void Execute(UIApplication app)
        {
            Actions.ForEach(x => x());
        }

        public string GetName()
        {
            return "External Events";
        }
    }
}