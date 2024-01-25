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
    internal sealed class SelectionFilterRoomsInLinkedDocument : ISelectionFilter
    {
        public Document LocalDocument {  get; set; }
        public Document LinkDocument { get; set; }
        public string LinkDocumentTitle { get; set; }


        //public SelectionFilterRoomsInLinkedDocument(Document doc)
        //{
        //    LocalDocument = doc;
        //}

        public SelectionFilterRoomsInLinkedDocument(Document doc, Document linkDocument)
        {
            LocalDocument = doc;
            LinkDocument = linkDocument;
            LinkDocumentTitle = linkDocument.Title;
        }

        //https://digitteck.com/dotnet/revit-api-custom-selection-filter/
        //Both need to be true for the element to be selectable
        public bool AllowElement(Element elem)
        {
            return true;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            
            RevitLinkInstance linkInstance = LocalDocument.GetElement(reference) as RevitLinkInstance;
            Document currentLinkedDocument = linkInstance.GetLinkDocument();

            if (currentLinkedDocument.Title == LinkDocumentTitle)
            {
                Element linkedElement = currentLinkedDocument.GetElement(reference.LinkedElementId);
                if (linkedElement is Room)
                {
                    return true;
                }
            }

            return false;
        }
    }




}
