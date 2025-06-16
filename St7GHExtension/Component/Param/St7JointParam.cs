using System;
using St7Toolkit.GHExtension.Template;
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension.Param
{
    public class St7JointParam : St7Param<GH_St7Element>
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
        public override Guid ComponentGuid { get; } = new Guid("57f52e17-5a5e-4d75-a9b8-a444e2b8ec57");

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName { get; } = "Strand7 Joint";

        
        /// <summary>
        /// Constructor
        /// </summary>
        public St7JointParam() : 
            base(
                "St7Joint", 
                "Joint", 
                "Represents a Strand7 joint condition",
                "St7Toolkit.GHExtension", 
                "Params"
                )
        {
        }
    }

}
