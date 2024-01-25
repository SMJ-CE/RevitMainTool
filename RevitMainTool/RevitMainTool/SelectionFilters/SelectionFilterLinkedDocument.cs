using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMainTool.SelectionFilters
{
    internal sealed class SelectionFilterLinkedDocument : ISelectionFilter
    {
        public Document LocalDocument {  get; set; }

        public SelectionFilterLinkedDocument(Document doc)
        {
            LocalDocument = doc;
        }

        //https://digitteck.com/dotnet/revit-api-custom-selection-filter/
        //Both need to be true for the element to be selectable
        public bool AllowElement(Element elem)
        {
            if(elem is RevitLinkInstance)
            {
                return true;
            }

            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            
            return true;
        }
    }
}
