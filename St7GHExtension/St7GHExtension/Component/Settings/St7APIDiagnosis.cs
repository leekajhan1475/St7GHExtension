using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters.Hints;
using St7Toolkit;

namespace St7GrasshopperExtensions
{
    public class St7APIDiagnosis : GH_Component
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
        public override Guid ComponentGuid { get; } = new Guid("d28e38c0-600c-486a-829f-3e79aa84c64f");

        

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public St7APIDiagnosis()
          : base("St7APIDiagnosis", 
                "St7API",
                "Check St7API availability",
                "St7Toolkit.GHExtension", 
                "Settings")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("TryInitialiseAPI", "Init", "Try to initialise Strand7 API diagnosis", GH_ParamAccess.item);
            pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("DiagnosisMessage", "Message", "Strand7 API status", GH_ParamAccess.item);
            pManager.AddTextParameter("St7APIPath", "Path", "The directory of St7API.dll", GH_ParamAccess.item);
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
            // Get input init flag
            bool init = false;
            if (!DA.GetData(0, ref init)) { return; } // User input

            bool iErr;
            

            if (init)
            {
                iErr = Compute.InitSt7API();
                if (!iErr)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "See message");
                    DA.SetData(0, "Fail to launch API");
                    DA.SetData(1, string.Empty);
                    return;
                }

                this._isLaunched = true;
                this._apiPath = "Strand7 API directory: " + Compute.GetAPIPath();
                DA.SetData(0, "API launched");
                DA.SetData(1, this._apiPath);
                return;
            }

            if (this._isLaunched)
            {
                iErr = Compute.ReleaseSt7API();
                if (!iErr)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "See message");
                    DA.SetData(0, "Fail to release API");
                    DA.SetData(1, this._apiPath);
                    return;
                }

                this._isLaunched = false;
                DA.SetData(0, "API released");
                DA.SetData(1, this._apiPath);
                return;
            }

            DA.SetData(0, "Set init to true to start diagnosis");
            DA.SetData(1, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _isLaunched = false;

        /// <summary>
        /// 
        /// </summary>
        private string _apiPath = string.Empty;
    }
}