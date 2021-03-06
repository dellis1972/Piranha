﻿using Mono.Cecil;
using System.Diagnostics;

namespace Ark.Cecil {
    public class ReferenceSearchingMetadataResolver : MetadataResolver {
        public ReferenceSearchingMetadataResolver(IAssemblyResolver assemblyResolver)
            : base(assemblyResolver) {
        }

        public override TypeDefinition Resolve(TypeReference type) {
            TypeDefinition result = null;
            result = TryResolve(type);
            if (result != null) {
                return result;
            }
            if (!(type is GenericParameter || type is TypeSpecification)) {
                var originalScope = type.Scope;
                foreach (var reference in type.Module.AssemblyReferences) {
                    type.Scope = reference;
                    result = TryResolve(type);
                    if (result != null) {
                        Trace.WriteLine(string.Format("Successfully forwarded the type {0} from {1} to {2}.", type, originalScope, type.Scope), "ReferenceSearchingMetadataResolver");
                        return result;
                    }
                }
            }
            return null;
        }

        public TypeDefinition TryResolve(TypeReference type) {
            try {
                return base.Resolve(type);
            } catch (AssemblyResolutionException) { }
            return null;
        }
    }
}
