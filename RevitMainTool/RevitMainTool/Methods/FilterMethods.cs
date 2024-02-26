using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMainTool.Methods
{
    public class FilterMethods
    {
        public static void CreateFiltersOnView(Document doc, string abbreviationString, View view)
        {
            var filtersInDoc = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).Cast<ParameterFilterElement>();
            var filtersIdInView = view.GetFilters();

            foreach (ElementId filterId in filtersIdInView)
            {
                view.RemoveFilter(filterId);
            }

            string prefixText = "Z ";
            string filterName = prefixText + abbreviationString;

            //________________________________
            //Create or find filter for Everything not current abbreviation
            string filterNameNotCurrentAbbreviation = filterName + "_Not Pipe";
            ParameterFilterElement filter = null;

            if (ParameterFilterElement.IsNameUnique(doc, filterNameNotCurrentAbbreviation))
            {
                ICollection<ElementId> builtInCategories = new List<ElementId>
                {
                    new ElementId(BuiltInCategory.OST_DuctAccessory),
                    new ElementId(BuiltInCategory.OST_DuctFitting),
                    new ElementId(BuiltInCategory.OST_DuctInsulations),
                    new ElementId(BuiltInCategory.OST_DuctLinings),
                    new ElementId(BuiltInCategory.OST_DuctCurves),
                    new ElementId(BuiltInCategory.OST_PlaceHolderDucts),
                    new ElementId(BuiltInCategory.OST_FlexDuctCurves),
                    new ElementId(BuiltInCategory.OST_FlexPipeCurves),
                    new ElementId(BuiltInCategory.OST_PipeAccessory),
                    new ElementId(BuiltInCategory.OST_PipeFitting),
                    new ElementId(BuiltInCategory.OST_PipeInsulations),
                    new ElementId(BuiltInCategory.OST_PlaceHolderPipes),
                    new ElementId(BuiltInCategory.OST_PipeCurves),
                    new ElementId(BuiltInCategory.OST_PlumbingFixtures)
                };

                FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM), abbreviationString);
                ElementParameterFilter to = new ElementParameterFilter(filterRule);
                filter = ParameterFilterElement.Create(doc, filterNameNotCurrentAbbreviation, builtInCategories, to);
            }
            else
            {
                filter = filtersInDoc.First(x => x.Name == filterNameNotCurrentAbbreviation);
            }

            //Apply filter to view and set visibility graphics
            ElementId filterIdNotCurrentAbbreviation = filter.Id;

            view.AddFilter(filterIdNotCurrentAbbreviation);
            OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();
            graphicSettings.SetHalftone(true);
            graphicSettings.SetProjectionLineColor(new Color(0, 0, 0));
            view.SetFilterOverrides(filterIdNotCurrentAbbreviation, graphicSettings);


            //________________________________
            //Create or find filter for Everything not Mechanical
            string filterNameNotMechanical = filterName + "_Not Mech";
            filter = null;

            if (ParameterFilterElement.IsNameUnique(doc, filterNameNotMechanical))
            {
                ICollection<ElementId> builtInCategories = new List<ElementId>
                    {
                    new ElementId(BuiltInCategory.OST_MechanicalEquipment)
                    };

                FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM), abbreviationString);
                ElementParameterFilter to = new ElementParameterFilter(filterRule);
                filter = ParameterFilterElement.Create(doc, filterNameNotMechanical, builtInCategories, to);
            }
            else
            {
                filter = filtersInDoc.First(x => x.Name == filterNameNotMechanical);
            }

            //Apply filter to view and set visibility graphics
            ElementId filterIdNotMech = filter.Id;
            view.AddFilter(filterIdNotMech);
            graphicSettings = new OverrideGraphicSettings();
            graphicSettings.SetHalftone(true);
            graphicSettings.SetProjectionLineColor(new Color(0, 0, 0));
            view.SetFilterOverrides(filterIdNotMech, graphicSettings);


            //________________________________
            //Create or find filter for Everything not Section in sheet
            string filterNameNotSection = filterName + "_Not Section";
            filter = null;
            string currentSheetNumber = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsValueString();

            if (currentSheetNumber != null)
            {
                ICollection<ElementId> builtInCategories = new List<ElementId>
                {
                    new ElementId(BuiltInCategory.OST_Sections)
                };

                FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.VIEWPORT_SHEET_NUMBER), currentSheetNumber);
                ElementParameterFilter to = new ElementParameterFilter(filterRule);
                if (ParameterFilterElement.IsNameUnique(doc, filterNameNotSection))
                {

                    filter = ParameterFilterElement.Create(doc, filterNameNotSection, builtInCategories, to);
                }
                else
                {
                    filter = filtersInDoc.First(x => x.Name == filterNameNotSection);
                    filter.SetElementFilter(to);
                }

                //Apply filter to view and set visibility graphics
                ElementId filterIdNotSectionInSheet = filter.Id;
                view.AddFilter(filterIdNotSectionInSheet);
                view.SetFilterVisibility(filterIdNotSectionInSheet, false);
            }


            //________________________________
            //Apply grids and level filter
            string filterNameGridsAndLevels = "SMJ Grids & Level Remove";
            filter = filtersInDoc.FirstOrDefault(x => x.Name == filterNameGridsAndLevels);

            //Apply filter to view and set visibility graphics
            if(filter != null)
            {
                ElementId filterIdGridsAndLevels = filter.Id;
                view.AddFilter(filterIdGridsAndLevels);
                view.SetFilterVisibility(filterIdGridsAndLevels, false);
            }
            

            //________________________________
            //Apply grids and level filter
            string filterNameVoids = "MC Voids hide";
            filter = filtersInDoc.FirstOrDefault(x => x.Name == filterNameVoids);

            //Apply filter to view and set visibility graphics
            if(filter != null)
            {
                ElementId filterIdVoidss = filter.Id;
                view.AddFilter(filterIdVoidss);
                view.SetFilterVisibility(filterIdVoidss, false);
            }
            
        }

    }
}
