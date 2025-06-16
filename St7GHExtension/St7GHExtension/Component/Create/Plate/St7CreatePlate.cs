using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using St7Toolkit.Element;

namespace St7Toolkit.GHExtension
{
    public class St7CreatePlate : GH_Component
    {
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("18d7a32b-843b-472c-83e0-ecda666b4e74");

        /// <summary>
        /// Expose the object in the first section on the toolbar.
        /// </summary>
        public override GH_Exposure Exposure { get; } = GH_Exposure.secondary;

        /// <summary>
        /// 
        /// </summary>
        public St7CreatePlate()
          : base("St7CreatePlate", 
                "St7Plate",
                "Create Strand7 plate element",
                "St7Toolkit.GHExtension", 
                "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Input data as tree
            pManager.AddIntegerParameter("Integer", "i", "Property number of the corner nodes of the plate element", GH_ParamAccess.tree);
            // Input data as tree
            pManager.AddIntegerParameter("Integer", "Id", "Property number of the plate element", GH_ParamAccess.tree);
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("St7Plate", "Elem", "Represent a Strand 7 plate element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            

        }
    }
}