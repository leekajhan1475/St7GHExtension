using System;
using St7Toolkit.GHExtension.Template;
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension.Param
{
    public class St7SupportParam : St7Param<GH_St7Element>
    {

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon { get; } = null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid { get; } = new Guid("0128181e-1e8c-4d6a-9192-7eca3a26c160");

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName { get; } = "Strand7 Support";

        
        /// <summary>
        /// Constructor
        /// </summary>
        public St7SupportParam() : 
            base(
                "St7Support",
                "Support",
                "Represents a Strand7 support object",
                "St7Toolkit.GHExtension", 
                "Params"
                )
        {
        }
    }

}
