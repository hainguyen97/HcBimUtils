using System.Diagnostics ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Plumbing ;
using Autodesk.Revit.UI ;

namespace HcBimUtils.Models
{
   public class R_Pipe : R_MEPCurve
   {
      #region Fields

      private Pipe m_Pipe;

      #endregion Fields

      #region Properties

      public Pipe Pipe => m_Pipe;

      public double Diameter
      {
         get => m_Pipe.Diameter;
         set
         {
            Parameter paraDiameter = m_Pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            if (paraDiameter != null)
            {
               paraDiameter.Set(value);
            }
         }
      }

      public ElementId SystemTypeId
      {
         get
         {
            Parameter pstypeLevel = m_Pipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            return pstypeLevel.AsElementId();
         }
      }

      public PipeType PipeType
      {
         get => m_Pipe.PipeType;
         set { m_Pipe.PipeType = value; }
      }

      public double InsulationThickness
      {
         get
         {
            double thickness = 0;
            foreach (var id in Insulations)
            {
               PipeInsulation pipeInsulation = Doc.GetElement(id) as PipeInsulation;
               thickness += pipeInsulation.Thickness;
            }
            return thickness;
         }
      }

      #endregion Properties

      #region Constructors

      public R_Pipe(Pipe pipe) : base(pipe)
      {
         if (pipe != null && pipe.IsValidObject)
         {
            m_Pipe = pipe;
         }
         else
         {
            throw new Exception("Pipe is null");
         }
      }

      public R_Pipe CreatePipe(XYZ start, XYZ end)
      {
         Pipe newPipe = null;
         R_Pipe newR_Pipe = null;
         try
         {
            newPipe = Pipe.Create(Doc, SystemTypeId, PipeType.Id, ReferenceLevel.Id, start, end);
            newR_Pipe = new R_Pipe(newPipe);
         }
         catch (Exception ex)
         {
            TaskDialog.Show("Error", ex.Message);
         }

         if (newPipe != null)
         {
            newR_Pipe.Diameter = Diameter;
         }
         return newR_Pipe;
      }

      public R_Pipe CreatePipe(PipingSystemType psType, PipeType pType, Level level, XYZ start, XYZ end)
      {
         Pipe newPipe = null;
         R_Pipe newR_Pipe = null;
         try
         {
            newPipe = Pipe.Create(Doc, psType.Id, pType.Id, level.Id, start, end);
            newR_Pipe = new R_Pipe(newPipe);
         }
         catch (Exception ex)
         {
            TaskDialog.Show("Error", ex.Message);
         }

         if (newPipe != null)
         {
            newR_Pipe.Diameter = Diameter;
         }
         return newR_Pipe;
      }

      #endregion Constructors

      #region Functions

      public override R_MEPCurve BreakCurve(XYZ point)
      {
         if (Doc != null && m_Pipe != null && point != null)
         {
            Transaction tr = new Transaction(Doc, "Break Curve");
            try
            {
               tr.Start();
            }
            catch { }
            try
            {
               ElementId newSplit = PlumbingUtils.BreakCurve(Doc, Id, point);
               if (tr.HasStarted())
               {
                  tr.Commit();
               }
               return new R_Pipe(Doc.GetElement(newSplit) as Pipe);
            }
            catch (Exception ex)
            {
               if (tr.HasStarted())
               {
                  tr.RollBack();
               }
               Debug.Print(ex.Message);
            }
         }
         return null;
      }

      public override ElementId GetInsulationTypeId()
      {
         if (Insulations != null && Insulations.Count > 0)
         {
            PipeInsulation pi = Doc.GetElement(Insulations.FirstOrDefault()) as PipeInsulation;
            return pi.GetTypeId();
         }
         return null;
      }

      public override void CopyInsulation(Element ele)
      {
         if (Insulations != null && Insulations.Count > 0 && ele.IsValidElement())
         {
            PipeInsulation di = Doc.GetElement(Insulations.FirstOrDefault()) as PipeInsulation;
            PipeInsulation.Create(Doc, ele.Id, di.GetTypeId(), InsulationThickness);
         }
      }

      public override InsulationLiningBase CreateInsulation(ElementId insulationTypeId, double insulationThickness)
      {
         return PipeInsulation.Create(Doc, Id, insulationTypeId, insulationThickness);
      }

      #endregion Functions
   }
}