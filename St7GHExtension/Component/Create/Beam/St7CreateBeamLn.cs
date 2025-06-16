using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using St7Toolkit.Element;
using St7Toolkit.GHExtension.Kernel;
using St7Toolkit.GHExtension.Goos;
using System.Linq;

namespace St7Toolkit.GHExtension
{
    public class St7CreateBeamLn : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("6e97cd35-2ff4-416d-92c0-790d6bb957a3");

        /// <summary>
        /// Expose the object in the first section on the toolbar.
        /// </summary>
        public override GH_Exposure Exposure { get; } = GH_Exposure.primary;

        /// <summary>
        /// Inherit GH_Component constructor
        /// </summary>
        public St7CreateBeamLn()
            : base(
                  "St7CreateBeamFromLines", 
                  "St7BeamLn",
                  "Create Strand7 beam elements from Rhino lines",
                  "St7Toolkit.GHExtension", 
                  "Elements"
                  )
        {
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        /// <param name="pManager">
        /// Instance of GH_InputParameterManager
        /// </param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Input line data
            pManager.AddLineParameter("Line", "Ln", "Represent a collection of lines", GH_ParamAccess.tree);
            // Input Id data
            pManager.AddIntegerParameter("BeamId", "Id", "Assign ID number(>0) of specific beams", GH_ParamAccess.tree);
            // Input property data
            pManager.AddIntegerParameter("PropertyNumber", "P", "Property number for the beams", GH_ParamAccess.tree);
            // Input color data
            pManager.AddColourParameter("Colour", "C", "Colour property of the beams", GH_ParamAccess.tree);
            // Optional Id numbers
            pManager[1].Optional = true;
            // Optional colour properties
            pManager[3].Optional = true;
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        /// <param name="pManager">
        /// Instance of GH_OutputParameterManager
        /// </param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Publish St7Element objects
            pManager.AddGenericParameter("St7Elements", "Elem", "Strand7 elements", GH_ParamAccess.list);
            // Publish elements' information
            pManager.AddTextParameter("St7ElementInfo", "Info", "Strand7 elements information", GH_ParamAccess.item);
        }


        /// <summary>
        /// Solve method that does the work during runtime.
        /// </summary>
        /// <param name="DA">
        /// The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool HasId = true;

            // Get the value of index A
            if (!DA.GetDataTree(0, out GH_Structure<GH_Line> ghLines)) return;
            // Exit solution when input lines includes null items
            foreach (GH_Line ln in ghLines)
            {
                if (!ln.IsValid)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input line cannot be null");
                    return;
                }
            }

            // Get the value of entity numbers for the beams
            // If no entity number is provided, the component will generate automatically
            if (!DA.GetDataTree(1, out GH_Structure<GH_Integer> entities))
            {
                HasId = false;
            }
            // Exit solution when input integers includes null items
            foreach (GH_Integer val in entities)
            {
                if (!val.IsValid)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input entity cannot be null");
                    return;
                }
            }

            // Set Start of entity number = 1
            int entityNum = 1;
            // Create a list to store all of the beams constructed from respective lines
            List<St7Beam> beams = new List<St7Beam>();
            // Iterate through all of the items
            for (int i = 0; i < ghLines.PathCount; i++)
            {
                for (int j = 0; j < ghLines.get_Branch(i).Count; j++)
                {
                    // Get instance Rhino.Geometry.Line associated with GH_Line parameter type
                    Line line = ghLines.get_DataItem(ghLines.Paths[i], j).Value;
                    // Construct Beam objects from input Rhino.Geometry.Line by default settings
                    St7Beam beam;
                    // Construct Beam objects from input Rhino.Geometry.Line by default settings
                    beam = new St7Beam(entityNum, line);

                    // Append all of the beams to list 
                    beams.Add(beam);
                    entityNum++;
                }
            }
            // Set up beams' connection info
            List<St7Node> nodes;
            if(!Compute.SetBeamConnectivity(beams, out nodes))
            { 
                this.AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error, 
                    "Failed to set beam connectivity"); 
            }

            // Create a list to store all of the GH_St7Elemets from St7Toolkit.Element.Beam objects
            List<GH_St7Element> ghSt7Elements = new List<GH_St7Element>();
            // Iterate through the beams list to convert to GH_St7Element
            for (int i = 0; i < beams.Count; i++)
            {
                // Construct a GH_St7Element object for data conversion into Grasshopper data type (GH_Goo<T>)
                GH_St7Element element = new GH_St7Element();
                // Convert data (St7Toolkit.Element.Beam) into GH_St7Element
                if (!element.CastFrom(beams[i]))
                {
                    this.AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Fail to convert type St7Toolkit.Element.Beam to GH_St7Element");
                }
                // Append element to list
                ghSt7Elements.Add(element);
            }

            // Iterate through the nodes list to convert to GH_St7Element
            for (int i = 0; i < nodes.Count; i++)
            {
                // Construct a GH_St7Element object for data conversion into Grasshopper data type (GH_Goo<T>)
                GH_St7Element element = new GH_St7Element();
                // Convert data (St7Toolkit.Element.Beam) into GH_St7Element
                if (!element.CastFrom(nodes[i]))
                {
                    this.AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Fail to convert type St7Toolkit.Element.Node to GH_St7Element");
                }
                // Append element to list
                ghSt7Elements.Add(element);
            }

            // Set output data list at index 0 to instance list
            DA.SetDataList(0, ghSt7Elements);
        }
    }
}