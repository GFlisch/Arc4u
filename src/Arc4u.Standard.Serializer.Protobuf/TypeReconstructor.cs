using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Arc4u.Serializer.ProtoBuf
{
    /// <summary>
    /// Reconstruct a type from its name
    /// </summary>
    static class TypeReconstructor
    {
        private static Dictionary<string, Type> _reconstructedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// From a fully qualified type name (the result of <see cref="Type.AssemblyQualifiedName"/>), load the type instance.
        /// </summary>
        /// <param name="assemblyQualifiedName">the fully qualified type name</param>
        /// <param name="throwOnError">true if an exception should be thrown if the type cannot be found or constructed</param>
        /// <param name="referencedAssemblies">optional extra referenced assemblies</param>
        /// <returns></returns>
        public static Type ReconstructType(string assemblyQualifiedName, bool throwOnError = true, params Assembly[] referencedAssemblies)
        {
            if (!_reconstructedTypes.TryGetValue(assemblyQualifiedName, out Type type))
            {
                type = InternalReconstructType(assemblyQualifiedName, throwOnError, referencedAssemblies);
                InterlockedExtensions.InterlockedUpdate(ref _reconstructedTypes, assemblyQualifiedName, type);
            }
            return type;
        }

        private static Type InternalReconstructType(string assemblyQualifiedName, bool throwOnError = true, params Assembly[] referencedAssemblies)
        {
            foreach (var asm in referencedAssemblies)
            {
                var fullNameWithoutAssemblyName = assemblyQualifiedName.Replace($", {asm.FullName}", "");
                var type = asm.GetType(fullNameWithoutAssemblyName, throwOnError: false);
                if (type != null)
                    return type;
            }

            {
                var type = assemblyQualifiedName.Contains("[[") ? ConstructGenericType(assemblyQualifiedName, throwOnError) : Type.GetType(assemblyQualifiedName, false);
                if (type != null)
                    return type;
            }

            if (throwOnError)
                throw new Exception($"The type \"{assemblyQualifiedName}\" cannot be found in referenced assemblies.");
            else
                return null;
        }

        private static readonly Regex _nameDecompositionRegex = new Regex(@"^(?<name>\w+(\.\w+)*)`(?<count>\d)\[(?<subtypes>\[.*\])\](, (?<assembly>\w+(\.\w+)*)[\w\s,=\.]+)$?", RegexOptions.Singleline | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));

        private static Type ConstructGenericType(string assemblyQualifiedName, bool throwOnError = true)
        {
            var match = _nameDecompositionRegex.Match(assemblyQualifiedName);
            if (!match.Success)
                if (!throwOnError)
                    return null;
                else
                    throw new Exception($"Unable to parse the type's assembly qualified name: {assemblyQualifiedName}");

            var typeName = match.Groups["name"].Value;
            int n = int.Parse(match.Groups["count"].Value, CultureInfo.InvariantCulture);
            //var assemblyName = match.Groups["assembly"].Value;
            var subtypes = match.Groups["subtypes"].Value;

            typeName += $"`{n}";
            var genericType = ReconstructType(typeName, throwOnError);
            if (genericType == null)
                return null;

            var typeNames = new List<string>();
            int ofs = 0;
            while (ofs < subtypes.Length && subtypes[ofs] == '[')
            {
                int end = ofs, level = 0;
                do
                {
                    switch (subtypes[end++])
                    {
                        case '[': ++level; break;
                        case ']': --level; break;
                    }
                }
                while (level > 0 && end < subtypes.Length);

                if (level == 0)
                {
                    typeNames.Add(subtypes.Substring(ofs + 1, end - ofs - 2));
                    if (end < subtypes.Length && subtypes[end] == ',')
                        ++end;
                }

                ofs = end;
                --n;  // just for checking the count
            }

            // This shouldn't ever happen!
            if (n != 0)
                throw new Exception("Generic type argument count mismatch! Type name: " + assemblyQualifiedName);

            var types = new Type[typeNames.Count];
            for (int i = 0; i < types.Length; i++)
                try
                {
                    types[i] = ReconstructType(typeNames[i], throwOnError);
                    if (types[i] == null)  // if throwOnError, should not reach this point if couldn't create the type
                        return null;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to reconstruct generic type. Failed on creating the type argument {(i + 1)}: {typeNames[i]}. Error message: {ex.Message}");
                }
            return genericType.MakeGenericType(types);
        }
    }
}
