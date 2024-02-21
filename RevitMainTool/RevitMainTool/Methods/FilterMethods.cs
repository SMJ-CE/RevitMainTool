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

            if (!filtersIdInView.Any(x => x == filterIdNotCurrentAbbreviation))
            {
                view.AddFilter(filterIdNotCurrentAbbreviation);
            };

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
            if (!filtersIdInView.Any(x => x == filterIdNotMech))
            {
                view.AddFilter(filterIdNotMech);
            };
            graphicSettings = new OverrideGraphicSettings();
            graphicSettings.SetHalftone(true);
            graphicSettings.SetProjectionLineColor(new Color(0, 0, 0));
            view.SetFilterOverrides(filterIdNotMech, graphicSettings);


            //________________________________
            //Create or find filter for Everything not Section in sheet
            string filterNameNotSection = filterName + "_Not Section";
            filter = null;
            string currentSheetNumber = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsValueString();

            if (currentSheetNumber.Length > 0)
            {
                if (ParameterFilterElement.IsNameUnique(doc, filterNameNotSection))
                {
                    ICollection<ElementId> builtInCategories = new List<ElementId>
                        {
                            new ElementId(BuiltInCategory.OST_Sections)
                        };

                    FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.VIEWPORT_SHEET_NUMBER), currentSheetNumber);
                    ElementParameterFilter to = new ElementParameterFilter(filterRule);
                    filter = ParameterFilterElement.Create(doc, filterNameNotSection, builtInCategories, to);
                }
                else
                {
                    filter = filtersInDoc.First(x => x.Name == filterNameNotSection);
                }

                //Apply filter to view and set visibility graphics
                ElementId filterIdNotSectionInSheet = filter.Id;
                if (!filtersIdInView.Any(x => x == filterIdNotSectionInSheet))
                {
                    view.AddFilter(filterIdNotSectionInSheet);
                };
                view.SetFilterVisibility(filterIdNotSectionInSheet, false);
            }


            //________________________________
            //Apply grids and level filter
            string filterUniqueIDGridsAndLevels = "20e660bd-bcbc-44c5-9406-ea365e080f93-01797d6b";
            filter = filtersInDoc.First(x => x.UniqueId == filterUniqueIDGridsAndLevels);

            //Apply filter to view and set visibility graphics
            ElementId filterIdGridsAndLevels = filter.Id;
            if (!filtersIdInView.Any(x => x == filterIdGridsAndLevels))
            {
                view.AddFilter(filterIdGridsAndLevels);
            };
            view.SetFilterVisibility(filterIdGridsAndLevels, false);


            //________________________________
            //Apply grids and level filter
            string filterUniqueIDVoids = "07c7e03e-af0d-4eb2-8d5e-1bd5e336fa1f-017a0e74";
            filter = filtersInDoc.First(x => x.UniqueId == filterUniqueIDVoids);

            //Apply filter to view and set visibility graphics
            ElementId filterIdVoidss = filter.Id;
            if (!filtersIdInView.Any(x => x == filterIdVoidss))
            {
                view.AddFilter(filterIdVoidss);
            };
            view.SetFilterVisibility(filterIdVoidss, false);


        }

    }
}
