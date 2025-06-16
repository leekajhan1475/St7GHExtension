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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

namespace St7GrasshopperExtensions
{
    public class OpenSt7File : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("e0a00125-4f28-463a-9282-1fad28b5ede5");

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OpenSt7File()
          : base("OpenExistingStrand7File",
                "OpenSt7File",
                "Open an existing Strand7 file",
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
            pManager.AddIntegerParameter("St7ModelFileId", "Id", "Input Strand7 model file Id", GH_ParamAccess.item);
            // Set Strand7 model file full path 
            pManager.AddTextParameter("St7FilePath", "Path", "Strand7 model file fullpath", GH_ParamAccess.item);
            // Set the first parameter's optional flag
            pManager[0].Optional = false;
            // Set the second parameter's optional flag
            pManager[1].Optional = false;
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
            // Set file Id
            int uId = -1;
            if (!DA.GetData(0, ref uId)) { return; }
            if (uId <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "St7Model file Id should be greater than 0");
                return;
            }

            // Get input Strand7 model file name and file path as string
            string fullPath = "";
            if (!DA.GetData(1, ref fullPath)) { return; }

            // Create a new Strand7 Model object
            St7Model model = new St7Model(uId);
            // Set model's directory
            model.FilePath = fullPath;

            // Initialize Strand7 API
            if (!Compute.InitSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to init St7API");
                return;
            }
            else { this._initAPI = true; } // API initialised, set flag to true

            // Try to open a file with the given directory
            if (!model.OpenFile())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to open Strand7 document");
                // Release API before exiting this method
                if (this._initAPI && Compute.ReleaseSt7API()) this._initAPI = false;
                return;
            }
            else { this._isOpened = true; }

            // Gets the imported "Strand7“ document's unit system
            int flag = model.GetSt7DocU(
                out LengthUnitSystem length,
                out ForceUnitSystem force,
                out StressUnitSystem stress,
                out MassUnitSystem mass,
                out TemperatureUnitSystem temp,
                out EnergyUnitSystem energy);

            if (flag == -1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Fail to get Strand7 document's unit system");
                DA.SetData(0, flag);
                // If file is opened, close the file
                if (this._isOpened && model.CloseFile()) this._isOpened = false;
                // Release API before exiting this method
                if (this._initAPI && Compute.ReleaseSt7API()) this._initAPI = false;
                return;
            }
            else if (flag == -2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "St7Model is not valid");
                DA.SetData(0, flag);
                // If file is opened, close the file
                if (this._isOpened && model.CloseFile()) this._isOpened = false;
                // Release API before exiting this method
                if (this._initAPI && Compute.ReleaseSt7API()) this._initAPI = false;
                return;
            }


            // Sets "This" St7Model's unit systems according to the imported Strand7 Document
            model.LengthUnit = length;
            model.ForceUnit = force;
            model.StressUnit = stress;
            model.MassUnit = mass;
            model.TempUnit = temp;
            model.EnergyUnit = energy;

            // Set the "Active“ Rhino model's unit system as same as the "Import Strand7 document"
            int state = model.SetRhinoDocU(RhinoDoc.ActiveDoc);
            if (state < 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Fail to convert active Rhino document's unit system to the opened Strand7 document");
                DA.SetData(0, (int)length);
                // If file is opened, close it
                if (this._isOpened && model.CloseFile()) this._isOpened = false;
                // Release API before exiting this method
                if (this._initAPI && Compute.ReleaseSt7API()) this._initAPI = false;
                return;
            }
            else if (state == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                "Active RhinoDoc's unit system is identical to the imported Strand7 document,\n" +
                "active document unit's system will remain unchanged");
            }
            else if (state == 2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "User has cancelled unit conversion operation,\n" +
                    "active Rhino document's unit system remains different to the imported Strand7 document");
            }

            /*
             * Pull objects, if any, from Strand7 document to Rhino document
             */


            // Save and close the file
            if (!model.SaveFile(true))
            {
                string s = $"Fail to save and close Strand7 document: {model.FilePath}";
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, s);
                // Release API before exiting this method
                if (this._initAPI && Compute.ReleaseSt7API()) this._initAPI = false;
                return;
            }

            // Release Strand7 API
            if (!Compute.ReleaseSt7API())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to release API");
                return;
            }
            else 
            { 
                this._initAPI = false; 
            }

            // Cast St7Model as GH_St7Model
            GH_St7Model St7_Model = new GH_St7Model();
            if (!St7_Model.CastFrom(model)) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create St7Model"); return; }

            // Set component output data
            DA.SetData(0, St7_Model);
        }

        private bool _initAPI = false;

        private bool _isOpened = false;
    }
}