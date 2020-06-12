using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("generated_feign")]

namespace FeignRestClient
{
    public static class FeignBuilder
    {
        private static AssemblyBuilder assemblyBuilder;

        public static T Build<T>()
        {
            T client = default(T);
            var interfaceType = typeof(T);

            if (!interfaceType.IsInterface)
                throw new InvalidOperationException("Type must be interface");

            var feign = interfaceType.GetCustomAttribute(typeof(FeignClientAttribute)) as FeignClientAttribute;
            if (feign == null)
                throw new InvalidOperationException("Interface must have FeignClientAttribute");

            var clientHandlerType = typeof(ClientHandler);
            if (assemblyBuilder == null)
                assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("generated_feign"), AssemblyBuilderAccess.Run);
            var existModule = assemblyBuilder.GetDynamicModule("mainModule_" + interfaceType.Name);
            if (existModule == null)
            {
                var definedModule = assemblyBuilder.DefineDynamicModule("mainModule_" + interfaceType.Name);
                var typeBuilder = definedModule.DefineType("generated_" + interfaceType.Name, TypeAttributes.Public |
                                  TypeAttributes.Class |
                                  TypeAttributes.AutoClass |
                                  TypeAttributes.AnsiClass |
                                  TypeAttributes.BeforeFieldInit |
                                  TypeAttributes.AutoLayout);
             
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[] { });
                var ctorGen = ctor.GetILGenerator();
                ctorGen.Emit(OpCodes.Ldarg_0);
                ctorGen.Emit(OpCodes.Ldstr, feign.Url);
                ctorGen.Emit(OpCodes.Stfld, clientHandlerType.GetField("baseUrl", BindingFlags.Instance | BindingFlags.NonPublic));
                ctorGen.Emit(OpCodes.Ret);

                typeBuilder.AddInterfaceImplementation(interfaceType);
                typeBuilder.SetParent(clientHandlerType);
                foreach (var item in interfaceType.GetMethods())
                {
                    var paramTypes = item.GetParameters().Select(x => x.ParameterType).ToArray();                                 
                    var method = typeBuilder.DefineMethod(item.Name, MethodAttributes.Public | MethodAttributes.Virtual, item.ReturnType, paramTypes);
                    var generator = method.GetILGenerator();
                    typeBuilder.DefineMethodOverride(method, item);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, paramTypes.Length);
                    generator.Emit(OpCodes.Newarr, typeof(object));
                    for (int j = 0; j < paramTypes.Length; j++)
                    {
                        generator.Emit(OpCodes.Dup);
                        generator.Emit(OpCodes.Ldc_I4, j);
                        generator.Emit(OpCodes.Ldarg, j + 1);
                        generator.Emit(OpCodes.Box, paramTypes[j]);
                        generator.Emit(OpCodes.Stelem_I4);
                    }
                    generator.Emit(OpCodes.Callvirt, clientHandlerType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.NonPublic));
                    if (item.ReturnType != typeof(void))
                        generator.Emit(OpCodes.Unbox_Any, item.ReturnType);
                    else
                        generator.Emit(OpCodes.Pop);
                    generator.Emit(OpCodes.Ret);
                }

                client = (T)Activator.CreateInstance(typeBuilder.CreateType());
                return client;
            }
            else
            {
                client = (T)assemblyBuilder.CreateInstance("generated_" + interfaceType.Name);
            }
            return client;
        }
    }
}
