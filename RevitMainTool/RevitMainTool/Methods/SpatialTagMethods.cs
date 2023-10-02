using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Diagnostics;

namespace RevitMainTool
{
    

    public static class SpatialTagMethods
    {
        public static void AlignTagsLeft(List<SpatialElementTag> tags)
        {
            if (tags.Count == 0)
            {
                return;
            }

            SpatialElementTag mostLeftTag = tags.OrderBy(tag => tag.TagHeadPosition.X).First();
            double leftX = mostLeftTag.TagHeadPosition.X;

            foreach (SpatialElementTag tag in tags)
            {
                if (!tag.HasLeader)
                {
                    tag.HasLeader = true;
                }

                MoveTagHeadToPoint(tag, new XYZ(leftX, tag.TagHeadPosition.Y, tag.TagHeadPosition.Z));
            }
        }

        public static void SpaceTagsEvenly(List<SpatialElementTag> tags, XOrY xOrY)
        {
            switch (xOrY)
            {
                case XOrY.XThenY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.X).ThenBy(p => p.TagHeadPosition.Y).ToList();
                    break;
                case XOrY.YThenX:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y).ThenBy(p => p.TagHeadPosition.X).ToList();
                    break;
                case XOrY.SumOfXandY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y + p.TagHeadPosition.X).ToList();
                    break;
            }

            

            SpatialElementTag firstTag = tags[0];
            SpatialElementTag lastTag = tags[tags.Count - 1];

            XYZ firstTagHeadPoint = firstTag.TagHeadPosition;

            XYZ distanceAsPoint = lastTag.TagHeadPosition.Subtract(firstTagHeadPoint);

            XYZ Segment = distanceAsPoint.Divide(tags.Count - 1);

            for (int i = 0; i < tags.Count; i++)
            {
                SpatialElementTag tag = tags[i];

                var newPoint = firstTagHeadPoint.Add(Segment.Multiply(i));

                MoveTagHeadToPoint(tag, newPoint);
            }
        }

        public static void MoveTagLocationToPoint(SpatialElementTag tag, XYZ Point)
        {
            XYZ tagLocation = ((LocationPoint)tag.Location).Point;
            XYZ translation = Point.Subtract(tagLocation);

            tag.Location.Move(translation);
        }

        public static void MoveTagHeadToPoint(SpatialElementTag tag, XYZ Point)
        {
            tag.TagHeadPosition = Point;

            //Wiggle to make sure Revit registers the change. Otherwise it doesn't for some reason
            tag.Location.Move(new XYZ(2, 0, 0));
            tag.Location.Move(new XYZ(-2, 0, 0));
        }



    }
}
