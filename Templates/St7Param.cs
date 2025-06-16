using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension.Template
{
    /// <summary>
    /// Represents Strand7 Grasshopper parameter component inherit class, for general settings. 
    /// </summary>
    /// <typeparam name="T"> IGH_Goo </typeparam>
    public class St7Param<T> : GH_PersistentParam<T> where T : class, IGH_Goo
    {
        /// <summary>
        /// 
        /// </summary>
        protected override System.Drawing.Bitmap Icon { get; }

        /// <summary>
        /// 
        /// </summary>
        public override System.Guid ComponentGuid { get; }

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName { get; }

        /// <summary>
        /// Expose the object in the fourth section on the toolbar.
        /// </summary>
        public override GH_Exposure Exposure { get; } = GH_Exposure.primary;

        /// <summary>
        /// 
        /// </summary>
        public bool Hidden { get; set; } = false;

        /*
         * public virtual bool IsPreviewCapable { get; } = true;
         */

        public Type ObjectType { get; set; } = typeof(object);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nickname"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        public St7Param(string name, string nickname, string description, string category, string subcategory): 
            base(name, nickname, description, category, subcategory)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override GH_GetterResult Prompt_Singular(ref T value) 
        { 
            return GH_GetterResult.cancel; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        protected override GH_GetterResult Prompt_Plural(ref List<T> values) 
        { 
            return GH_GetterResult.cancel; 
        }
    }
}
