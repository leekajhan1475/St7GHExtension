using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using St7Toolkit;
using St7Toolkit.GHExtension;
using St7Toolkit.Element;
using St7Toolkit.GHExtension.Goos;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using Rhino;

namespace St7GrasshopperExtensions
{
    public class St7AnalyseModel : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("f7b14357-92b2-401a-9ab7-e539e9d55079");

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public St7AnalyseModel()
          : base("AnalyseSt7Model", 
                "AnalyseModel",
                "Perform analysis to a Strand7 model",
                "St7Toolkit.GHExtension", 
                "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Set Strand7 model file path
            pManager.AddGenericParameter("St7Model", "Model", "Strand7 model file", GH_ParamAccess.item);
            // Add beam elements to the model
            pManager.AddGenericParameter("LoadCase", "LCase", "Load case number of analysis result", GH_ParamAccess.list);
            // Set St7Model input at [0] as Compulsory data input
            pManager[0].Optional = false;
            // Set load case number input at [1] as optional data input
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Publish St7Model
            pManager.AddGenericParameter("St7Model", "Model", "Represent a Strand7 assembly model", GH_ParamAccess.item);
            // Overview of maximum displacement information
            pManager.AddNumberParameter("MaxDisplacement", "MaxDisp", "Represents the maximum displacement of the analysis result", GH_ParamAccess.item);
            // Overview of reaction forces of the supports
            pManager.AddNumberParameter("ReactionForces", "Forces", "Represents the reactional forces of the supports", GH_ParamAccess.item);
            // Overview of elastic energy
            pManager.AddNumberParameter("ElasticEnergy", "Energy", "Represents the elastic energy of the overal deformation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">
        /// The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Get input Strand7 model file
            GH_St7Model inputGHSt7Model = new GH_St7Model();
            // Get GH_St7Model object
            DA.GetData(0, ref inputGHSt7Model);

            // Try to convert GH_Goo to St7Model type
            St7Model st7Model = new St7Model(-1);
            if (!inputGHSt7Model.CastTo(ref st7Model))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to convert to St7_Model object type");
                return;
            }
            // Check if the St7Model object is valid after casting
            if (!st7Model.IsValid)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "St7_Model is broken, cannot continue operation");
                return;
            }

            // Get input Strand7 element data
            List<GH_St7Element> elementAll = new List<GH_St7Element>(); 
            DA.GetDataList(1, elementAll);

            // Declare variables to store Node, Beam, Plate items
            List<St7Node> nodes = new List<St7Node>();
            List<St7Beam> beams = new List<St7Beam>();
            List<St7Plate> plates = new List<St7Plate>();

            // Filter elements in input list "elementAll" by type:
            // 1. Node
            // 2. Beam
            // 3. Plate
            // Append each type to its designated list for each iteration
            foreach(GH_St7Element Elem in elementAll)
            {
                // Firstly, try convert GH_Goo to St7Toolkit.Element.Node
                St7Node node = new St7Node();
                if (Elem.CastTo(ref node))
                {
                    // Check if the Node object is valid after casting
                    // If the Node is valid, meaning since the GH_Element "Only" own this object
                    // we could skip to the next iteration
                    if (node.IsValid) 
                    { 
                        nodes.Add(node); 
                    }
                    continue;
                }

                // Secondly, try convert GH_Goo to St7Toolkit.Element.Beam
                St7Beam beam = new St7Beam();
                if (Elem.CastTo(ref beam))
                {
                    // Check if the Beam object is valid after casting
                    // If the Beam is valid, meaning since the GH_Element "Only" own this object
                    // we could skip to the next iteration
                    if (beam.IsValid) 
                    { 
                        beams.Add(beam); 
                    }
                    continue;
                }

                // Try convert GH_Goo to St7Toolkit.Element.Plate
                St7Plate plate = new St7Plate();
                if (Elem.CastTo(ref plate))
                {
                    // Check if the Plate object is valid after casting
                    if (plate.IsValid) 
                    {
                        plates.Add(plate);
                    }
                }
            }

            // Initialize Strand7 API
            // Error message for tracking and handling
            //string errorMsg;
            bool isInitiated = false;
            if (!Compute.InitSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to init API");
                return;
            } else
            {
                isInitiated = true;
            }

            // Open St7Model file
            if (!st7Model.OpenFile())
            {
                // Immediate release API if errors occurred
                if (isInitiated) { if (Compute.ReleaseSt7API()) { isInitiated = false; } }
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Strand7 Model");
                return;
            }
            
            /***** Perform analysis based on current model settings *****/
            

            // Save and close Strand7 model file
            if(!st7Model.SaveFile(true)) 
            {
                // Immediate release API if errors occurred
                if (isInitiated) { if (Compute.ReleaseSt7API()) { isInitiated = false; } }
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to save Strand7 Model");
                return;
            }

            // Release Strand7 API
            if (!Compute.ReleaseSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to release API");
                return;
            }

            // Cast St7Model as GH_St7Model
            GH_St7Model convertGHSt7Model = new GH_St7Model();
            if (!convertGHSt7Model.CastFrom(st7Model)) 
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create St7Model");
                return;
            }
            
            // Set API initiation flag to false if it's active
            if (isInitiated) isInitiated = false;

            // Set component output data
            DA.SetData(0, convertGHSt7Model);
            //DA.SetData(1, new GH_Integer(st7Model.UId));
        }
    }
}