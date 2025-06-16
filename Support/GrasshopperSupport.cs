using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace St7Toolkit.GHExtension.Support
{
    public static class GrasshopperSupport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gh_structure"></param>
        /// <returns></returns>
        internal static DataTree<T> ToDataTree<T>(this GH_Structure<IGH_Goo> gh_structure)
        {
            DataTree<T> tree = new DataTree<T>();

            for (int i = 0; i < gh_structure.PathCount; i++)
            {
                GH_Path path = gh_structure.Paths[i];
                List<IGH_Goo> list = gh_structure.Branches[i];
                for (int j = 0; j < list.Count; j++)
                {
                    tree.Add((T)list[j], path);
                }
            }
            return tree;
        }

        /// <summary>
        /// Check if the <see cref="GH_Structure{T}"/> contains objects that is not the legal type for object casting.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="legalType">
        /// A collection of System.<see cref="Type"/>
        /// </param>
        /// <param name="type"></param>
        /// <returns>
        /// Type: <see cref="bool"/><br/>
        /// true if the <see cref="GH_Structure{T}"/> contains illegal types that is not the <see cref="Type"/> <paramref name="legalType"/>, false otherwise.
        /// </returns>
        internal static bool HasIllegalType(this GH_Structure<IGH_Goo> data, Type legalType, out Type type)
        {
            GH_Structure<IGH_Goo> temp = data.Duplicate();
            temp.Flatten();

            foreach (IGH_Goo element in temp)
            {
                // Get element type
                Type elementType = element.GetType();

                if (elementType != legalType)
                {
                    type = elementType;
                    return true;
                }
            }
            type = null;
            return false;
        }

        /// <summary>
        /// Check if the <see cref="GH_Structure{T}"/> contains objects that is not the legal types of <paramref name="queryTypes"/> for object casting.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="legalTypes">
        /// A collection of System.<see cref="Type"/>
        /// </param>
        /// <param name="msg">
        /// Text message from operation upon success or error.
        /// </param>
        /// <returns>
        /// Type: <see cref="bool"/><br/>
        /// true if the <see cref="GH_Structure{T}"/> contains illegal types that are not one of the query types, false otherwise.
        /// </returns>
        internal static bool HasIllegalType(this GH_Structure<IGH_Goo> data, IEnumerable<Type> legalTypes, out string msg)
        {
            // Create a duplication of the data to prevent accidental modifications to the input data
            GH_Structure<IGH_Goo> temp = data.Duplicate();
            temp.Flatten();

            int illegalMatch = 0;

            int matchId = -1;

            for (int i = 0; i < temp.Branches[0].Count; i++)
            {
                // Get type
                Type elementType = temp.Branches[0][i].GetType();

                // A pointer to store the count of found legal match
                int legalMatch = 0;

                // Store previous match id for next iteration use
                int prevMatch = matchId;

                // Perform check
                for (int j = 0; j < legalTypes.Count(); j++)
                {
                    Type legalType = legalTypes.ElementAt(j);

                    // If legal type is found, increment the match counter
                    if (elementType == legalType) 
                    {
                        matchId = j;
                        legalMatch++;
                    }
                }

                // Make sure that matchId points to the same index (Type) to ensure consistent object types in the data list
                // Condition check when the previous match id is not -1 (initiated)
                if(prevMatch > -1 && matchId != prevMatch)
                {
                    msg = "Input data contains multiple object types";
                    return true;
                }

                // If no legal match is found, increment the "illegal match"
                if (legalMatch < 1) { illegalMatch++; }
            }

            // If illegal match > 0
            if (illegalMatch > 0)
            {
                // Set output "count" to the number of illegal matches found in the data
                msg = $"Found {illegalMatch} number of illegal object types";
                // Returns "true" if illegal types are found
                return true;
            }

            // Set output "count" to the number of illegal matches found in the data
            msg = $"Data collection is clean, illegal object types not found";
            // Returns "true" if illegal types are found
            return false;
        }
    }
}
