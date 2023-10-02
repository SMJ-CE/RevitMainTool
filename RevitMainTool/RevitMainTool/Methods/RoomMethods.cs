using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;


namespace RevitMainTool
{
    public static class RoomMethods
    {
        public static Room TryMoveRoomLocationToCenter(Room room)
        {
            return TryMoveRoomLocationToCenter(room, ViewMethods.CreateViewForRay(room.Document));
        }

        public static Room TryMoveRoomLocationToCenter(Room room, View3D view)
        {
            MoveRoomLocationToCentroid(room.Document, room);
            Room output = AlignRoomXY(room, view);

            return output;
        }


        public static Room AlignRoomXY(Room room, View3D view)
        {
            var test = new ReferenceIntersector(view);
            test.FindReferencesInRevitLinks = true;

            XYZ roomLocation = (room.Location as LocationPoint).Point;

            var Right = test.FindNearest(roomLocation, new XYZ(1, 0, 0)).Proximity;
            var Left = test.FindNearest(roomLocation, new XYZ(-1, 0, 0)).Proximity;

            var up = test.FindNearest(roomLocation, new XYZ(0, 1, 0)).Proximity;
            var down = test.FindNearest(roomLocation, new XYZ(0, -1, 0)).Proximity;

            double halfOfTotalX = Left - ((Right + Left) / 2);
            double halfOfTotalY = down - ((up + down) / 2);

            XYZ newPoint = new XYZ(roomLocation.X - halfOfTotalX, roomLocation.Y - halfOfTotalY, roomLocation.Z + (room.UnboundedHeight / 2));

            if (room.IsPointInRoom(newPoint))
            {
                XYZ translation = newPoint.Subtract(roomLocation);
                room.Location.Move(translation);
            }
            else
            {
                AlignRoomX(room, view);

            }

            return room;
        }


        public static Room AlignRoomX(Room room, View3D view)
        {
            var test = new ReferenceIntersector(view);
            test.FindReferencesInRevitLinks = true;

            XYZ roomLocation = (room.Location as LocationPoint).Point;

            var Right = test.FindNearest(roomLocation, new XYZ(1, 0, 0)).Proximity;
            var Left = test.FindNearest(roomLocation, new XYZ(-1, 0, 0)).Proximity;

            double halfOfTotal = Left - ((Right + Left) / 2);

            XYZ newPoint = new XYZ(roomLocation.X - halfOfTotal, roomLocation.Y, roomLocation.Z + (room.UnboundedHeight / 2));

            if (room.IsPointInRoom(newPoint))
            {
                XYZ translation = newPoint.Subtract(roomLocation);
                room.Location.Move(translation);
            }


            return room;
        }

        public static Room AlignRoomY(Room room, View3D view)
        {
            var test = new ReferenceIntersector(view);
            test.FindReferencesInRevitLinks = true;

            XYZ roomLocation = (room.Location as LocationPoint).Point;

            var up = test.FindNearest(roomLocation, new XYZ(0, 1, 0)).Proximity;
            var down = test.FindNearest(roomLocation, new XYZ(0, -1, 0)).Proximity;

            double halfOfTotal = down - ((up + down) / 2);

            XYZ newPoint = new XYZ(roomLocation.X, roomLocation.Y - halfOfTotal, roomLocation.Z + (room.UnboundedHeight / 2));

            if (room.IsPointInRoom(newPoint))
            {
                XYZ translation = newPoint.Subtract(roomLocation);
                room.Location.Move(translation);
            }


            return room;
        }


        public static Room MoveRoomLocationToCentroid(Document doc, Room room)
        {
            if (room == null || room.Area == 0)
            {
                return null;
            }

            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);

            Solid roomSolid = results.GetGeometry();
            PlanarFace planar = null;

            foreach (Face face in roomSolid.Faces)
            {
                PlanarFace pf = face as PlanarFace;
                if (pf != null)
                {
                    if (pf.FaceNormal.Z == -1)
                    {
                        planar = pf;
                        break;
                    }
                }
            }

            if (planar == null)
            {
                return null;
            }

            EdgeArray edges = planar.EdgeLoops.get_Item(0);
            List<XYZ> test = new List<XYZ>();

            foreach (Edge edge in edges)
            {
                test.Add(edge.AsCurve().GetEndPoint(0));
                test.Add(edge.AsCurve().GetEndPoint(1));
            }

            var maxX = test.Max(i => i.X);
            var maxY = test.Max(i => i.Y);
            var maxZ = test.Max(i => i.Z);

            var minX = test.Min(i => i.X);
            var minY = test.Min(i => i.Y);
            var minZ = test.Min(i => i.Z);

            var x = (maxX + minX) / 2;
            var y = (maxY + minY) / 2;
            var z = (maxZ + minZ) / 2;

            XYZ roomLocation = (room.Location as LocationPoint).Point;
            XYZ centeroid = new XYZ(x, y, z);

            if (room.IsPointInRoom(centeroid))
            {
                XYZ translation = centeroid.Subtract(roomLocation);
                room.Location.Move(translation);
            }

            return room;
        }


        public static Room CreateOrUpdateRoomFromLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            string name = roomFromLinkedFile.LookupParameter("Name").AsValueString();
            string number = roomFromLinkedFile.LookupParameter("Number").AsValueString();
            Room roomAtPoint = WhatRoomHasPoint(currentDoc, roomFromLinkedFile);

            if (roomAtPoint != null)
            {
                roomAtPoint.Name = name;
                roomAtPoint.Number = number;
            }
            else
            {
                CreateRoomFromLinkedFile(currentDoc, roomFromLinkedFile);
            }


            return null;
        }


        public static Room CreateRoomFromLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            Level levelInLinkedModel = roomFromLinkedFile.Level;
            Level theChosenLevel = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, levelInLinkedModel, roomFromLinkedFile.BaseOffset);

            double theDifference = levelInLinkedModel.Elevation - theChosenLevel.Elevation;

            if (theChosenLevel != null)
            {
                XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
                Room createdRoom = currentDoc.Create.NewRoom(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));

                createdRoom.Name = roomFromLinkedFile.Name;
                createdRoom.Number = roomFromLinkedFile.Number;

                Level upperLevelLimitInLinked = roomFromLinkedFile.UpperLimit;
                double limitOffsetInLinked = roomFromLinkedFile.LimitOffset;
                double baseOffset = roomFromLinkedFile.BaseOffset + theDifference;

                createdRoom.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(baseOffset);

                if (upperLevelLimitInLinked != null)
                {
                    Level upperLevelInCurrent = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, upperLevelLimitInLinked, limitOffsetInLinked);

                    if (upperLevelInCurrent != null)
                    {
                        limitOffsetInLinked += upperLevelLimitInLinked.Elevation - upperLevelInCurrent.Elevation;
                        Parameter limitOffset = createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                        limitOffset.Set(limitOffsetInLinked);
                        createdRoom.UpperLimit = upperLevelInCurrent;
                    }
                    else
                    {
                        double limtThingy = limitOffsetInLinked + (upperLevelLimitInLinked.Elevation - theChosenLevel.Elevation);
                        createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(limtThingy);
                    }
                }
                else
                {
                    createdRoom.get_Parameter(BuiltInParameter.OFFSET_FROM_REFERENCE_BASE).Set(limitOffsetInLinked + theDifference);
                }
            }

            return null;
        }


        public static Room WhatRoomHasPoint(Document doc, Room roomFromLinkedFile)
        {
            XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
            double halfHeight = roomFromLinkedFile.UnboundedHeight / 2;
            List<XYZ> points = new List<XYZ>() { xyzPoint, new XYZ(xyzPoint.X, xyzPoint.Y, xyzPoint.Z + halfHeight) };
            Room roomAtPoint = WhatRoomHasPoint(doc, points);

            return roomAtPoint;
        }

        public static Room WhatRoomHasPoint(Document doc, List<XYZ> points)
        {
            Room test = null;
            foreach (XYZ point in points)
            {
                test = WhatRoomHasPoint(doc, point);
                if (test != null)
                {
                    return test;
                }
            }

            return test;
        }

        public static Room WhatRoomHasPoint(Document doc, XYZ point)
        {
            var rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            Room output = null;

            foreach (Room room in rooms)
            {
                if (room.IsPointInRoom(point))
                {
                    output = room;
                }
            }

            return output;
        }

        public static bool IsPointWithinARoom(Document doc, XYZ point)
        {
            var rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Room room in rooms)
            {
                if (room.IsPointInRoom(point))
                {
                    return true;
                }
            }
            return false;
        }



        public static Room WhatRoomIsAlmostTheSame(Document currentDoc, Room roomFromLinkedFile)
        {
            var rooms = new FilteredElementCollector(currentDoc).OfCategory(BuiltInCategory.OST_Rooms);
            double variance = 0.10;
            double LinkedRoomArea = roomFromLinkedFile.Area;
            double curremtRoomArea;
            XYZ linkedRoomPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
            XYZ currentRoomPoint;

            Room output = WhatRoomHasPoint(currentDoc, linkedRoomPoint);

            if (output == null)
            {
                foreach (Room room in rooms.Cast<Room>())
                {
                    curremtRoomArea = room.Area;
                    currentRoomPoint = (room.Location as LocationPoint).Point;

                    bool test1 = Math.Abs(curremtRoomArea - LinkedRoomArea) <= (variance * LinkedRoomArea);
                    bool test2 = linkedRoomPoint.IsAlmostEqualTo(currentRoomPoint, variance);
                    bool test3 = true;

                    View that = currentDoc.ActiveView;

                    if (that is View3D)
                    {

                        var test = new ReferenceIntersector(that as View3D);
                        test.FindReferencesInRevitLinks = true;

                        var objStuff = test.Find(linkedRoomPoint, currentRoomPoint);

                        foreach (var obj in objStuff)
                        {
                            var eleId = obj.GetReference().ElementId;
                            var ele = currentDoc.GetElement(eleId);
                            if (ele is RevitLinkInstance instance)
                            {
                                eleId = obj.GetReference().LinkedElementId;
                                ele = instance.GetLinkDocument().GetElement(eleId);
                            }

                            double number = linkedRoomPoint.DistanceTo(obj.GetReference().GlobalPoint);
                            double number2 = linkedRoomPoint.DistanceTo(currentRoomPoint);

                            if (number < number2 && number != 0)
                            {
                                TaskDialog task = new TaskDialog("Yo");
                                task.MainContent = (currentDoc.GetElement(obj.GetReference().ElementId) as RevitLinkInstance).GetLinkDocument().GetElement(eleId).Name;
                                task.Show();

                                test3 = false;
                                break;
                            }

                        }

                    }

                    //using (Transaction tx = new Transaction(currentDoc, "Create 3D View"))
                    //{
                    //    tx.Start();

                    //    new3DView = View3D.CreateIsometric(currentDoc, (new FilteredElementCollector(currentDoc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(v => v.ViewFamily == ViewFamily.ThreeDimensional)).Id);

                    //    tx.Commit();
                    //}








                    if (test1 && test2 && test3)
                    {
                        output = room;
                    }
                }
            }

            return output;
        }

    }
}
