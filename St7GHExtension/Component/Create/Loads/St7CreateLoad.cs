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
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension
{
    public class St7CreateLoad : GH_Component
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
        public override Guid ComponentGuid => new Guid("2151b5aa-8e5c-4936-b728-1ac2c12b843b");

        /// <summary>
        /// Expose the object in the first section on the toolbar.
        /// </summary>
        public override GH_Exposure Exposure { get; } = GH_Exposure.secondary;

        /// <summary>
        /// 
        /// </summary>
        public St7CreateLoad()
          : base("St7AddLoad", 
                "AddLoad",
                "Apply load to the model",
                "St7Toolkit.GHExtension",
                "Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Input data as tree
            pManager.AddVectorParameter("Load", "Vec", "Vector representation of applied load", GH_ParamAccess.tree);
            // Input plane data as tree
            pManager.AddIntegerParameter("Load Case", "LCase", "Case number to assign to this load", GH_ParamAccess.tree);

            pManager[0].Optional = false; // Set vector input as false, by default assign world -Z Vector
            pManager[1].Optional = false; // By default set as 1
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("St7Load", "Load", "Represent a Strand7 load element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get the value of index A
            if (!DA.GetDataTree(0, out GH_Structure<GH_Integer> iA)) return;

            // Get the value of index B
            if (!DA.GetDataTree(0, out GH_Structure<GH_Integer> iB)) return;

            // List<int> list = new List<int>();  
            List<GH_St7Element> list = new List<GH_St7Element>();

            // Iterate through all of the items
            for (int i = 0; i < iA.PathCount; i++)
            {
                for (int j = 0; j < iA.get_Branch(i).Count; j++)
                {
                    // Get start node index
                    int startId = iA.get_DataItem(iA.Paths[i], j).Value;
                    // Get end node index
                    int endId = iB.get_DataItem(iB.Paths[i], j).Value;

                    // Construct a Beam object
                    St7Beam beam = new St7Beam();
                    // Construct a GH_St7Element object for data conversion into Grasshopper data type (GH_Goo<T>)
                    GH_St7Element element = new GH_St7Element();
                    // Convert data (St7Toolkit.Element.Beam) into GH_St7Element
                    if (!element.CastFrom(beam))
                    {
                        this.AddRuntimeMessage(
                            GH_RuntimeMessageLevel.Error,
                            "Fail to convert type Beam to Grasshopper St7Element");
                    }
                    // Append element to list
                    list.Add(element);
                }
            }
            // Set output data list at index 0 to instance list
            DA.SetDataList(0, list);

        }
    }
}