using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;

namespace RevitMainTool
{
    public static class LevelMethods
    {

        public static Level GetLevelInCurrentThatMatchesLinkedLevel(Document currentDoc, Level levelInLinkedDocument)
        {
            return GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, levelInLinkedDocument, 0);
        }

        public static Level GetLevelInCurrentThatMatchesLinkedLevel(Document currentDoc, Level levelInLinkedDocument, double offset)
        {
            Level theChosenLevel = null;
            double elevationInLinkedDocument = levelInLinkedDocument.Elevation + offset;
            List<Level> levelsInCurrentDocument = new FilteredElementCollector(currentDoc).OfClass(typeof(Level)).Cast<Level>().ToList();

            double previousDifference = 200000;
            int chosenIndex = 0;

            for (int i = 0; i < levelsInCurrentDocument.Count(); i++)
            {
                Level level = levelsInCurrentDocument[i];
                double currentDifference = Math.Abs(elevationInLinkedDocument - level.Elevation);

                if (currentDifference < previousDifference)
                {
                    previousDifference = currentDifference;
                    chosenIndex = i;

                    if (previousDifference == 0)
                    {
                        theChosenLevel = level;
                        return theChosenLevel;
                    }
                }
            }

            if (chosenIndex != -1)
            {
                theChosenLevel = levelsInCurrentDocument[chosenIndex];
            }

            return theChosenLevel;
        }

    }
}
