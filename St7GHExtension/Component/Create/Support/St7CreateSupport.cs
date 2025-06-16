using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using GHExtension.UIWidgets;
using System.Linq;
using St7Toolkit.GHExtension.Support;

namespace St7Toolkit.GHExtension
{
    public class St7CreateSupport : GH_ExtendableComponent
    {
        /// <summary>
        /// 
        /// </summary>
        private MenuRadioButtonGroup _buttonGrp;

        /// <summary>
        /// 
        /// </summary>
        private List<GH_Integer> _dofValues = new List<GH_Integer>();

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
        public override Guid ComponentGuid => new Guid("f6c2f2a0-cdf7-492a-b1de-e1c967c29bc8");

        /// <summary>
        /// Expose the object in the first section on the toolbar.
        /// </summary>
        public override GH_Exposure Exposure { get; } = GH_Exposure.secondary;

        /// <summary>
        /// 
        /// </summary>
        public St7CreateSupport()
          : base("St7CreateSupport", 
                "St7Support",
                "Create Strand7 support element",
                "St7Toolkit.GHExtension", 
                "Supports")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Input generic data (IGH_Goo) as tree
            pManager.AddGenericParameter("Support Locations", "Pos|Id", "Point/Property number of the support", GH_ParamAccess.tree);

            // Optional input planes' (GH_Plane) data as tree 
            pManager.AddPlaneParameter("Local Planes", "Pln", "Coordinate system to define support local axes", GH_ParamAccess.tree);

            // Optional input DoFs data as tree
            pManager.AddGenericParameter("Degree of Freedom", "Dofs", "Sets the Degree of Freedom of the supports", GH_ParamAccess.tree);

            // To model different soil conditions, translational and rotational stiffness values
            // Optional input translational stiffness data as tree
            pManager.AddNumberParameter("Translational Stiffness", 
                                         "Ct", 
                                         "Translational stiffness [0-1] for modelling spring/flexible support behaviour\n" + 
                                         "Default value set to 1.0", 
                                         GH_ParamAccess.tree);

            // Optional input rotational stiffness data as tree
            pManager.AddNumberParameter("Rotational Stiffness",
                                        "Cr", 
                                        "Rotational stiffness [0-1] for modelling spring/flexible support behaviour\n" +
                                        "Default value set to 1.0",
                                        GH_ParamAccess.tree);

            // Set input data optional pointer
            pManager[0].Optional = false;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("St7Support", "Elem", "Represent a collection of St7Support elements", GH_ParamAccess.list);

            pManager.AddIntegerParameter("St7Support Dofs", "Dofs", "Represent a collection of St7Support degree of freedom settings", GH_ParamAccess.tree);

            //pManager.AddGenericParameter("St7Support", "Elem", "Represent a collection of St7Support elements", GH_ParamAccess.list);
        }

        protected override void Setup(GH_ExtendableComponentAttributes attr)
        {

            // Create first extendable menu with 6 radio buttons
            GH_ExtendableMenu extMenu = new GH_ExtendableMenu(1, "Dofs Menu");
            extMenu.Name = "Dof Conditions";
            extMenu.Expand();
            attr.AddMenu(extMenu);

            // Creates the first panel for the extended menu
            MenuPanel ctrlPanel = new MenuPanel(1, "radioButton");
            ctrlPanel.Header = "random words";

            this._buttonGrp = new MenuRadioButtonGroup(1, "radiogrp_color");
            this._buttonGrp.Direction = MenuRadioButtonGroup.LayoutDirection.Horizontal;
            this._buttonGrp.ValueChanged += _colorGrp__valueChanged;
            // Allow maximum dofs input count as 6 -> completely fixed
            this._buttonGrp.MaxActive = 6;
            // Allow maximum dofs input count as 0 -> completely free
            this._buttonGrp.MinActive = 0;

            MenuRadioButton menuRadioButton1 = new MenuRadioButton(0, "Button1", "tX", MenuRadioButton.Alignment.Vertical);
            menuRadioButton1.Name = "Translation X";

            MenuRadioButton menuRadioButton2 = new MenuRadioButton(1, "Button2", "tY", MenuRadioButton.Alignment.Vertical);
            menuRadioButton2.Name = "Translation Y";

            MenuRadioButton menuRadioButton3 = new MenuRadioButton(2, "Button3", "tZ", MenuRadioButton.Alignment.Vertical);
            menuRadioButton3.Name = "Translation Z";

            MenuRadioButton menuRadioButton4 = new MenuRadioButton(3, "Button4", "rX", MenuRadioButton.Alignment.Vertical);
            menuRadioButton4.Name = "Rotation X";

            MenuRadioButton menuRadioButton5 = new MenuRadioButton(4, "Button5", "rY", MenuRadioButton.Alignment.Vertical);
            menuRadioButton5.Name = "Rotation Y";

            MenuRadioButton menuRadioButton6 = new MenuRadioButton(5, "Button6", "rZ", MenuRadioButton.Alignment.Vertical);
            menuRadioButton6.Name = "Rotation Z";

            // Append buttons to button group
            this._buttonGrp.AddButton(menuRadioButton1);
            this._buttonGrp.AddButton(menuRadioButton2);
            this._buttonGrp.AddButton(menuRadioButton3);
            this._buttonGrp.AddButton(menuRadioButton4);
            this._buttonGrp.AddButton(menuRadioButton5);
            this._buttonGrp.AddButton(menuRadioButton6);

            // Append group (all buttons) to control panel
            ctrlPanel.AddControl(this._buttonGrp);
            // Append control panel to menu
            extMenu.AddControl(ctrlPanel);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /***** Try to get supports' location data *****/
            // If fail to acquire data from plug, exit solution
            if (!DA.GetDataTree(0, out GH_Structure<IGH_Goo> locs)) return;

            // Creates a data-tree to store all generic data
            // data could either be a GH_Point or GH_Integer
            DataTree<IGH_Goo> support_locs = new DataTree<IGH_Goo>();

            // If Location plug has input points/indices data
            if (locs.DataCount > 0) {
                // Perform pre-check of the data type, make sure it's either GH_Integer or GH_Point
                List<Type> legalTypes =  new List<Type>() { typeof(GH_Integer), typeof(GH_Point) };
                // Check input data
                if (locs.HasIllegalType(legalTypes, out string msg))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    return;
                }

                // After check, convert location data (either point or index) to list
                support_locs = locs.ToDataTree<IGH_Goo>();
            }

            /********** Gets supports' local plane data **********/

            // If fail to acquire data from plug, exit solution
            if (!DA.GetDataTree(1, out GH_Structure<GH_Plane> plns)) return;

            // Creates a list to store all supports' local plane
            DataTree<GH_Plane> planes = new DataTree<GH_Plane>();

            // If planes plug has input data
            if (plns.DataCount > 0) {
                // Then, checks if previous input "Locations"
                // have the same number of data as this input "Planes"
                if (support_locs.DataCount != plns.DataCount)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                                           $"Data count mismatch:\n" +
                                           $"Input total number of positions: {support_locs.DataCount}\n" +
                                           $"Input total number of local planes: {planes.DataCount}");
                    return; // Exit solution
                }
            }
            // If no values is provided, supply each support with a WorldXY plane by default.
            else
            {
                for (int i = 0; i < support_locs.BranchCount; i++)
                {
                    for (int j = 0; j < support_locs.Branch(i).Count; j++)
                    {
                        // Appends a new World XYPlane by default
                        planes.Add(new GH_Plane(), new GH_Path(support_locs.Paths[i]));
                    }
                }
            }


            /********** Gets supports' dof data **********/
            
            // If fail to acquire data from plug, exit solution
            if (!DA.GetDataTree(2, out GH_Structure<IGH_Goo> _dofs)) return;

            // Creates a list to store all supports' degree of freedom data
            DataTree<IGH_Goo> dofs_as_tree = new DataTree<IGH_Goo>();

            // If dofs plug has input data
            if (_dofs.DataCount > 0)
            {
                // Perform pre-check of the data type, make sure it's either GH_Integer or GH_Point
                List<Type> legalTypes = new List<Type>() { typeof(GH_Integer), typeof(GH_Boolean) };
                // Check input data
                if (locs.HasIllegalType(legalTypes, out string msg))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    return;
                }
                // Get data tree
                dofs_as_tree = _dofs.ToDataTree<IGH_Goo>();
            }
            // If no values is provided, use the radio buttons INPUTS instead.
            else
            {
                // Assign radio buttons 6 input values to each support
                for (int i = 0; i < support_locs.BranchCount; i++) 
                {
                    GH_Path path = support_locs.Paths[i];

                    int[] indices = path.Indices;

                    for (int j = 0; j < support_locs.Branch(i).Count; j++)
                    {
                        int[] graft_indices = new int[indices.Length + 1];
                        for(int k = 0; k < indices.Length; k++) { graft_indices[k] = indices[k]; }

                        graft_indices[indices.Length] = j;

                        GH_Path graft_path = new GH_Path(graft_indices);

                        dofs_as_tree.AddRange(this._dofValues, graft_path);
                    }
                }
            }



            /*
            // Creates a list to store the St7Supports (convert to GH_St7Elements)
            List<GH_St7Element> list = new List<GH_St7Element>();

            // Iterate through all of the items
            for (int i = 0; i < pos.PathCount; i++)
            {
                for (int j = 0; j < iA.get_Branch(i).Count; j++)
                {

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
            */

            // Set output data list at index 0 to instance list
            DA.SetDataList(0, this._dofValues);
            DA.SetDataTree(1, dofs_as_tree);
        }

        private void _colorGrp__valueChanged(object sender, EventArgs e)
        {
            // Gets the active buttons indices
            List<int> activeRadioIndices = this._buttonGrp.GetActiveInt();
            // Reset the dofValue list if it contains any object
            if(this._dofValues.Count > 0) this._dofValues = new List<GH_Integer>();
            // Append active indices to list
            foreach (int id in activeRadioIndices)
            {
                GH_Integer gH_Integer = new GH_Integer();
                gH_Integer.Value = id;
                this._dofValues.Add(gH_Integer);
            }
            // Recompute
            setModelProps();
        }

        // Recompute this component SolveInstance() method.
        protected void setModelProps()
        {
            ((GH_DocumentObject)this).ExpireSolution(true);
        }
    }
}