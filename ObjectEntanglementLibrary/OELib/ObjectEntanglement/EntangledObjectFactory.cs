using OELib.PokingConnection;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OELib.ObjectEntanglement
{
    public static class EntangeledObjectFactory
    {
        public static T CreateEntangledObject<T>(string assemblyName, string moduleName, string typeName, Reactor reactor)
        {
            Type targetInterfaceType = typeof(T);
            if (!targetInterfaceType.IsInterface) throw new NotSupportedException("Entangled objects can only be created from interfaces");
            if (targetInterfaceType.GetProperties().Any()) throw new NotImplementedException("Properties not implemented yet");
            var typeAssemblyBuilder = init(assemblyName, moduleName, typeName);
            FieldInfo fi = typeAssemblyBuilder.Item1.DefineField("_reactor", typeof(Reactor), FieldAttributes.Private);
            makeCtor(targetInterfaceType, typeAssemblyBuilder.Item1, fi);
            implementInterface(targetInterfaceType, typeAssemblyBuilder.Item1, fi);
            Type generatedType = typeAssemblyBuilder.Item1.CreateType();
            var instance = (T)Activator.CreateInstance(generatedType, new object[] { reactor });
            typeAssemblyBuilder.Item2.Save("debug.dll");
            return instance;
        }

        private static void implementInterface(Type targetInterfaceType, TypeBuilder typeBuilder, FieldInfo ractorField)
        {
            typeBuilder.AddInterfaceImplementation(targetInterfaceType);
            var methodInfos = targetInterfaceType.GetMethods().ToList();
            foreach (var methodInfo in methodInfos)
            {
                string methodName = methodInfo.Name;
                ParameterInfo retParam = methodInfo.ReturnParameter;
                var inputParams = methodInfo.GetParameters().ToList();
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                                                                        MethodAttributes.Public
                                                                        | MethodAttributes.HideBySig
                                                                        | MethodAttributes.NewSlot
                                                                        | MethodAttributes.Virtual
                                                                        | MethodAttributes.Final,
                                                                        retParam.ParameterType,
                                                                        inputParams.Select(p => p.ParameterType).ToArray()
                                                                        );
                ILGenerator ilGen = methodBuilder.GetILGenerator();
                ilGen.Emit(OpCodes.Nop);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, ractorField);
                ilGen.Emit(OpCodes.Ldstr, methodInfo.Name);
                ilGen.Emit(OpCodes.Ldc_I4, inputParams.Count());
                ilGen.Emit(OpCodes.Newarr, typeof(Object));
                for (int i = 0; i < inputParams.Count; i++)
                {
                    ilGen.Emit(OpCodes.Dup);
                    ilGen.Emit(OpCodes.Ldc_I4, i);

                    ilGen.Emit(OpCodes.Ldarg, i + 1);
                    if (inputParams[i].ParameterType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Box, inputParams[i].ParameterType);
                    }
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }
                ilGen.Emit(OpCodes.Callvirt, typeof(Reactor).GetMethod("CallRemoteMethod", new Type[] { typeof(string), typeof(object[]) }));
                if (retParam.ParameterType != typeof(void))
                {
                    if (retParam.ParameterType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Unbox_Any, retParam.ParameterType);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Castclass, retParam.ParameterType);
                    }
                }
                else
                {
                    ilGen.Emit(OpCodes.Pop);
                }
                ilGen.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }
        }

        private static Tuple<TypeBuilder, AssemblyBuilder> init(string assemblyName, string moduleName, string typeName)
        {
            AppDomain appDom = AppDomain.CurrentDomain;
            AssemblyName assName = new AssemblyName();
            assName.Name = assemblyName;
            AssemblyBuilder assBuilder = appDom.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder modBuild = assBuilder.DefineDynamicModule(moduleName, "debug.dll");
            TypeBuilder typBuild = modBuild.DefineType(typeName, TypeAttributes.Public);
            return new Tuple<TypeBuilder, AssemblyBuilder>(typBuild, assBuilder);
        }

        private static void makeCtor(Type targetInterfaceType, TypeBuilder typeBuilder, FieldInfo fi)
        {
            ConstructorBuilder cb = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(OELib.PokingConnection.Reactor) });
            Type objType = Type.GetType("System.Object");
            ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);
            ILGenerator ilGen = cb.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Call, objCtor);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, fi);
            ilGen.Emit(OpCodes.Ret);
        }
    }
}