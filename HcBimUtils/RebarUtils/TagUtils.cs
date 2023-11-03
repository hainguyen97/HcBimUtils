using Autodesk.Revit.DB;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils.RebarUtils
{
    public static class TagUtils
    {
        public static IndependentTag CreateIndependentTag(ElementId tagId, ElementId viewId, Reference rf, bool addLeader, TagOrientation orientation, XYZ point)
        {
            IndependentTag tag = null;
#if Version2017
         tag = AC.Document.Create.NewTag(viewId.ToElement() as View, rf.ToElement(), addLeader, TagMode.TM_ADDBY_CATEGORY,
             orientation, point);
         if (tagId != null)
         {
            tag.ChangeTypeId(tagId);
         }
#elif Version2018
         tag = IndependentTag.Create(AC.Document, viewId, rf, addLeader, TagMode.TM_ADDBY_CATEGORY, orientation, point);
         tag.ChangeTypeId(tagId);
#elif Version2023
         var rfString = $"{rf.ConvertToStableRepresentation(AC.Document)}:2000000:SUBELEMENT";
         var rff = Reference.ParseFromStableRepresentation(AC.Document, rfString);
         tag = IndependentTag.Create(AC.Document, tagId, viewId, rff, addLeader, orientation, point);

#else
            tag = IndependentTag.Create(AC.Document, tagId, viewId, rf, addLeader, orientation, point);
#endif
            return tag;
        }


        public static void SetLeaderElbow(this IndependentTag tag, XYZ point)
        {
#if R19 || R20 || R21 || R22

            tag.LeaderElbow = point;

#else
         tag.SetLeaderElbow(tag.GetTaggedReferences().FirstOrDefault(), point);
#endif
        }

        public static void SetLeaderEnd(this IndependentTag tag, XYZ point)
        {
#if R19 || R20 || R21 || R22
            tag.LeaderElbow = point;
#else
         tag.SetLeaderEnd(tag.GetTaggedReferences().FirstOrDefault(), point);
#endif
        }

        public static XYZ LeaderElbow(this IndependentTag tag)
        {
#if R19 || R20 || R21 || R22
            return tag.LeaderElbow;
#else

         return tag.GetLeaderElbow(tag.GetTaggedReferences().FirstOrDefault());
#endif
        }
    }
}