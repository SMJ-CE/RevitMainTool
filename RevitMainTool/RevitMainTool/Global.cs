using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using RevitMainTool.Models;

namespace RevitMainTool
{

    public enum XOrY
    {
        XThenY = 0,
        YThenX = 1,
        SumOfXandY = 2,
    }

    public enum Alignment
    {
        Left = 0,
        Right = 1,
        Center = 2,
    }

    public static class Global
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }


        public static string ApplicationPath = "C:\\Program Files\\Vormadal Brothers\\SMJAddin";
        public static string Modifiable = "";
        public static Guid GUIDTriggerSheetName = new Guid("fafbf6b2-4c06-42d4-97c1-d1b4eb593eff");
        public static Guid GUIDTriggerSheetNumber = new Guid("dea3bf3d-74eb-42a1-a757-1539271fc0d0");
        public static Guid GUIDTriggerTitleBlockChanged = new Guid("124d2dea-8ec6-40f5-9bcb-f84d23ee3309");

        public static List<PaperSizes> PaperSizes = new List<PaperSizes>()
        {
            new PaperSizes()
            {
                Name = "A0",
                Height = 841,
                Width = 1189
            },
            new PaperSizes()
            {
                Name = "A0",
                Height = 1189,
                Width = 841
            },
            new PaperSizes()
            {
                Name = "A1",
                Height = 841,
                Width = 594
            },
            new PaperSizes()
            {
                Name = "A1",
                Height = 594,
                Width = 841
            },
            new PaperSizes()
            {
                Name = "A2",
                Height = 420,
                Width = 594
            },
            new PaperSizes()
            {
                Name = "A2",
                Height = 594,
                Width = 420
            },
            new PaperSizes()
            {
                Name = "A3",
                Height = 420,
                Width = 297
            },
            new PaperSizes()
            {
                Name = "A3",
                Height = 297,
                Width = 420
            },
            new PaperSizes()
            {
                Name = "A4",
                Height = 210,
                Width = 297
            },
            new PaperSizes()
            {
                Name = "A4",
                Height = 297,
                Width = 210
            }
        };


        public static Alignment EventTagAlignment;
        public static TagOrientation EventTagOrientation;
        public static List<IndependentTag> EventTags;
        public static double EventDistance;




        public static T MostCommon<T>(this IEnumerable<T> list)
        {
            var most = (from i in list
                        group i by i into grp
                        orderby grp.Count() descending
                        select grp.Key).First();
            return most;
        }
    }



    public class EventPlaceTagsFixedDistance : IExternalEventHandler
    {
        public static ExternalEvent HandlerEvent = null;
        public static EventPlaceTagsFixedDistance Handler = null;
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            try
            {
                //, List<IndependentTag> tags, double distance, Alignment alignment 

                using (var tx = new TransactionGroup(doc))
                {
                    tx.Start("Place Tags Fixed Distance");

                    if(Global.EventTagOrientation == TagOrientation.Horizontal)
                    {
                        IndepententTagMethods.SpaceTagsFixedDistanceHorizontal(Global.EventTags, Global.EventDistance, Global.EventTagAlignment);
                    }
                    else
                    {
                        IndepententTagMethods.SpaceTagsFixedDistanceVerticale(Global.EventTags, Global.EventDistance, Global.EventTagAlignment);
                    }

                    ICollection<ElementId> newSelection = new HashSet<ElementId>(Global.EventTags.Select(tag => tag.Id));
                    app.ActiveUIDocument.Selection.SetElementIds(newSelection);

                    tx.Assimilate();
                }
            }
            catch (InvalidOperationException)
            {

                throw;
            }
        }

        public string GetName()
        {
            return "EventPlaceTagsFixedDistance";
        }
        public void GetData(MechanicalEquipment mechEq)
        {

        }
    }


}
