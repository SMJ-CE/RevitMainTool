using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMainTool.Models
{
    public class PaperSizes
    {
        public string Name {  get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        public PaperSizes() { }


        public bool IsAMatch(double inputHeight, double inputWidth, double varience)
        {
            double HeightHigh = Height + varience;
            double HeightLow = Height - varience;
            double WidthHigh = Width + varience;
            double WidthLow = Width - varience;

            if (inputHeight > HeightLow 
                && HeightHigh > inputHeight 
                && inputWidth > WidthLow
                && WidthHigh > inputWidth)
            {
                return true;
            }

            return false;
        }

    }
}
