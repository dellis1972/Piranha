﻿using Ark.Cecil;
using Ark.Piranha;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piranha {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Usage: piranha.exe <library>");
                Environment.Exit(1);
            }
            string inputFileName = args[0];
            //string inputFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string outputFileBase = Path.GetFileNameWithoutExtension(inputFileName);
            string outputFileName = outputFileBase + ".skeleton.dll";

            var inputStream = File.OpenRead(inputFileName);
            var outputStream = File.Create(outputFileName);

            var assemblyDef = AssemblyDefinition.ReadAssembly(inputStream, new ReaderParameters() { ReadSymbols = true });
            //assemblyDef.Name.Name += " (Skeleton)";

            DumpAssemblyAndUsageLists(assemblyDef, outputFileBase, 0);

            var usedConstructors = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Default);

            //Step 1: Removing all bodies.
            new RemoveMethodBodiesProcessor().ProcessAssembly(assemblyDef);

            DumpAssemblyAndUsageLists(assemblyDef, outputFileBase, 1);

            //Step 2 and 3: Removing all private members
            new RemovePrivateMembersProcessor().ProcessAssembly(assemblyDef);

            DumpAssemblyAndUsageLists(assemblyDef, outputFileBase, 3);

            //Step 4: Removing all private types            
            foreach (var typeDef in assemblyDef.GetTypesIncludingNested().ToList()) {
                if (!typeDef.IsPublic) {
                    if (typeDef.IsNested) {
                        typeDef.DeclaringType.NestedTypes.Remove(typeDef);
                    } else {
                        typeDef.Module.Types.Remove(typeDef);
                    }
                }
            }

            DumpAssemblyAndUsageLists(assemblyDef, outputFileBase, 4);

            assemblyDef.Write(outputStream);
        }

        static void DumpAssemblyAndUsageLists(AssemblyDefinition assemblyDef, string fileNameBase, int step) {
            var oldName = assemblyDef.Name.Name;
            assemblyDef.Name.Name += " " + step.ToString();
            assemblyDef.Write(fileNameBase + ".skeleton." + step.ToString() + ".dll");
            assemblyDef.Name.Name = oldName;

            var usedTypes = new HashSet<TypeReference>(assemblyDef.GetAllUsedTypes().Where(t => t != null).Select(t => t.TryResolve() ?? t), TypeReferenceEqualityComparer.Default);
            using (var usedTypesWriter = File.CreateText(fileNameBase + ".usedTypes." + step.ToString() + ".txt")) {
                foreach (string fullTypeName in usedTypes.Select(typeRef => "[" + (typeRef.Module == null ? "?" : typeRef.Module.Assembly.Name.Name) + "]" + typeRef.FullName).OrderBy(tn => tn).Distinct()) {
                    usedTypesWriter.WriteLine(fullTypeName);
                }
            }
            var usedAssemblies = new HashSet<AssemblyDefinition>(usedTypes.Where(typeRef => typeRef.Module != null).Select(typeRef => typeRef.Module.Assembly));
            using (var usedAssembliesWriter = File.CreateText(fileNameBase + ".usedAssemblies." + step.ToString() + ".txt")) {
                foreach (var assemblyName in usedAssemblies.Select(a => a.Name.ToString()).OrderBy(_ => _)) {
                    usedAssembliesWriter.WriteLine(assemblyName);
                }
            }
        }
    }
}
