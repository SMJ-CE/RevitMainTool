using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace RevitMainTool.Methods
{
    public static class XYZMethods
    {
        public static double GetLength(this XYZ start, XYZ endPoint)
        {
            return endPoint.Subtract(start).GetLength();
        }


        public static UV ConvertToUV(this XYZ xyz)
        {
            return new UV(xyz.X, xyz.Y);
        }

        public static bool ApproximatelySameDirection(this XYZ xyz, XYZ direction, int precision)
        {
            var test1 = xyz.Normalize();
            var test2 = test1.Subtract(direction);
            var test3 = test2.Round(precision);
            var test4 = test3.ConvertToUV().IsZeroLength();

            return xyz.Normalize().Subtract(direction).Round(precision).ConvertToUV().IsZeroLength();
        }

        public static XYZ Round(this XYZ xyz, int decimalPoints)
        {
            return new XYZ(Math.Round(xyz.X, decimalPoints), Math.Round(xyz.Y, decimalPoints), Math.Round(xyz.Z, decimalPoints));
        }

        public static UV IntersectionOfTwoLines(UV point11, UV point12, UV point21, UV point22)
        {
            double x1_line1 = point11.U;
            double y1_line1 = point11.V;
            double x2_line1 = point12.U;
            double y2_line1 = point12.V;

            // Define the points on the second line
            double x1_line2 = point21.U;
            double y1_line2 = point21.V;
            double x2_line2 = point22.U;
            double y2_line2 = point22.V;

            // Calculate the slopes (m) for both lines
            double m1, m2;

            // Avoid division by zero errors
            if (x2_line1 - x1_line1 != 0)
                m1 = (y2_line1 - y1_line1) / (x2_line1 - x1_line1);
            else
                m1 = double.PositiveInfinity; // Vertical line

            if (x2_line2 - x1_line2 != 0)
                m2 = (y2_line2 - y1_line2) / (x2_line2 - x1_line2);
            else
                m2 = double.PositiveInfinity; // Vertical line

            // Check if lines are parallel
            if (m1 == m2)
            {
                // Parallel lines, no intersection
                return null; // Or throw an exception, depending on your requirements
            }

            // Calculate the y-intercepts (b) for both lines using one of the points
            double b1 = y1_line1 - m1 * x1_line1;
            double b2 = y1_line2 - m2 * x1_line2;

            // Finding the x-coordinate where the lines intersect
            double xIntersection;

            // Handle case where one or both lines are vertical
            if (double.IsInfinity(m1))
            {
                xIntersection = x1_line1;
            }
            else if (double.IsInfinity(m2))
            {
                xIntersection = x1_line2;
            }
            else
            {
                xIntersection = (b2 - b1) / (m1 - m2);
            }

            // Finding the y-coordinate using one of the equations
            double yIntersection;

            // Handle case where one or both lines are vertical
            if (double.IsInfinity(m1))
            {
                yIntersection = m2 * xIntersection + b2;
            }
            else if (double.IsInfinity(m2))
            {
                yIntersection = m1 * xIntersection + b1;
            }
            else
            {
                yIntersection = m1 * xIntersection + b1;
            }

            return new UV(xIntersection, yIntersection);
        }

        public static UV RotateVector(this UV vector, double angleDegrees)
        {
            double x = vector.U;
            double y = vector.V;

            // Convert angle to radians
            double angleRadians = angleDegrees * Math.PI / 180;

            // Perform rotation
            double newX = x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians);
            double newY = x * Math.Sin(angleRadians) + y * Math.Cos(angleRadians);

            return new UV(newX, newY);
        }

        public static XYZ ConvertToXYZ(this UV point)
        {
            return new XYZ(point.U, point.V, 0);
        }

        public static XYZ ConvertToXYZ(this UV point, double Z)
        {
            return new XYZ(point.U, point.V, Z);
        }

    }
}
