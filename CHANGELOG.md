# [2.6.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.5.0...2.6.0) (2021-08-22)


### Bug Fixes

* Added TEST_FRAMEWORK define symbol to unity test assemblies ([73ecc9d](https://github.com/SolidAlloy/GenericUnityObjects/commit/73ecc9d0a561c948130df5343b6619b80da03b41))
* Fixed ArgumentOutOfRange exception when trying to find a custom script icon ([464e043](https://github.com/SolidAlloy/GenericUnityObjects/commit/464e043b2314849243bb0cd20165d204339c2daf))
* Fixed attempt to generate a concrete class while it already exists when a generic scriptable object is created ([167e87c](https://github.com/SolidAlloy/GenericUnityObjects/commit/167e87c3dae6cd13d0546c205cbfb3f674f57012))
* Fixed long delay before the interactive creation of a scriptable object ([d3f9e4f](https://github.com/SolidAlloy/GenericUnityObjects/commit/d3f9e4fd0f2d753f0002ba567cb6d1c1625b0181))
* Fixed some generic types being regenerated when Unity starts with compilation errors ([920cbe1](https://github.com/SolidAlloy/GenericUnityObjects/commit/920cbe1b3683c95b57aefcc196e0481b27c60d09))
* Removed unnecessary Assembly-CSharp inclusion in type dropdown ([c4e1663](https://github.com/SolidAlloy/GenericUnityObjects/commit/c4e1663c410ce885ba432162f5bd4789ec5fdf34))
* Updated according to changes in TypeReferences ([a8d1883](https://github.com/SolidAlloy/GenericUnityObjects/commit/a8d18830fc14d51f8345e25abf50003137a902fc))


### Features

* Added ability to assign the created asset after assembly recompilation ([feab545](https://github.com/SolidAlloy/GenericUnityObjects/commit/feab545a640304cef31d141035461e3746001021))
* Added object pinging in the hierarchy when an object is created through the Creatable attribute ([4f45bd1](https://github.com/SolidAlloy/GenericUnityObjects/commit/4f45bd1b647087452fd5e717b271224d000eacd6))
* Added the creatable attribute that adds a Create button in the inspector next to object field ([cde98f2](https://github.com/SolidAlloy/GenericUnityObjects/commit/cde98f2d981701db3ee72d2fba93f8fc05a94184))

# [2.5.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.10...2.5.0) (2021-06-20)


### Bug Fixes

* Changed how generic UnityObjects are drawn with Odin ([681eea7](https://github.com/SolidAlloy/GenericUnityObjects/commit/681eea703c4c7a19a036b3fa92c4a2a017880041))


### Features

* Added GenericSOCreator.CreateAssetAtPath method ([38ec365](https://github.com/SolidAlloy/GenericUnityObjects/commit/38ec36577b2dd2b15088284b10c957a9609b3f3d))

## [2.4.10](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.9...2.4.10) (2021-04-30)


### Bug Fixes

* Fixed custom icons not being set for generated classes in Unity 2021 ([8dde5cb](https://github.com/SolidAlloy/GenericUnityObjects/commit/8dde5cbcce4aefe1836ebe40355b1a196ff8daf1))
* Fixed generic types databases not being initialized sometimes ([0e65d84](https://github.com/SolidAlloy/GenericUnityObjects/commit/0e65d84a5aafe8fd653ff15317ef152b6fcd2936))
* Fixed missing type exception when drawing a generic object selector in Unity 2020 ([f301bda](https://github.com/SolidAlloy/GenericUnityObjects/commit/f301bda6d0aca61aac3ea2e3b886532afc808b7b))
* Fixed MissingMethodException when using Unity 2021 ([5289e7c](https://github.com/SolidAlloy/GenericUnityObjects/commit/5289e7c5c5ffd2069ae3ce6f599d0fc5b69a0dca))
* Fixed NullReferenceException when entering PlayMode with PlayModeOptions disabled ([fc85fdf](https://github.com/SolidAlloy/GenericUnityObjects/commit/fc85fdf8d39915627eb91f37da15fb15a31e4779))
* Replaced the wrong using statement in IconFinder ([b48c844](https://github.com/SolidAlloy/GenericUnityObjects/commit/b48c844385c6b4be74906d6f0a1976d32441208a))

## [2.4.9](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.8...2.4.9) (2021-04-01)


### Bug Fixes

* Fixed GUIContent null error when using GenericObjectDrawer.ObjectField with a non-generic UnityEngine.Object instance ([bd71fa6](https://github.com/SolidAlloy/GenericUnityObjects/commit/bd71fa61920babd938535baa3ef31d216c226382))

## [2.4.8](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.7...2.4.8) (2021-03-23)


### Bug Fixes

* Adjusted integration tests to the new OneTypeSelectionWindow class ([dd317ab](https://github.com/SolidAlloy/GenericUnityObjects/commit/dd317ab123cbc99353ef5e0bae62fca99bc63ef3))
* Made OnTypeSelectionWindow a POCO that instantiates dropdown window as a popup ([200b34e](https://github.com/SolidAlloy/GenericUnityObjects/commit/200b34ee9500502f4141ee8320f14594829337ef))
* Updated the Type References dependency ([24178a5](https://github.com/SolidAlloy/GenericUnityObjects/commit/24178a574c5317a34a232ae6922384b06b795992))

## [2.4.7](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.6...2.4.7) (2021-03-19)


### Bug Fixes

* Fixed custom Odin drawers drawing non-generic scriptable objects ([4b892e4](https://github.com/SolidAlloy/GenericUnityObjects/commit/4b892e462963b79c0f01cc4f4216c639e869ebd3))

## [2.4.6](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.5...2.4.6) (2021-03-01)


### Bug Fixes

* Made GenericUnityObjectDrawer override the default behaviour of Odin ([ec518fd](https://github.com/SolidAlloy/GenericUnityObjects/commit/ec518fd3bd7273418b24d01dfd073c9e2d85e871))

## [2.4.5](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.4...2.4.5) (2021-02-23)


### Bug Fixes

* If a child type object is assigned to a generic object field that requires a parent type, the child type will be shown instead of parent ([1b0d432](https://github.com/SolidAlloy/GenericUnityObjects/commit/1b0d432f6a50af9fba14ea66644f65672d7001a2))

## [2.4.4](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.3...2.4.4) (2021-02-22)


### Bug Fixes

* Fixed compilation error on build in TypeCache.GetTypesDerivedFrom() ([e6a3653](https://github.com/SolidAlloy/GenericUnityObjects/commit/e6a3653e99f5f4fa86e06f0e69d743be73fb72cf))

## [2.4.3](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.2...2.4.3) (2021-02-20)


### Bug Fixes

* Added constraints to test assembly definitions ([4ae82a0](https://github.com/SolidAlloy/GenericUnityObjects/commit/4ae82a03251590e2a09275af5f20ae455f85004e))

## [2.4.2](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.1...2.4.2) (2021-02-19)


### Bug Fixes

* Replaced the broken dependency dll ([4623aef](https://github.com/SolidAlloy/GenericUnityObjects/commit/4623aef2783fe3ae99dba5f8eb99bee2c277394b))

## [2.4.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.0...2.4.1) (2021-02-19)


### Bug Fixes

* Fixed compilation errors occurring in dependencies ([7883825](https://github.com/SolidAlloy/GenericUnityObjects/commit/7883825776e1572d516652973a76f9942e95a710))

# [2.4.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.3.0...2.4.0) (2021-02-15)


### Bug Fixes

* Fixed a few errors in the unit tests ([58663ce](https://github.com/SolidAlloy/GenericUnityObjects/commit/58663ce2fc8fd8c1f10249b824d2f6a7379228fd))
* Fixed built-in types not being replaced when displaying generic type name ([f539141](https://github.com/SolidAlloy/GenericUnityObjects/commit/f539141fda421540481ba4cb6f21723b5d88772c))
* Fixed interactive renaming of a scriptable object stopping immediately ([f29f6e0](https://github.com/SolidAlloy/GenericUnityObjects/commit/f29f6e0ece574bb9775049e95c3a07eafd291286))
* Started updating asset title if it was renamed. ([066308f](https://github.com/SolidAlloy/GenericUnityObjects/commit/066308fe908f30b1d3a0d389a1eebd20a7e0e95a))


### Features

* Made generated DLLs respect the custom icons set in generic scripts ([17b9073](https://github.com/SolidAlloy/GenericUnityObjects/commit/17b907357855b9c7f4a15021e9dfd97d88684c03))

# [2.3.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.2.0...2.3.0) (2021-02-08)


### Bug Fixes

* Fixed missing method signatures in GenericUnityEditorInternals ([37795f5](https://github.com/SolidAlloy/GenericUnityObjects/commit/37795f555401e326f945a950f286df76fd26b9c8))


### Features

* Made generic components show up correctly in UnityEvent UI. ([da223f2](https://github.com/SolidAlloy/GenericUnityObjects/commit/da223f2dd08b1fe105b6c13ed25d3a53157b3e17))
* Made Odin show generic classes correctly in the UnityEvent UI. ([b5ce462](https://github.com/SolidAlloy/GenericUnityObjects/commit/b5ce462f510db3147e6b9df28bda5936b6d223d8))

# [2.2.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.1.0...2.2.0) (2021-02-06)


### Bug Fixes

* Added back using statements that are required with GENERIC_UNITY_OBJECTS_DEBUG ([4c287e5](https://github.com/SolidAlloy/GenericUnityObjects/commit/4c287e544e0f775ee04bfd0caa92868ba4418431))
* Allowed label to be null for generic object field ([cb10ff3](https://github.com/SolidAlloy/GenericUnityObjects/commit/cb10ff3c83492967058bd0e354a130a7cbdf559b))
* Fixed the missing dependency caused by SolidUtilities update ([7aced0c](https://github.com/SolidAlloy/GenericUnityObjects/commit/7aced0c644518c3250d5fb9523847795847ce75a))


### Features

* Started adding components to the component menu by their full namespace path ([9133ccf](https://github.com/SolidAlloy/GenericUnityObjects/commit/9133ccfbb71313fd66a523b9793a50b8168ffe4b))

# [2.1.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.0.1...2.1.0) (2021-01-30)


### Features

* Moved GenericUnityObjects folder into Plugins ([9cda042](https://github.com/SolidAlloy/GenericUnityObjects/commit/9cda04242bb04392e30a2fa128030f387ad44bb9))

# [1.2.0](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.5...1.2.0) (2021-01-30)


### Features

* Allowed usage of non-serialized types when creating generic UnityEngine Objects and removed the possibility to choose custom namespace name and scripts path when generating concrete types from generic scriptable objects ([b66a7e5](https://github.com/SolidAlloy/GenericScriptableObjects/commit/b66a7e5d46c869d2fc49cc624bab8a3cae14d830))
* Removed GenericScriptableObject.CreateInstance(Type genericTypeWithoutTypeParams, params Type[] paramTypes) ([a4b7505](https://github.com/SolidAlloy/GenericScriptableObjects/commit/a4b750591d582770a393c5fc8c8cd5f38429081c))


### Performance Improvements

* Improved performance in GenericTypesChecker and DictInitializer ([6206842](https://github.com/SolidAlloy/GenericScriptableObjects/commit/6206842e562ad28fcaaa9cb8c6a23cbc1fab77e7))
* Significantly improved performance in ArgumentsChecker ([545b8b5](https://github.com/SolidAlloy/GenericScriptableObjects/commit/545b8b5069d933abf9fc5d1ca5940e8c34bfc665))

## [1.1.4](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.3...1.1.4) (2020-12-03)


### Bug Fixes

* Default file name for a generic SO is now using short type name instead of full one. ([7b690b3](https://github.com/SolidAlloy/GenericScriptableObjects/commit/7b690b35ed70cc54e7da602036ceb49a8e286da6))
* Disable the warning that appeared after the usage sample is imported ([0d0a24b](https://github.com/SolidAlloy/GenericScriptableObjects/commit/0d0a24ba27d2ef1b02fe24e21ab6e82cd9fbb146))
* Fixed asset creation menus not appearing immediately after the samples import. ([15a8baa](https://github.com/SolidAlloy/GenericScriptableObjects/commit/15a8baa459cdb9201dda2aa0aae3e97a03640530))
* Fixed the bug where methods were added to the MenuItems class on editor start and caused compilation errors ([f44a920](https://github.com/SolidAlloy/GenericScriptableObjects/commit/f44a920abdb6f14ad1985d8731296ecaa5c376b6))
* Reduced GenericSODrawer.OnGUI() execution time by using a type cache ([5adf695](https://github.com/SolidAlloy/GenericScriptableObjects/commit/5adf695af67273d54ad6fdbe271c06a0f09823a7))
* Replaced a method available only in Unity 2020.1+ ([ca415b6](https://github.com/SolidAlloy/GenericScriptableObjects/commit/ca415b61d4478ba8c073f24592552d22c00da5d1))
* Started rounding up values of the window position before its creation. ([0a05863](https://github.com/SolidAlloy/GenericScriptableObjects/commit/0a058633b03f28f3e59dc9c2ea96d2c8e784879a))
* Started using the correct name of GenericSOPersistentStorage class ([92cacc2](https://github.com/SolidAlloy/GenericScriptableObjects/commit/92cacc24a9a8eb3234da4437c3f1a934a2b9f924))

## [1.1.3](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.2...1.1.3) (2020-11-09)


### Bug Fixes

* Added an additional error message when generic scriptable object is instantiated in the field initializer ([fc7904e](https://github.com/SolidAlloy/GenericScriptableObjects/commit/fc7904e84f7acc0808dccb2c4db4bd8490599348))
* Marked GenericScriptableObject.CreateInstance() as pure method ([5903a16](https://github.com/SolidAlloy/GenericScriptableObjects/commit/5903a1666a3cb9b63e42b5772f8236bc802f5a78))

## [1.1.2](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.1...1.1.2) (2020-11-08)


### Bug Fixes

* ObjectField of a non-generic class derived from GenericScriptableObject works without errors now ([a7bec7d](https://github.com/SolidAlloy/GenericScriptableObjects/commit/a7bec7d332f651043126206e1858ce38da639ec9))

## [1.1.1](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.0...1.1.1) (2020-11-05)


### Bug Fixes

* Fixed warning about inconsistent new lines in MenuItems.cs ([44e5231](https://github.com/SolidAlloy/GenericScriptableObjects/commit/44e5231d5b3dd0e980641db24e37744437433a01))

# [1.1.0](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.0.0...1.1.0) (2020-11-04)


### Features

* Added a helper that removes auto-generated code after the GenericScriptableObject class removal ([83d6b41](https://github.com/SolidAlloy/GenericScriptableObjects/commit/83d6b419a13d33bb34b12a599b50179983329384))

# 1.0.0 (2020-11-03)


### Bug Fixes

* The database is now saved to disk when changed ([2787414](https://github.com/SolidAlloy/GenericScriptableObjects/commit/27874147f4ab49e87d497fbb5fb65df12d5e14e3))
