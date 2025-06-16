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
using Rhino;

namespace St7GrasshopperExtensions
{
    public class CreateSt7File : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("2f195969-79a2-4cad-9e4c-51c4e493c025");

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CreateSt7File()
          : base("CreateStrand7File", 
                "CreateSt7File",
                "Create a new Strand7 file",
                "St7Toolkit.GHExtension", 
                "File")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Set Strand7 model file ID
            pManager.AddIntegerParameter("St7ModelFileId", "Id", "Optional input Strand7 model file Id", GH_ParamAccess.item);
            // Set Strand7 model file name 
            pManager.AddTextParameter("St7FileName", "Name", "Strand7 model file name", GH_ParamAccess.item);
            // Set Strand7 model file path 
            pManager.AddTextParameter("St7FileLocation", "Location", "Strand7 model file location", GH_ParamAccess.item);

            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("St7Model", "St7Model", "Represent a Strand7 assembly model", GH_ParamAccess.item);
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
            int uId = -1;
            DA.GetData(0, ref uId);
            if (uId <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "St7Model file Id should be greater than 0");
                return;
            }

            // Get input Strand7 model file name and file path as string
            string fileName = "";
            string filePath = "";
            DA.GetData(1, ref fileName);
            DA.GetData(2, ref filePath);
            string fullPath = filePath + $"\\{fileName}.st7";

            // Initialize Strand7 API
            if (!Compute.InitSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to init API");
                return;
            }

            // Create a Strand7 Model
            St7Model model = new St7Model(uId);
            // Try to create a new file with the given directory
            if (!model.NewFile(fullPath))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to create new file");
                return;
            }
            /*
            // Set new Strand7 document's unit system as same as the "Active Rhino document"
            if (!model.SetSt7DocU(RhinoDoc.ActiveDoc, 
                                  St7Model.ForceUnitSystem.KN, 
                                  St7Model.StressUnitSystem.KPa, 
                                  St7Model.MassUnitSystem.Kg, 
                                  St7Model.TemperatureUnitSystem.C,
                                  St7Model.EnergyUnitSystem.Kj))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set St7Model and document's unit systems");
                return;
            }
            */

            // Save and close the file
            if (!model.SaveFile(true))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to save and close new file");
                return;
            }
            // Release Strand7 API
            if (!Compute.ReleaseSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to release API");
                return;
            }

            // Cast St7Model as GH_St7Model
            GH_St7Model St7_Model = new GH_St7Model();
            if (!St7_Model.CastFrom(model)) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create St7Model");  return; }

            // Set component output data
            DA.SetData(0, St7_Model);
        }

        
    }
}