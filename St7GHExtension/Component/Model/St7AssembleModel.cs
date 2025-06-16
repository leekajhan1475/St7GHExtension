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
    public class St7AssembleModel : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("28a9f49d-3f86-4c3b-b940-95df7633f057");

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public St7AssembleModel()
          : base("AssembleSt7Model", 
                "AssembleModel",
                "Assemble a Strand7 model",
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
            pManager.AddGenericParameter("St7Element", "Elem", "A collection of St7 elements", GH_ParamAccess.list);
            // Add beam supports to the model
            pManager.AddGenericParameter("St7Support", "Support", "A collection of supports", GH_ParamAccess.list);
            // Add beam loads to the model
            pManager.AddGenericParameter("St7Load", "Load", "A collection of loads", GH_ParamAccess.list);
            // Add beam joints to the model
            pManager.AddGenericParameter("St7Joint", "Joint", "A collection of joints", GH_ParamAccess.list);
            // Assign materials to the elements
            pManager.AddGenericParameter("St7Material", "Mat", "A collection of St7 materials", GH_ParamAccess.list);

            // Set St7Model input at [0] as compulsory
            pManager[0].Optional = false;
            // Set St7Element input at [1] as compulsory
            pManager[1].Optional = false;
            // Set support input at [2] as compulsory
            pManager[2].Optional = false;
            // Set load input at [3] as compulsory
            pManager[3].Optional = false;
            // Set joint input at [4] as optional
            pManager[4].Optional = true;
            // Set material input at [5] as optional
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Publish St7Model
            pManager.AddGenericParameter("St7Model", "Model", "Represent a Strand7 assembly model", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("St7ModelId", "uId", "Represent the model file ID of this Strand7 assembly model", GH_ParamAccess.item);
            // Publish model information
            pManager.AddTextParameter("St7ModelInformation", "Info", "Strand7 model information", GH_ParamAccess.item);

            pManager.AddTextParameter("St7ModelMass", "Mass", "Strand7 model mass", GH_ParamAccess.item);
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
            // Get Active Rhino Document
            RhinoDoc activeDoc = Rhino.RhinoDoc.ActiveDoc;
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
            
            /***** Push data to Strand7 *****/
            // Add and push Nodes to Strand7 model
            int nodeIndex = st7Model.AddNodes(nodes);
            if (nodeIndex > -1) 
            {
                // Immediate release API if errors occurred
                if (isInitiated) { if (Compute.ReleaseSt7API()) { isInitiated = false; } }
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to push Node:{nodeIndex} to Strand7 Model");
                return; 
            }

            // Add and push Beams to Strand7 model
            int beamIndex = st7Model.AddBeams(beams);
            if (beamIndex > -1) 
            {
                // Immediate release API if errors occurred
                if (isInitiated) { if (Compute.ReleaseSt7API()) { isInitiated = false; } }
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to push Beam:{beamIndex} to Strand7 Model");
                return; 
            }

            // Add and push Plates to Strand7 model
            int plateIndex = st7Model.AddPlates(plates);
            if (plateIndex > -1) 
            {
                // Immediate release API if errors occurred
                if (isInitiated) { if (Compute.ReleaseSt7API()) { isInitiated = false; } }
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to push Plate:{plateIndex} to Strand7 Model");
                return; 
            }

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
            DA.SetData(1, new GH_Integer(st7Model.UId));
        }
    }
}