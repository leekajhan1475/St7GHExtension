using System;
using Grasshopper.Kernel.Types;
using St7Toolkit.Element;
using St7Toolkit.GHExtension.Kernel;

namespace St7Toolkit.GHExtension.Goos
{
    public class GH_St7Model : GH_Goo<object>
    {

        /// <summary>
        /// Override name property
        /// </summary>
        public override string TypeName { get; } = "St7Model";

        /// <summary>
        /// Override description property
        /// </summary>
        public override string TypeDescription { get; } = "Represents a Strant7 Model";

        /// <summary>
        /// 
        /// </summary>
        public Guid ReferenceID { get; set; } = Guid.Empty;

        /// <summary>
        /// Get whether Strand7 model is valid 
        /// </summary>
        public override bool IsValid 
        { 
            get 
            { 
                return this.Value != null; 
            } 
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GH_St7Model() : base() { }

        /// <summary>
        /// This function is called when Grasshopper needs to convert this
        /// instance of <see cref="GH_St7Model"/> into <see cref="St7Model"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns>
        /// True if successful, or false on failure.
        /// </returns>
        public override bool CastTo<T>(ref T target)
        {
            // Check if this.Value object's type is similar
            // to the type of the target T
            if (this.Value.GetType() == typeof(T))
            {
                object ptr = this.Value;
                target = (T)ptr;
                return true;
            }
            return false;
        }

        /// <summary>
        /// This function is called when Grasshopper needs to convert other data into GH_St7Model.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override bool CastFrom(object source)
        {
            // Abort immediately on bogus data.
            if (source == null) { return false; }

            // Try using the St7Toolkit model converter.
            // it should Convert since this is a model object
            St7Model model = new St7Model(-1);
            if (GH_St7Convert.ToSt7Model(source, ref model))
            {
                this.Value = model;
                return true;
            }

            // Exhausted the possible conversions,
            // it seems that input "source" cannot
            // be converted into a GH_St7Model type.
            return false;
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <returns></returns>
        public override IGH_Goo Duplicate()
        {
            return new GH_St7Element { Value = Value };
        }

        /// <summary>
        /// Override ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (this.Value == null) ? "null" : Value.ToString();
        }
    }
}