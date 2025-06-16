using System;
using St7Toolkit.Element;
using St7Toolkit.Types;
using St7Toolkit.GHExtension.Goos;

namespace St7Toolkit.GHExtension.Kernel
{
    public static class GH_St7Convert
    {
        /// <summary>
        /// Performs a direct cast from type <see cref="GH_St7Element"/> to St7Toolkit.Element.<see cref="St7Beam"/>. Data may not be duplicated.
        /// </summary>
        /// <param name="data">
        /// Data to convert
        /// </param>
        /// <param name="rc">
        /// 
        /// </param>
        /// <returns>
        /// True if successful, or false on failure.
        /// </returns>
        public static bool ToSt7Beam(object data, ref St7Beam rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            // Get object's GUID
            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idBEAM)
            {
                rc = (St7Beam)data;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Performs a direct cast from type <see cref="GH_St7Element"/> to St7Toolkit.Element.<see cref="St7Node"/>. 
        /// Data may not be duplicated.
        /// </summary>
        /// <param name="data">Data to convert</param>
        /// <param name="rc"></param>
        /// <returns>
        /// True if successful, or false on failure.
        /// </returns>
        public static bool ToSt7Node(object data, ref St7Node rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idNODE)
            {
                rc = (St7Node)data;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Performs a direct cast from type <see cref="GH_St7Element"/> to St7Toolkit.Element.<see cref="St7Plate"/>
        /// </summary>
        /// <param name="data">
        /// Data to convert.
        /// </param>
        /// <param name="rc">
        /// 
        /// </param>
        /// <returns>
        /// True if successful, or false on failure.
        /// </returns>
        public static bool ToSt7Plate(object data, ref St7Plate rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idPLATE)
            {
                rc = (St7Plate)data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs a direct cast from type <see cref="GH_St7Model"/> to St7Toolkit.<see cref="St7Model"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rc"></param>
        /// <returns>
        /// Type: <see cref="bool"/>
        /// </returns>
        public static bool ToSt7Model(object data, ref St7Model rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idMODEL)
            {
                rc = (St7Model)data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static bool ToSt7Material(object data, ref St7Material rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idMATERIAL)
            {
                rc = (St7Material)data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static bool ToSt7Load(object data, ref St7Load rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idLOAD)
            {
                rc = (St7Load)data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs a direct cast from type GH_St7Element to St7Toolkit.Element.St7Support
        /// </summary>
        /// <param name="data">
        /// System.Object to convert type.
        /// </param>
        /// <param name="rc">
        /// St7Toolkit.Element.St7Support after successful cast, or <see cref="St7Support.Unset"/> if failed.
        /// </param>
        /// <returns></returns>
        public static bool ToSt7Support(object data, ref St7Support rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idSUPPORT)
            {
                rc = (St7Support)data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static bool ToSt7Joint(object data, ref St7Joint rc)
        {
            // Abort immediately on bogus data.
            if (data == null) { return false; }

            Guid gUID = data.GetType().GUID;
            if (gUID == St7ObjectType.idJOINT)
            {
                rc = (St7Joint)data;
                return true;
            }
            return false;
        }
    }
}
