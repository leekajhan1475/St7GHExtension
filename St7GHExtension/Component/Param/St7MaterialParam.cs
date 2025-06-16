using System;
using St7Toolkit.GHExtension.Template;
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension.Param
{
    public class St7MaterialParam : St7Param<GH_St7Element>
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
        public override Guid ComponentGuid { get; } = new Guid("b964374c-ec3f-479e-8a3e-2355d31e9881");

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName { get; } = "Strand7 Material";

        
        /// <summary>
        /// Constructor
        /// </summary>
        public St7MaterialParam() : 
            base(
                "St7Material", 
                "Material", 
                "Represents a Strand7 material",
                "St7Toolkit.GHExtension", 
                "Params"
                )
        {
        }
    }
}
