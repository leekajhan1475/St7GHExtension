using System;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using St7Toolkit.Element;
using St7Toolkit.GHExtension.Kernel;
using St7Toolkit.Types;

namespace St7Toolkit.GHExtension.Goos
{
    /// <summary>
    /// Strand7 element data type.
    /// </summary>
    public class GH_St7Element : GH_Goo<object>
    {
        #region Properties/Variables
        /// <summary>
        /// Override name property
        /// </summary>
        public override string TypeName { get; } = "GH_St7Element";

        /// <summary>
        /// Override description property
        /// </summary>
        public override string TypeDescription { get; } = "Represents a Grasshopper Strand7 Element";

        /// <summary>
        /// 
        /// </summary>
        public Guid ReferenceID { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets whether this <see cref="GH_St7Element"/> is valid.<br/>
        /// For a <see cref="GH_St7Element"/> to be valid, it must own one of the following object types of St7Toolkit.<see cref="St7Toolkit.Element"/><br/>
        /// 1. St7Node<br/>
        /// 2. St7Beam<br/>
        /// 3. St7Plate<br/>
        /// 4. St7Support<br/>
        /// 5. St7Joint<br/>
        /// 6. St7Load<br/>
        /// 7. St7Material<br/>
        /// </summary>
        /// <returns>
        /// Type: <see cref="Boolean"/><br/>
        /// true if this GH_St7Element owns an St7Toolkit.Element object, false otherwise.
        /// </returns>
        public override bool IsValid
        { 
            get 
            { 
                return this.Value != null;
            }
        }

        /// <summary>
        /// Gets the value indicating whether this <see cref="GH_St7Element"/> holds a <see cref="St7Node"/>.<br/>
        /// </summary>
        /// <returns>
        /// Type: <see cref="Boolean"/><br/>
        /// true if this GH_St7Element owns a St7Node object, false otherwise.
        /// </returns>
        public bool IsSt7Node
        {
            get => (this.Value.GetType().GUID == St7ObjectType.idNODE);
        }

        /// <summary>
        /// Gets the value indicating whether this <see cref="GH_St7Element"/> holds a <see cref="St7Beam"/>.<br/>
        /// </summary>
        /// <returns>
        /// Type: <see cref="Boolean"/><br/>
        /// true if this GH_St7Element owns a St7Beam object, false otherwise.
        /// </returns>
        public bool IsSt7Beam
        {
            get => (this.Value.GetType().GUID == St7ObjectType.idBEAM);
        }

        /// <summary>
        /// Gets the value indicating whether this <see cref="GH_St7Element"/> holds a <see cref="St7Beam"/>.<br/>
        /// </summary>
        /// <returns>
        /// Type: <see cref="Boolean"/><br/>
        /// true if this GH_St7Element owns a St7Beam object, false otherwise.
        /// </returns>
        public bool IsSt7Plate
        {
            get => (this.Value.GetType().GUID == St7ObjectType.idPLATE);
        }

        /// <summary>
        /// Gets the value indicating whether this <see cref="GH_St7Element"/> holds a <see cref="St7Joint"/>.<br/>
        /// </summary>
        /// <returns>
        /// Type: <see cref="Boolean"/><br/>
        /// true if this GH_St7Element owns a St7Joint object, false otherwise.
        /// </returns>
        public bool IsSt7Joint
        {
            get => (this.Value.GetType().GUID == St7ObjectType.idJOINT);
        }

        #endregion

        /// <summary>
        /// Default inherit constructor from GH_Goo<T>
        /// </summary>
        public GH_St7Element() : base() { }


        /// <summary>
        /// This function is called when Grasshopper needs to convert this
        /// instance of GH_St7Element into some other type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns>
        /// True if successful, or false on failure.
        /// </returns>
        public override bool CastTo<T>(ref T target)
        {
            // First, see if T is similar to the type St7Toolkit.Element.Node.
            if (this.Value.GetType() == typeof(T))
            {
                object ptr = this.Value;
                target = (T)ptr;
                return true;
            }
            return false;
        }

        /// <summary>
        /// This function is called when Grasshopper needs to convert other data into a <see cref="GH_St7Element"/><br/>
        /// by storing the input <see cref="Object"/> (<paramref name="source"/>) in its <see cref="Object"/> pointer - Value.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override bool CastFrom(object source)
        {
            // Abort immediately on bogus data.
            if (source == null) { return false; }

            // First try using the St7Toolkit node element converter.
            St7Node node = new St7Node();
            if (GH_St7Convert.ToSt7Node(source, ref node))
            {
                this.Value = node;
                return true;
            }

            // Second, try using the St7Toolkit beam element converter.
            St7Beam beam = new St7Beam();
            if (GH_St7Convert.ToSt7Beam(source, ref beam))
            {
                this.Value = beam;
                return true;
            }

            // Third, try using the St7Toolkit plate element converter.
            St7Plate plate = new St7Plate();
            if (GH_St7Convert.ToSt7Plate(source, ref plate))
            {
                this.Value = plate;
                return true;
            }

            // Forth, try using the St7Toolkit support element converter.
            St7Support support = new St7Support();
            if (GH_St7Convert.ToSt7Support(source, ref support))
            {
                this.Value = support;
                return true;
            }

            // Forth, try using the St7Toolkit joint element converter.
            St7Joint joint = new St7Joint();
            if (GH_St7Convert.ToSt7Joint(source, ref joint))
            {
                this.Value = joint;
                return true;
            }

            // Forth, try using the St7Toolkit joint element converter.
            St7Load load = new St7Load();
            if (GH_St7Convert.ToSt7Load(source, ref load))
            {
                this.Value = load;
                return true;
            }

            // Exhausted the possible conversions,
            // it seems that input "source" cannot
            // be converted into a GH_St7Element type.
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
            return (this.Value == null) ? "null" : this.Value.ToString();
        }
    }
}