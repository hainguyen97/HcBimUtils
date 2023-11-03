using Autodesk.Revit.DB;

namespace HcBimUtils.WarmingUtils
{
    public static class FailureUtil
    {
        public static void SetFailuresPreprocessorInTransaction(this Transaction tx)
        {
            var failuresPreprocessor = new FuncFailuresPreprocessor()
            {
                FuncAccessor = x => PreprocessFailures(x)
            };

            var failureHandlingOptions = tx.GetFailureHandlingOptions();

            failureHandlingOptions.SetFailuresPreprocessor(failuresPreprocessor);

            failureHandlingOptions.SetClearAfterRollback(true);

            tx.SetFailureHandlingOptions(failureHandlingOptions);
        }
        public static void SetIgnoreWarning(this Transaction tx)
        {
            FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new IgnoreWarning());
            failOpt.SetClearAfterRollback(true);
            tx.SetFailureHandlingOptions(failOpt);
        }
        private static FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessageAccessors = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessors)
            {
                FailureDefinitionId fdId = failureMessageAccessor.GetFailureDefinitionId();

                if (null != fdId)
                {
                    failuresAccessor.DeleteWarning(failureMessageAccessor);
                }

                //Xóa
                // failuresAccessor.ResolveFailure(failureMessageAccessor);
                // return Autodesk.Revit.DB.FailureProcessingResult.ProceedWithCommit;
            }
            return FailureProcessingResult.Continue;
        }

        public class WarningDiscard : IFailuresPreprocessor
        {
            FailureProcessingResult IFailuresPreprocessor.PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                String transactionName = failuresAccessor.GetTransactionName();

                IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();

                if (fmas.Count == 0)
                {
                    return FailureProcessingResult.Continue;
                }

                bool isResolved = false;

                foreach (FailureMessageAccessor fma in fmas)
                {
                    if (fma.HasResolutions())
                    {
                        failuresAccessor.ResolveFailure(fma);
                        isResolved = true;
                    }
                }

                if (isResolved)
                {
                    return FailureProcessingResult.ProceedWithCommit;
                }

                return FailureProcessingResult.Continue;
            }
        }
    }

    public class FuncFailuresPreprocessor : IFailuresPreprocessor
    {
        public Func<FailuresAccessor, FailureProcessingResult> FuncAccessor { get; set; }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            return FuncAccessor(failuresAccessor);
        }
    }
    public class IgnoreWarning : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            string transName = failuresAccessor.GetTransactionName();
            IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();
            // Inside event handler, get all warnings                
            if (failList.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }
            foreach (FailureMessageAccessor failure in failList)
            {
                bool hasResolutions = failure.HasResolutions();
                ICollection<ElementId> failureIds = failure.GetFailingElementIds();
                ICollection<ElementId> additionIds = failure.GetAdditionalElementIds();
                //FailureResolutionType failureResolutionType = failure.GetCurrentResolutionType();
                string defaultResolutionCaption = failure.GetDefaultResolutionCaption();
                string descriptionText = failure.GetDescriptionText();
                int numberOfResolutions = failure.GetNumberOfResolutions();
                FailureDefinitionId failureDefinitionId = failure.GetFailureDefinitionId();
                FailureMessage failureMessage = failure.CloneFailureMessage();
                FailureSeverity s = failure.GetSeverity();
                if (s == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
                else if (s == FailureSeverity.Error)
                {
                    failuresAccessor.ResolveFailure(failure);
                }
            }
            return FailureProcessingResult.ProceedWithCommit;
        }
    }
}